// Data/Entities/Specialty.cs
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Data.Entities;

public class Specialty
{
    public int SpecialtyId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
}