using System.Net;

namespace WeddingRsvp.Client;

public class ClientResponse<TFail> where TFail : new()
{
    public bool IsSuccess { get; private set; }
    public TFail ValueFail { get; private set; } = new();

    public static ClientResponse<TFail> CreateSuccess() => new() { IsSuccess = true };

    public static ClientResponse<TFail> CreateFail(TFail fail) => new() { IsSuccess = false, ValueFail = fail };
}

public class ClientResponse<TSuccess, TFail> where TSuccess : class where TFail : new()
{
    public bool IsSuccess { get; private set; }
    public TSuccess? ValueSuccess { get; private set; }
    public TFail ValueFail { get; private set; } = new();

    public static ClientResponse<TSuccess, TFail> CreateSuccess(TSuccess success)
    {
        ClientResponse<TSuccess, TFail> result = new()
        {
            IsSuccess = true,
            ValueSuccess = success
        };

        return result;
    }

    public static ClientResponse<TSuccess, TFail> CreateFail(TFail fail)
    {
        ClientResponse<TSuccess, TFail> result = new()
        {
            IsSuccess = false,
            ValueFail = fail
        };

        return result;
    }
}