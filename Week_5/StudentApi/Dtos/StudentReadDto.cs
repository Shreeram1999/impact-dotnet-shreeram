namespace StudentApi.Dtos;

// Task 5.7 - the shape a client gets BACK. Notice there is no InternalNotes
// property here at all - not hidden, not nulled out, simply never declared
// - which is what makes it structurally impossible for Student.InternalNotes
// to leak into an API response, no matter what StudentMapper.ToReadDto does.
public class StudentReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string RollNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Score { get; set; }
}
