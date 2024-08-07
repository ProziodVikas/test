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
                //csv.WriteField(nameof(billMetadata.BillIdentifier));
                csv.WriteField(nameof(billMetadata.AccountNumber));
                csv.WriteField(nameof(billMetadata.InvoiceNumber));
                csv.WriteField(nameof(billMetadata.BillingPeriod));
                csv.WriteField(nameof(billMetadata.IssueDate));
                csv.WriteField(nameof(billMetadata.DueDate));
                csv.WriteField(nameof(billMetadata.ServiceDescription));
                csv.WriteField(nameof(billMetadata.TotalAmountDue));
                csv.WriteField(nameof(billMetadata.PaymentMethod));
                csv.WriteField(nameof(billMetadata.OpeningBalance));
                csv.WriteField(nameof(billMetadata.PreviousPayment));
                csv.WriteField(nameof(billMetadata.CustomerServiceContact));
                csv.WriteField(nameof(billMetadata.BillingAddress));
                csv.WriteField(nameof(billMetadata.CurrentBillAmount));


                csv.WriteField(nameof(billMetadata.ICP));
                csv.WriteField(nameof(billMetadata.ReadStartDate));
                csv.WriteField(nameof(billMetadata.ReadEndDate));
                csv.WriteField(nameof(billMetadata.FixedChargeQuantity));
                csv.WriteField(nameof(billMetadata.FixedChargeRate));
                csv.WriteField(nameof(billMetadata.FixedChargeTotal));
                csv.WriteField(nameof(billMetadata.GST));
                //csv.WriteField(nameof(billMetadata.AccountDate));              
                //csv.WriteField(nameof(billMetadata.Address));
                //csv.WriteField(nameof(billMetadata.City));
                //csv.WriteField(nameof(billMetadata.Postcode));
                //csv.WriteField(nameof(billMetadata.PeriodFrom));
                //csv.WriteField(nameof(billMetadata.PeriodTo));        
                csv.NextRecord();
                csv.WriteField(billMetadata.AccountNumber);
                csv.WriteField(billMetadata.InvoiceNumber);
                csv.WriteField(billMetadata.BillingPeriod);
                csv.WriteField(billMetadata.IssueDate);
                csv.WriteField(billMetadata.DueDate);
                csv.WriteField(billMetadata.ServiceDescription);
                csv.WriteField(billMetadata.TotalAmountDue);
                csv.WriteField(billMetadata.PaymentMethod);
                csv.WriteField(billMetadata.OpeningBalance);
                csv.WriteField(billMetadata.PreviousPayment);
                csv.WriteField(billMetadata.CustomerServiceContact);
                csv.WriteField(billMetadata.BillingAddress);
                csv.WriteField(billMetadata.CurrentBillAmount);




                csv.WriteField(billMetadata.ICP);
                csv.WriteField(billMetadata.ReadStartDate);
                csv.WriteField(billMetadata.ReadEndDate);
                csv.WriteField(billMetadata.FixedChargeQuantity);
                csv.WriteField(billMetadata.FixedChargeRate);
                csv.WriteField(billMetadata.FixedChargeTotal);
                csv.WriteField(billMetadata.GST);


                csv.NextRecord();
                csv.NextRecord();

                csv.WriteField(nameof(billMetadata.MeterNumber));
                csv.WriteField(nameof(billMetadata.Multiplier));
                csv.WriteField(nameof(billMetadata.Type));
                csv.WriteField(nameof(billMetadata.PreviousReading));
                csv.WriteField(nameof(billMetadata.CurrentReading));
                csv.WriteField(nameof(billMetadata.Rate));
                csv.WriteField(nameof(billMetadata.Quantity));
                csv.WriteField(nameof(billMetadata.Total));
                csv.NextRecord();

                csv.WriteField(billMetadata.MeterNumber);
                csv.WriteField(billMetadata.Multiplier);
                csv.WriteField(billMetadata.Type);
                csv.WriteField(billMetadata.PreviousReading);
                csv.WriteField(billMetadata.CurrentReading);
                csv.WriteField(billMetadata.Rate);
                csv.WriteField(billMetadata.Quantity);
                csv.WriteField(billMetadata.Total);
                csv.NextRecord();










                // Write the values for the metadata fields
                // csv.WriteField(billMetadata.BillIdentifier);
               

                //csv.WriteField(billMetadata.AccountDate);
                //csv.WriteField(billMetadata.Address);
                //csv.WriteField(billMetadata.City);
                //csv.WriteField(billMetadata.Postcode);
                //csv.WriteField(billMetadata.PeriodFrom?.ToString("dd-MM-yyyy"));
                //csv.WriteField(billMetadata.PeriodTo?.ToString("dd-MM-yyyy"));
                csv.NextRecord();




                

                // Add a blank line for separation
                csv.NextRecord();

                // Write charges header

                // Write the charges
                //foreach (var charge in billMetadata.Charges)
                //{
                //    csv.WriteField(billMetadata.ICP);
                //    csv.WriteField(billMetadata.ReadStartDate);
                //    csv.WriteField(billMetadata.ReadEndDate);
                //    csv.WriteField(billMetadata.FixedChargeQuantity);
                //    csv.WriteField(billMetadata.FixedChargeRate);
                //    csv.WriteField(billMetadata.FixedChargeTotal);
                //    csv.WriteField(billMetadata.GST);
                //    csv.NextRecord();
                //}

                // Add a blank line for separation
                //csv.NextRecord();

                // Write total header
                
                   
                

                // Write the total
                
            }
        }
    }
}
