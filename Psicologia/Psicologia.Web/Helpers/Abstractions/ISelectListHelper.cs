using Microsoft.AspNetCore.Mvc.Rendering;

namespace Psicologia.Web.Helpers.Abstractions
{
    public interface ISelectListHelper
    {
        Task<List<SelectListItem>> GetRolesSelectListAsync();

        // Especialidades aprobadas y activas
        Task<List<SelectListItem>> GetSpecialtiesSelectListAsync();

        // Doctores (Value = DoctorProfileId). onlyApproved: solo aprobados con cuenta activa
        Task<List<SelectListItem>> GetDoctorsSelectListAsync(bool onlyApproved = true);

        // Pacientes activos (Value = UserId). withoutClinicalRecord: solo los que aún no tienen historia clínica
        Task<List<SelectListItem>> GetPatientsSelectListAsync(bool withoutClinicalRecord = false);

        // Servicios activos
        Task<List<SelectListItem>> GetClinicalServicesSelectListAsync();
    }
}
