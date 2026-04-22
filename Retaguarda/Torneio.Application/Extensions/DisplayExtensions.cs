using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Torneio.Application.Extensions;

public static class DisplayExtensions
{
    public static string ObterDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.GetName() ?? value.ToString();
    }
}
