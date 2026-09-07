using System.Diagnostics;
using DividendHarvest.Application.Contracts;
using DividendHarvest.Application.Diagnostics;
using Serilog.Context;

namespace DividendHarvest.Diagnostics;

/// <summary>
/// 基于 <see cref="Activity"/>（W3C Trace Context）的 <see cref="IDiagnosticContext"/> 实现，
/// 取代早期只依赖 Serilog <see cref="LogContext"/> 的定制关联方案。每个作用域会启动一个 Activity，
/// 允许字段作为 Activity Tag 附加（可被未来接入的 OpenTelemetry 导出器采集），同时仍然写入
/// LogContext，保证既有结构化日志字段（correlation_id、run_id、security_code 等）不受影响；
/// TraceId/SpanId 由 <c>Serilog.Enrichers.Span</c> 的 <c>Enrich.WithSpan()</c> 自动注入日志。
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
        var activity = DividendHarvestActivitySource.Instance.StartActivity(operationName);

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
