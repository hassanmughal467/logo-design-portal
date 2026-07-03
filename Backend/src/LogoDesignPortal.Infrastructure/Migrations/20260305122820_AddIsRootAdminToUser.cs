using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddIsRootAdminToUser : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsRootAdmin",
            table: "Users",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.Sql(@"
                UPDATE Users u
                SET u.IsRootAdmin = 1
                WHERE u.Id = (
                    SELECT Id FROM (
                        SELECT u2.Id FROM Users u2
                        INNER JOIN Roles r ON u2.RoleId = r.Id
                        WHERE r.Name = 'SuperAdmin' AND u2.IsDeleted = 0
                        ORDER BY u2.CreatedAt ASC
                        LIMIT 1
                    ) tmp
                );
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsRootAdmin",
            table: "Users");
    }
}
