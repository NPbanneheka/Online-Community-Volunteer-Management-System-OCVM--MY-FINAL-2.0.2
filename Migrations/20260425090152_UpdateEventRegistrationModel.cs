// ================================================================
// VIVA COMMENTED VERSION - Migrations/20260425090152_UpdateEventRegistrationModel.cs
// Purpose: Source file in the OVMS/OCVMS project.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCVMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEventRegistrationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "EventRegistrations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_UserId",
                table: "EventRegistrations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventRegistrations_AspNetUsers_UserId",
                table: "EventRegistrations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventRegistrations_AspNetUsers_UserId",
                table: "EventRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_EventRegistrations_UserId",
                table: "EventRegistrations");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "EventRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
