using System;
using System.Globalization;
using System.IO;
using System.Text;
using Parsec;
using Parsec.Shaiya.Data;

namespace Sample.SahStringObfuscator;

internal static class Program
{
    private const byte DefaultKey = 0x2E;

    private static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            PrintUsage();
            return;
        }

        var command = args[0].ToLowerInvariant();
        if (command != "obfuscate" && command != "deobfuscate")
        {
            Console.Error.WriteLine($"Unknown command '{args[0]}'. Expected 'obfuscate' or 'deobfuscate'.");
            PrintUsage();
            Environment.Exit(1);
            return;
        }

        var inputPath = args[1];
        var outputPath = args[2];

        byte key = DefaultKey;
        if (args.Length >= 4)
        {
            if (!TryParseKey(args[3], out key))
            {
                Console.Error.WriteLine($"Invalid key '{args[3]}'. Provide a value between 0x00 and 0xFF (e.g. '0x2E' or '46').");
                Environment.Exit(1);
                return;
            }
        }

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"Input file not found: {inputPath}");
            Environment.Exit(1);
            return;
        }

        Sah sah;
        try
        {
            sah = ParsecReader.FromFile<Sah>(inputPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to read SAH file: {ex.Message}");
            Environment.Exit(1);
            return;
        }

        Console.WriteLine($"Command : {command}");
        Console.WriteLine($"Input   : {inputPath}");
        Console.WriteLine($"Output  : {outputPath}");
        Console.WriteLine($"Key     : 0x{key:X2}");
        Console.WriteLine();

        int dirCount = 0;
        int fileCount = 0;

        ObfuscateDirectory(sah.RootDirectory, key, ref dirCount, ref fileCount);

        Console.WriteLine($"Processed {dirCount} director{(dirCount == 1 ? "y" : "ies")} and {fileCount} file{(fileCount == 1 ? "" : "s")}.");

        try
        {
            sah.Write(outputPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to write output SAH file: {ex.Message}");
            Environment.Exit(1);
            return;
        }

        Console.WriteLine($"Done. Output written to: {outputPath}");
    }

    private static void ObfuscateDirectory(SDirectory directory, byte key, ref int dirCount, ref int fileCount)
    {
        // Obfuscate the root directory's real name if it is non-empty
        if (directory.ParentDirectory == null)
        {
            if (!string.IsNullOrEmpty(directory.RealName))
            {
                directory.RealName = XorString(directory.RealName, key);
            }
        }
        else
        {
            directory.Name = XorString(directory.Name, key);
        }

        dirCount++;

        foreach (var file in directory.Files)
        {
            file.Name = XorString(file.Name, key);
            fileCount++;
        }

        foreach (var subDirectory in directory.Directories)
        {
            ObfuscateDirectory(subDirectory, key, ref dirCount, ref fileCount);
        }
    }

    private static string XorString(string input, byte key)
    {
        var bytes = Encoding.ASCII.GetBytes(input);
        for (var i = 0; i < bytes.Length; i++)
        {
            var xored = (byte)(bytes[i] ^ key);
            // Skip bytes that would produce 0x00 -- null bytes corrupt the
            // length-prefixed null-terminated string format used by SAH.
            // Leaving the byte unchanged keeps the operation symmetric:
            // if byte == key then both obfuscation and deobfuscation skip it.
            if (xored != 0x00)
            {
                bytes[i] = xored;
            }
        }

        return Encoding.ASCII.GetString(bytes);
    }

    private static bool TryParseKey(string value, out byte key)
    {
        key = 0;
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (uint.TryParse(value[2..], NumberStyles.HexNumber, null, out var hexVal) && hexVal <= 0xFF)
            {
                key = (byte)hexVal;
                return true;
            }

            return false;
        }

        if (uint.TryParse(value, out var decVal) && decVal <= 0xFF)
        {
            key = (byte)decVal;
            return true;
        }

        return false;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  SahStringObfuscator obfuscate   <input.sah> <output.sah> [key]");
        Console.WriteLine("  SahStringObfuscator deobfuscate <input.sah> <output.sah> [key]");
        Console.WriteLine();
        Console.WriteLine("  key  Optional XOR key byte (0x00–0xFF). Hex (e.g. 0x2E) or decimal (e.g. 46).");
        Console.WriteLine($"       Default: 0x{DefaultKey:X2}");
    }
}
