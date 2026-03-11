namespace Parsec.Common;

/// <summary>
/// Interface that defines the behavior of files that can be exported as json
/// </summary>
public interface IJsonSerializable
{
    /// <summary>
    /// Serializes an object into json
    /// </summary>
    /// <returns>The serialized object as a json string</returns>
    string JsonSerialize();
}
