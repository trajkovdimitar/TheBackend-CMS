using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheBackendCmsSolution.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentItems_ContentTypes_Type",
                table: "ContentItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentTypes",
                table: "ContentTypes");

            migrationBuilder.DropIndex(
                name: "IX_ContentItems_Type",
                table: "ContentItems");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "ContentTypes");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "ContentItems");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ContentItems");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ContentTypes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ContentTypes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ContentTypes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "Fields",
                table: "ContentTypes",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ContentItems",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<Guid>(
                name: "ContentTypeId",
                table: "ContentItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Dictionary<string, object>>(
                name: "Fields",
                table: "ContentItems",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentTypes",
                table: "ContentTypes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_ContentTypeId",
                table: "ContentItems",
                column: "ContentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentItems_ContentTypes_ContentTypeId",
                table: "ContentItems",
                column: "ContentTypeId",
                principalTable: "ContentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentItems_ContentTypes_ContentTypeId",
                table: "ContentItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentTypes",
                table: "ContentTypes");

            migrationBuilder.DropIndex(
                name: "IX_ContentItems_ContentTypeId",
                table: "ContentItems");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ContentTypes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ContentTypes");

            migrationBuilder.DropColumn(
                name: "Fields",
                table: "ContentTypes");

            migrationBuilder.DropColumn(
                name: "ContentTypeId",
                table: "ContentItems");

            migrationBuilder.DropColumn(
                name: "Fields",
                table: "ContentItems");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ContentTypes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "ContentTypes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ContentItems",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "ContentItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ContentItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentTypes",
                table: "ContentTypes",
                column: "Name");

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
    }
}
