using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.ClinicalRecord;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IClinicalRecordsService
    {
        public Task<Response<PaginationResponse<ClinicalRecordDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<ClinicalRecordDTO>> GetOneAsync(Guid id);
        public Task<Response<ClinicalRecordDTO>> CreateAsync(CreateClinicalRecordDTO dto);
        public Task<Response> UpdateAsync(UpdateClinicalRecordDTO dto);

        public Task<Response<SaveClinicalNoteDTO>> GetNoteAsync(Guid noteId);
        public Task<Response> SaveNoteAsync(SaveClinicalNoteDTO dto);

        public Task<Response> AddDiagnosisAsync(CreateDiagnosisDTO dto);
        public Task<Response> UpdateDiagnosisStatusAsync(UpdateDiagnosisStatusDTO dto);
    }
}
