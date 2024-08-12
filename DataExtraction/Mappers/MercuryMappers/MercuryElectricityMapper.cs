//using System;
//using System.Collections.Generic;
//using System.Drawing.Drawing2D;
//using System.Globalization;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using System.Threading.Tasks.Dataflow;
//using Aspose.Pdf;
//using Aspose.Pdf.AI;
//using Aspose.Pdf.Drawing;
//using Aspose.Pdf.Operators;
//using DataExtraction.Library.Enums;
//using DataExtraction.Library.Interfaces;
//using DataExtraction.Library.Services;
//using UglyToad.PdfPig.Graphics.Operations.PathPainting;

//namespace DataExtraction.Library.Mappers.MercuryMappers
//{
//public class MercuryElectricityMapper : IMapper
//{
//    private readonly CsvBillMapper _csvBillMapper;

//    public MercuryElectricityMapper(CsvBillMapper csvBillMapper)
//    {
//        _csvBillMapper = csvBillMapper;
//    }

//    public async Task ProcessAsync(string groupedText, List<string> extractedText)
//    {
//        string combinedText = string.Join(Environment.NewLine, extractedText);



//        foreach (var billMetadata in billMetadataList)
//        {
//            // Print BillMetadata fields
//            accountNumber = Console.WriteLine($"Account Number: {billMetadata.AccountNumber}");
//            Console.WriteLine($"Invoice Number: {billMetadata.InvoiceNumber}");
//            Console.WriteLine($"Issue Date: {billMetadata.IssueDate}");
//            Console.WriteLine($"Due Date: {billMetadata.DueDate}");
//            Console.WriteLine($"Total Amount Due: {billMetadata.TotalAmountDue}");
//            Console.WriteLine($"Payment Method: {billMetadata.PaymentMethod}");
//            Console.WriteLine($"Opening Balance: {billMetadata.OpeningBalance}");
//            Console.WriteLine($"Previous Payment: {billMetadata.PreviousPayment}");
//            Console.WriteLine($"Customer Service Contact: {billMetadata.CustomerServiceContact}");
//            Console.WriteLine($"Current Bill Amount: {billMetadata.CurrentBillAmount}");

//            // Loop through ICPS
//            foreach (var icp in billMetadata.ICPS)
//            {
//                Console.WriteLine($"\nICP Code: {icp.ICPCode}");
//                Console.WriteLine($"Billing Address: {icp.BillingAddress}");
//                Console.WriteLine($"Billing Period: {icp.BillingPeriod}");
//                Console.WriteLine($"Read Start Date: {icp.ReadStartDate}");
//                Console.WriteLine($"Read End Date: {icp.ReadEndDate}");

//                // Loop through Meters
//                foreach (var meter in icp.Meters)
//                {
//                    Console.WriteLine($"\nMeter Number: {meter.MeterNumber}");
//                    Console.WriteLine($"Fixed Charge Quantity: {meter.FixedChargeQuantity}");
//                    Console.WriteLine($"Fixed Charge Rate: {meter.FixedChargeRate}");
//                    Console.WriteLine($"Fixed Charge Total: {meter.FixedChargeTotal}");
//                    Console.WriteLine($"GST: {meter.GST}");

//                    // Loop through Types
//                    foreach (var type in meter.Types)
//                    {
//                        Console.WriteLine($"\nType Name: {type.TypeName}");
//                        Console.WriteLine($"Previous Reading: {type.PreviousReading}");
//                        Console.WriteLine($"Current Reading: {type.CurrentReading}");
//                        Console.WriteLine($"Rate: {type.Rate}");
//                        Console.WriteLine($"Quantity: {type.Quantity}");
//                        Console.WriteLine($"Total: {type.Total}");
//                    }
//                }
//            }
//        }
        





//    var billMetadataList = new List<BillMetadata>
//        {
//        new BillMetadata
//            { 
//            //BillIdentifier = billIdentifier,
//            AccountNumber = accountNumber,
//            InvoiceNumber = invoiceNumber,
//            IssueDate = issueDate,
//            DueDate = dueDate,
//            TotalAmountDue = totalAmountDue,
//            PaymentMethod = paymentMethod,
//            OpeningBalance = openingBalance,
//            PreviousPayment = previousPayment,
//            CustomerServiceContact = customerServiceContact,
//            CurrentBillAmount = currentBillAmount,

//            ICPS = new List<ICP>
//            {
//                new ICP
//                {
//            ICPCode = icp,
//            ServiceDescription = serviceDescription,
//            BillingAddress = billingAddress,
//            BillingPeriod = billingPeriod,
//            ReadStartDate = readStartDate,
//            ReadEndDate = readEndDate,
//                Meters = new List<Meter>
//                {
//                    new Meter
//                    {
//                        MeterNumber = meterNumber,
//            FixedChargeQuantity = fixedChargeQuantity,
//            FixedChargeRate = fixedChargeRate,
//            FixedChargeTotal = fixedChargeTotal,
//            GST = gst,
//            Types = new List<Type>
//            {
//                new Type
//                {
//            TypeName = type,
//            PreviousReading = previousReading,
//            CurrentReading = currentReading,
//            Rate = rate,
//            Quantity = quantity,
//            Total = total
//            }
//                }
//                    }
//                }
//                }

//                }


//        }

//        }; await _csvBillMapper.WriteToCsvAsync(billMetadataList);


//}
//}
//}
