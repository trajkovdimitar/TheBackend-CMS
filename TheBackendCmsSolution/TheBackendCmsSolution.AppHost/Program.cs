using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var contentPostgres = builder.AddPostgres("contentdbserver").WithPgAdmin();
var contentDb = contentPostgres.AddDatabase("contentdb");

var tenantsPostgres = builder.AddPostgres("tenantsdbserver").WithPgAdmin();
var tenantsDb = tenantsPostgres.AddDatabase("tenantsdb");

var taxonomyPostgres = builder.AddPostgres("taxonomydbserver").WithPgAdmin();
var taxonomyDb = taxonomyPostgres.AddDatabase("taxonomydb");

var usersPostgres = builder.AddPostgres("usersdbserver").WithPgAdmin();
var usersDb = usersPostgres.AddDatabase("usersdb");

var apiService = builder.AddProject<Projects.TheBackendCmsSolution_ApiService>("apiservice")
                       .WithReference(contentDb)
                       .WithReference(tenantsDb)
                       .WithReference(taxonomyDb)
                       .WithReference(usersDb);
builder.AddProject<Projects.TheBackendCmsSolution_Web>("webfrontend")
       .WithReference(apiService);

builder.Build().Run();
