// Data/Entities/Role.cs
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Data.Entities;

public class Role
{
    public int RoleId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
}