var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("weddingrsvp-mongo")
    .WithDataBindMount("data/mongo")
    .WithLifetime(ContainerLifetime.Persistent);

var api = builder.AddProject<Projects.WeddingRsvp_Api>("api")
    .WaitFor(mongodb)
    .WithReference(mongodb);

builder.AddProject<Projects.WeddingRsvp_WebApp>("webapp")
    .WaitFor(api)
    .WithReference(api);

builder.Build().Run();
