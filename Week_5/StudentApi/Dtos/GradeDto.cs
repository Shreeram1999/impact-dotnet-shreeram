namespace StudentApi.Dtos;

// Task 5.8 - the response shape for GET api/students/{id}/grade, so the
// caller sees which scale was actually applied alongside the result (useful
// once an unrecognized ?scale= silently falls back to percentage).
public record GradeDto(int StudentId, string Scale, string Grade);
