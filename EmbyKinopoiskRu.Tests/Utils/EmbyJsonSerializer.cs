using System.Text;

using MediaBrowser.Model.Serialization;

using Newtonsoft.Json;

namespace EmbyKinopoiskRu.Tests.Utils;

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
        throw new NotSupportedException();
    }

    public T DeserializeFromBytes<T>(ReadOnlySpan<byte> bytes)
    {
        throw new NotSupportedException();
    }

    public void DeserializePartialJsonInto(string json, object obj)
    {
        throw new NotSupportedException();
    }

    public T DeserializeFromFile<T>(string file) where T : class
    {
        throw new NotSupportedException();
    }

    public void SerializeToFile(object obj, string file, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public Task<object> DeserializeFromFileAsync(Type type, string file)
    {
        throw new NotSupportedException();
    }

    public Task<T> DeserializeFromFileAsync<T>(string file) where T : class
    {
        throw new NotSupportedException();
    }

    public T DeserializeFromSpan<T>(ReadOnlySpan<char> text)
    {
        throw new NotSupportedException();
    }

    public object DeserializeFromSpan(ReadOnlySpan<char> json, Type type)
    {
        throw new NotSupportedException();
    }

    public object DeserializeFromStream(Stream stream, Type type)
    {
        throw new NotSupportedException();
    }

    public Task<T> DeserializeFromStreamAsync<T>(Stream stream)
    {
        throw new NotSupportedException();
    }

    public Task<object> DeserializeFromStreamAsync(Stream stream, Type type)
    {
        throw new NotSupportedException();
    }

    public object DeserializeFromString(string json, Type type)
    {
        throw new NotSupportedException();
    }

    public void SerializeToStream(object obj, Stream stream, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public void SerializeToFile(object obj, string file)
    {
        throw new NotSupportedException();
    }

    public string SerializeToString(object obj, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public ReadOnlySpan<char> SerializeToSpan(object obj)
    {
        throw new NotSupportedException();
    }

    public void SerializeToStream(object obj, Stream stream)
    {
        throw new NotSupportedException();
    }

    public string SerializeToString(object obj)
    {
        throw new NotSupportedException();
    }
}
