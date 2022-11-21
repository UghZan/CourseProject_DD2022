using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    public partial class subscription_support : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReactionType",
                table: "Reactions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "UserUser",
                columns: table => new
                {
                    SubscribersId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserUser", x => new { x.SubscribersId, x.SubscriptionsId });
                    table.ForeignKey(
                        name: "FK_UserUser_Users_SubscribersId",
                        column: x => x.SubscribersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserUser_Users_SubscriptionsId",
                        column: x => x.SubscriptionsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserUser_SubscriptionsId",
                table: "UserUser",
                column: "SubscriptionsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserUser");

            migrationBuilder.AlterColumn<string>(
                name: "ReactionType",
                table: "Reactions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
