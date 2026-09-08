using System.Diagnostics;
using Portwise.Application.Contracts;
using Portwise.Application.Diagnostics;
using Serilog.Context;

namespace Portwise.Diagnostics;

/// <summary>
/// An <see cref="IDiagnosticContext"/> implementation based on <see cref="Activity"/>
/// and W3C Trace Context. Each scope starts an Activity, attaches fields as tags,
/// and continues writing to <see cref="LogContext"/> so existing structured fields
/// remain available. TraceId and SpanId are added by <c>Serilog.Enrichers.Span</c>.
/// </summary>
public sealed class ActivityDiagnosticContext : IDiagnosticContext
{
    private static readonly HashSet<string> AllowedOperations =
    [
        "http_request",
        "http_error",
        "daily_stock_data_sync",
        "stock_data_sync",
        "ftshare_mcp"
    ];

    private static readonly HashSet<string> AllowedDataKinds =
    [
        "profile",
        "market",
        "dividend",
        "financial"
    ];

    public IDisposable BeginScope(DiagnosticScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var operationName = Sanitize(scope.Operation, AllowedOperations) ?? "unknown_operation";
        var activity = PortwiseActivitySource.Instance.StartActivity(operationName);

        var properties = new List<IDisposable>();
        Push(properties, activity, "diagnostic_operation", scope.Operation, AllowedOperations);
        Push(properties, activity, "correlation_id", scope.CorrelationId);
        Push(properties, activity, "run_id", scope.RunId);
        Push(properties, activity, "security_code", scope.SecurityCode);
        Push(properties, activity, "exchange_code", scope.ExchangeCode);
        Push(properties, activity, "data_kind", scope.DataKind, AllowedDataKinds);
        Push(properties, activity, "error_code", scope.ErrorCode);
        Push(properties, activity, "severity", scope.Severity);

        return new CompositeDisposable(activity, properties);
    }

    private static void Push(
        ICollection<IDisposable> properties,
        Activity? activity,
        string propertyName,
        string? value,
        IReadOnlySet<string>? allowedValues = null)
    {
        var safeValue = Sanitize(value, allowedValues);
        if (safeValue is null)
        {
            return;
        }

        activity?.SetTag(propertyName, safeValue);
        properties.Add(LogContext.PushProperty(propertyName, safeValue));
    }

    private static string? Sanitize(string? value, IReadOnlySet<string>? allowedValues = null)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > 128
            || value.Any(character =>
                !char.IsLetterOrDigit(character)
                && character is not '-'
                and not '_'
                and not '.'
                and not ':'))
        {
            return null;
        }

        return allowedValues is null || allowedValues.Contains(value) ? value : null;
    }

    private sealed class CompositeDisposable(
        Activity? activity,
        IReadOnlyList<IDisposable> properties) : IDisposable
    {
        public void Dispose()
        {
            for (var index = properties.Count - 1; index >= 0; index--)
            {
                properties[index].Dispose();
            }

            activity?.Dispose();
        }
    }
}
