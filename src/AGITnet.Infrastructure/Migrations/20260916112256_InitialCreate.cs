using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AGITnet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plannings",
                columns: table => new
                {
                    PlanningId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CandidateToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plannings", x => x.PlanningId);
                });

            migrationBuilder.CreateTable(
                name: "PlanningSlots",
                columns: table => new
                {
                    PlanningSlotId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanningId = table.Column<int>(type: "integer", nullable: false),
                    SlotOrder = table.Column<int>(type: "integer", nullable: false),
                    SlotName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OriginalQuantity = table.Column<int>(type: "integer", nullable: false),
                    BalancedQuantity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningSlots", x => x.PlanningSlotId);
                    table.ForeignKey(
                        name: "FK_PlanningSlots_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "PlanningId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Plannings_RequestCode",
                table: "Plannings",
                column: "RequestCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanningSlots_PlanningId",
                table: "PlanningSlots",
                column: "PlanningId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanningSlots");

            migrationBuilder.DropTable(
                name: "Plannings");
        }
    }
}
