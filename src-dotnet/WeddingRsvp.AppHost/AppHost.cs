using WeddingRsvp.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("weddingrsvp-mongo")
    .WithDataBindMount("data/mongo")
    .WithMongoExpress()
    .WithLifetime(ContainerLifetime.Persistent);

var api = builder.AddProject<Projects.WeddingRsvp_Api>("api")
    .WaitFor(mongodb)
    .WithReference(mongodb)
    .WithEnvironment("WeddingRsvpApi:AdminIdentifier", "73f71b43-2a0d-48e8-8b43-4b5d89f94edf")
    .WithEnvironment("WeddingRsvpApi:ApiKey", "my-secret-key")
    .WithCleanDatabaseCommand("my-secret-key");

builder.AddProject<Projects.WeddingRsvp_WebApp>("webapp")
    .WaitFor(api)
    .WithReference(api)
    .WithEnvironment("WeddingRsvpClient:ApiKey", "my-secret-key")
    .WithEnvironment("WeddingRsvpClient:AdminIdentifier", "73f71b43-2a0d-48e8-8b43-4b5d89f94edf")
    .WithEnvironment("WeddingRsvpWebApp:AdminPassword", "secret");

builder.Build().Run();