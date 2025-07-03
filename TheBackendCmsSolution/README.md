TheBackend-CMS
A cloud-agnostic, modular Content Management System (CMS) built with .NET 9 and .NET Aspire, inspired by Orchard Core. This project aims to provide a flexible, extensible CMS with features like content management, multi-tenancy, and headless APIs, deployable to any cloud provider (e.g., AWS, Azure, Google Cloud) or on-premises.
Status
Work in progress. Currently includes a minimal .NET Aspire solution with a CmsCore API and PostgreSQL database.
Prerequisites

.NET 9 SDK: Version 9.0.301 or later. Download.
Docker Desktop: For running PostgreSQL and other containerized services. Download.
Git: For cloning the repository. Download.
Code Editor: Visual Studio 2022 (17.9+), VS Code, or similar. Recommended: VS Code with C# extension.
Optional: PostgreSQL client (e.g., pgAdmin, included via Aspire) for database management.

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
dotnet run --project TheBackendCmsSolution.AppHost


This starts the Aspire dashboard, CmsCore API, and PostgreSQL database.
Open the dashboard at http://localhost:18888 (port may vary) to monitor services.
Access the CmsCore API at http://localhost:<port> (e.g., http://localhost:5181) to see the "Hello from TheBackend-CMS!" response.



Project Structure

TheBackendCmsSolution.AppHost: Orchestrates services (e.g., CmsCore, PostgreSQL).
TheBackendCmsSolution.ServiceDefaults: Shared configurations (telemetry, health checks).
TheBackendCmsSolution.CmsCore: Minimal API for CMS functionality (work in progress).
docs/: Detailed documentation (coming soon).

Contributing
Contributions are welcome! See CONTRIBUTING.md (TBD) for guidelines.
License
MIT License (TBD).