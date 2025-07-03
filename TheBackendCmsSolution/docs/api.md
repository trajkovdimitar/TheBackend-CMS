API Documentation
This document describes the ApiService API endpoints for TheBackend-CMS.
Base URL
http://localhost:<port> (port assigned by .NET Aspire, e.g., 5181). Check the Aspire dashboard for the exact port.
Endpoints
POST /content
Creates a new content item (e.g., blog post).

Request:{
  "Title": "string",
  "Body": "string"
}


Response (201 Created):{
  "Id": "guid",
  "Title": "string",
  "Body": "string",
  "CreatedAt": "datetime",
  "UpdatedAt": null
}


Example:curl -X POST http://localhost:5181/content -H "Content-Type: application/json" -d '{"Title":"My First Post","Body":"Hello, CMS!"}'



GET /content/{id}
Retrieves a content item by ID.

Request: GET /content/{guid}
Response (200 OK):{
  "Id": "guid",
  "Title": "string",
  "Body": "string",
  "CreatedAt": "datetime",
  "UpdatedAt": null
}


Error (404 Not Found): If the ID doesn’t exist.
Example:curl http://localhost:5181/content/123e4567-e89b-12d3-a456-426614174000



GET /content
Retrieves all content items.

Request: GET /content
Response (200 OK):[
  {
    "Id": "guid",
    "Title": "string",
    "Body": "string",
    "CreatedAt": "datetime",
    "UpdatedAt": null
  }
]


Example:curl http://localhost:5181/content



Notes

The API uses PostgreSQL, orchestrated by .NET Aspire.
Telemetry (request counts, latency) is available in the Aspire dashboard (http://localhost:18888).
