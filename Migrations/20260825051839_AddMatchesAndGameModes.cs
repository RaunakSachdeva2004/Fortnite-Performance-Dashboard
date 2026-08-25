using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FortniteDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchesAndGameModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AvgDamage",
                table: "Stats",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AvgPlacement",
                table: "Stats",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PerformanceScore",
                table: "Stats",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Recommendations",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MetricName",
                table: "Recommendations",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecommendedAction",
                table: "Recommendations",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "Recommendations",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Players",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Players",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredGameMode",
                table: "Players",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameModes",
                columns: table => new
                {
                    GameModeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxPlayers = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameModes", x => x.GameModeId);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    MatchId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    GameModeId = table.Column<int>(type: "INTEGER", nullable: true),
                    GameModeName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Placement = table.Column<int>(type: "INTEGER", nullable: false),
                    Eliminations = table.Column<int>(type: "INTEGER", nullable: false),
                    Deaths = table.Column<int>(type: "INTEGER", nullable: false),
                    Assists = table.Column<int>(type: "INTEGER", nullable: false),
                    DamageDealt = table.Column<int>(type: "INTEGER", nullable: false),
                    Accuracy = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    IsWin = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.MatchId);
                    table.ForeignKey(
                        name: "FK_Matches_GameModes_GameModeId",
                        column: x => x.GameModeId,
                        principalTable: "GameModes",
                        principalColumn: "GameModeId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Matches_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "GameModes",
                columns: new[] { "GameModeId", "Code", "Description", "IsActive", "MaxPlayers", "Name" },
                values: new object[,]
                {
                    { 1, "SOLO", "100 players individual battle royale mode", true, 100, "Battle Royale Solo" },
                    { 2, "DUOS", "50 teams of 2 players", true, 100, "Battle Royale Duos" },
                    { 3, "SQUADS", "25 teams of 4 players", true, 100, "Battle Royale Squads" },
                    { 4, "ZB_SOLO", "Tactical combat without building mechanics", true, 100, "Zero Build Solo" },
                    { 5, "RANKED_BR", "Competitive skill-based ranked queue", true, 100, "Ranked Battle Royale" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedDate", "Email", "IsActive", "LastLoginAt", "Name", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@fortnitedashboard.local", true, null, "System Administrator", "AQAAAAIAAYagAAAAEGA3xfnYZUgLokiDTlliblDQFgXUr5S5vFVLG07eYtH1EFPnNK1lRiL7GLA7JEO9Dw==", "Administrator" },
                    { 2, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "ninja@esports.local", true, null, "Ninja_Pro", "AQAAAAIAAYagAAAAELNIfSfWiix3RC2DW7ndoaO9nMRvKWQXwvGEDVsmgdVjRoCGmalP6VeeVghjj/tzQQ==", "Player" },
                    { 3, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bucey@esports.local", true, null, "Bucey_FTW", "AQAAAAIAAYagAAAAEJO3m7sriDoUCh3CZp/uglbLksxCZMbb3qZrJ0SVHe6x9mzrgB9nWJyHeNzHe0v/7g==", "Player" }
                });

            migrationBuilder.InsertData(
                table: "Players",
                columns: new[] { "PlayerId", "AvatarUrl", "Bio", "CreatedDate", "FortniteUsername", "Game", "PreferredGameMode", "Team", "UserId" },
                values: new object[,]
                {
                    { 1, null, "Competitive Fortnite player focusing on late-game rotations and tournament play.", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Ninja_Pro", "Fortnite", "Ranked Battle Royale", "FaZe Clan", 2 },
                    { 2, null, "Zero Build specialist and aim practitioner.", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bucey_FTW", "Fortnite", "Zero Build Solo", "Team Liquid", 3 }
                });

            migrationBuilder.InsertData(
                table: "Matches",
                columns: new[] { "MatchId", "Accuracy", "Assists", "DamageDealt", "Deaths", "Eliminations", "GameModeId", "GameModeName", "IsWin", "Placement", "PlayedAt", "PlayerId", "Score" },
                values: new object[,]
                {
                    { 1, 34.5m, 3, 1420, 0, 8, 5, "Ranked Battle Royale", true, 1, new DateTime(2026, 2, 20, 14, 0, 0, 0, DateTimeKind.Utc), 1, 2100 },
                    { 2, 29.0m, 0, 980, 1, 5, 1, "Battle Royale Solo", false, 3, new DateTime(2026, 2, 20, 15, 0, 0, 0, DateTimeKind.Utc), 1, 1450 },
                    { 3, 21.0m, 1, 410, 1, 2, 4, "Zero Build Solo", false, 12, new DateTime(2026, 2, 18, 9, 30, 0, 0, DateTimeKind.Utc), 2, 680 }
                });

            migrationBuilder.InsertData(
                table: "Recommendations",
                columns: new[] { "RecommendationId", "Category", "CreatedDate", "MetricName", "PlayerId", "RecommendationText", "RecommendedAction", "Severity" },
                values: new object[,]
                {
                    { 1, "Combat Efficiency", new DateTime(2026, 2, 20, 15, 31, 0, 0, DateTimeKind.Utc), "K/D Ratio", 1, "Excellent K/D ratio (4.00)! You are consistently winning trades.", "Keep maintaining height advantage during late-game box fights.", "Low" },
                    { 2, "Late Game Conversion", new DateTime(2026, 2, 18, 10, 1, 0, 0, DateTimeKind.Utc), "Win Rate", 2, "Your win rate is 6.67%. You are reaching mid-game but struggling in final circles.", "Practice early rotations to secure high ground in moving storm zones.", "High" }
                });

            migrationBuilder.InsertData(
                table: "Stats",
                columns: new[] { "StatId", "Accuracy", "AvgDamage", "AvgPlacement", "Deaths", "Eliminations", "KDRatio", "MatchesPlayed", "PerformanceScore", "PlayerId", "RecordedAt", "SyncedUsername", "WinRate", "Wins" },
                values: new object[,]
                {
                    { 1, 28.5m, 450.0m, 8.2m, 38, 145, 3.82m, 50, 84.5m, 1, new DateTime(2026, 2, 10, 12, 0, 0, 0, DateTimeKind.Utc), "Ninja_Pro", 24.0m, 12 },
                    { 2, 31.2m, 520.0m, 6.4m, 47, 188, 4.00m, 65, 89.0m, 1, new DateTime(2026, 2, 20, 15, 30, 0, 0, DateTimeKind.Utc), "Ninja_Pro", 27.69m, 18 },
                    { 3, 19.8m, 280.0m, 18.5m, 28, 42, 1.50m, 30, 61.2m, 2, new DateTime(2026, 2, 18, 10, 0, 0, 0, DateTimeKind.Utc), "Bucey_FTW", 6.67m, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameModes_Code",
                table: "GameModes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_GameModeId",
                table: "Matches",
                column: "GameModeId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_PlayerId_PlayedAt",
                table: "Matches",
                columns: new[] { "PlayerId", "PlayedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "GameModes");

            migrationBuilder.DeleteData(
                table: "Recommendations",
                keyColumn: "RecommendationId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Recommendations",
                keyColumn: "RecommendationId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Stats",
                keyColumn: "StatId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Stats",
                keyColumn: "StatId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Stats",
                keyColumn: "StatId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvgDamage",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "AvgPlacement",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "PerformanceScore",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "MetricName",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "RecommendedAction",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "PreferredGameMode",
                table: "Players");
        }
    }
}
