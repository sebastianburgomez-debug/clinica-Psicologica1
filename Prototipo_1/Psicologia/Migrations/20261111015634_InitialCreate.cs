
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace Psicologia.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MODULOS
            migrationBuilder.CreateTable(
                name: "Modulos",
                columns: table => new
                {
                    IdModulo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Nombre = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modulos", x => x.IdModulo);
                });

            // PERMISOS
            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    IdPermiso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Nombre = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.IdPermiso);
                });

            migrationBuilder.CreateTable(
                name: "Secciones",
                columns: table => new
                {
                    IdSeccion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Nombre = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    IdModulo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Secciones", x => x.IdSeccion);

                    table.ForeignKey(
                        name: "FK_Secciones_Modulos_IdModulo",
                        column: x => x.IdModulo,
                        principalTable: "Modulos",
                        principalColumn: "IdModulo",
                        onDelete: ReferentialAction.Cascade);
                });

            
            migrationBuilder.CreateTable(
                name: "RolPermisos",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),

                    PermisoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_RolPermisos",
                        x => new { x.RoleId, x.PermisoId });

                    table.ForeignKey(
                        name: "FK_RolPermisos_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permisos",
                        principalColumn: "IdPermiso",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_RolPermisos_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Name = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    Correo = table.Column<string>(
                        type: "nvarchar(450)",
                        nullable: false),

                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);

                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            // ROL SECCIONES
            migrationBuilder.CreateTable(
                name: "RolSecciones",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),

                    SeccionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_RolSecciones",
                        x => new { x.RoleId, x.SeccionId });

                    table.ForeignKey(
                        name: "FK_RolSecciones_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_RolSecciones_Secciones_SeccionId",
                        column: x => x.SeccionId,
                        principalTable: "Secciones",
                        principalColumn: "IdSeccion",
                        onDelete: ReferentialAction.Cascade);
                });

            // USUARIO ESPECIALIDADES
            
            migrationBuilder.CreateTable(
                name: "UsuarioEspecialidades",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),

                    SpecialtyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_UsuarioEspecialidades",
                        x => new { x.UsuarioId, x.SpecialtyId });

                    table.ForeignKey(
                        name: "FK_UsuarioEspecialidades_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "SpecialtyId",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_UsuarioEspecialidades_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            // DATOS INICIALES DE USUARIOS
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "UsuarioId", "Correo", "Name", "RoleId" },
                values: new object[]
                {
                    1,
                    "kata@gmail.com",
                    "Katalina gonzalez",
                    1
                });


            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_PermisoId",
                table: "RolPermisos",
                column: "PermisoId");

            migrationBuilder.CreateIndex(
                name: "IX_RolSecciones_SeccionId",
                table: "RolSecciones",
                column: "SeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Secciones_IdModulo",
                table: "Secciones",
                column: "IdModulo");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioEspecialidades_SpecialtyId",
                table: "UsuarioEspecialidades",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RoleId",
                table: "Usuarios",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Solo se eliminan las tablas creadas por esta migración.

            migrationBuilder.DropTable(
                name: "RolPermisos");

            migrationBuilder.DropTable(
                name: "RolSecciones");

            migrationBuilder.DropTable(
                name: "UsuarioEspecialidades");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Secciones");

            migrationBuilder.DropTable(
                name: "Modulos");

            // NO eliminar Roles.
            // NO eliminar Specialties.
        }
    }
}
