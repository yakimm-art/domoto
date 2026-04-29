using System.Collections.Generic;
using System.IO;
using System.Text;
using Domoto.Models;

namespace Domoto.Services
{
    public static class ExportService
    {
        public static void ExportToCsv(List<TaskItem> tasks, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Title,Description,DueDate,Priority,Category,Status,CreatedDate");
            foreach (var t in tasks)
            {
                sb.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",
                    Escape(t.Title),
                    Escape(t.Description),
                    t.DueDate.ToString("yyyy-MM-dd HH:mm"),
                    t.Priority,
                    t.Category,
                    t.IsCompleted ? "Completed" : "Incomplete",
                    t.CreatedDate.ToString("yyyy-MM-dd HH:mm")));
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string Escape(string value)
        {
            if (value == null) return "";
            return value.Replace("\"", "\"\"");
        }
    }
}
