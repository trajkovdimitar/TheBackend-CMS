using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheBackendCmsSolution.Modules.Content.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContentTypesAndItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "ContentTypes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "ContentTypes");
        }
    }
}
