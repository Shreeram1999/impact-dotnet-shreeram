using System.Diagnostics;
using System.Security.Cryptography;
using SecureFileVault.Demos;
using SecureFileVault.Integrity;
using SecureFileVault.Streaming;
using SecureFileVault.Vault;

namespace SecureFileVault;

// Command dispatch for the console tool, kept out of Program.cs and written
// against TextWriter/TextReader so the tests can drive every command without
// a real console.
public class VaultCli
{
    public const string PasswordEnvironmentVariable = "SFV_PASSWORD";

    private readonly TextWriter output;
    private readonly TextReader input;
    private readonly SecureVault vault;

    public VaultCli(TextWriter output, TextReader input, SecureVault? vault = null)
    {
        this.output = output;
        this.input = input;
        this.vault = vault ?? new SecureVault();
    }

    public int Run(string[] args)
    {
        if (args.Length == 0)
            return Usage();

        try
        {
            return args[0].ToLowerInvariant() switch
            {
                "demo" => Demo(),
                "generate" when args.Length == 3 => Generate(args[1], int.Parse(args[2])),
                "encrypt" when args.Length >= 3 => Encrypt(args[1], args[2], args),
                "decrypt" when args.Length >= 3 => Decrypt(args[1], args[2], args),
                "hash" when args.Length == 2 => Hash(args[1]),
                "compare" when args.Length == 3 => Compare(args[1], args[2]),
                "tamper" when args.Length == 3 => Tamper(args[1], long.Parse(args[2])),
                "stream-demo" when args.Length == 3 => StreamDemo(args[1], int.Parse(args[2])),
                _ => Usage()
            };
        }
        catch (AuthenticationTagMismatchException)
        {
            output.WriteLine("REJECTED: wrong password, or the file has been tampered with. No output was kept.");
            return 2;
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or FormatException or ArgumentException)
        {
            output.WriteLine($"ERROR: {ex.Message}");
            return 1;
        }
    }

    private int Demo()
    {
        new DemoRunner(output).RunAll();
        return 0;
    }

    private int Generate(string path, int megabytes)
    {
        var bytes = LargeFileGenerator.Generate(path, megabytes);
        output.WriteLine($"Generated {path} ({bytes:N0} bytes).");
        return 0;
    }

    private int Encrypt(string source, string destination, string[] args)
    {
        var password = ReadPassword(args);
        var timer = Stopwatch.StartNew();
        vault.EncryptFile(source, destination, password);
        output.WriteLine($"Encrypted {source} -> {destination} in {timer.Elapsed.TotalSeconds:0.0}s " +
                         $"(peak working set {PeakMegabytes()} MB).");
        return 0;
    }

    private int Decrypt(string source, string destination, string[] args)
    {
        var password = ReadPassword(args);
        var timer = Stopwatch.StartNew();
        vault.DecryptFile(source, destination, password);
        output.WriteLine($"Decrypted and verified {source} -> {destination} in {timer.Elapsed.TotalSeconds:0.0}s " +
                         $"(peak working set {PeakMegabytes()} MB).");
        return 0;
    }

    private int Hash(string path)
    {
        output.WriteLine($"SHA-256 {Convert.ToHexString(FileHasher.Sha256File(path))}");
        output.WriteLine($"SHA-512 {Convert.ToHexString(FileHasher.Sha512File(path))}");
        return 0;
    }

    private int Compare(string first, string second)
    {
        var identical = DigestComparer.AreEqual(FileHasher.Sha256File(first), FileHasher.Sha256File(second));
        output.WriteLine(identical ? "IDENTICAL (SHA-256 digests match)" : "DIFFERENT (SHA-256 digests differ)");
        return identical ? 0 : 3;
    }

    // Evidence helper for Task 6.9 at file level: flip one bit at an offset.
    private int Tamper(string path, long offset)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
        if (offset < 0 || offset >= stream.Length)
            throw new ArgumentException($"Offset must be between 0 and {stream.Length - 1}.");

        stream.Position = offset;
        var original = stream.ReadByte();
        stream.Position = offset;
        stream.WriteByte((byte)(original ^ 0x01));
        output.WriteLine($"Flipped 1 bit at offset {offset} of {path}.");
        return 0;
    }

    // Task 6.12 literally: FileStream -> CryptoStream (AES-CBC), round-trip,
    // and prove the copy is byte-identical while memory stays flat.
    private int StreamDemo(string folder, int megabytes)
    {
        Directory.CreateDirectory(folder);
        var plain = Path.Combine(folder, "large.bin");
        var encrypted = Path.Combine(folder, "large.bin.cbc");
        var decrypted = Path.Combine(folder, "large.decrypted.bin");

        LargeFileGenerator.Generate(plain, megabytes);
        var key = RandomNumberGenerator.GetBytes(32);
        CbcFileStreamer.EncryptFile(plain, encrypted, key);
        CbcFileStreamer.DecryptFile(encrypted, decrypted, key);

        output.WriteLine($"{megabytes} MB file: {new FileInfo(plain).Length:N0} bytes -> encrypted {new FileInfo(encrypted).Length:N0} bytes -> decrypted {new FileInfo(decrypted).Length:N0} bytes");
        output.WriteLine($"Peak working set during the whole run: {PeakMegabytes()} MB");
        return Compare(plain, decrypted);
    }

    private string ReadPassword(string[] args)
    {
        var index = Array.IndexOf(args, "--password");
        if (index >= 0 && index + 1 < args.Length)
            return args[index + 1];

        var fromEnvironment = Environment.GetEnvironmentVariable(PasswordEnvironmentVariable);
        if (!string.IsNullOrEmpty(fromEnvironment))
            return fromEnvironment;

        output.Write("Password: ");
        var password = input == Console.In && !Console.IsInputRedirected ? ReadMaskedFromConsole() : input.ReadLine();
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("A password is required.");
        return password;
    }

    private static string ReadMaskedFromConsole()
    {
        var chars = new List<char>();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace && chars.Count > 0)
                chars.RemoveAt(chars.Count - 1);
            else if (!char.IsControl(key.KeyChar))
                chars.Add(key.KeyChar);
        }

        Console.WriteLine();
        return new string(chars.ToArray());
    }

    private static long PeakMegabytes() => Process.GetCurrentProcess().PeakWorkingSet64 / (1024 * 1024);

    private int Usage()
    {
        output.WriteLine("""
            SecureFileVault - PBKDF2 + AES-GCM streaming file encryption (Week 6)

              demo                              run every Task 6.1-6.13 demo
              generate <file> <MB>              create a random test file (e.g. 120)
              encrypt  <in> <out> [--password p] encrypt with a password (or SFV_PASSWORD / prompt)
              decrypt  <in> <out> [--password p] decrypt + verify; tampering is rejected
              hash     <file>                   SHA-256 and SHA-512 digests
              compare  <a> <b>                  constant-time digest comparison
              tamper   <file> <offset>          flip one bit (to demonstrate rejection)
              stream-demo <folder> <MB>         Task 6.12 FileStream -> CryptoStream round-trip
            """);
        return 64;
    }
}
