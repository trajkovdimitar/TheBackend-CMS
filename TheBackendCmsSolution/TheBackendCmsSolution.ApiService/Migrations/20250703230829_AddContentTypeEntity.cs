using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheBackendCmsSolution.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTypeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "ContentItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "ContentTypes",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentTypes", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_Type",
                table: "ContentItems",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentItems_ContentTypes_Type",
                table: "ContentItems",
                column: "Type",
                principalTable: "ContentTypes",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentItems_ContentTypes_Type",
                table: "ContentItems");

            migrationBuilder.DropTable(
                name: "ContentTypes");

            migrationBuilder.DropIndex(
                name: "IX_ContentItems_Type",
                table: "ContentItems");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "ContentItems",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
