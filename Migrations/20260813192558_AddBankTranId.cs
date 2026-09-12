using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBankTranId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankTranId",
                table: "Enrollments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankTranId",
                table: "Enrollments");
        }
    }
}
