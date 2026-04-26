
using System;
using System.Reflection.Emit;
using System.Text;
using UVMBinding;

namespace YunyunLocalePatcher
{
    public static class Csv
    {
        public static List<string> ParseLine(string csv, ref int index)
        {
            if (index >= csv.Length) return null;

            var fields = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;

            for(; index < csv.Length; index++)
            {
                char c = csv[index];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (index + 1 < csv.Length && csv[index + 1] == '"')
                        {
                            field.Append('"');
                            index++;
                        }
                        else inQuotes = false;
                    }
                    else field.Append(c);

                    continue;
                }

                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else if (c == '\r')
                {
                    // handle \r\n
                    index++;
                    if (index < csv.Length && csv[index] == '\n') index++;
                    fields.Add(field.ToString());
                    return fields;
                }
                else if (c == '\n')
                {
                    index++;
                    fields.Add(field.ToString());
                    return fields;
                }
                else
                {
                    field.Append(c);
                }
            }

            fields.Add(field.ToString()); // Add the last field in case of EoF

            return fields;
        }

        public static string SerializeLine(IEnumerable<string> fields)
        {
            var row = new StringBuilder();
            bool first = true;
            foreach (string raw in fields)
            {
                if (!first) row.Append(',');
                else first = false;

                string value = raw ?? string.Empty;
                bool needsQuotes = value.Contains(',')
                                || value.Contains('"')
                                || value.Contains('\n')
                                || value.Contains('\r');

                if (needsQuotes)
                {
                    row.Append('"');
                    row.Append(value.Replace("\"", "\"\""));
                    row.Append('"');
                }
                else
                {
                    row.Append(value);
                }
            }

            return row.ToString();
        }
    }
}
