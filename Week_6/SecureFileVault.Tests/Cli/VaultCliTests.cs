using System.Security.Cryptography;
using SecureFileVault.Demos;
using SecureFileVault.Tests.TestSupport;
using SecureFileVault.Vault;

namespace SecureFileVault.Tests.Cli;

public class VaultCliTests
{
    private static (VaultCli Cli, StringWriter Output) CreateCli(string stdin = "")
    {
        var output = new StringWriter();
        return (new VaultCli(output, new StringReader(stdin), new SecureVault(4096)), output);
    }

    [Fact]
    public void NoArguments_PrintsUsage()
    {
        var (cli, output) = CreateCli();

        Assert.Equal(64, cli.Run([]));
        Assert.Contains("encrypt", output.ToString());
    }

    [Fact]
    public void UnknownCommand_PrintsUsage()
    {
        var (cli, _) = CreateCli();

        Assert.Equal(64, cli.Run(["explode"]));
    }

    [Fact]
    public void EncryptThenDecrypt_WithPasswordArgument_ProducesAnIdenticalFile()
    {
        using var temp = new TempFolder();
        var (cli, output) = CreateCli();
        File.WriteAllBytes(temp.File("in.bin"), RandomNumberGenerator.GetBytes(20_000));

        Assert.Equal(0, cli.Run(["encrypt", temp.File("in.bin"), temp.File("in.sfv"), "--password", "pw-123"]));
        Assert.Equal(0, cli.Run(["decrypt", temp.File("in.sfv"), temp.File("out.bin"), "--password", "pw-123"]));
        Assert.Equal(0, cli.Run(["compare", temp.File("in.bin"), temp.File("out.bin")]));
        Assert.Contains("IDENTICAL", output.ToString());
    }

    [Fact]
    public void Encrypt_PromptsForThePassword_WhenNoneIsGiven()
    {
        using var temp = new TempFolder();
        var (cli, output) = CreateCli(stdin: "typed-password\ntyped-password\n");
        File.WriteAllBytes(temp.File("in.bin"), new byte[100]);

        Assert.Equal(0, cli.Run(["encrypt", temp.File("in.bin"), temp.File("in.sfv")]));
        Assert.Equal(0, cli.Run(["decrypt", temp.File("in.sfv"), temp.File("out.bin")]));
        Assert.Contains("Password:", output.ToString());
    }

    [Fact]
    public void Encrypt_EmptyPromptedPassword_IsAnError()
    {
        using var temp = new TempFolder();
        var (cli, output) = CreateCli(stdin: "\n");
        File.WriteAllBytes(temp.File("in.bin"), new byte[100]);

        Assert.Equal(1, cli.Run(["encrypt", temp.File("in.bin"), temp.File("in.sfv")]));
        Assert.Contains("password is required", output.ToString());
    }

    [Fact]
    public void Tamper_ThenDecrypt_IsRejectedWithExitCode2_AndNoOutputIsKept()
    {
        using var temp = new TempFolder();
        var (cli, output) = CreateCli();
        File.WriteAllBytes(temp.File("in.bin"), RandomNumberGenerator.GetBytes(20_000));
        cli.Run(["encrypt", temp.File("in.bin"), temp.File("in.sfv"), "--password", "pw"]);

        Assert.Equal(0, cli.Run(["tamper", temp.File("in.sfv"), "10000"]));
        Assert.Equal(2, cli.Run(["decrypt", temp.File("in.sfv"), temp.File("out.bin"), "--password", "pw"]));
        Assert.Contains("REJECTED", output.ToString());
        Assert.False(File.Exists(temp.File("out.bin")));
    }

    [Fact]
    public void Tamper_OffsetOutsideTheFile_IsAnError()
    {
        using var temp = new TempFolder();
        var (cli, _) = CreateCli();
        File.WriteAllBytes(temp.File("f.bin"), new byte[10]);

        Assert.Equal(1, cli.Run(["tamper", temp.File("f.bin"), "10"]));
    }

    [Fact]
    public void Decrypt_ANonVaultFile_IsAnError()
    {
        using var temp = new TempFolder();
        var (cli, output) = CreateCli();
        File.WriteAllBytes(temp.File("plain.bin"), new byte[100]);

        Assert.Equal(1, cli.Run(["decrypt", temp.File("plain.bin"), temp.File("out.bin"), "--password", "pw"]));
        Assert.Contains("Not a SecureFileVault file", output.ToString());
    }

    [Fact]
    public void Compare_DifferentFiles_ReturnsExitCode3()
    {
        using var temp = new TempFolder();
        var (cli, output) = CreateCli();
        File.WriteAllText(temp.File("a.txt"), "a");
        File.WriteAllText(temp.File("b.txt"), "b");

        Assert.Equal(3, cli.Run(["compare", temp.File("a.txt"), temp.File("b.txt")]));
        Assert.Contains("DIFFERENT", output.ToString());
    }

    [Fact]
    public void Hash_PrintsBothDigests()
    {
        using var temp = new TempFolder();
        var (cli, output) = CreateCli();
        File.WriteAllText(temp.File("abc.txt"), "abc");

        Assert.Equal(0, cli.Run(["hash", temp.File("abc.txt")]));
        Assert.Contains("SHA-256 BA7816BF", output.ToString());
        Assert.Contains("SHA-512 ", output.ToString());
    }

    [Fact]
    public void Generate_CreatesAFileOfTheRequestedSize()
    {
        using var temp = new TempFolder();
        var (cli, _) = CreateCli();

        Assert.Equal(0, cli.Run(["generate", temp.File("g.bin"), "1"]));
        Assert.Equal(1024 * 1024, new FileInfo(temp.File("g.bin")).Length);
    }

    [Fact]
    public void Generate_NonNumericSize_IsAnError()
    {
        using var temp = new TempFolder();
        var (cli, _) = CreateCli();

        Assert.Equal(1, cli.Run(["generate", temp.File("g.bin"), "lots"]));
    }

    [Fact]
    public void StreamDemo_RoundTripsThroughCryptoStream()
    {
        using var temp = new TempFolder();
        var (cli, output) = CreateCli();

        Assert.Equal(0, cli.Run(["stream-demo", temp.Path, "1"]));
        Assert.Contains("IDENTICAL", output.ToString());
    }

    [Fact]
    public void MissingInputFile_IsAnError()
    {
        using var temp = new TempFolder();
        var (cli, _) = CreateCli();

        Assert.Equal(1, cli.Run(["hash", temp.File("missing.bin")]));
    }
}

public class DemoRunnerTests
{
    [Fact]
    public void RunAll_PrintsTheEvidenceForEveryTask()
    {
        var output = new StringWriter();

        new DemoRunner(output).RunAll();

        var text = output.ToString();
        Assert.Contains("byte-identical: True", text);
        Assert.Contains("ciphertexts differ: True", text);
        Assert.Contains("same password + same salt -> same key: True", text);
        Assert.Contains("same password + new salt  -> same key: False", text);
        Assert.Contains("AuthenticationTagMismatchException", text);
        Assert.Contains("FixedTimeEquals(other) = False", text);
        Assert.Contains("max plaintext per RSA operation: 190 bytes", text);
    }

    [Fact]
    public void VaultCli_DemoCommand_RunsTheDemos()
    {
        var output = new StringWriter();

        Assert.Equal(0, new VaultCli(output, new StringReader("")).Run(["demo"]));
        Assert.Contains("== 6.13", output.ToString());
    }
}
