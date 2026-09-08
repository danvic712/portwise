using Portwise.Application.Diagnostics;

namespace Portwise.Application.Contracts;

public interface IDiagnosticContext
{
    IDisposable BeginScope(DiagnosticScope scope);
}
