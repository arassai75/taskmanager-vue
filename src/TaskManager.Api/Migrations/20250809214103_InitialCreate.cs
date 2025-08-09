using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.CheckConstraint("CK_Categories_Color_Format", "Color IS NULL OR (LENGTH(Color) = 7 AND Color LIKE '#%')");
                    table.CheckConstraint("CK_Categories_Name_NotEmpty", "LENGTH(TRIM(Name)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    DueDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "DECIMAL(6,2)", nullable: true),
                    NotificationsEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.CheckConstraint("CK_Tasks_DeletedAt_Logic", "(IsDeleted = 0 AND DeletedAt IS NULL) OR (IsDeleted = 1 AND DeletedAt IS NOT NULL)");
                    table.CheckConstraint("CK_Tasks_EstimatedHours_Positive", "EstimatedHours IS NULL OR EstimatedHours > 0");
                    table.CheckConstraint("CK_Tasks_EstimatedHours_Reasonable", "EstimatedHours IS NULL OR EstimatedHours <= 999.99");
                    table.CheckConstraint("CK_Tasks_Priority_Valid", "Priority IN (1, 2, 3)");
                    table.CheckConstraint("CK_Tasks_Title_NotEmpty", "LENGTH(TRIM(Title)) > 0");
                    table.ForeignKey(
                        name: "FK_Tasks_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "CreatedAt", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "#6B7280", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "General tasks without specific category", true, "General" },
                    { 2, "#3B82F6", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Work-related tasks and projects", true, "Work" },
                    { 3, "#10B981", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Personal tasks and reminders", true, "Personal" },
                    { 4, "#EF4444", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "High priority urgent tasks", true, "Urgent" }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "DeletedAt", "Description", "DueDate", "EstimatedHours", "NotificationsEnabled", "Priority", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "Write comprehensive documentation for the TaskManager API", new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), 2.5m, true, 2, "Complete project documentation", new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 2, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), null, "Review and approve pending pull requests in the repository", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 1.0m, true, 2, "Review pull requests", new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 3, new DateTime(2023, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), null, "Weekly grocery shopping - milk, bread, fruits", new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), 0.5m, true, 1, "Buy groceries", new DateTime(2023, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 3, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, "Annual dental checkup appointment", new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), 1.5m, true, 2, "Schedule dentist appointment", new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "DeletedAt", "Description", "DueDate", "EstimatedHours", "IsCompleted", "Priority", "Title", "UpdatedAt" },
                values: new object[] { 5, 2, new DateTime(2023, 12, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, "Deploy the latest version to production environment", null, 4.0m, true, 3, "Deploy to production", new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name_Unique",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CategoryId",
                table: "Tasks",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DueDate",
                table: "Tasks",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_EstimatedHours",
                table: "Tasks",
                column: "EstimatedHours");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_IsCompleted_IsDeleted",
                table: "Tasks",
                columns: new[] { "IsCompleted", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_IsDeleted_CreatedAt",
                table: "Tasks",
                columns: new[] { "IsDeleted", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Notifications",
                table: "Tasks",
                columns: new[] { "DueDate", "NotificationsEnabled", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status_Priority",
                table: "Tasks",
                columns: new[] { "IsCompleted", "Priority", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Title",
                table: "Tasks",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
