using CsvHelper;
using System.Diagnostics.Metrics;
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
                //// Write the global fields
                //csv.WriteField("billingCurrency");
                //csv.WriteField("billingAddress");
                //csv.WriteField("totalAmountDue");
                //csv.WriteField("dueDate");
                //csv.WriteField("customerServiceContact");
                //csv.WriteField("currentBillAmount");
                //csv.WriteField("accountNumber");
                //csv.WriteField("invoiceNumber");
                //csv.WriteField("invoiceDate");
                //csv.WriteField("fixedChargeTotal");
                //csv.WriteField("ICP");
                //csv.WriteField("billingPeriod");
                //csv.WriteField("gst");
                //csv.WriteField("fixedChargeQuantity");
                //csv.WriteField("fixedChargeRate");
                //csv.WriteField("paymentMethods");
                //csv.WriteField("previousBalance");
                //csv.WriteField("previousPayment");
                //csv.WriteField("meterReadEndDate");
                //csv.WriteField("meterReadStartDate");
                //csv.NextRecord();



                //csv.WriteField(billMetadata.billingCurrency);
                //csv.WriteField(billMetadata.billingAddress);
                //csv.WriteField(billMetadata.totalAmountDue);
                //csv.WriteField(billMetadata.dueDate);
                //csv.WriteField(billMetadata.customerServiceContact);
                //csv.WriteField(billMetadata.currentBillAmount);
                //csv.WriteField(billMetadata.accountNumber);
                //csv.WriteField(billMetadata.invoiceNumber);
                //csv.WriteField(billMetadata.invoiceDate);
                //csv.WriteField(billMetadata.fixedChargeTotal);
                //csv.WriteField(billMetadata.ICP);
                //csv.WriteField(billMetadata.billingPeriod);
                //csv.WriteField(billMetadata.gst);
                //csv.WriteField(billMetadata.fixedChargeQuantity);
                //csv.WriteField(billMetadata.fixedChargeRate);
                //csv.WriteField(billMetadata.paymentMethods);
                //csv.WriteField(billMetadata.previousBalance);
                //csv.WriteField(billMetadata.previousPayment);
                //csv.WriteField(billMetadata.meterReadEndDate);
                //csv.WriteField(billMetadata.meterReadStartDate);
                //csv.NextRecord();

                //// Write ICP, Meter, and Type Data
                //foreach (var metersData in billMetadata.metersData)
                //{
                //    // Write ICP header row
                //    csv.WriteField("meterNumber");
                //    csv.WriteField("meterMultiplier");
                //    csv.WriteField("type");
                //    csv.WriteField("rate");
                //    csv.WriteField("quantity");
                //    csv.WriteField("total");
                //    csv.WriteField("previousReading");
                //    csv.WriteField("currentReading");
                //    csv.NextRecord();


                //    // Write ICP data
                //    csv.WriteField(metersData.meterNumber);
                //    csv.WriteField(metersData.meterMultiplier);
                //    csv.WriteField(metersData.type);
                //    csv.WriteField(metersData.rate);
                //    csv.WriteField(metersData.quantity);
                //    csv.WriteField(metersData.total);
                //    csv.WriteField(metersData.previousReading);
                //    csv.WriteField(metersData.currentReading);
                //    csv.NextRecord();


                //}
                //csv.WriteField("templateId");
                //csv.WriteField("templateVersion");
                //csv.WriteField("utilityType");
                //csv.WriteField("supplierName");
                //csv.WriteField("customerName");
                //csv.WriteField("fileName");
                //csv.WriteField("fileExtension");
                //csv.NextRecord();

                //csv.WriteField(billMetadata.templateId);
                //csv.WriteField(billMetadata.templateVersion);
                //csv.WriteField(billMetadata.utilityType);
                //csv.WriteField(billMetadata.supplierName);
                //csv.WriteField(billMetadata.customerName);
                //csv.WriteField(billMetadata.fileName);
                //csv.WriteField(billMetadata.fileExtension);
                //csv.NextRecord();
            }
            
        }
    }
}





