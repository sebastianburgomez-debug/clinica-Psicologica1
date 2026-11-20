using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Psicologia.Web.Core.Extensions
{
    public static class EnumExtensions
    {
        // Devuelve el texto del atributo [Display(Name = "...")] del valor del enum
        public static string GetDisplayName(this Enum value)
        {
            MemberInfo? member = value.GetType().GetMember(value.ToString()).FirstOrDefault();

            DisplayAttribute? attribute = member?.GetCustomAttribute<DisplayAttribute>();

            return attribute?.GetName() ?? value.ToString();
        }
    }
}
