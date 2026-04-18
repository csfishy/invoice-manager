using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase1CoreSchema : Migration
    {
        private static readonly Guid WaterCategoryId = new("11111111-1111-1111-1111-111111111111");
        private static readonly Guid ElectricityCategoryId = new("22222222-2222-2222-2222-222222222222");
        private static readonly Guid GasCategoryId = new("33333333-3333-3333-3333-333333333333");
        private static readonly Guid TaxCategoryId = new("44444444-4444-4444-4444-444444444444");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bill_attachments_bills_BillId",
                table: "bill_attachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bill_attachments",
                table: "bill_attachments");

            migrationBuilder.RenameTable(
                name: "bill_attachments",
                newName: "bill_files");

            migrationBuilder.RenameIndex(
                name: "IX_bill_attachments_BillId",
                table: "bill_files",
                newName: "IX_bill_files_BillId");

            migrationBuilder.CreateTable(
                name: "bill_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bill_categories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "bill_categories",
                columns: new[] { "Id", "Name", "Type", "Description", "SortOrder", "IsActive", "IsSystemDefault", "CreatedAtUtc" },
                values: new object[,]
                {
                    { WaterCategoryId, "Water Utility", "Water", "Standard municipal or industrial water billing.", 10, true, true, new DateTime(2026, 4, 18, 0, 0, 0, DateTimeKind.Utc) },
                    { ElectricityCategoryId, "Electricity Utility", "Electricity", "Electricity bills for offices, plants, and properties.", 20, true, true, new DateTime(2026, 4, 18, 0, 0, 0, DateTimeKind.Utc) },
                    { GasCategoryId, "Gas Utility", "Gas", "Natural gas or fuel gas recurring charges.", 30, true, true, new DateTime(2026, 4, 18, 0, 0, 0, DateTimeKind.Utc) },
                    { TaxCategoryId, "Tax Assessment", "Tax", "Local tax, land tax, or related statutory charges.", 40, true, true, new DateTime(2026, 4, 18, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.AddColumn<Guid>(
                name: "BillCategoryId",
                table: "bills",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertyName",
                table: "bills",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bill_files",
                table: "bill_files",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "license_bindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MachineFingerprintHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BindingStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BoundAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastValidatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FeaturesJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_license_bindings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payment_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidOn = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_records_bills_BillId",
                        column: x => x.BillId,
                        principalTable: "bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reminder_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BillCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DaysBeforeDue = table.Column<int>(type: "integer", nullable: false),
                    Recipient = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminder_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reminder_rules_bill_categories_BillCategoryId",
                        column: x => x.BillCategoryId,
                        principalTable: "bill_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    });

            migrationBuilder.Sql($"""
                UPDATE bills
                SET "PropertyName" = CASE
                    WHEN COALESCE("PropertyName", '') = '' THEN COALESCE("CustomerName", '')
                    ELSE "PropertyName"
                END;
                """);

            migrationBuilder.Sql($"""
                UPDATE bills
                SET "BillCategoryId" = CASE
                    WHEN "Type" = 'Water' THEN '{WaterCategoryId}'
                    WHEN "Type" = 'Electricity' THEN '{ElectricityCategoryId}'
                    WHEN "Type" = 'Gas' THEN '{GasCategoryId}'
                    WHEN "Type" = 'Tax' THEN '{TaxCategoryId}'
                    ELSE '{WaterCategoryId}'
                END
                WHERE "BillCategoryId" IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "BillCategoryId",
                table: "bills",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_bills_BillCategoryId",
                table: "bills",
                column: "BillCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_bills_PropertyName",
                table: "bills",
                column: "PropertyName");

            migrationBuilder.CreateIndex(
                name: "IX_bill_categories_Type_Name",
                table: "bill_categories",
                columns: new[] { "Type", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_license_bindings_LicenseId",
                table: "license_bindings",
                column: "LicenseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_license_bindings_MachineFingerprintHash",
                table: "license_bindings",
                column: "MachineFingerprintHash");

            migrationBuilder.CreateIndex(
                name: "IX_payment_records_BillId",
                table: "payment_records",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_records_PaidOn",
                table: "payment_records",
                column: "PaidOn");

            migrationBuilder.CreateIndex(
                name: "IX_reminder_rules_BillCategoryId",
                table: "reminder_rules",
                column: "BillCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_bill_files_bills_BillId",
                table: "bill_files",
                column: "BillId",
                principalTable: "bills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_bills_bill_categories_BillCategoryId",
                table: "bills",
                column: "BillCategoryId",
                principalTable: "bill_categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bill_files_bills_BillId",
                table: "bill_files");

            migrationBuilder.DropForeignKey(
                name: "FK_bills_bill_categories_BillCategoryId",
                table: "bills");

            migrationBuilder.DropTable(
                name: "license_bindings");

            migrationBuilder.DropTable(
                name: "payment_records");

            migrationBuilder.DropTable(
                name: "reminder_rules");

            migrationBuilder.DropTable(
                name: "bill_categories");

            migrationBuilder.DropIndex(
                name: "IX_bills_BillCategoryId",
                table: "bills");

            migrationBuilder.DropIndex(
                name: "IX_bills_PropertyName",
                table: "bills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bill_files",
                table: "bill_files");

            migrationBuilder.DropColumn(
                name: "BillCategoryId",
                table: "bills");

            migrationBuilder.DropColumn(
                name: "PropertyName",
                table: "bills");

            migrationBuilder.RenameTable(
                name: "bill_files",
                newName: "bill_attachments");

            migrationBuilder.RenameIndex(
                name: "IX_bill_files_BillId",
                table: "bill_attachments",
                newName: "IX_bill_attachments_BillId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bill_attachments",
                table: "bill_attachments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_bill_attachments_bills_BillId",
                table: "bill_attachments",
                column: "BillId",
                principalTable: "bills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
