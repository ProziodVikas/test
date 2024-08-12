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
            using (var writer = new StreamWriter(_csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Write the global fields
                csv.WriteField("Account Number");
                csv.WriteField("Invoice Number");
                csv.WriteField("Issue Date");
                csv.WriteField("Due Date");
                csv.WriteField("Next Billing Date");
                csv.WriteField("Total Amount Due");
                csv.WriteField("Payment Method");
                csv.WriteField("Opening Balance");
                csv.WriteField("Previous Payment");
                csv.WriteField("Customer Service Contact");
                csv.WriteField("Current Bill Amount");
                csv.WriteField("Discount Amount");
                csv.NextRecord();

                csv.WriteField(billMetadata.AccountNumber);
                csv.WriteField(billMetadata.InvoiceNumber);
                csv.WriteField(billMetadata.IssueDate);
                csv.WriteField(billMetadata.DueDate);
                csv.WriteField(billMetadata.NextBillingDate);
                csv.WriteField(billMetadata.TotalAmountDue);
                csv.WriteField(billMetadata.PaymentMethod);
                csv.WriteField(billMetadata.OpeningBalance);
                csv.WriteField(billMetadata.PreviousPayment);
                csv.WriteField(billMetadata.CustomerServiceContact);
                csv.WriteField(billMetadata.CurrentBillAmount);
                csv.WriteField(billMetadata.DiscountAmount);
                csv.NextRecord();
                csv.NextRecord();

                // Write ICP, Meter, and Type Data
                foreach (var icp in billMetadata.ICPS)
                {
                    // Write ICP header row
                    csv.WriteField("ICP");
                    csv.WriteField("Service Description");
                    csv.WriteField("Billing Address");
                    csv.WriteField("Billing Period");
                    csv.WriteField("Read Start Date");
                    csv.WriteField("Read End Date");
                    csv.NextRecord();

                    // Write ICP data
                    csv.WriteField(icp.ICPCode);
                    csv.WriteField(icp.ServiceDescription);
                    csv.WriteField(icp.BillingAddress);
                    csv.WriteField(icp.BillingPeriod);
                    csv.WriteField(icp.ReadStartDate);
                    csv.WriteField(icp.ReadEndDate);
                    csv.NextRecord();
                    csv.NextRecord();

                    // Write Meter and Type Data
                    foreach (var meter in icp.Meters)
                    {
                        // Write Meter header row
                        csv.WriteField("Meter Number");
                        csv.WriteField("Fixed Charge Quantity (Days)");
                        csv.WriteField("Fixed Charge Rate");
                        csv.WriteField("Fixed Charge Total");
                        csv.WriteField("GST");
                        csv.NextRecord();

                        // Write Meter data
                        csv.WriteField(meter.MeterNumber);
                        csv.WriteField(meter.FixedChargeQuantity);
                        csv.WriteField(meter.FixedChargeRate);
                        csv.WriteField(meter.FixedChargeTotal);
                        csv.WriteField(meter.GST);
                        csv.NextRecord();
                        csv.NextRecord();

                        // Write Type Data
                        foreach (var type in meter.Types)
                        {
                            // Write Type header row
                            csv.WriteField("Type");
                            csv.WriteField("Multiplier");
                            csv.WriteField("Previous Reading");
                            csv.WriteField("Current Reading");
                            csv.WriteField("Rate");
                            csv.WriteField("Quantity");
                            csv.WriteField("Total");
                            csv.NextRecord();

                            // Write Type data
                            csv.WriteField(type.TypeName);
                            csv.WriteField(type.Multiplier);
                            csv.WriteField(type.PreviousReading);
                            csv.WriteField(type.CurrentReading);
                            csv.WriteField(type.Rate);
                            csv.WriteField(type.Quantity);
                            csv.WriteField(type.Total);
                            csv.NextRecord();
                            csv.NextRecord();
                        }
                    }
                    csv.NextRecord(); // Add an empty line between ICPs
                }
            }
        }
    }
}





