using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bamboozlers.Migrations
{
    /// <inheritdoc />
    public partial class Migrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 1,
                column: "SentAt",
                value: new DateTime(2024, 2, 29, 11, 50, 3, 776, DateTimeKind.Local).AddTicks(6370));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 2,
                column: "SentAt",
                value: new DateTime(2024, 2, 29, 11, 50, 3, 776, DateTimeKind.Local).AddTicks(6450));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 3,
                column: "SentAt",
                value: new DateTime(2024, 2, 29, 11, 50, 3, 776, DateTimeKind.Local).AddTicks(6460));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 4,
                column: "SentAt",
                value: new DateTime(2024, 2, 29, 11, 50, 3, 776, DateTimeKind.Local).AddTicks(6460));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 5,
                column: "SentAt",
                value: new DateTime(2024, 2, 29, 11, 50, 3, 776, DateTimeKind.Local).AddTicks(6460));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 6,
                column: "SentAt",
                value: new DateTime(2024, 2, 29, 11, 50, 3, 776, DateTimeKind.Local).AddTicks(6460));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 7,
                column: "SentAt",
                value: new DateTime(2024, 2, 29, 11, 50, 3, 776, DateTimeKind.Local).AddTicks(6470));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 8,
                column: "SentAt",
                value: new DateTime(2024, 2, 29, 11, 50, 3, 776, DateTimeKind.Local).AddTicks(6470));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 1,
                column: "SentAt",
                value: new DateTime(2024, 2, 13, 20, 8, 11, 48, DateTimeKind.Local).AddTicks(1153));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 2,
                column: "SentAt",
                value: new DateTime(2024, 2, 13, 20, 8, 11, 48, DateTimeKind.Local).AddTicks(1198));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 3,
                column: "SentAt",
                value: new DateTime(2024, 2, 13, 20, 8, 11, 48, DateTimeKind.Local).AddTicks(1201));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 4,
                column: "SentAt",
                value: new DateTime(2024, 2, 13, 20, 8, 11, 48, DateTimeKind.Local).AddTicks(1204));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 5,
                column: "SentAt",
                value: new DateTime(2024, 2, 13, 20, 8, 11, 48, DateTimeKind.Local).AddTicks(1206));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 6,
                column: "SentAt",
                value: new DateTime(2024, 2, 13, 20, 8, 11, 48, DateTimeKind.Local).AddTicks(1255));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 7,
                column: "SentAt",
                value: new DateTime(2024, 2, 13, 20, 8, 11, 48, DateTimeKind.Local).AddTicks(1258));

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "ID",
                keyValue: 8,
                column: "SentAt",
                value: new DateTime(2024, 2, 13, 20, 8, 11, 48, DateTimeKind.Local).AddTicks(1261));
        }
    }
}
