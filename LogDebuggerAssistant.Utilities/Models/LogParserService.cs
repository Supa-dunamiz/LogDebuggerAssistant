using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogDebuggerAssistant.Utilities.Models
{
    //public class LogParserService
    //{
    //    public List<LogEntry> ParseLog(string content)
    //    {
    //        var entries = new List<LogEntry>();
    //        var lines = content.Split('\n');
    //        foreach (var line in lines)
    //        {
    //            if (line.Contains("Exception") || line.Contains("ERROR"))
    //            {
    //                entries.Add(new LogEntry
    //                {
    //                    Timestamp = DateTime.Now.ToString(),
    //                    Level = "ERROR",
    //                    Message = line,
    //                    StackTrace = ExtractStackTrace(lines, line)
    //                });
    //            }
    //        }
    //        return entries;
    //    }

    //    private string ExtractStackTrace(string[] lines, string startLine)
    //    {
    //        int startIndex = Array.IndexOf(lines, startLine);
    //        var stack = new List<string>();
    //        for (int i = startIndex + 1; i < lines.Length && lines[i].StartsWith("   at"); i++)
    //        {
    //            stack.Add(lines[i]);
    //        }
    //        return string.Join("\n", stack);
    //    }
    //}
}
