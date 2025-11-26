var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("weddingrsvp-mongo")
    .WithDataBindMount("data/mongo")
    .WithLifetime(ContainerLifetime.Persistent);

var api = builder.AddProject<Projects.WeddingRsvp_Api>("api")
    .WaitFor(mongodb)
    .WithReference(mongodb)
    .WithEnvironment("WeddingRsvp:AdminIdentifier", "73f71b43-2a0d-48e8-8b43-4b5d89f94edf")
    .WithEnvironment("WeddingRsvp:ApiKey", "my-secret-key");

builder.AddProject<Projects.WeddingRsvp_WebApp>("webapp")
    .WaitFor(api)
    .WithReference(api)
    .WithEnvironment("WeddingRsvpClient:ApiKey", "my-secret-key");

builder.Build().Run();