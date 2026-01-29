using System.Net;

namespace WeddingRsvp.Api.Services.Generics;

public class ServiceResponse
{
    public bool IsSuccess { get; private set; }
    public HttpStatusCode StatusCode { get; set; }

    public static ServiceResponse CreateSuccess() => new() { IsSuccess = true, StatusCode = HttpStatusCode.OK };

    public static ServiceResponse CreateFail(HttpStatusCode statusCode ) => new() { IsSuccess = false, StatusCode = statusCode };
}

public class ServiceResponse<TSuccess, TFail> where TSuccess : class where TFail : new()
{
    public bool IsSuccess { get; private set; }
    public HttpStatusCode StatusCode { get; set; }
    public TSuccess? ValueSuccess { get; private set; }
    public TFail ValueFail { get; private set; } = new();

    public static ServiceResponse<TSuccess, TFail> CreateSuccess( TSuccess success )
    {
        ServiceResponse<TSuccess, TFail> result = new()
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            ValueSuccess = success
        };

        return result;
    }

    public static ServiceResponse<TSuccess, TFail> CreateFail( HttpStatusCode statusCode, TFail fail )
    {
        ServiceResponse<TSuccess, TFail> result = new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ValueFail = fail
        };

        return result;
    }
}