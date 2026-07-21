using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloggingApp.Migrations
{
    /// <inheritdoc />
    public partial class PopulateUserIdInPostsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Posts SET UserId = '3961a2bb-f019-41dc-891a-defd68a45210' WHERE UserId IS NULL"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
