using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BikeShop.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomersAndWorkOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    PhoneDigits = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    BikeMakeModel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BikeColour = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    JobType = table.Column<string>(type: "TEXT", nullable: false),
                    WorkRequested = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    EstimatedLabourMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    LabourRateCentsPerHour = table.Column<long>(type: "INTEGER", nullable: false),
                    EstimatedPartsCents = table.Column<long>(type: "INTEGER", nullable: false),
                    PromisedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    AssignedToUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CheckedInByUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckedInAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StatusChangedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoldReason = table.Column<string>(type: "TEXT", nullable: true),
                    PosReceiptNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CancellationReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Users_CheckedInByUserId",
                        column: x => x.CheckedInByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    WrittenByUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    WrittenAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WorkOrderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobNotes_Users_WrittenByUserId",
                        column: x => x.WrittenByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobNotes_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabourEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MechanicUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Minutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LoggedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WorkOrderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabourEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabourEntries_Users_MechanicUserId",
                        column: x => x.MechanicUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourEntries_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPriceCents = table.Column<long>(type: "INTEGER", nullable: false),
                    AddedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WorkOrderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartLines_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneDigits",
                table: "Customers",
                column: "PhoneDigits");

            migrationBuilder.CreateIndex(
                name: "IX_JobNotes_WorkOrderId",
                table: "JobNotes",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_JobNotes_WrittenByUserId",
                table: "JobNotes",
                column: "WrittenByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourEntries_MechanicUserId",
                table: "LabourEntries",
                column: "MechanicUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourEntries_WorkOrderId",
                table: "LabourEntries",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PartLines_WorkOrderId",
                table: "PartLines",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AssignedToUserId",
                table: "WorkOrders",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CheckedInByUserId",
                table: "WorkOrders",
                column: "CheckedInByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CustomerId",
                table: "WorkOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_Status",
                table: "WorkOrders",
                column: "Status");

            // Job numbers start at #1001.
            migrationBuilder.Sql("INSERT INTO sqlite_sequence (name, seq) VALUES ('WorkOrders', 1000);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobNotes");

            migrationBuilder.DropTable(
                name: "LabourEntries");

            migrationBuilder.DropTable(
                name: "PartLines");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
