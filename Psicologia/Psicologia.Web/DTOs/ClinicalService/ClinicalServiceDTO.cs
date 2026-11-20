namespace Psicologia.Web.DTOs.ClinicalService
{
    public class ClinicalServiceDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public decimal ReferenceRate { get; set; }
        public Guid? SpecialtyId { get; set; }
        public string? SpecialtyName { get; set; }
        public bool IsActive { get; set; }
    }
}
