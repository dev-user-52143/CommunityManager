using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var femboyRabbit = builder.AddRabbitMQ("queue")
    .WithLifetime(ContainerLifetime.Persistent);

var backend = builder.AddProject<Projects.CommunityManager_Backend>("backend")
    .WaitFor(femboyRabbit)
    .WithReference(femboyRabbit);

builder.AddProject<Projects.CommunityManager_Frontend>("frontend")
    .WaitFor(backend)
    .WithReference(backend);

builder.Build().Run();