using System.Net;

namespace WeddingRsvp.Api.Services.Generics;

public class ServiceResponse
{
    public bool IsSuccess { get; private set; }
    public HttpStatusCode StatusCode { get; set; }

    public static ServiceResponse CreateSuccess() => new() { IsSuccess = true, StatusCode = HttpStatusCode.OK };

    public static ServiceResponse CreateFail(HttpStatusCode statusCode ) => new() { IsSuccess = false, StatusCode = statusCode };
}

public class ServiceResponse<TSuccess> where TSuccess : class
{
    public bool IsSuccess { get; private set; }
    public HttpStatusCode StatusCode { get; set; }
    public TSuccess? ValueSuccess { get; private set; }

    public static ServiceResponse<TSuccess> CreateSuccess( TSuccess success )
    {
        ServiceResponse<TSuccess> result = new()
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            ValueSuccess = success
        };

        return result;
    }

    public static ServiceResponse<TSuccess> CreateFail( HttpStatusCode statusCode)
    {
        ServiceResponse<TSuccess> result = new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
        };

        return result;
    }
}