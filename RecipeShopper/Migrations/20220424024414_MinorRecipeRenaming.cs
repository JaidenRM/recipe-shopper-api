using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeShopper.Migrations
{
    public partial class MinorRecipeRenaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "Recipes",
                newName: "LastModifiedUTC");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "Recipes",
                newName: "DurationMinutes");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Recipes",
                newName: "CreatedOnUTC");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Ingredients",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastModifiedUTC",
                table: "Recipes",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "DurationMinutes",
                table: "Recipes",
                newName: "Duration");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUTC",
                table: "Recipes",
                newName: "CreatedOn");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Ingredients",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
