using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Aspose.Pdf.AI;
using Aspose.Pdf.Drawing;
using Aspose.Pdf.Operators;
using CsvHelper.Configuration;
using CsvHelper;
using DataExtraction.Library.Enums;
using DataExtraction.Library.Interfaces;
using DataExtraction.Library.Services;
using UglyToad.PdfPig.Graphics.Operations.PathPainting;
using static UglyToad.PdfPig.Core.PdfSubpath;

namespace DataExtraction.Library.Mappers.NovaMappers.NovaElectricityAndGasMappers
{
    public class NovaElectricityAndGasMappers : IMapper
    {
        private readonly CsvBillMapper _csvBillMapper;

        public NovaElectricityAndGasMappers(CsvBillMapper csvBillMapper)
        {
            _csvBillMapper = csvBillMapper;
        }


        public async Task ProcessAsync(string groupedText, List<string> extractedText)
        {

            string combinedText = string.Join(Environment.NewLine, extractedText);



            //global fields

            ////Aspose.PDF AccountNumber
            var accountNumber = string.Empty;
            var accountNumberKeyword = new[] { "Your customer number" };

            // Find the index of the line containing the keyword
            var keywordIndex = extractedText
                .Select((line, index) => new { Line = line, Index = index })
                .FirstOrDefault(x => accountNumberKeyword.Any(k => x.Line.Contains(k, StringComparison.OrdinalIgnoreCase)))?.Index;

            if (keywordIndex.HasValue && keywordIndex.Value + 1 < extractedText.Count)
            {
                // Extract the account number from the line immediately after the keyword line
                var accountNumberText = extractedText[keywordIndex.Value + 1].Trim();

                // Remove any non-numeric characters except dashes
                var filteredAccountNumber = new string(accountNumberText.Where(c => char.IsDigit(c) || c == '-').ToArray());

                // Clean up leading zeros only for the numeric parts
                var parts = filteredAccountNumber.Split('-');
                if (parts.Length > 0)
                {
                    // Remove leading zeros for each part
                    parts[0] = parts[0].TrimStart('0');
                    // Reassemble the parts
                    accountNumber = string.Join("-", parts);
                }
            }


            ////Aspose.PDF invoiceNumber
            var invoiceNumber = string.Empty;
            var invoiceNumberKeyword = new[] { "Tax Invoice/Statement number" };

            // Find the index of the line containing the keyword
            keywordIndex = extractedText
                .Select((line, index) => new { Line = line, Index = index })
                .FirstOrDefault(x => invoiceNumberKeyword.Any(k => x.Line.Contains(k, StringComparison.OrdinalIgnoreCase)))?.Index;

            if (keywordIndex.HasValue && keywordIndex.Value + 1 < extractedText.Count)
            {
                // Extract the account number from the line immediately after the keyword line
                var invoiceNumberText = extractedText[keywordIndex.Value + 1].Trim();

                // Remove any non-numeric characters except dashes
                var filteredInvoiceNumber = new string(invoiceNumberText.Where(c => char.IsDigit(c) || c == '-').ToArray());

                // Clean up leading zeros only for the numeric parts
                var parts = filteredInvoiceNumber.Split('-');
                if (parts.Length > 0)
                {
                    // Remove leading zeros for each part
                    parts[0] = parts[0].TrimStart('0');
                    // Reassemble the parts
                    invoiceNumber = string.Join("-", parts);
                }
            }




            ////Aspose.PDF issueDate
            var issueDate = string.Empty;

            // Find the index of the line that contains "Invoice date"
            var invoiceDateIndex = extractedText.FindIndex(s => s.Contains("Invoice date"));

            if (invoiceDateIndex != -1 && invoiceDateIndex + 1 < extractedText.Count)
            {
                // Get the next line which should contain the date
                var dateLine = extractedText[invoiceDateIndex + 1].Trim();

                // Split the date line into parts and manually construct the date
                var dateParts = dateLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Assume the format is "1 August 2024"
                var day = dateParts[3];
                var month = dateParts[4];
                var year = dateParts[5];

                // Construct the full date string
                var dateString = $"{day} {month} {year}";

                // Parse the date using DateTime.ParseExact
                // if (DateTime.TryParseExact(dateString, "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                //{
                if (DateTime.TryParse(dateString, out var parsedDate))
                {
                    issueDate = parsedDate.ToString("dd/MM/yyyy");
                }
                //}

            }

            ////Aspose.PDF dueDate
            var dueDate = string.Empty;

            if (extractedText.Any(s => s.Contains("due by ")))
            {
                var dueDateText = extractedText.LastOrDefault(s => s.Contains("due by "));

                // Split by "due by " and take the part that contains the date
                var datePart = dueDateText.Split(new[] { "due by " }, StringSplitOptions.None).Last().Trim();

                // Now split by space and take the first 3 parts as the date
                var dueDateParts = datePart.Split(' ').Take(3).ToArray(); // Take the first 3 parts to cover "19 August 2024"
                var dueDateString = string.Join(" ", dueDateParts); // Combine the parts back into a single string

                // Parse the string into a DateTime object
                if (DateTime.TryParseExact(dueDateString, "dd MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    dueDate = parsedDate.ToString("dd/MM/yyyy"); // Format the date as "dd/MM/yyyy"
                }
            }

            string totalAmountDue = string.Empty;

            // Check each line for the phrase "Total amount due"
            foreach (var line in extractedText)
            {
                if (line.Contains("Total amount due by ", StringComparison.OrdinalIgnoreCase))
                {
                    // Split the line at "Total amount due" and take the second part if it exists
                    var parts = line.Split(new string[] { "Total amount due by " }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        // Clean up the resulting string
                        string remainingText = parts[1].Trim();

                        // Find the first occurrence of the currency symbol ($) and extract the amount
                        int dollarIndex = remainingText.IndexOf('$');
                        if (dollarIndex != -1)
                        {
                            totalAmountDue = remainingText.Substring(dollarIndex + 1).Trim(); // Skip the dollar sign
                            break;
                        }
                    }
                }
            }


            // PAYMENT METHOD

            string paymentMethod = string.Empty;
            // Check if the line contains the "Pay by" keyword
            if (extractedText.Any(s => s.Contains("Pay by ")))
            {
                var paymentMethodText = extractedText.FirstOrDefault(s => s.Contains("Pay by "));
                paymentMethod = paymentMethodText.Split("Pay by ").Last().Trim();

                // Remove everything after the first period (including the period)
                int periodIndex = paymentMethod.IndexOf('.');
                if (periodIndex >= 0)
                {
                    paymentMethod = paymentMethod.Substring(0, periodIndex).Trim();
                }
            }

            var openingBalance = string.Empty;

            // Check each line for the phrase "Opening balance"
            if (extractedText.Any(s => s.Contains("Opening balance ")))
            {
                var openingBalanceText = extractedText.FirstOrDefault(s => s.Contains("Opening balance "));
                openingBalance = openingBalanceText.Split("Opening balance ").Last().Trim();
                if (openingBalanceText.Length > 1)
                {
                    // Remove everything after the first period (including the period)
                    int periodIndex = openingBalanceText.IndexOf('$');
                    string amountPart = openingBalanceText.Substring(periodIndex + 1).Trim();

                    // Further trim any non-numeric characters that may follow the amount
                    int endOfAmountIndex = amountPart.IndexOfAny(new char[] { ' ', ';', '\n', '\r', '\t' });
                    if (endOfAmountIndex != -1)
                    {
                        openingBalance = amountPart.Substring(0, endOfAmountIndex);
                    }
                }
            }

            var previousPayment = string.Empty;

            foreach (var line in extractedText)
            {
                if (line.Contains("Amount due on your last bill ", StringComparison.OrdinalIgnoreCase))
                {
                    // Split the line at "Opening balance" and take the second part if it exists
                    var parts = line.Split(new string[] { "Amount due on your last bill " }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        // Clean up the resulting string
                        string remainingText = parts[1].Trim();

                        // Find the first occurrence of the currency symbol ($) and extract the amount
                        int dollarIndex = remainingText.IndexOf('$');
                        if (dollarIndex != -1)
                        {
                            // Extract the amount after the dollar sign
                            string amountPart = remainingText.Substring(dollarIndex + 1).Trim();

                            // Further trim any non-numeric characters that may follow the amount
                            int endOfAmountIndex = amountPart.IndexOfAny(new char[] { ' ', ';', '\n', '\r', '\t' });
                            if (endOfAmountIndex != -1)
                            {
                                previousPayment = amountPart.Substring(0, endOfAmountIndex);
                            }
                        }
                    }
                }
            }


            // Updated regex pattern to match the customer service contact number (e.g., "110-430-256")
            string customerServiceContactPattern = @"(\b\d{3}-\d{3}-\d{3}\b)";

            // Initialize the customer service contact variable
            var customerServiceContact = string.Empty;

            // Find the line containing the customer service contact
            foreach (var line in extractedText)
            {
                // Match the customer service contact number
                var match = Regex.Match(line, customerServiceContactPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    customerServiceContact = match.Groups[1].Value; // Extract the customer service contact number
                    break; // Stop after finding the first match
                }
            }




            //CURRENT BILLING AMOUNT

            var currentBillAmount = string.Empty;

            // Check each line for the phrase "Current charges inc "
            foreach (var line in extractedText)
            {
                if (line.Contains("Current charges inc ", StringComparison.OrdinalIgnoreCase))
                {
                    // Split the line at "Current charges inc " and take the second part
                    var parts = line.Split(new string[] { "Current charges inc " }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        // Split the remaining part by the dollar sign to separate different amounts
                        var amounts = parts[1].Split('$');

                        // The last element in the 'amounts' array should be the final amount
                        if (amounts.Length > 1)
                        {
                            currentBillAmount = amounts.Last().Trim();
                            break;
                        }
                    }
                }
            }







            //var icpCode = string.Empty;
            //var charge = extractedText.FindIndex(s => s.Contains("Electricity Charges"));
            //var nextLine = extractedText[charge + 1];
            //if (extractedText.Any(s => s.Contains("ICP number: ")))
            //{
            //    var icpText = extractedText.FirstOrDefault(s => s.Contains("ICP number: "));

            //    // Extract the part after "ICP number: "
            //    var icpPart = icpText.Split(new[] { "ICP number: " }, StringSplitOptions.None).Last().Trim();

            //    // Find the first part that is fully numeric
            //    var icpParts = icpPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //    icpCode = icpParts[0].Split("ICP number: ")[0].Trim();

            //}



            //icpCode = string.Empty;
            //charge = extractedText.FindIndex(s => s.Contains("Gas Charges"));
            //nextLine = extractedText[charge + 1];
            //{
            //    if (extractedText.Any(s => s.Contains("ICP number: ")))
            //    {
            //        var icpText = extractedText.FirstOrDefault(s => s.Contains("ICP number: "));

            //        // Extract the part after "ICP number: "
            //        var icpPart = icpText.Split(new[] { "ICP number: " }, StringSplitOptions.None).Last().Trim();

            //        // Find the first part that is fully numeric
            //        var icpParts = icpPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //        icpCode = icpParts[0].Split("ICP number: ")[0].Trim();

            //    }
            //}



            // SERVICE DESCRIPTION


            //string serviceDescription = string.Empty;

            //foreach (var line in extractedText)
            //{
            //    // Check if the line contains the "Supply address" keyword
            //    if (line.Contains("Supply address:"))
            //    {
            //        // Extract the service description after "Supply address:"
            //        var parts = line.Split(new[] { "Supply address:" }, StringSplitOptions.None);
            //        if (parts.Length > 1)
            //        {
            //            // Trim the extracted service description
            //            serviceDescription = parts[1].Split("ICP")[0].Trim();
            //        }
            //        break; // Exit loop after finding and processing the description
            //    }
            //}









            //string billingAddress = string.Empty;
            //foreach (var line in extractedText)
            //{
            //    // Check if the line contains the "Supply address" keyword
            //    if (line.Contains("Supply address:"))
            //    {
            //        // Extract the service description after "Supply address:"
            //        var parts = line.Split(new[] { "Supply address:" }, StringSplitOptions.None);
            //        if (parts.Length > 1)
            //        {
            //            // Trim the extracted service description
            //            billingAddress = parts[1].Split("ICP")[0].Trim();
            //        }
            //        break; // Exit loop after finding and processing the description
            //    }
            //}







            //var billingPeriod = string.Empty;
            //var billDetailsIndex = extractedText.FindIndex(s => s.Contains("Billed Period: "));

            //if (billDetailsIndex != -1)
            //{
            //    // Adjust to access the correct line with the billing period details
            //    var dateRangeLine = extractedText[billDetailsIndex].Trim();

            //    // Split the line by the "Billed Period: " prefix
            //    var billingPeriodPart = dateRangeLine.Split(new[] { "Billed Period: " }, StringSplitOptions.None).Last().Trim();

            //    // Split the resulting string by " to " to get the start and end dates
            //    var dateParts = billingPeriodPart.Split(new[] { " to " }, StringSplitOptions.None);

            //    if (dateParts.Length == 2)
            //    {
            //        var startDateString = dateParts[0].Trim();
            //        var endDateString = dateParts[1].Trim();
            //        var endDateStringClean = endDateString.Split(' ')[0].Trim();
            //        if (DateTime.TryParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) &&
            //            DateTime.TryParseExact(endDateStringClean, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
            //        {
            //            var startFormatted = startDate.ToString("dd/MM/yyyy");
            //            var endFormatted = endDate.ToString("dd/MM/yyyy");
            //            billingPeriod = $"{startFormatted} to {endFormatted}";
            //        }
            //    }
            //}




            //var readStartDate = string.Empty;
            //billDetailsIndex = extractedText.FindIndex(s => s.Contains("Billed Period: "));

            //if (billDetailsIndex != -1)
            //{
            //    // Adjust to access the correct line with the billing period details
            //    var dateRangeLine = extractedText[billDetailsIndex].Trim();

            //    // Split the line by the "Billed Period: " prefix
            //    var readStartDatePart = dateRangeLine.Split(new[] { "Billed Period: " }, StringSplitOptions.None).Last().Trim();

            //    // Split the resulting string by " to " to get the start and end dates
            //    var dateParts = readStartDatePart.Split(new[] { " to " }, StringSplitOptions.None);

            //    if (dateParts.Length == 2)
            //    {
            //        var startDateString = dateParts[0].Trim();
            //        //var endDateString = dateParts[1].Trim();
            //        //var endDateStringClean = endDateString.Split(' ')[0].Trim();
            //        if (DateTime.TryParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
            //        {
            //            var startFormatted = startDate.ToString("dd/MM/yyyy");
            //            readStartDate = $"{startFormatted}";
            //        }
            //    }
            //}


            //var readEndDate = string.Empty;
            //billDetailsIndex = extractedText.FindIndex(s => s.Contains("Billed Period: "));

            //if (billDetailsIndex != -1)
            //{
            //    // Adjust to access the correct line with the billing period details
            //    var dateRangeLine = extractedText[billDetailsIndex].Trim();

            //    // Split the line by the "Billed Period: " prefix
            //    var readEndDatePart = dateRangeLine.Split(new[] { "Billed Period: " }, StringSplitOptions.None).Last().Trim();

            //    // Split the resulting string by " to " to get the start and end dates
            //    var dateParts = readEndDatePart.Split(new[] { " to " }, StringSplitOptions.None);

            //    if (dateParts.Length == 2)
            //    {

            //        var endDateString = dateParts[1].Trim();
            //        var endDateStringClean = endDateString.Split(' ')[0].Trim();
            //        if (DateTime.TryParseExact(endDateStringClean, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
            //        {
            //            var endFormatted = endDate.ToString("dd/MM/yyyy");
            //            readEndDate = $"{endFormatted}";
            //        }
            //    }
            //}


            var icps = new List<ICP>();

            void ProcessChargeType(string chargeType)
            {
                var chargeIndex = extractedText.FindIndex(s => s.Contains(chargeType));
                if (chargeIndex == -1) return;

                var icpCode = string.Empty;
                var serviceDescription = string.Empty;
                var billingAddress = string.Empty;
                var billingPeriod = string.Empty;
                var readStartDate = string.Empty;
                var readEndDate = string.Empty;

                // Extract ICP Code
                var nextLine = extractedText[chargeIndex + 1];
                if (nextLine.Contains("ICP number: "))
                {
                    icpCode = ExtractIcpCode(nextLine);
                }

                // Extract Service Description and Billing Address
                if (nextLine.Contains("Supply address:"))
                {
                    serviceDescription = ExtractAddress(nextLine);
                    billingAddress = ExtractAddress(nextLine); // Assuming billingAddress is the same as serviceDescription
                }

                // Extract Billing Period, Start Date, and End Date
                var periodLineIndex = extractedText.FindIndex(chargeIndex, s => s.Contains("Billed Period: "));
                if (periodLineIndex != -1)
                {
                    var periodLine = extractedText[periodLineIndex];
                    billingPeriod = ExtractBillingPeriod(periodLine);
                    readStartDate = ExtractStartDate(periodLine);
                    readEndDate = ExtractEndDate(periodLine);
                }

                // Add to list
                icps.Add(new ICP
                {
                    ICPCode = icpCode,
                    ServiceDescription = serviceDescription,
                    BillingAddress = billingAddress,
                    BillingPeriod = billingPeriod,
                    ReadStartDate = readStartDate,
                    ReadEndDate = readEndDate
                });
            }

            // Process both Electricity and Gas Charges
            ProcessChargeType("Electricity Charges");
            ProcessChargeType("Gas Charges");

            

        

        static string ExtractIcpCode(string textLine)
        {
            var icpPart = textLine.Split(new[] { "ICP number: " }, StringSplitOptions.None).Last().Trim();
            var icpParts = icpPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return icpParts[0].Trim();
        }

        static string ExtractAddress(string line)
        {
            var parts = line.Split(new[] { "Supply address:" }, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                return parts[1].Split("ICP")[0].Trim();
            }
            return string.Empty;
        }

        static string ExtractBillingPeriod(string line)
        {
            var billingPeriodPart = line.Split(new[] { "Billed Period: " }, StringSplitOptions.None).Last().Trim();
            var dateParts = billingPeriodPart.Split(new[] { " to " }, StringSplitOptions.None);
            if (dateParts.Length == 2)
            {
                if (DateTime.TryParseExact(dateParts[0].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) &&
                    DateTime.TryParseExact(dateParts[1].Trim().Split(' ')[0], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                {
                    return $"{startDate:dd/MM/yyyy} to {endDate:dd/MM/yyyy}";
                }
            }
            return string.Empty;
        }

        static string ExtractStartDate(string line)
        {
            var billingPeriodPart = line.Split(new[] { "Billed Period: " }, StringSplitOptions.None).Last().Trim();
            var dateParts = billingPeriodPart.Split(new[] { " to " }, StringSplitOptions.None);
            if (dateParts.Length > 0)
            {
                if (DateTime.TryParseExact(dateParts[0].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                {
                    return startDate.ToString("dd/MM/yyyy");
                }
            }
            return string.Empty;
        }

        static string ExtractEndDate(string line)
        {
            var billingPeriodPart = line.Split(new[] { "Billed Period: " }, StringSplitOptions.None).Last().Trim();
            var dateParts = billingPeriodPart.Split(new[] { " to " }, StringSplitOptions.None);
            if (dateParts.Length > 1)
            {
                var endDateString = dateParts[1].Trim().Split(' ')[0];
                if (DateTime.TryParseExact(endDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                {
                    return endDate.ToString("dd/MM/yyyy");
                }
            }
            return string.Empty;
        }

    













        //METER NUMBER

        var meterNumber = string.Empty;
            var chargeType = "Electricity Charges"; // Define the charge type you're interested in
            bool isChargeTypeFound = false;

            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains the charge type
                if (line.Contains(chargeType))
                {
                    isChargeTypeFound = true;
                }
                else if (isChargeTypeFound)
                {
                    // Look for the line that contains "Meter"
                    if (line.Contains("Meter"))
                    {
                        // Ensure that the "Number" label and the actual meter number are within bounds
                        if (i + 2 < extractedText.Count)
                        {
                            var meterLabelLine = extractedText[i + 2]; // The line under "Meter"
                            var meterNumberLine = extractedText[i + 3]; // The actual meter number line

                            // Check if the line below "Meter" contains "Number"
                            if (meterLabelLine.Contains("Number"))
                            {
                                // Use a refined regex pattern to match the meter number exactly
                                string meterNumberPattern = @"\b(\d+)(\/\d+)?\b";
                                var match = Regex.Match(meterNumberLine, meterNumberPattern, RegexOptions.IgnoreCase);

                                if (match.Success)
                                {
                                    meterNumber = match.Groups[1].Value; // Get only the first capturing group, excluding "/1"

                                    break; // Exit loop once the meter number is found
                                }
                            }
                        }
                    }
                }
            }







            string fixedChargeQuantity = string.Empty;
            string dailyChargePattern = @"Daily Charge\s*(\d+)\s*days\s*([\d\.]+)c\s*\$\s*([\d\.]+)";
            foreach (var line in extractedText)
            {
                // Match the Daily Charge line
                var match = Regex.Match(line, dailyChargePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    fixedChargeQuantity = match.Groups[1].Value;
                    break;
                }
            }




            string fixedChargeRate = string.Empty;

            foreach (var line in extractedText)
            {
                // Match the Daily Charge line
                var match = Regex.Match(line, dailyChargePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    fixedChargeRate = match.Groups[2].Value;
                    break;
                }
            }



            string fixedChargeTotal = string.Empty;
            foreach (var line in extractedText)
            {
                // Match the Daily Charge line
                var match = Regex.Match(line, dailyChargePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    fixedChargeTotal = match.Groups[3].Value;
                    break;
                }
            }



            string gstPattern = @"GST\s*\d+%\s*\$([\d\.]+)";
            string gst = string.Empty;

            // Loop through each line of extracted text
            foreach (var line in extractedText)
            {
                // Match the GST line
                var gstMatch = Regex.Match(line, gstPattern, RegexOptions.IgnoreCase);
                if (gstMatch.Success)
                {
                    gst = gstMatch.Groups[1].Value; // Extract the GST amount
                    break; // Stop after finding the GST amount
                }
            }





            //TYPE

            var typeName = string.Empty;
            bool inElectricityChargesSection = false;

            // Loop through each line in the extracted text with an index
            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i].Trim();

                // Check if the line contains "Electricity Charges" to identify the section
                if (line.Contains("Electricity Charges"))
                {
                    inElectricityChargesSection = true;
                    continue; // Move to the next line
                }

                // Check if we are in the electricity charges section and the line contains the "Item" header
                if (inElectricityChargesSection && line.Contains("Item"))
                {
                    // Check if the next line exists
                    if (i + 2 < extractedText.Count)
                    {
                        // Get the next line which contains the type details
                        var typeLine = extractedText[i + 2].Trim();

                        // Split the type line to get the type from the "Item" column
                        var typeParts = typeLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Assuming the type is the first element in the split line (under "Item")
                        if (typeParts.Length > 0)
                        {
                            typeName = typeParts[0].Trim();
                        }
                    }
                    // Exit the loop once the type is found
                    break;
                }
            }




            var multiplier = string.Empty;
            chargeType = "Electricity Charges"; // Define the charge type you're interested in
            isChargeTypeFound = false;

            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains the charge type
                if (line.Contains(chargeType))
                {
                    isChargeTypeFound = true;
                }
                else if (isChargeTypeFound)
                {
                    // Look for the line that contains "Meter"
                    if (line.Contains("Meter"))
                    {
                        // Ensure that the "Number" label and the actual meter number are within bounds
                        if (i + 2 < extractedText.Count)
                        {
                            var meterLabelLine = extractedText[i + 2]; // The line under "Meter"
                            var meterNumberLine = extractedText[i + 3]; // The actual meter number line

                            // Check if the line below "Meter" contains "Number"
                            if (meterLabelLine.Contains("Number"))
                            {
                                // Use a refined regex pattern to match the meter number exactly
                                string meterNumberPattern = @"\b(\d+)(\/\d+)?\b";
                                var match = Regex.Match(meterNumberLine, meterNumberPattern, RegexOptions.IgnoreCase);

                                if (match.Success)
                                {
                                    multiplier = match.Groups[2].Value; // Get only the first capturing group, excluding "/1"
                                    multiplier = multiplier.Replace("/", "");
                                }
                                break;
                            }
                        }
                    }
                }
            }









            var previousReading = string.Empty;

            chargeType = "Electricity Charges"; // Define the charge type you're interested in
            isChargeTypeFound = false;

            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains the charge type
                if (line.Contains(chargeType))
                {
                    isChargeTypeFound = true;
                }
                else if (isChargeTypeFound)
                {
                    // Look for the line that contains "Previous"
                    if (line.Contains("Previous"))
                    {
                        // Ensure that the next lines are within bounds
                        if (i + 2 < extractedText.Count)
                        {
                            var meterNumberLine = extractedText[i + 2]; // The line containing meter readings
                            string meterNumberPattern = @"\b(\d+)\b";

                            // Find all matches on the line
                            var matches = Regex.Matches(meterNumberLine, meterNumberPattern, RegexOptions.IgnoreCase);

                            if (matches.Count > 0)
                            {
                                // Assume the first match is the previous reading
                                previousReading = matches[2].Value;
                                break;
                            }
                        }
                    }
                }
            }



            var currentReading = string.Empty;

            chargeType = "Electricity Charges"; // Define the charge type you're interested in
            isChargeTypeFound = false;

            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains the charge type
                if (line.Contains(chargeType))
                {
                    isChargeTypeFound = true;
                }
                else if (isChargeTypeFound)
                {
                    // Look for the line that contains "Previous"
                    if (line.Contains("Current"))
                    {
                        // Ensure that the next lines are within bounds
                        if (i + 2 < extractedText.Count)
                        {
                            var meterNumberLine = extractedText[i + 2]; // The line containing meter readings
                            string meterNumberPattern = @"\b(\d+)\b";

                            // Find all matches on the line
                            var matches = Regex.Matches(meterNumberLine, meterNumberPattern, RegexOptions.IgnoreCase);

                            if (matches.Count > 0)
                            {
                                // Assume the first match is the previous reading
                                currentReading = matches[3].Value;
                                break;
                            }
                        }
                    }
                }
            }




            var rate = string.Empty;
            chargeType = "Electricity Charges"; // Define the charge type you're interested in
            isChargeTypeFound = false;

            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains the charge type
                if (line.Contains(chargeType))
                {
                    isChargeTypeFound = true;
                }
                else if (isChargeTypeFound)
                {
                    // Look for the line that contains "Previous"
                    if (line.Contains("Previous"))
                    {
                        // Ensure that the next lines are within bounds
                        if (i + 2 < extractedText.Count)
                        {
                            
                            var meterNumberLine = extractedText[i + 2]; // The line with "Anytime"

                                // Extract the rate
                                string ratePattern = @"\b(\d{1,2}\.\d{3})c\b"; // Pattern to match the rate, e.g., "19.999c"
                                var rateMatch = Regex.Match(meterNumberLine, ratePattern);

                                if (rateMatch.Success)
                                {
                                     rate = rateMatch.Groups[1].Value;
                                }

                                break; // Exit the loop after extracting the rate
                            
                        }
                    }
                }
            }


            var quantity = string.Empty;
            chargeType = "Electricity Charges"; // Define the charge type you're interested in
            isChargeTypeFound = false;

            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains the charge type
                if (line.Contains(chargeType))
                {
                    isChargeTypeFound = true;
                }
                else if (isChargeTypeFound)
                {
                    // Look for the line that contains "Previous"
                    if (line.Contains("Previous"))
                    {
                        // Ensure that the next lines are within bounds
                        if (i + 2 < extractedText.Count)
                        {
                            var meterNumberLine = extractedText[i + 2]; // The line with "Anytime"

                            // Extract the quantity (e.g., "2326 kWh")
                            string quantityPattern = @"\b(\d+)\s*kWh\b"; // Pattern to match the quantity, e.g., "2326 kWh"
                            var quantityMatch = Regex.Match(meterNumberLine, quantityPattern);

                            if (quantityMatch.Success)
                            {
                                quantity = quantityMatch.Groups[1].Value; // Fetch the quantity value
                            }

                            break; // Exit the loop after extracting the quantity
                        }
                    }
                }
            }


            var total = string.Empty;
            chargeType = "Electricity Charges"; // Define the charge type you're interested in
            isChargeTypeFound = false;

            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains the charge type
                if (line.Contains(chargeType))
                {
                    isChargeTypeFound = true;
                }
                else if (isChargeTypeFound)
                {
                    // Look for the line that contains "Previous"
                    if (line.Contains("Previous"))
                    {
                        // Ensure that the next lines are within bounds
                        if (i + 2 < extractedText.Count)
                        {
                            var meterNumberLine = extractedText[i + 2]; // The line with "Anytime"

                            // Extract the total amount
                            string totalPattern = @"\$\d+(\.\d{2})?"; // Pattern to match the total amount, e.g., "$465.18"
                            var totalMatch = Regex.Match(meterNumberLine, totalPattern);

                            if (totalMatch.Success)
                            {
                                total = totalMatch.Value;
                                total = total.Replace("$", "");
                            }

                            break; // Exit the loop after extracting the total amount
                        }
                    }
                }
            }








            var billMetadata = new BillMetadata
                            {
                                //BillIdentifier = billIdentifier,
                                AccountNumber = accountNumber,
                                InvoiceNumber = invoiceNumber,
                                IssueDate = issueDate,
                                DueDate = dueDate,
                                TotalAmountDue = totalAmountDue,
                                PaymentMethod = paymentMethod,
                                OpeningBalance = openingBalance,
                                PreviousPayment = previousPayment,
                                CustomerServiceContact = customerServiceContact,
                                CurrentBillAmount = currentBillAmount,

                                ICPS = new List<ICP>
                {
                    new ICP
                    {
               //ICPCode = icpCode,
              //  ServiceDescription = serviceDescription,
                //BillingAddress = billingAddress,
                //BillingPeriod = billingPeriod,
                //ReadStartDate = readStartDate,
                //ReadEndDate = readEndDate,
                 Meters = new List<Meter>
                 {
                     new Meter
                     {
                          MeterNumber = meterNumber,
                FixedChargeQuantity = fixedChargeQuantity,
                FixedChargeRate = fixedChargeRate,
                FixedChargeTotal = fixedChargeTotal,
                GST = gst,
               Types = new List<Type>
               {
                   new Type
                   {
                TypeName = typeName,
                Multiplier = multiplier,
                PreviousReading = previousReading,
                CurrentReading = currentReading,
                Rate = rate,
                Quantity = quantity,
                Total = total
                }
                   }
                     }
                 }
                    }

                    }
                            };

                            await _csvBillMapper.WriteToCsvAsync(billMetadata);
                        
            
        }
    }
}
