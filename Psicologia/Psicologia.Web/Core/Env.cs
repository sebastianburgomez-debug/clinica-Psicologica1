namespace Psicologia.Web.Core
{
    public static class Env
    {
        public const string SUPER_ADMIN_ROLE_NAME = "Administrador";
        public const string DOCTOR_ROLE_NAME = "Doctor";
        public const string SECRETARY_ROLE_NAME = "Secretaria";
        public const string PATIENT_ROLE_NAME = "Paciente";
        public const string BOSS_ROLE_NAME = "Jefe";

        // Permiso que permite aprobar doctores y especialidades.
        // Quien lo tiene crea registros ya aprobados; quien no, los deja pendientes.
        public const string APPROVALS_MANAGE_PERMISSION = "approvals.manage";
    }
}
