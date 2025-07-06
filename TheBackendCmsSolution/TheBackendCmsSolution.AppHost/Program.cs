using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var contentPostgres = builder.AddPostgres("contentdbserver").WithPgAdmin();
var contentDb = contentPostgres.AddDatabase("contentdb");

var tenantsPostgres = builder.AddPostgres("tenantsdbserver").WithPgAdmin();
var tenantsDb = tenantsPostgres.AddDatabase("tenantsdb");

var apiService = builder.AddProject<Projects.TheBackendCmsSolution_ApiService>("apiservice")
                       .WithReference(contentDb)
                       .WithReference(tenantsDb);
builder.AddProject<Projects.TheBackendCmsSolution_Web>("webfrontend")
       .WithReference(apiService);

builder.Build().Run();
