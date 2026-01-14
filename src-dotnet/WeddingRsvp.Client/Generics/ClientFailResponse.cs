using System.Net;

namespace WeddingRsvp.Client.Generics;

public class ClientFailResponse
{
    public HttpStatusCode StatusCode { get; set; }

    public ClientFailResponse()
    {
        StatusCode = HttpStatusCode.InternalServerError;
    }

    public ClientFailResponse(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }
}