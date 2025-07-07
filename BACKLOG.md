# Project Backlog

This backlog lists key issues needed to recreate and extend Orchard Core using .NET 9 and .NET Aspire.

## High Priority
- **Solution Setup** - Ensure the solution builds on .NET 9 with Aspire integration and containerized databases.
- **Module Loader Enhancements** - Extend the module loader to dynamically discover and load modules from external packages.
- **Content Type System** - Implement a flexible content type system with fields, parts, and display options.
- **Multi‑Tenancy** - Support per‑tenant configuration with database isolation.
- **Headless API** - Provide REST/GraphQL endpoints for content management with authentication.
- **Admin Dashboard** - Build a web admin for managing content types, items, and tenants.

## Medium Priority
- **Theme Engine** - Add theming with Razor templates and static asset management.
- **Workflow Engine** - Integrate a workflow system for content publishing and approvals.
- **Localization** - Support multiple languages with culture‑specific content.
- **Search** - Add search capabilities using PostgreSQL full‑text or another provider.
- **Deployment Scripts** - Provide scripts for local dev, Docker, and cloud environments.

## Low Priority
- **Background Tasks** - Infrastructure for scheduled tasks within modules.
- **Media Library** - Upload and serve media assets with storage abstraction.
- **Documentation** - Write detailed contributor guides and API docs.

Each backlog item can become a GitHub issue with details on tasks and acceptance criteria.
