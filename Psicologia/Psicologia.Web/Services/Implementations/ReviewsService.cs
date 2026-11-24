using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.Review;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    // Reseñas: el paciente califica una sesión atendida; el personal consulta las calificaciones
    public class ReviewsService : CustomQueryableOperationsService, IReviewsService
    {
        private readonly DataContext _context;
        private readonly IAccountService _accountService;
        private readonly IAuditService _auditService;

        public ReviewsService(DataContext context,
                              IMapper mapper,
                              IAccountService accountService,
                              IAuditService auditService) : base(context, mapper)
        {
            _context = context;
            _accountService = accountService;
            _auditService = auditService;
        }

        public async Task<Response<PaginationResponse<ReviewDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<Review> query = _context.Reviews.AsNoTracking()
                                                       .Include(r => r.Patient)
                                                       .Include(r => r.Appointment)
                                                           .ThenInclude(a => a!.ClinicalService)
                                                       .Include(r => r.DoctorProfile)
                                                           .ThenInclude(d => d!.User);

            // Un doctor solo ve las reseñas de sus propias sesiones
            DoctorProfile? ownProfile = await _accountService.GetAccountDoctorProfileAsync();
            if (ownProfile is not null)
            {
                query = query.Where(r => r.DoctorProfileId == ownProfile.Id);
            }

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();
                query = query.Where(r => r.Patient!.Name.ToLower().Contains(filter)
                                         || r.DoctorProfile!.User!.Name.ToLower().Contains(filter)
                                         || (r.Comment != null && r.Comment.ToLower().Contains(filter)));
            }

            query = query.OrderByDescending(r => r.CreatedAt);

            return await GetPagedListAsync<ReviewDTO, Review>(request, query);
        }

        public async Task<Response<CreateReviewDTO>> GetForCreateAsync(Guid appointmentId)
        {
            try
            {
                User? patient = await GetPatientAsync();
                if (patient is null)
                {
                    return Response<CreateReviewDTO>.Failure("Solo los pacientes pueden calificar sesiones");
                }

                Appointment? appointment = await _context.Appointments.AsNoTracking()
                                                                      .Include(a => a.ClinicalService)
                                                                      .Include(a => a.DoctorProfile)
                                                                          .ThenInclude(d => d!.User)
                                                                      .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patient.Id);

                string? error = await ValidateAppointmentAsync(appointment);
                if (error is not null || appointment is null)
                {
                    return Response<CreateReviewDTO>.Failure(error ?? "No existe la cita indicada");
                }

                CreateReviewDTO dto = new CreateReviewDTO
                {
                    AppointmentId = appointment.Id,
                    DoctorName = appointment.DoctorProfile?.User?.Name,
                    ServiceName = appointment.ClinicalService?.Name,
                    SessionDate = appointment.StartsAt
                };

                return Response<CreateReviewDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<CreateReviewDTO>.Failure(ex);
            }
        }

        public async Task<Response> CreateAsync(CreateReviewDTO dto)
        {
            try
            {
                User? patient = await GetPatientAsync();
                if (patient is null)
                {
                    return Response.Failure("Solo los pacientes pueden calificar sesiones");
                }

                if (dto.Rating is null || dto.Rating < 1 || dto.Rating > 5)
                {
                    return Response.Failure("La calificación debe estar entre 1 y 5");
                }

                // Filtrar por PatientId impide calificar la cita de otro paciente
                Appointment? appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && a.PatientId == patient.Id);

                string? error = await ValidateAppointmentAsync(appointment);
                if (error is not null || appointment is null)
                {
                    return Response.Failure(error ?? "No existe la cita indicada");
                }

                Review review = new Review
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointment.Id,
                    DoctorProfileId = appointment.DoctorProfileId,
                    PatientId = patient.Id,
                    Rating = dto.Rating.Value,
                    Comment = dto.Comment?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Reviews.AddAsync(review);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.CREATE, "Reseña", review.Id.ToString(),
                                             $"El paciente {patient.Name} calificó con {review.Rating}/5 la sesión del {appointment.StartsAt:dd/MM/yyyy HH:mm}");

                return Response.Success("¡Gracias por su calificación!");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        private async Task<User?> GetPatientAsync()
        {
            User? user = await _accountService.GetAccountUserAsync();
            return user?.Role?.Name == Env.PATIENT_ROLE_NAME ? user : null;
        }

        // Una cita solo se puede calificar si fue atendida y aún no tiene reseña
        private async Task<string?> ValidateAppointmentAsync(Appointment? appointment)
        {
            if (appointment is null)
            {
                return "No existe la cita indicada";
            }

            if (appointment.Status != AppointmentStatus.Attended)
            {
                return "Solo se pueden calificar las sesiones atendidas";
            }

            bool alreadyReviewed = await _context.Reviews.AnyAsync(r => r.AppointmentId == appointment.Id);
            if (alreadyReviewed)
            {
                return "Esta sesión ya fue calificada";
            }

            return null;
        }
    }
}
