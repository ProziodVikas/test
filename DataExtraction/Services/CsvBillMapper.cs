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
                csv.WriteField("billingCurrency");
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
                
                // Write ICP, Meter, and Type Data
                foreach (var metersData in billMetadata.ICPS)
                {
                    // Write ICP header row
                    csv.WriteField("UtilityType");
                    csv.WriteField("ICP");
                    csv.WriteField("Service Description");
                    csv.WriteField("Billing Address");
                    csv.WriteField("Billing Period");
                    csv.WriteField("Read Start Date");
                    csv.WriteField("Read End Date");



                    foreach (var meter in icp.Meters)
                    {
                        // Write Meter header row
                        csv.WriteField("Meter Number");
                        csv.WriteField("Fixed Charge Quantity (Days)");
                        csv.WriteField("Fixed Charge Rate");
                        csv.WriteField("Fixed Charge Total");
                        csv.WriteField("GST");


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


                csv.WriteField(billMetadata.supplierName);
                csv.WriteField(billMetadata.accountNumber);
                csv.WriteField(billMetadata.invoiceNumber);
                csv.WriteField(billMetadata.invoiceDate);
                csv.WriteField(billMetadata.dueDate);
                csv.WriteField(billMetadata.nextBillingDate);
                csv.WriteField(billMetadata.totalAmountDue);
                csv.WriteField(billMetadata.paymentMethods);
                csv.WriteField(billMetadata.previousBalance);
                csv.WriteField(billMetadata.previousPayment);
                csv.WriteField(billMetadata.customerServiceContact);
                csv.WriteField(billMetadata.currentBillAmount);

               

                    // Write ICP data
                    csv.WriteField(icp.utilityType);
                    csv.WriteField(icp.ICPCode);
                    csv.WriteField(icp.serviceDescription);
                    csv.WriteField(icp.billingAddress);
                    csv.WriteField(icp.billingPeriod);
                    csv.WriteField(icp.meterReadStartDate);
                    csv.WriteField(icp.meterReadEndDate);

                    // Write Meter and Type Data
                   

                        // Write Meter data
                        csv.WriteField(meter.meterNumber);
                        csv.WriteField(meter.fixedChargeQuantity);
                        csv.WriteField(meter.fixedChargeRate);
                        csv.WriteField(meter.fixedChargeTotal);
                        csv.WriteField(meter.gst);

                        // Write Type Data
                       

                            // Write Type data
                            csv.WriteField(type.type);
                            csv.WriteField(type.meterMultiplier);
                            csv.WriteField(type.previousReading);
                            csv.WriteField(type.currentReading);
                            csv.WriteField(type.rate);
                            csv.WriteField(type.quantity);
                            csv.WriteField(type.total);
                        }
                    }
                    csv.NextRecord(); // Add an empty line between ICPs
                }
            }
        }
    }
}





