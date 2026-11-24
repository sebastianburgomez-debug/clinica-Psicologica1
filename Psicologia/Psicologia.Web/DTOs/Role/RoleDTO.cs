namespace Psicologia.Web.DTOs.Role
{//cuando se cargan en la pantalla el id indica a que registro apuntan
    //no hay required porque vienen de la base de datos
    public class RoleDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<PermissionsForRoleDTO>? Permissions { get; set; }
    }
}
