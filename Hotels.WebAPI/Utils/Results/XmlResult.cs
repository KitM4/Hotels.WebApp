using System.Xml.Serialization;

namespace Hotels.WebAPI.Utils.Results;

public class XmlResult<T>(T result) : IResult
{
    private static readonly XmlSerializer _xmlSerializer = new(typeof(T));
    private readonly T _result = result;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        using MemoryStream memoryStream = new();
        _xmlSerializer.Serialize(memoryStream, _result);
        httpContext.Response.ContentType = "application/xml";
        memoryStream.Position = 0;

        return memoryStream.CopyToAsync(httpContext.Response.Body);
    }
}