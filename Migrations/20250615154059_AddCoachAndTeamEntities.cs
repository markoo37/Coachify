using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoachCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachAndTeamEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlans_Athletes_AthleteId",
                table: "TrainingPlans");

            migrationBuilder.AlterColumn<int>(
                name: "AthleteId",
                table: "TrainingPlans",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "TrainingPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Athletes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Coaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CoachId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_TeamId",
                table: "TrainingPlans",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Athletes_TeamId",
                table: "Athletes",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CoachId",
                table: "Teams",
                column: "CoachId");

            migrationBuilder.AddForeignKey(
                name: "FK_Athletes_Teams_TeamId",
                table: "Athletes",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlans_Athletes_AthleteId",
                table: "TrainingPlans",
                column: "AthleteId",
                principalTable: "Athletes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlans_Teams_TeamId",
                table: "TrainingPlans",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Athletes_Teams_TeamId",
                table: "Athletes");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlans_Athletes_AthleteId",
                table: "TrainingPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlans_Teams_TeamId",
                table: "TrainingPlans");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Coaches");

            migrationBuilder.DropIndex(
                name: "IX_TrainingPlans_TeamId",
                table: "TrainingPlans");

            migrationBuilder.DropIndex(
                name: "IX_Athletes_TeamId",
                table: "Athletes");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "TrainingPlans");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Athletes");

            migrationBuilder.AlterColumn<int>(
                name: "AthleteId",
                table: "TrainingPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlans_Athletes_AthleteId",
                table: "TrainingPlans",
                column: "AthleteId",
                principalTable: "Athletes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
