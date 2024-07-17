using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class v1authdbsetRenamed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_ApplicationRoles_RoleId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Tenants_TenantId",
                table: "RolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RolePermissions",
                table: "RolePermissions");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                newName: "ApplicationRolePermissions");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_TenantId",
                table: "ApplicationRolePermissions",
                newName: "IX_ApplicationRolePermissions_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "ApplicationRolePermissions",
                newName: "IX_ApplicationRolePermissions_PermissionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationRolePermissions",
                table: "ApplicationRolePermissions",
                columns: new[] { "RoleId", "PermissionId", "TenantId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRolePermissions_ApplicationRoles_RoleId",
                table: "ApplicationRolePermissions",
                column: "RoleId",
                principalTable: "ApplicationRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRolePermissions_Permissions_PermissionId",
                table: "ApplicationRolePermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRolePermissions_Tenants_TenantId",
                table: "ApplicationRolePermissions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationRolePermissions_ApplicationRoles_RoleId",
                table: "ApplicationRolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationRolePermissions_Permissions_PermissionId",
                table: "ApplicationRolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationRolePermissions_Tenants_TenantId",
                table: "ApplicationRolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationRolePermissions",
                table: "ApplicationRolePermissions");

            migrationBuilder.RenameTable(
                name: "ApplicationRolePermissions",
                newName: "RolePermissions");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationRolePermissions_TenantId",
                table: "RolePermissions",
                newName: "IX_RolePermissions_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationRolePermissions_PermissionId",
                table: "RolePermissions",
                newName: "IX_RolePermissions_PermissionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolePermissions",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId", "TenantId" });

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_ApplicationRoles_RoleId",
                table: "RolePermissions",
                column: "RoleId",
                principalTable: "ApplicationRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Tenants_TenantId",
                table: "RolePermissions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
