// ================================================================
// VIVA COMMENTED VERSION - Migrations/20260425120443_AddPostCommentsFinal.cs
// Purpose: Source file in the OVMS/OCVMS project.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCVMS.Migrations
{
    /// <inheritdoc />
    public partial class AddPostCommentsFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "PostComments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "PostComments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
