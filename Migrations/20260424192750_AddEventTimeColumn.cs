using System;
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
