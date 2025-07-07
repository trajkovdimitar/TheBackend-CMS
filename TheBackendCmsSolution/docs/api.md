API Documentation
This document describes the ApiService API endpoints for TheBackend-CMS.
Base URL
http://localhost:<port> (port assigned by .NET Aspire, e.g., 7309). Check the Aspire dashboard for the exact port.
Endpoints
POST /content
Creates a new content item.

Request:{
  "Title": "string",
  "Body": "string",
  "Type": "string" (e.g., "blogpost")
}


Response (201 Created):{
  "Id": "guid",
  "Title": "string",
  "Body": "string",
  "Type": "string",
  "CreatedAt": "datetime",
  "UpdatedAt": null,
  "ContentType": {
    "Name": "string",
    "DisplayName": "string"
  }
}


Example:curl -X POST http://localhost:7309/content -H "Content-Type: application/json" -d '{"Title":"My First Post","Body":"Hello, CMS!","Type":"blogpost"}'



GET /content/{id}
Retrieves a content item by ID.

Request: GET /content/{guid}
Response (200 OK):{
  "Id": "guid",
  "Title": "string",
  "Body": "string",
  "Type": "string",
  "CreatedAt": "datetime",
  "UpdatedAt": null,
  "ContentType": {
    "Name": "string",
    "DisplayName": "string"
  }
}


Error (404 Not Found): If the ID doesn’t exist.
Example:curl http://localhost:7309/content/123e4567-e89b-12d3-a456-426614174000



GET /content
Retrieves all content items.

Request: GET /content
Response (200 OK):[
  {
    "Id": "guid",
    "Title": "string",
    "Body": "string",
    "Type": "string",
    "CreatedAt": "datetime",
    "UpdatedAt": null,
    "ContentType": {
      "Name": "string",
      "DisplayName": "string"
    }
  }
]


Example:curl http://localhost:7309/content



GET /api/content/{type}
Retrieves content items by type.

Request: GET /api/content/{type} (e.g., blogpost)
Response (200 OK):[
  {
    "Id": "guid",
    "Title": "string",
    "Body": "string",
    "Type": "string",
    "CreatedAt": "datetime",
    "UpdatedAt": null,
    "ContentType": {
      "Name": "string",
      "DisplayName": "string"
    }
  }
]


Error (404 Not Found): If the type doesn’t exist.
Example:curl http://localhost:7309/api/content/blogpost



PUT /content/{id}
Updates an existing content item.

Request: PUT /content/{guid}
Request Body:{
  "Title": "string",
  "Body": "string",
  "Type": "string"
}


Response (200 OK):{
  "Id": "guid",
  "Title": "string",
  "Body": "string",
  "Type": "string",
  "CreatedAt": "datetime",
  "UpdatedAt": "datetime",
  "ContentType": {
    "Name": "string",
    "DisplayName": "string"
  }
}


Error (404 Not Found): If the ID doesn’t exist.
Example:curl -X PUT http://localhost:7309/content/123e4567-e89b-12d3-a456-426614174000 -H "Content-Type: application/json" -d '{"Title":"Updated Post","Body":"Updated content","Type":"blogpost"}'



DELETE /content/{id}
Deletes a content item.

Request: DELETE /content/{guid}
Response (204 No Content): If successful.
Error (404 Not Found): If the ID doesn’t exist.
Example:curl -X DELETE http://localhost:7309/content/123e4567-e89b-12d3-a456-426614174000

Content Types
POST /content-types
Creates a new content type.

Request:
{
  "Name": "blogpost",
  "DisplayName": "Blog Post",
  "Fields": {
    "Body": "string",
    "Tags": "string[]"
  }
}

GET /content-types
Retrieves all content types.

GET /content-types/{id}
Retrieves a content type by ID.

PUT /content-types/{id}
Updates an existing content type.

DELETE /content-types/{id}
Deletes a content type.



Notes

The API uses PostgreSQL, orchestrated by .NET Aspire.
Telemetry (request counts, latency) is available in the Aspire dashboard (http://localhost:18888).
Responses include a simplified ContentType object to avoid circular references.

## License
This project is licensed under the [MIT License](../../LICENSE).

