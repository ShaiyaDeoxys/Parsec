using System.Text;

namespace Parsec.Shaiya.Data;

public sealed class Saf : IDisposable
{
    private readonly FileStream _fileStream;

    internal FileStream FileStream => _fileStream;

    public Saf(string path)
    {
        Path = path;
        _fileStream = File.Open(Path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }

    /// <summary>
    /// Absolute path to the saf file
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Reads an array of bytes from the saf file
    /// </summary>
    /// <param name="offset">Offset where to start reading</param>
    /// <param name="length">Amount of bytes to read</param>
    public byte[] ReadBytes(long offset, int length)
    {
        using var binaryReader = new BinaryReader(_fileStream, Encoding.Default, leaveOpen: true);
        binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
        return binaryReader.ReadBytes(length);
    }

    /// <summary>
    /// Clears a section of the saf file. The section will be set to `0`, since it can't be removed directly
    /// </summary>
    /// <param name="offset">Offset where to start clearing</param>
    /// <param name="length">Amount of bytes to clear</param>
    public void ClearBytes(long offset, int length)
    {
        using var safWriter = new BinaryWriter(_fileStream, Encoding.Default, leaveOpen: true);
        safWriter.BaseStream.Seek(offset, SeekOrigin.Begin);

        var emptyBytes = new byte[length];
        safWriter.Write(emptyBytes);
    }

    public void Dispose()
    {
        _fileStream.Dispose();
    }
}
