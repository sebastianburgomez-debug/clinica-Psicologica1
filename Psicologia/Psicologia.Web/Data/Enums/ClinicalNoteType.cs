using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Enums
{
    public enum ClinicalNoteType
    {
        [Display(Name = "Nota de sesión")]
        Session = 0,

        [Display(Name = "Evolución")]
        Evolution = 1
    }
}
