using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bamboozlers.Migrations
{
    /// <inheritdoc />
    public partial class MessageAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageAttachment",
                table: "MessageAttachment");

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "MessageAttachment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageAttachment",
                table: "MessageAttachment",
                columns: new[] { "ID", "Index" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageAttachment",
                table: "MessageAttachment");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "MessageAttachment");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageAttachment",
                table: "MessageAttachment",
                column: "ID");
        }
    }
}
