using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vita.Identity.Persistance.Sql.Migrations;

public partial class LoginProvider : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LoginProviders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ExternalUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LoginProviders", x => x.Id);
                table.ForeignKey(
                    name: "FK_LoginProviders_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_LoginProviders_Name_ExternalUserId",
            table: "LoginProviders",
            columns: new[] { "Name", "ExternalUserId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_LoginProviders_UserId",
            table: "LoginProviders",
            column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LoginProviders");
    }
}
