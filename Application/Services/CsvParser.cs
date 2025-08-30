using PersonalFinanceTracker.Models;
using System.Globalization;

namespace PersonalFinanceTracker.Services
{
    public class CsvParser
    {
        public List<Transaction> ParseCsv(string csvContent)
        {
            var transactions = new List<Transaction>();
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Console.WriteLine($"Processing {lines.Length} lines");

            foreach (var line in lines)
            {
                try
                {
                    var fields = ParseCsvLine(line);

                    // Skip if we don't have enough fields or if the first field isn't a date
                    if (fields.Length < 5 || string.IsNullOrWhiteSpace(fields[0]))
                        continue;

                    // Try to parse the date - if it fails, skip this row
                    if (!TryParseDate(fields[0].Trim(), out DateTime date))
                        continue;

                    var description = fields[1].Trim().Trim('"');
                    var creditStr = fields[2].Trim();
                    var debitStr = fields[3].Trim();
                    var balanceStr = fields[4].Trim();

                    decimal credit = 0;
                    decimal debit = 0;
                    decimal balance = 0;

                    if (!string.IsNullOrEmpty(creditStr))
                        decimal.TryParse(creditStr, out credit);

                    if (!string.IsNullOrEmpty(debitStr))
                        decimal.TryParse(debitStr.Replace("-", ""), out debit);

                    decimal.TryParse(balanceStr, out balance);

                    decimal amount = credit > 0 ? credit : -debit;

                    transactions.Add(new Transaction
                    {
                        Id = transactions.Count + 1,
                        Date = date,
                        Description = description,
                        Credit = amount,
                        Balance = balance,
                        Category = "Uncategorized"
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Skipping row {line}: {e.Message}");
                }
            }

            Console.WriteLine($"Successfully parsed {transactions.Count} transactions");
            return transactions;
        }

        private string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var currentField = new System.Text.StringBuilder();
            bool inQuotes = false;

            foreach (char character in line)
            {
                if (character == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (character == ',' && !inQuotes)
                {
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                    continue;
                }
                currentField.Append(character);
            }

            fields.Add(currentField.ToString());
            return fields.ToArray();
        }

        private bool TryParseDate(string dateString, out DateTime date)
        {
            string[] formats = {
                "dd/MM/yyyy",
                "d/MM/yyyy",
                "dd/M/yyyy", 
                "d/M/yyyy"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return true;
                }
            }
            
            date = DateTime.MinValue;
            return false;
        }
    }
}
