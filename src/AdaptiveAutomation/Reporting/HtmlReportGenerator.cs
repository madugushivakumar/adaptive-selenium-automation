using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdaptiveAutomation.Models;

namespace AdaptiveAutomation.Reporting;

public static class HtmlReportGenerator
{
    public static string Generate(string targetUrl, IEnumerable<TestResult> results, string? outputPath = null)
    {
        outputPath ??= Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "reports", "latest-report.html");
        string? dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var resultList = new List<TestResult>(results);
        int passed = resultList.Count(r => r.Status == TestStatus.Passed);
        int failed = resultList.Count(r => r.Status == TestStatus.Failed);
        int ambiguous = resultList.Count(r => r.Status == TestStatus.Ambiguous);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang='en'>");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset='UTF-8'>");
        sb.AppendLine("  <title>Adaptive Selenium Automation — Execution Report</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background: #f8fafc; color: #1e293b; margin: 0; padding: 24px; }");
        sb.AppendLine("    .container { max-width: 1100px; margin: 0 auto; background: #fff; border-radius: 12px; box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1); padding: 32px; }");
        sb.AppendLine("    h1 { margin-top: 0; font-size: 26px; color: #0f172a; }");
        sb.AppendLine("    .meta { font-size: 14px; color: #64748b; margin-bottom: 24px; }");
        sb.AppendLine("    .stats { display: flex; gap: 16px; margin-bottom: 28px; }");
        sb.AppendLine("    .card { flex: 1; padding: 16px; border-radius: 8px; background: #f1f5f9; text-align: center; }");
        sb.AppendLine("    .card .num { font-size: 28px; font-weight: bold; margin-bottom: 4px; }");
        sb.AppendLine("    .card.pass { background: #dcfce7; color: #15803d; }");
        sb.AppendLine("    .card.fail { background: #fee2e2; color: #b91c1c; }");
        sb.AppendLine("    .card.ambig { background: #fef3c7; color: #b45309; }");
        sb.AppendLine("    table { width: 100%; border-collapse: collapse; margin-top: 16px; font-size: 14px; }");
        sb.AppendLine("    th, td { padding: 12px 14px; text-align: left; border-bottom: 1px solid #e2e8f0; }");
        sb.AppendLine("    th { background: #f8fafc; font-weight: 600; color: #475569; }");
        sb.AppendLine("    .badge { display: inline-block; padding: 4px 10px; border-radius: 9999px; font-size: 12px; font-weight: 600; }");
        sb.AppendLine("    .badge.passed { background: #dcfce7; color: #15803d; }");
        sb.AppendLine("    .badge.failed { background: #fee2e2; color: #b91c1c; }");
        sb.AppendLine("    .badge.ambiguous { background: #fef3c7; color: #b45309; }");
        sb.AppendLine("    .score-high { color: #15803d; font-weight: 600; }");
        sb.AppendLine("    .score-med { color: #d97706; font-weight: 600; }");
        sb.AppendLine("    .score-low { color: #dc2626; font-weight: 600; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <div class='container'>");
        sb.AppendLine("    <h1>Adaptive Selenium Automation — Execution Report</h1>");
        sb.AppendLine($"    <div class='meta'><strong>Target URL:</strong> {targetUrl} &nbsp;|&nbsp; <strong>Generated:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</div>");
        sb.AppendLine("    <div class='stats'>");
        sb.AppendLine($"      <div class='card'><div class='num'>{resultList.Count}</div>Total Tests</div>");
        sb.AppendLine($"      <div class='card pass'><div class='num'>{passed}</div>Passed</div>");
        sb.AppendLine($"      <div class='card fail'><div class='num'>{failed}</div>Failed</div>");
        sb.AppendLine($"      <div class='card ambig'><div class='num'>{ambiguous}</div>Ambiguous</div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("    <table>");
        sb.AppendLine("      <thead>");
        sb.AppendLine("        <tr>");
        sb.AppendLine("          <th>Test Name</th>");
        sb.AppendLine("          <th>Field</th>");
        sb.AppendLine("          <th>Status</th>");
        sb.AppendLine("          <th>Confidence</th>");
        sb.AppendLine("          <th>Expected</th>");
        sb.AppendLine("          <th>Actual</th>");
        sb.AppendLine("          <th>Duration</th>");
        sb.AppendLine("        </tr>");
        sb.AppendLine("      </thead>");
        sb.AppendLine("      <tbody>");

        foreach (var r in resultList)
        {
            string statusClass = r.Status.ToString().ToLowerInvariant();
            string scoreClass = r.ConfidenceScore >= 80 ? "score-high" : (r.ConfidenceScore >= 60 ? "score-med" : "score-low");
            sb.AppendLine("        <tr>");
            sb.AppendLine($"          <td><strong>{r.TestName}</strong></td>");
            sb.AppendLine($"          <td>{r.FieldName}</td>");
            sb.AppendLine($"          <td><span class='badge {statusClass}'>{r.Status}</span></td>");
            sb.AppendLine($"          <td><span class='{scoreClass}'>{r.ConfidenceScore:F1}</span></td>");
            sb.AppendLine($"          <td><code>{r.ExpectedValue}</code></td>");
            sb.AppendLine($"          <td><code>{r.ActualValue}</code></td>");
            sb.AppendLine($"          <td>{r.Duration.TotalSeconds:F2}s</td>");
            sb.AppendLine("        </tr>");
        }

        sb.AppendLine("      </tbody>");
        sb.AppendLine("    </table>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        File.WriteAllText(outputPath, sb.ToString());
        return outputPath;
    }
}

