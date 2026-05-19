using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Migrations
{
    /// <inheritdoc />
    public partial class RemovePaymentRefundTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "THANHTOAN_HOANTIEN");

            migrationBuilder.DropCheckConstraint(
                name: "CK_THANHTOAN_SoTien",
                table: "THANHTOAN");

            migrationBuilder.DropCheckConstraint(
                name: "CK_THANHTOAN_TrangThai",
                table: "THANHTOAN");

            migrationBuilder.DropColumn(
                name: "SoTienHoan",
                table: "THANHTOAN");

            migrationBuilder.AddCheckConstraint(
                name: "CK_THANHTOAN_SoTien",
                table: "THANHTOAN",
                sql: "[SoTien] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_THANHTOAN_TrangThai",
                table: "THANHTOAN",
                sql: "[TrangThai] IN ('Pending','Paid','Failed','Cancelled')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_THANHTOAN_SoTien",
                table: "THANHTOAN");

            migrationBuilder.DropCheckConstraint(
                name: "CK_THANHTOAN_TrangThai",
                table: "THANHTOAN");

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienHoan",
                table: "THANHTOAN",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "THANHTOAN_HOANTIEN",
                columns: table => new
                {
                    MaHoanTien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaThanhToan = table.Column<int>(type: "int", nullable: false),
                    MaDonHang = table.Column<int>(type: "int", nullable: false),
                    SoTienHoan = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaGiaoDichHoanTien = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    LyDo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ResponseRaw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "sysutcdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THANHTOAN_HOANTIEN", x => x.MaHoanTien);
                    table.CheckConstraint("CK_THANHTOAN_HOANTIEN_SoTien", "[SoTienHoan] > 0");
                    table.CheckConstraint("CK_THANHTOAN_HOANTIEN_TrangThai", "[TrangThai] IN ('Pending','Succeeded','Failed','Cancelled')");
                    table.ForeignKey(
                        name: "FK_THANHTOAN_HOANTIEN_DONHANG_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DONHANG",
                        principalColumn: "MaDonHang");
                    table.ForeignKey(
                        name: "FK_THANHTOAN_HOANTIEN_THANHTOAN_MaThanhToan",
                        column: x => x.MaThanhToan,
                        principalTable: "THANHTOAN",
                        principalColumn: "MaThanhToan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_THANHTOAN_SoTien",
                table: "THANHTOAN",
                sql: "[SoTien] > 0 AND [SoTienHoan] >= 0 AND [SoTienHoan] <= [SoTien]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_THANHTOAN_TrangThai",
                table: "THANHTOAN",
                sql: "[TrangThai] IN ('Pending','Paid','Failed','Cancelled','Refunded','PartiallyRefunded')");

            migrationBuilder.CreateIndex(
                name: "IX_THANHTOAN_HOANTIEN_MaDonHang",
                table: "THANHTOAN_HOANTIEN",
                column: "MaDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_THANHTOAN_HOANTIEN_MaGiaoDichHoanTien",
                table: "THANHTOAN_HOANTIEN",
                column: "MaGiaoDichHoanTien",
                unique: true,
                filter: "[MaGiaoDichHoanTien] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_THANHTOAN_HOANTIEN_MaThanhToan_NgayTao",
                table: "THANHTOAN_HOANTIEN",
                columns: new[] { "MaThanhToan", "NgayTao" });
        }
    }
}
