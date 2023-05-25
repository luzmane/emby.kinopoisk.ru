using System.Text;

using MediaBrowser.Model.Serialization;

using Newtonsoft.Json;

namespace EmbyKinopoiskRu.Tests.EmbyMock;
public class EmbyJsonSerializer : IJsonSerializer
{

    public T? DeserializeFromString<T>(string text)
    {
        return JsonConvert.DeserializeObject<T>(text);
    }
    public T? DeserializeFromStream<T>(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var text = reader.ReadToEnd();
        return DeserializeFromString<T>(text);
    }

    public object DeserializeFromBytes(ReadOnlySpan<byte> bytes, Type type)
    {
        throw new NotImplementedException();
    }
    public T DeserializeFromBytes<T>(ReadOnlySpan<byte> bytes)
    {
        throw new NotImplementedException();
    }
    public T DeserializeFromFile<T>(string file) where T : class
    {
        throw new NotImplementedException();
    }
    public Task<object> DeserializeFromFileAsync(Type type, string file)
    {
        throw new NotImplementedException();
    }
    public Task<T> DeserializeFromFileAsync<T>(string file) where T : class
    {
        throw new NotImplementedException();
    }
    public T DeserializeFromSpan<T>(ReadOnlySpan<char> text)
    {
        throw new NotImplementedException();
    }
    public object DeserializeFromSpan(ReadOnlySpan<char> json, Type type)
    {
        throw new NotImplementedException();
    }
    public object DeserializeFromStream(Stream stream, Type type)
    {
        throw new NotImplementedException();
    }
    public Task<T> DeserializeFromStreamAsync<T>(Stream stream)
    {
        throw new NotImplementedException();
    }
    public Task<object> DeserializeFromStreamAsync(Stream stream, Type type)
    {
        throw new NotImplementedException();
    }
    public object DeserializeFromString(string json, Type type)
    {
        throw new NotImplementedException();
    }
    public void SerializeToFile(object obj, string file)
    {
        throw new NotImplementedException();
    }
    public ReadOnlySpan<char> SerializeToSpan(object obj)
    {
        throw new NotImplementedException();
    }
    public void SerializeToStream(object obj, Stream stream)
    {
        throw new NotImplementedException();
    }
    public string SerializeToString(object obj)
    {
        throw new NotImplementedException();
    }
}
