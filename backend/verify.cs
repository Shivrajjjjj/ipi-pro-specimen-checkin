#!/usr/bin/env dotnet-script
// To run: dotnet script verify.cs

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class VerificationRunner
{
    private static readonly (ConsoleColor color, string symbol) Pass = (ConsoleColor.Green, "✅");
    private static readonly (ConsoleColor color, string symbol) Fail = (ConsoleColor.Red, "❌");
    private static readonly (ConsoleColor color, string symbol) Info = (ConsoleColor.Cyan, "ℹ️ ");
    private static readonly (ConsoleColor color, string symbol) Warn = (ConsoleColor.Yellow, "⚠️ ");

    static async Task Main()
    {
        Console.Clear();
        PrintHeader("IPI PRO — BACKEND VERIFICATION");

        var passed = 0;
        var total = 0;

        // 1. Project Structure
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("▶ PROJECT STRUCTURE");
        Console.ResetColor();

        var files = new[]
        {
            "IpiPro.Api/Program.cs",
            "IpiPro.Api/Context/AppDbContext.cs",
            "IpiPro.Api/Context/DbInitializer.cs",
            "IpiPro.Api/Models/Entities.cs",
            "IpiPro.Api/Services/TenantProvider.cs",
            "IpiPro.Api/Controllers/ManifestsController.cs",
            "IpiPro.Tests/VerificationTests.cs"
        };

        foreach (var file in files)
        {
            var fullPath = Path.Combine("backend", file);
            if (File.Exists(fullPath))
            {
                PrintResult(Pass, $"{file}");
                passed++;
            }
            else
            {
                PrintResult(Fail, $"{file} — NOT FOUND");
            }
            total++;
        }

        // 2. NuGet Dependencies
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("▶ NUGET DEPENDENCIES");
        Console.ResetColor();

        var csprojPath = Path.Combine("backend", "IpiPro.Api", "IpiPro.Api.csproj");
        if (File.Exists(csprojPath))
        {
            var csprojContent = File.ReadAllText(csprojPath);
            var requiredPackages = new[] { "EntityFrameworkCore", "AspNetCore" };

            foreach (var pkg in requiredPackages)
            {
                if (csprojContent.Contains(pkg))
                {
                    PrintResult(Pass, $"{pkg} reference found");
                    passed++;
                }
                else
                {
                    PrintResult(Warn, $"{pkg} not verified in .csproj");
                }
                total++;
            }
        }

        // 3. Build
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("▶ BUILD & COMPILE");
        Console.ResetColor();

        var buildResult = RunCommand("dotnet", "build backend/IpiPro.Api/IpiPro.Api.csproj --configuration Release");
        if (buildResult.success)
        {
            PrintResult(Pass, "Backend builds successfully");
            passed++;
        }
        else
        {
            PrintResult(Fail, $"Build failed: {buildResult.error}");
        }
        total++;

        // 4. Run Tests
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("▶ UNIT TESTS");
        Console.ResetColor();

        var testResult = RunCommand("dotnet", "test backend/IpiPro.Tests/IpiPro.Tests.csproj --verbosity quiet");
        if (testResult.success)
        {
            PrintResult(Pass, "All tests pass");
            passed++;
        }
        else
        {
            PrintResult(Warn, "Tests need review");
        }
        total++;

        // 5. Database Setup
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("▶ DATABASE");
        Console.ResetColor();

        var dbPath = Path.Combine("backend", "ipipro.db");
        if (File.Exists(dbPath))
        {
            PrintResult(Pass, $"SQLite database exists ({new FileInfo(dbPath).Length / 1024}KB)");
            passed++;
        }
        else
        {
            PrintResult(Warn, "Database will be created on first run");
        }
        total++;

        // 6. API Endpoints
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("▶ API ENDPOINTS");
        Console.ResetColor();

        var controllerPath = Path.Combine("backend", "IpiPro.Api", "Controllers", "ManifestsController.cs");
        if (File.Exists(controllerPath))
        {
            var controllerContent = File.ReadAllText(controllerPath);
            var endpoints = new[]
            {
                ("GET /manifests", "GetManifests"),
                ("GET /manifests/{id}", "GetManifest"),
                ("POST /manifests/{id}/specimens/{sid}/receive", "MarkReceived"),
                ("POST /manifests/{id}/specimens/{sid}/flag", "FlagMissing"),
                ("POST /manifests/{id}/close", "CloseManifest")
            };

            foreach (var (route, method) in endpoints)
            {
                if (controllerContent.Contains(method))
                {
                    PrintResult(Pass, route);
                    passed++;
                }
                else
                {
                    PrintResult(Fail, $"{route} — method not found");
                }
                total++;
            }
        }

        // 7. Tenant Isolation
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("▶ TENANT ISOLATION");
        Console.ResetColor();

        if (File.Exists(controllerPath))
        {
            var controllerContent = File.ReadAllText(controllerPath);
            if (controllerContent.Contains("GetCurrentLabId") && controllerContent.Contains("LabId =="))
            {
                PrintResult(Pass, "Tenant checks enforced in controller");
                passed++;
            }
            else
            {
                PrintResult(Fail, "Tenant isolation checks missing");
            }
            total++;
        }

        var dbContextPath = Path.Combine("backend", "IpiPro.Api", "Context", "AppDbContext.cs");
        if (File.Exists(dbContextPath))
        {
            var dbContent = File.ReadAllText(dbContextPath);
            if (dbContent.Contains("HasQueryFilter") && dbContent.Contains("SaveChangesAsync"))
            {
                PrintResult(Pass, "Query filters + injection implemented");
                passed++;
            }
            else
            {
                PrintResult(Fail, "Tenant filtering incomplete");
            }
            total++;
        }

        // Summary
        Console.WriteLine();
        Console.WriteLine(new string('═', 60));
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("📊 VERIFICATION SUMMARY");
        Console.ResetColor();
        Console.WriteLine(new string('═', 60));

        var percentage = (passed * 100) / total;
        var color = percentage >= 90 ? ConsoleColor.Green : percentage >= 70 ? ConsoleColor.Yellow : ConsoleColor.Red;

        Console.ForegroundColor = color;
        Console.WriteLine($"BACKEND: {passed}/{total} ({percentage}% complete)");
        Console.ResetColor();

        if (passed == total)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Backend is ready for submission!");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️  {total - passed} items need attention");
            Console.ResetColor();
        }
    }

    static void PrintHeader(string title)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', 60));
        Console.WriteLine(title);
        Console.WriteLine(new string('═', 60));
        Console.ResetColor();
    }

    static void PrintResult((ConsoleColor color, string symbol) status, string message)
    {
        Console.ForegroundColor = status.color;
        Console.Write(status.symbol);
        Console.ResetColor();
        Console.WriteLine($" {message}");
    }

    static (bool success, string error) RunCommand(string command, string args)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return (process.ExitCode == 0, error);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}