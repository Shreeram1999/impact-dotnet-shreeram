using System.Data;
using Microsoft.Data.SqlClient;
using StudentApi.Models;

namespace StudentApi.Data.AdoNet;

// Same shape as AdoNetStudentRepository (parameterized commands, a using on
// every connection/command/reader), for the Teachers table - so switching
// the provider moves BOTH entities onto the same database.
public class AdoNetTeacherRepository : IRepository<Teacher>
{
    private const string SelectColumns = "Id, Name, Email, Designation, UserId";

    private readonly ISqlConnectionFactory connectionFactory;

    public AdoNetTeacherRepository(ISqlConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public IEnumerable<Teacher> GetAll()
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand($"SELECT {SelectColumns} FROM dbo.Teachers ORDER BY Id;", connection);
        connection.Open();

        using var reader = command.ExecuteReader();
        var teachers = new List<Teacher>();
        while (reader.Read())
            teachers.Add(Map(reader));
        return teachers;
    }

    public Teacher? GetById(int id)
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand($"SELECT {SelectColumns} FROM dbo.Teachers WHERE Id = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        connection.Open();

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public void Add(Teacher entity)
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand(
            """
            INSERT INTO dbo.Teachers (Name, Email, Designation, UserId)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Email, @Designation, @UserId);
            """, connection);
        AddFieldParameters(command, entity);
        connection.Open();

        entity.Id = (int)command.ExecuteScalar()!;
    }

    public bool Update(Teacher entity)
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand(
            "UPDATE dbo.Teachers SET Name = @Name, Email = @Email, Designation = @Designation, UserId = @UserId WHERE Id = @Id;",
            connection);
        AddFieldParameters(command, entity);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = entity.Id;
        connection.Open();

        return command.ExecuteNonQuery() == 1;
    }

    public bool Delete(int id)
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand("DELETE FROM dbo.Teachers WHERE Id = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        connection.Open();

        return command.ExecuteNonQuery() == 1;
    }

    private static void AddFieldParameters(SqlCommand command, Teacher entity)
    {
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = entity.Name;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = entity.Email;
        command.Parameters.Add("@Designation", SqlDbType.NVarChar, 100).Value = entity.Designation;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = (object?)entity.UserId ?? DBNull.Value;
    }

    private static Teacher Map(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Email = reader.GetString(2),
        Designation = reader.GetString(3),
        UserId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
    };
}
