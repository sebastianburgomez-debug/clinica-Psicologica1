using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Enums
{
    public enum DiagnosisStatus
    {
        [Display(Name = "Presuntivo")]
        Presumptive = 0,

        [Display(Name = "Confirmado")]
        Confirmed = 1,

        [Display(Name = "Resuelto")]
        Resolved = 2,

        [Display(Name = "Descartado")]
        Discarded = 3
    }
}
