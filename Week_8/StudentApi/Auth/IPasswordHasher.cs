namespace StudentApi.Auth;

// Task 6.15 - behind an interface so AuthService can be unit-tested with a
// mocked hasher (no 100,000-iteration PBKDF2 in every service test).
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string storedHash);
}
