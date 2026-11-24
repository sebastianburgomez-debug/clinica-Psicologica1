namespace Psicologia.Web.Core
{
    // Acciones registradas en la tabla AuditLogs (máximo 32 caracteres)
    public static class AuditActions
    {
        public const string LOGIN = "Inicio de sesión";
        public const string LOGIN_FAILED = "Inicio de sesión fallido";
        public const string LOGOUT = "Cierre de sesión";
        public const string CREATE = "Creación";
        public const string UPDATE = "Modificación";
        public const string DELETE = "Eliminación";
        public const string ACTIVATE = "Activación";
        public const string DEACTIVATE = "Desactivación";
        public const string APPROVE = "Aprobación";
        public const string REJECT = "Rechazo";
    }
}
