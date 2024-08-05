using CsvHelper;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace DataExtraction.Library.Services
{
    public class CsvBillMapper
    {
        private readonly string _csvFilePath;

        public CsvBillMapper(string csvFilePath)
        {
            _csvFilePath = csvFilePath;
        }

        public async Task WriteToCsvAsync(BillMetadata billMetadata)
        {
            // Create or overwrite the CSV file
            using (var writer = new StreamWriter(_csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Write the header row for the metadata fields
                csv.WriteField(nameof(billMetadata.AccountNumber));
                csv.WriteField(nameof(billMetadata.InvoiceNumber));
                csv.WriteField(nameof(billMetadata.Country));
                csv.WriteField(nameof(billMetadata.Commodity));
                csv.WriteField(nameof(billMetadata.RetailerShortName));
                csv.WriteField(nameof(billMetadata.Address));
                csv.WriteField(nameof(billMetadata.City));
                csv.WriteField(nameof(billMetadata.Postcode));
                csv.WriteField(nameof(billMetadata.PeriodFrom));
                csv.WriteField(nameof(billMetadata.PeriodTo));
                csv.WriteField(nameof(billMetadata.IssueDate));
                csv.WriteField(nameof(billMetadata.DueDate));
                csv.NextRecord();

                // Write the values for the metadata fields
                csv.WriteField(billMetadata.AccountNumber);
                csv.WriteField(billMetadata.InvoiceNumber);
                csv.WriteField(billMetadata.Country);
                csv.WriteField(billMetadata.Commodity);
                csv.WriteField(billMetadata.RetailerShortName);
                csv.WriteField(billMetadata.Address);
                csv.WriteField(billMetadata.City);
                csv.WriteField(billMetadata.Postcode);
                csv.WriteField(billMetadata.PeriodFrom?.ToString("dd-MM-yyyy"));
                csv.WriteField(billMetadata.PeriodTo?.ToString("dd-MM-yyyy"));
                csv.WriteField(billMetadata.IssueDate?.ToString("MM-dd-yyyy HH:mm"));
                csv.WriteField(billMetadata.DueDate?.ToString("MM-dd-yyyy HH:mm"));
                csv.NextRecord();

                // Add a blank line for separation
                csv.NextRecord();

                // Write charges header
                csv.WriteField("ChargeName");
                csv.WriteField("Quantity");
                csv.WriteField("Price");
                csv.WriteField("Cost");
                csv.NextRecord();

                // Write the charges
                foreach (var charge in billMetadata.Charges)
                {
                    csv.WriteField(charge.ChargeName);
                    csv.WriteField(charge.Quantity);
                    csv.WriteField(charge.Price);
                    csv.WriteField(charge.Cost);
                    csv.NextRecord();
                }

                // Add a blank line for separation
                csv.NextRecord();

                // Write total header
                csv.WriteField("");
                csv.WriteField("Total Quantity");
                csv.WriteField("Total Price");
                csv.WriteField("Total Cost");
                csv.NextRecord();

                // Write the total
                csv.WriteField(billMetadata.Total);
                csv.WriteField(billMetadata.Total.Quantity);
                csv.WriteField(billMetadata.Total.Price);
                csv.WriteField(billMetadata.Total.Cost);
                csv.NextRecord();
            }
        }
    }
}
