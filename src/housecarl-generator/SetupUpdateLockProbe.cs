using SetupProgram = HousecarlSetup.Program;

namespace HousecarlGenerator;

/// <summary>
/// Regression guard for the Codex installer's update lock. It drives the real installer over a
/// synthetic package and temporary home, proving a clean install, an executable-lock refusal before
/// any copy, and the defense-in-depth sharing-violation catch for a locked sibling DLL.
///
/// Run: dotnet run --project src/housecarl-generator setup-update-lock-guard
/// </summary>
internal static class SetupUpdateLockProbe
{
    public static int RunGuard(string[] args)
    {
        Console.WriteLine("================================================================");
        Console.WriteLine(" setup update-lock guard — Codex installer");
        Console.WriteLine("================================================================");
        Console.WriteLine();
        int fail = 0;
        void Check(bool condition, string label)
        {
            Console.WriteLine((condition ? "  PASS  " : "  FAIL  ") + label);
            if (!condition) fail++;
        }

        string? originalCodexHome = Environment.GetEnvironmentVariable("CODEX_HOME");
        Environment.SetEnvironmentVariable("CODEX_HOME", null);

        string root = Path.Combine(Path.GetTempPath(), "hc-setup-lock-" + Guid.NewGuid().ToString("N"));
        string package = Path.Combine(root, "package");
        string source = Path.Combine(package, "housecarl");
        string home = Path.Combine(root, "home");
        string serverDir = Path.Combine(home, "houseCARL", "server");
        string exe = Path.Combine(serverDir, "housecarl-mcp.exe");
        string dll = Path.Combine(serverDir, "Mutagen.Bethesda.dll");
        string sentinel = Path.Combine(home, ".agents", "skills", "demo-skill", "SKILL.md");
        string config = Path.Combine(home, ".codex", "config.toml");

        byte[] version1 = { 1, 1, 1, 1 };
        byte[] version2 = { 2, 2, 2, 2 };

        try
        {
            WriteFile(Path.Combine(source, ".codex-plugin", "plugin.json"), "{}");
            WriteFile(Path.Combine(source, "server", "housecarl-mcp.exe"), version1);
            WriteFile(Path.Combine(source, "server", "Mutagen.Bethesda.dll"), new byte[] { 9 });
            WriteFile(Path.Combine(source, "skills", "demo-skill", "SKILL.md"), "demo");

            Console.WriteLine("--- T1: a clean install succeeds ---");
            var clean = SetupProgram.TryInstall(source, home, home);
            Check(clean.Outcome == SetupProgram.InstallOutcome.Installed, "clean install => Installed");
            Check(File.Exists(exe), "server executable installed");
            Check(File.Exists(sentinel), "skill installed under ~/.agents/skills");
            Check(File.Exists(config), "MCP registration written under ~/.codex");

            Console.WriteLine();
            Console.WriteLine("--- T2: a locked server refuses before any copy ---");
            WriteFile(Path.Combine(source, "server", "housecarl-mcp.exe"), version2);
            File.Delete(sentinel);
            SetupProgram.InstallResult lockedExe;
            using (HoldLikeRunning(exe))
                lockedExe = SetupProgram.TryInstall(source, home, home);
            Check(lockedExe.Outcome == SetupProgram.InstallOutcome.ServerInUse, "locked exe => ServerInUse");
            Check(lockedExe.RefusedBeforeAnyCopy, "refused at pre-flight");
            Check(!File.Exists(sentinel), "no copy ran after the refusal");
            Check(File.ReadAllBytes(exe).AsSpan().SequenceEqual(version1), "installed executable remains intact");

            WriteFile(sentinel, "demo");
            WriteFile(Path.Combine(source, "server", "housecarl-mcp.exe"), version1);

            Console.WriteLine();
            Console.WriteLine("--- T3: a locked sibling DLL is caught during copy ---");
            SetupProgram.InstallResult lockedDll;
            using (HoldLikeRunning(dll))
                lockedDll = SetupProgram.TryInstall(source, home, home);
            Check(lockedDll.Outcome == SetupProgram.InstallOutcome.ServerInUse,
                "locked DLL => ServerInUse instead of an uncaught IOException");
            Check(!lockedDll.RefusedBeforeAnyCopy, "the defense-in-depth catch remains distinct from pre-flight");
        }
        finally
        {
            Environment.SetEnvironmentVariable("CODEX_HOME", originalCodexHome);
            try { Directory.Delete(root, recursive: true); } catch { /* non-fatal */ }
        }

        Console.WriteLine();
        Console.WriteLine(fail == 0
            ? "================ ALL PASS ================"
            : $"================ {fail} CHECK(S) FAILED ================");
        return fail == 0 ? 0 : 1;
    }

    private static FileStream HoldLikeRunning(string path)
        => new(path, FileMode.Open, FileAccess.Read, FileShare.Read);

    private static void WriteFile(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    private static void WriteFile(string path, byte[] bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
    }
}
