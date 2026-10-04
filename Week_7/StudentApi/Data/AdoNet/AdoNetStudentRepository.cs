using System.Data;
using Microsoft.Data.SqlClient;
using StudentApi.Models;

namespace StudentApi.Data.AdoNet;

// Tasks 7.2-7.4 - IRepository<Student> in raw ADO.NET: hand-written SQL,
// SqlCommand + SqlDataReader, and two stored procedures.
//
// Every connection, command and reader is in a using, so they're closed and
// returned to the pool even when a query throws halfway through (Task 7.2's
// "connections close on exception").
//
// Task 7.3 - no SQL is ever built by concatenating a value into a string.
// Every value travels as a typed SqlParameter, so a name like
// "'; DROP TABLE Students;--" is just 25 characters of data to SQL Server -
// it is stored and read back literally (see AdoNetStudentRepositoryTests).
public class AdoNetStudentRepository : IRepository<Student>
{
    private const string SelectColumns = "Id, Name, Age, RollNumber, Email, Score, InternalNotes, EnrolledOn, UserId";

    private readonly ISqlConnectionFactory connectionFactory;

    public AdoNetStudentRepository(ISqlConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    // Task 7.2 - read path: SqlCommand + SqlDataReader.
    public IEnumerable<Student> GetAll()
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand($"SELECT {SelectColumns} FROM dbo.Students ORDER BY Id;", connection);
        connection.Open();

        using var reader = command.ExecuteReader();
        var students = new List<Student>();
        while (reader.Read())
            students.Add(Map(reader));
        return students;
    }

    // Task 7.4 - stored procedure usp_GetStudentById.
    public Student? GetById(int id)
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand("dbo.usp_GetStudentById", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        connection.Open();

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    // Task 7.4 - stored procedure usp_InsertStudent, with the new IDENTITY
    // value coming back through an OUTPUT parameter.
    public void Add(Student entity)
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand("dbo.usp_InsertStudent", connection) { CommandType = CommandType.StoredProcedure };
        AddFieldParameters(command, entity);
        var newId = command.Parameters.Add("@NewId", SqlDbType.Int);
        newId.Direction = ParameterDirection.Output;
        connection.Open();

        command.ExecuteNonQuery();
        entity.Id = (int)newId.Value;
    }

    // Task 7.3 - write path: parameterized UPDATE/DELETE. ExecuteNonQuery's
    // rows-affected count tells us whether the Id existed.
    public bool Update(Student entity)
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand(
            """
            UPDATE dbo.Students
            SET Name = @Name, Age = @Age, RollNumber = @RollNumber, Email = @Email,
                Score = @Score, InternalNotes = @InternalNotes, EnrolledOn = @EnrolledOn
            WHERE Id = @Id;
            """, connection);
        AddFieldParameters(command, entity);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = entity.Id;
        connection.Open();

        return command.ExecuteNonQuery() == 1;
    }

    public bool Delete(int id)
    {
        using var connection = connectionFactory.Create();
        using var command = new SqlCommand("DELETE FROM dbo.Students WHERE Id = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        connection.Open();

        return command.ExecuteNonQuery() == 1;
    }

    // Syllabus - DataAdapter/DataSet: the disconnected model. The adapter
    // opens the connection, fills an in-memory DataTable and closes it again,
    // so the rows can be worked with (or bound to a report grid) without
    // holding a connection. Not used by the API itself - IRepository<T>
    // deals in Student objects - but kept to show the contrast with the
    // connected SqlDataReader above.
    public DataTable GetAllAsDataTable()
    {
        using var connection = connectionFactory.Create();
        using var adapter = new SqlDataAdapter($"SELECT {SelectColumns} FROM dbo.Students ORDER BY Id;", connection);
        var dataSet = new DataSet("StudentPortal");
        adapter.Fill(dataSet, "Students");
        return dataSet.Tables["Students"]!;
    }

    private static void AddFieldParameters(SqlCommand command, Student entity)
    {
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = entity.Name;
        command.Parameters.Add("@Age", SqlDbType.Int).Value = entity.Age;
        command.Parameters.Add("@RollNumber", SqlDbType.NVarChar, 20).Value = entity.RollNumber;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = entity.Email;
        command.Parameters.Add("@Score", SqlDbType.Int).Value = entity.Score;
        command.Parameters.Add("@InternalNotes", SqlDbType.NVarChar, 500).Value = entity.InternalNotes;
        command.Parameters.Add("@EnrolledOn", SqlDbType.Date).Value =
            entity.EnrolledOn is { } enrolled ? enrolled.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
    }

    private static Student Map(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        Age = reader.GetInt32(reader.GetOrdinal("Age")),
        RollNumber = reader.GetString(reader.GetOrdinal("RollNumber")),
        Email = reader.GetString(reader.GetOrdinal("Email")),
        Score = reader.GetInt32(reader.GetOrdinal("Score")),
        InternalNotes = reader.GetString(reader.GetOrdinal("InternalNotes")),
        EnrolledOn = reader.IsDBNull(reader.GetOrdinal("EnrolledOn"))
            ? null
            : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("EnrolledOn"))),
        UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetInt32(reader.GetOrdinal("UserId"))
    };
}
