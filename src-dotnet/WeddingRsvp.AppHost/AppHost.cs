var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("weddingrsvp-mongo")
    .WithDataBindMount("data/mongo")
    .WithLifetime(ContainerLifetime.Persistent);

var api = builder.AddProject<Projects.WeddingRsvp_Api>("api")
    .WaitFor(mongodb)
    .WithReference(mongodb)
    .WithEnvironment("WeddingRsvp:AdminIdentifier", "73f71b43-2a0d-48e8-8b43-4b5d89f94edf")
    .WithEnvironment("WeddingRsvp:ApiKey", "my-secret-key");

CommandOptions options = new()
{
    IconName = "Database",
    IconVariant = IconVariant.Filled,
    ConfirmationMessage = "Are you sure you want to clean and seed the database?"
};
api.WithCommand("clean-db", "Clean and seed Database",
    async _ =>
    {
        try
        {
            // Create a handler that ignores SSL certificate errors
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("x-api-key", "my-secret-key");

            var response = await client.PostAsync($"{api.GetEndpoint("http").Url}/api/rsvps/seed", null);

            if (response.IsSuccessStatusCode)
                return new ExecuteCommandResult { Success = true };

            return new ExecuteCommandResult
            {
                Success = false,
                ErrorMessage = response.ReasonPhrase
            };
        }
        catch (Exception ex)
        {
            return new ExecuteCommandResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    },
    options
);

builder.AddProject<Projects.WeddingRsvp_WebApp>("webapp")
    .WaitFor(api)
    .WithReference(api)
    .WithEnvironment("WeddingRsvpClient:ApiKey", "my-secret-key");

builder.Build().Run();