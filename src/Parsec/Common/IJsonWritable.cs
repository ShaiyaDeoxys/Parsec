namespace Parsec.Common;

public interface IJsonWritable : IJsonSerializable
{
    /// <summary>
    /// Writes the file as JSON with the possibility of ignoring some properties
    /// </summary>
    /// <param name="path">Path where to save the json file</param>
    void WriteJson(string path);
}
