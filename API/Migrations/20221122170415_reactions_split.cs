using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    public partial class reactions_split : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reactions");

            migrationBuilder.CreateTable(
                name: "CommentReactions",
                columns: table => new
                {
                    ReactionCommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactionAuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactionType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_CommentReactions_Comments_ReactionCommentId",
                        column: x => x.ReactionCommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentReactions_Users_ReactionAuthorId",
                        column: x => x.ReactionAuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostReactions",
                columns: table => new
                {
                    ReactionPostId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactionAuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactionType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_PostReactions_Posts_ReactionPostId",
                        column: x => x.ReactionPostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostReactions_Users_ReactionAuthorId",
                        column: x => x.ReactionAuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentReactions_ReactionAuthorId",
                table: "CommentReactions",
                column: "ReactionAuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReactions_ReactionCommentId_ReactionAuthorId",
                table: "CommentReactions",
                columns: new[] { "ReactionCommentId", "ReactionAuthorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostReactions_ReactionAuthorId",
                table: "PostReactions",
                column: "ReactionAuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_PostReactions_ReactionPostId_ReactionAuthorId",
                table: "PostReactions",
                columns: new[] { "ReactionPostId", "ReactionAuthorId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentReactions");

            migrationBuilder.DropTable(
                name: "PostReactions");

            migrationBuilder.CreateTable(
                name: "Reactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactionAuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactionPostId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactionType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reactions_Posts_ReactionPostId",
                        column: x => x.ReactionPostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reactions_Users_ReactionAuthorId",
                        column: x => x.ReactionAuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_ReactionAuthorId",
                table: "Reactions",
                column: "ReactionAuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_ReactionPostId",
                table: "Reactions",
                column: "ReactionPostId");
        }
    }
}
