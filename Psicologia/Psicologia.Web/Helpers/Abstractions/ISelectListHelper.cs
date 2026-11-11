using Microsoft.AspNetCore.Mvc.Rendering;

namespace Psicologia.Web.Helpers.Abstractions
{
    public interface ISelectListHelper
    {
        Task<List<SelectListItem>> GetRolesSelectListAsync();
    }
}
