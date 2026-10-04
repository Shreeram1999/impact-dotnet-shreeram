using System.Data;
using Microsoft.Data.SqlClient;
using StudentApi.Auth;
using StudentApi.Models;

namespace StudentApi.Data.AdoNet;

// Task 7.5 - the Week 6 login user moves into SQL Server. This replaces
// InMemoryUserStore behind the same IUserStore interface, so AuthService,
// AuthController and the JWT code are untouched by the swap - login simply
// verifies against dbo.Users now.
//
// This is the security-critical table, and that's where raw ADO.NET's
// explicitness pays off: one short, readable, parameterized query per
// operation, with no ORM translation layer between the login path and the
// database (Week 10 promotes exactly this into the Identity service).
public class AdoNetUserStore : IUserStore
{
    private readonly ISqlConnectionFactory connectionFactory;

    public AdoNetUserStore(ISqlConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    // Case-insensitive because the column uses SQL Server's default
    // case-insensitive collation.
    public User? FindByUsername(string username)
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand(
            "SELECT Id, Username, PasswordHash, Role, DisplayName, DateOfBirth FROM dbo.Users WHERE Username = @Username;", connection);
        command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
        connection.Open();

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        return new User
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            Role = reader.GetString(3),
            DisplayName = reader.IsDBNull(4) ? null : reader.GetString(4),
            DateOfBirth = reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5))
        };
    }

    public void Add(User user)
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand(
            """
            INSERT INTO dbo.Users (Username, PasswordHash, Role, DisplayName, DateOfBirth)
            OUTPUT INSERTED.Id
            VALUES (@Username, @PasswordHash, @Role, @DisplayName, @DateOfBirth);
            """, connection);
        command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = user.Username;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 200).Value = user.PasswordHash;
        command.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = user.Role;
        command.Parameters.Add("@DisplayName", SqlDbType.NVarChar, 100).Value = (object?)user.DisplayName ?? DBNull.Value;
        command.Parameters.Add("@DateOfBirth", SqlDbType.Date).Value =
            user.DateOfBirth is { } dob ? dob.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
        connection.Open();

        try
        {
            user.Id = (int)command.ExecuteScalar()!;
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601) // unique constraint/index violation
        {
            throw new InvalidOperationException($"Username '{user.Username}' already exists.", ex);
        }
    }
}
