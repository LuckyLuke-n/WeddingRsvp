var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.WeddingRsvp_Api>("api");

builder.AddProject<Projects.WeddingRsvp_WebApp>("webapp");

builder.Build().Run();
