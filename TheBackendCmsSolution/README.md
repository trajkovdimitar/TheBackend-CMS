TheBackend-CMS
A cloud-agnostic, modular Content Management System (CMS) built with .NET 9 and .NET Aspire, inspired by Orchard Core. This project aims to provide a flexible, extensible CMS with features like content management, multi-tenancy, and headless APIs, deployable to any cloud provider (e.g., AWS, Azure, Google Cloud) or on-premises.
Status
Work in progress. Includes a ApiService minimal API with basic content management (create/read/update/delete content items by type) and a PostgreSQL database.
Prerequisites

.NET 8 SDK.
Docker Desktop: For running PostgreSQL and other containerized services. Download.
Git: For cloning the repository. Download.
Code Editor: Visual Studio 2022 (17.9+), VS Code, or similar. Recommended: Visual Studio 2022.
Optional: PostgreSQL client (e.g., pgAdmin, included via Aspire) or API client (e.g., Postman, curl) for testing.

Setup

Clone the Repository:
git clone https://github.com/trajkovdimitar/TheBackend-CMS.git
cd TheBackend-CMS


Navigate to the Solution:
cd TheBackendCmsSolution


Install .NET Aspire Workload (if not already installed):
dotnet workload install aspire


Restore Dependencies:
dotnet restore



Running the Project

Ensure Docker Desktop is Running:

Start Docker Desktop to support PostgreSQL containers.


Run the .NET Aspire AppHost:

In Visual Studio: Set TheBackendCmsSolution.AppHost as the startup project and press F5.
Or via command line:dotnet run --project TheBackendCmsSolution.AppHost


This starts the Aspire dashboard, ApiService, Web, and PostgreSQL database.
Open the dashboard at http://localhost:18888 (port may vary) to monitor services.
Access the ApiService API at http://localhost:<port> (e.g., http://localhost:7309) to interact with endpoints.



Using the API
The ApiService API provides content management endpoints. Use an API client like Postman or curl to test.

Create a Content Item:
curl -X POST http://localhost:<port>/content -H "Content-Type: application/json" -d '{"Title":"My First Post","Body":"Hello, CMS!","Type":"blogpost"}'

Returns the created item with a unique Id.

Retrieve a Content Item:
curl http://localhost:<port>/content/{id}

Returns the item by Id with its ContentType, or 404 if not found.

Retrieve All Content Items:
curl http://localhost:<port>/content

Returns a list of all content items with their ContentType.

Retrieve Content Items by Type:
curl http://localhost:<port>/api/content/{type}

Returns a list of items by type (e.g., blogpost) with their ContentType, or 404 if the type doesn’t exist.

Update a Content Item:
curl -X PUT http://localhost:<port>/content/{id} -H "Content-Type: application/json" -d '{"Title":"Updated Post","Body":"Updated content","Type":"blogpost"}'

Returns the updated item with its ContentType, or 404 if not found.

Delete a Content Item:
curl -X DELETE http://localhost:<port>/content/{id}

Returns 204 No Content if successful, or 404 if not found.


See docs/api.md for detailed API documentation.
Project Structure

TheBackendCmsSolution.AppHost: Orchestrates services (e.g., ApiService, Web, PostgreSQL).
TheBackendCmsSolution.ServiceDefaults: Shared configurations (telemetry, health checks).
TheBackendCmsSolution.ApiService: Minimal API for CMS functionality (content management).
TheBackendCmsSolution.Web: Web frontend (work in progress).
docs/: Detailed documentation.

Contributing
Contributions are welcome! See CONTRIBUTING.md (TBD) for guidelines.
License
MIT License (TBD).