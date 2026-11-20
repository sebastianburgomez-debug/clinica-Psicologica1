using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.Helpers.Abstractions;

namespace Psicologia.Web.Helpers.Implementations
{
    public class SelectListHelper : ISelectListHelper
    {
        private readonly DataContext _context;

        public SelectListHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<List<SelectListItem>> GetRolesSelectListAsync()
        {
            return await _context.PsicologiaRoles.OrderBy(r => r.Name)
                                                 .Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Id.ToString()
            }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetSpecialtiesSelectListAsync()
        {
            return await _context.Specialties.Where(s => s.IsActive && s.Status == ApprovalStatus.Approved)
                                             .OrderBy(s => s.Name)
                                             .Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetDoctorsSelectListAsync(bool onlyApproved = true)
        {
            IQueryable<DoctorProfile> query = _context.DoctorProfiles.AsQueryable();

            if (onlyApproved)
            {
                query = query.Where(d => d.Status == ApprovalStatus.Approved && d.User!.IsActive);
            }

            return await query.OrderBy(d => d.User!.Name)
                              .Select(d => new SelectListItem
            {
                Text = d.User!.Name + " - " + d.Specialty!.Name,
                Value = d.Id.ToString()
            }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetPatientsSelectListAsync(bool withoutClinicalRecord = false)
        {
            IQueryable<User> query = _context.Users.Where(u => u.IsActive && u.Role!.Name == Env.PATIENT_ROLE_NAME);

            if (withoutClinicalRecord)
            {
                query = query.Where(u => !_context.ClinicalRecords.Any(r => r.PatientId == u.Id));
            }

            return await query.OrderBy(u => u.Name)
                              .Select(u => new SelectListItem
            {
                Text = u.Name + " (" + u.Email + ")",
                Value = u.Id
            }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetClinicalServicesSelectListAsync()
        {
            return await _context.ClinicalServices.Where(cs => cs.IsActive)
                                                  .OrderBy(cs => cs.Name)
                                                  .Select(cs => new SelectListItem
            {
                Text = cs.Name + " (" + cs.DurationMinutes.ToString() + " min)",
                Value = cs.Id.ToString()
            }).ToListAsync();
        }
    }
}
