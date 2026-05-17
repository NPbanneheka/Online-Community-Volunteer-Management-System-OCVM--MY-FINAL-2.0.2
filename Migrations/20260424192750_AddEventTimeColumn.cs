// ================================================================
// VIVA COMMENTED VERSION - Migrations/20260424192750_AddEventTimeColumn.cs
// Purpose: Source file in the OVMS/OCVMS project.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCVMS.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTimeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "EventTime",
                table: "VolunteerEvents",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventTime",
                table: "VolunteerEvents");
        }
    }
}
