using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotAI.Application.Common.Errors;
public record Error(string Code, string Message)
{
    public static Error None => new("None", string.Empty);
    public static Error Unexpected => new("Unexpected", "An unexpected error occurred.");
    public static Error Validation(string message) => new("Validation", message);
}