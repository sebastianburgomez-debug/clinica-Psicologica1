namespace Psicologia.Data.Entities;

public class UsuarioEspecialidad
{
    public int UsuarioId { get; set; }

    public int SpecialtyId { get; set; }

    // Relaciones
    public Usuario Usuario { get; set; } = null!;

    public Specialty Specialty { get; set; } = null!;
}