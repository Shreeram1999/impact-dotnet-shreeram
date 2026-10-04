using System.Data;
using Identity.Api.Models;
using Microsoft.Data.SqlClient;

namespace Identity.Api.Data;

public interface IUserStore
{
    User? FindByUsername(string username);
    void Add(User user);
}

public interface ISqlConnectionFactory
{
    SqlConnection Create();
}

public class SqlConnectionFactory : ISqlConnectionFactory
{
    public const string ConnectionName = "IdentityDb";

    private readonly string connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString(ConnectionName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionName}' is not configured.");
    }

    public SqlConnectionFactory(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public SqlConnection Create() => new(connectionString);
}

// Task 10.2 - the security-critical store in ADO.NET, calling ONLY the two
// stored procedures (no SQL text in the service at all). Promoted from Week
// 7's AdoNetUserStore: same typed parameters, same using-per-call.
public class AdoNetUserStore : IUserStore
{
    private readonly ISqlConnectionFactory connectionFactory;

    public AdoNetUserStore(ISqlConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public User? FindByUsername(string username)
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand("dbo.usp_GetUserByUsername", connection) { CommandType = CommandType.StoredProcedure };
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
        using var command = new SqlCommand("dbo.usp_InsertUser", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = user.Username;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 200).Value = user.PasswordHash;
        command.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = user.Role;
        command.Parameters.Add("@DisplayName", SqlDbType.NVarChar, 100).Value = (object?)user.DisplayName ?? DBNull.Value;
        command.Parameters.Add("@DateOfBirth", SqlDbType.Date).Value =
            user.DateOfBirth is { } dob ? dob.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
        var newId = command.Parameters.Add("@NewId", SqlDbType.Int);
        newId.Direction = ParameterDirection.Output;
        connection.Open();

        try
        {
            command.ExecuteNonQuery();
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new InvalidOperationException($"Username '{user.Username}' already exists.", ex);
        }

        user.Id = (int)newId.Value;
    }
}
