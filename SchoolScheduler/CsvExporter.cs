using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SchoolScheduler
{
    public static class CsvExporter
    {
        public static void ExportDataGridViewToCsv(DataGridView dgv, string filePath)
        {
            var sb = new StringBuilder();

            // Заголовки столбцов
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                sb.Append(EscapeCsv(dgv.Columns[i].HeaderText));
                if (i < dgv.Columns.Count - 1)
                    sb.Append(";");
            }
            sb.AppendLine();

            // Данные
            for (int r = 0; r < dgv.Rows.Count; r++)
            {
                for (int c = 0; c < dgv.Columns.Count; c++)
                {
                    var val = dgv.Rows[r].Cells[c].Value?.ToString() ?? "";
                    sb.Append(EscapeCsv(val));
                    if (c < dgv.Columns.Count - 1)
                        sb.Append(";");
                }
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string EscapeCsv(string s)
        {
            if (s.Contains(";") || s.Contains("\"") || s.Contains("\n") || s.Contains("\r"))
            {
                s = s.Replace("\"", "\"\"");
                return $"\"{s}\"";
            }
            return s;
        }
    }
}
