var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.TheBackendCmsSolution_ApiService>("apiservice");

builder.AddProject<Projects.TheBackendCmsSolution_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
