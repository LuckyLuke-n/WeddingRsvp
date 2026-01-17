namespace WeddingRsvp.AppHost;

public static class ProjectResourceBuilderExtensions
{
    public static IResourceBuilder<ProjectResource> WithCleanDatabaseCommand(
        this IResourceBuilder<ProjectResource> builder, 
        string apiKey)
    {
        var commandOptionsSeed = new CommandOptions
        {
            IconName = "Database",
            IconVariant = IconVariant.Filled,
            ConfirmationMessage = "Are you sure you want to clean and seed the database?"
        };
        
        var commandOptionsClean = new CommandOptions
        {
            IconName = "Database",
            IconVariant = IconVariant.Filled,
            ConfirmationMessage = "Are you sure you want to clean the database?"
        };

        builder.WithCommand(
            "clean-and-seed-db",
            "Clean and seed database",
            context => ExecuteCleanAndSeedDatabaseAsync(context, builder, apiKey),
            commandOptionsSeed
        );
        
        builder.WithCommand(
            "clean-db",
            "Clean database",
            context => ExecuteCleanDatabaseAsync(context, builder, apiKey),
            commandOptionsClean
        );

        return builder;
    }

    private static async Task<ExecuteCommandResult> ExecuteCleanAndSeedDatabaseAsync(
        ExecuteCommandContext context,
        IResourceBuilder<ProjectResource> apiBuilder,
        string apiKey)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var endpoint = apiBuilder.Resource.GetEndpoint("http");
            var response = await client.PostAsync($"{endpoint.Url}/api/rsvps/seed", null);

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
                ErrorMessage = ex.Message
            };
        }
    }
    
    private static async Task<ExecuteCommandResult> ExecuteCleanDatabaseAsync(
        ExecuteCommandContext context,
        IResourceBuilder<ProjectResource> apiBuilder,
        string apiKey)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var endpoint = apiBuilder.Resource.GetEndpoint("http");
            var response = await client.PostAsync($"{endpoint.Url}/api/rsvps/clean", null);

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
                ErrorMessage = ex.Message
            };
        }
    }
}