using Hotels.WebAPI.Utils.Results;

namespace Hotels.WebAPI.Utils.Extensions;

public static class XmlResultExtensions
{
    public static IResult Xml<T>(this IResultExtensions _, T result) =>
        new XmlResult<T>(result);
}