using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Parsec.Common;
using Parsec.Extensions;
using Parsec.Helpers;
using Parsec.Serialization;
using Parsec.Shaiya.Data;

namespace Parsec.Shaiya.Core;

public abstract class FileBase : IJsonWritable
{
    /// <summary>
    /// Full path to the file
    /// </summary>
    [JsonIgnore]
    public string Path { get; set; } = "";

    [JsonIgnore]
    public abstract string Extension { get; }

    public Episode Episode { get; set; } = Episode.Unknown;

    [JsonIgnore]
    public Encoding Encoding { get; set; } = Encoding.ASCII;

    /// <summary>
    /// Plain file name
    /// </summary>
    [JsonIgnore]
    public string FileName => System.IO.Path.GetFileName(Path);

    /// <summary>
    /// File name without the extension (.xx)
    /// </summary>
    [JsonIgnore]
    public string FileNameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(Path);

    /// <inheritdoc/>
    public void WriteJson(string path)
    {
        FileHelper.WriteFile(path, JsonSerialize(), Encoding);
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        DefaultValueHandling = DefaultValueHandling.Include,
        StringEscapeHandling = StringEscapeHandling.EscapeHtml,
        Formatting = Formatting.Indented,
        Converters =
        {
            new StringEnumConverter()
        }
    };

    /// <inheritdoc/>
    public virtual string JsonSerialize() => JsonConvert.SerializeObject(this, JsonSerializerSettings);

    protected abstract void Read(SBinaryReader binaryReader);

    protected abstract void Write(SBinaryWriter binaryWriter);

    public void Write(string path)
    {
        var serializationOptions = new BinarySerializationOptions(Episode, Encoding);
        using var binaryWriter = new SBinaryWriter(path, serializationOptions);
        Write(binaryWriter);
    }

    public void Write(string path, Episode episode, Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;

        var serializationOptions = new BinarySerializationOptions(episode, encoding);
        using var binaryWriter = new SBinaryWriter(path, serializationOptions);
        Write(binaryWriter);
    }

    /// <summary>
    /// Reads the shaiya file format from a file
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="serializationOptions">Serialization options</param>
    /// <typeparam name="T">Shaiya File Format Type</typeparam>
    /// <returns>T instance</returns>
    internal static T ReadFromFile<T>(string path, BinarySerializationOptions serializationOptions) where T : FileBase, new()
    {
        var instance = new T
        {
            Path = path,
            Episode = serializationOptions.Episode,
            Encoding = serializationOptions.Encoding
        };
        using var binaryReader = new SBinaryReader(path, serializationOptions);

        if (instance is IEncryptable encryptableInstance)
        {
            encryptableInstance.DecryptBuffer(binaryReader);
        }

        instance.Read(binaryReader);
        return instance;
    }

    /// <summary>
    /// Reads the shaiya file format from a file
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="type">FileBase child type to be read</param>
    /// <param name="serializationOptions">Serialization options</param>
    /// <returns>FileBase instance</returns>
    internal static FileBase ReadFromFile(string path, Type type, BinarySerializationOptions serializationOptions)
    {
        if (!type.GetBaseClassesAndInterfaces().Contains(typeof(FileBase)))
            throw new ArgumentException("Type must be a child of FileBase");

        var instanceObject = Activator.CreateInstance(type);

        if (instanceObject == null)
        {
            throw new Exception("Cannot create instance of FileBase");
        }

        var instance = (FileBase)instanceObject;
        instance.Path = path;
        instance.Episode = serializationOptions.Episode;
        instance.Encoding = serializationOptions.Encoding;

        using var binaryReader = new SBinaryReader(path, serializationOptions);

        if (instance is IEncryptable encryptableInstance)
        {
            encryptableInstance.DecryptBuffer(binaryReader);
        }

        instance.Read(binaryReader);
        return instance;
    }

    /// <summary>
    /// Reads the shaiya file format from a buffer (byte array)
    /// </summary>
    /// <param name="name">File name</param>
    /// <param name="buffer">File buffer</param>
    /// <param name="serializationOptions">Serialization options</param>
    /// <typeparam name="T">Shaiya File Format Type</typeparam>
    /// <returns>T instance</returns>
    internal static T ReadFromBuffer<T>(string name, byte[] buffer, BinarySerializationOptions serializationOptions) where T : FileBase, new()
    {
        var instance = new T
        {
            Path = name,
            Episode = serializationOptions.Episode,
            Encoding = serializationOptions.Encoding
        };
        using var binaryReader = new SBinaryReader(buffer, serializationOptions);

        if (instance is IEncryptable encryptableInstance)
        {
            encryptableInstance.DecryptBuffer(binaryReader);
        }

        instance.Read(binaryReader);
        return instance;
    }

    /// <summary>
    /// Reads the shaiya file format from a buffer (byte array)
    /// </summary>
    /// <param name="name">File name</param>
    /// <param name="buffer">File buffer</param>
    /// <param name="type">FileBase child type to be read</param>
    /// <param name="serializationOptions">Serialization options</param>
    /// <returns>FileBase instance</returns>
    internal static FileBase ReadFromBuffer(string name, byte[] buffer, Type type, BinarySerializationOptions serializationOptions)
    {
        if (!type.GetBaseClassesAndInterfaces().Contains(typeof(FileBase)))
            throw new ArgumentException("Type must be a child of FileBase");

        var instanceObject = Activator.CreateInstance(type);

        if (instanceObject == null)
        {
            throw new Exception("Cannot create instance of FileBase");
        }

        var instance = (FileBase)instanceObject;
        instance.Path = name;
        instance.Episode = serializationOptions.Episode;
        instance.Encoding = serializationOptions.Encoding;

        using var binaryReader = new SBinaryReader(buffer, serializationOptions);

        if (instance is IEncryptable encryptableInstance)
        {
            encryptableInstance.DecryptBuffer(binaryReader);
        }

        instance.Read(binaryReader);
        return instance;
    }

    /// <summary>
    /// Reads the shaiya file format from a buffer (byte array) within a <see cref="Data"/> instance
    /// </summary>
    /// <param name="data"><see cref="Data"/> instance</param>
    /// <param name="file"><see cref="SFile"/> instance</param>
    /// <param name="serializationOptions">Serialization options</param>
    /// <returns>FileBase instance</returns>
    internal static T ReadFromData<T>(Data.Data data, SFile file, BinarySerializationOptions serializationOptions) where T : FileBase, new()
    {
        return ReadFromBuffer<T>(file.Name, data.GetFileBuffer(file), serializationOptions);
    }

    /// <summary>
    /// Reads the shaiya file format from a buffer (byte array) within a <see cref="Data"/> instance
    /// </summary>
    /// <param name="data"><see cref="Data"/> instance</param>
    /// <param name="file"><see cref="SFile"/> instance</param>
    /// <param name="type">FileBase child type to be read</param>
    /// <param name="serializationOptions"></param>
    /// <returns>FileBase instance</returns>
    internal static FileBase ReadFromData(Data.Data data, SFile file, Type type, BinarySerializationOptions serializationOptions)
    {
        if (!data.FileIndex.ContainsKey(file.RelativePath))
            throw new FileNotFoundException("The provided SFile instance is not part of the Data");

        return ReadFromBuffer(file.Name, data.GetFileBuffer(file), type, serializationOptions);
    }

    public IReadOnlyCollection<byte> GetBytes()
    {
        var serializationOptions = BinarySerializationOptions.Default;
        return GetBytes(serializationOptions);
    }

    public IReadOnlyCollection<byte> GetBytes(Episode episode, Encoding? encoding = null)
    {
        var serializationOptions = new BinarySerializationOptions(episode, encoding);
        return GetBytes(serializationOptions);
    }

    public IReadOnlyCollection<byte> GetBytes(BinarySerializationOptions serializationOptions)
    {
        using var memoryStream = new MemoryStream();
        using var binaryWriter = new SBinaryWriter(memoryStream, serializationOptions);
        Write(binaryWriter);
        return memoryStream.ToArray();
    }
}
