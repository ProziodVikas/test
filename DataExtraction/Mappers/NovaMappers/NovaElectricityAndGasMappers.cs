using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Aspose.Pdf.AI;
using Aspose.Pdf.Drawing;
using Aspose.Pdf.Operators;
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


            var billingPeriod = string.Empty;
            var billDetailsIndex = extractedText.FindIndex(s => s.Contains("Billed Period: "));

            if (billDetailsIndex != -1)
            {
                // Adjust to access the correct line with the billing period details
                var dateRangeLine = extractedText[billDetailsIndex].Trim();

                // Split the line by the "Billed Period: " prefix
                var billingPeriodPart = dateRangeLine.Split(new[] { "Billed Period: " }, StringSplitOptions.None).Last().Trim();

                // Split the resulting string by " to " to get the start and end dates
                var dateParts = billingPeriodPart.Split(new[] { " to " }, StringSplitOptions.None);

                if (dateParts.Length == 2)
                {
                    var startDateString = dateParts[0].Trim();
                    var endDateString = dateParts[1].Trim();
                    var endDateStringClean = endDateString.Split(' ')[0].Trim();
                    if (DateTime.TryParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) &&
                        DateTime.TryParseExact(endDateStringClean, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                    {
                        var startFormatted = startDate.ToString("dd/MM/yyyy");
                        var endFormatted = endDate.ToString("dd/MM/yyyy");
                        billingPeriod = $"{startFormatted} to {endFormatted}";
                    }
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



            //Aspose PeriodFrom
            var readStartDate = string.Empty;
            billDetailsIndex = extractedText.FindIndex(s => s.Contains("Billed Period: "));

            if (billDetailsIndex != -1)
            {
                // Adjust to access the correct line with the billing period details
                var dateRangeLine = extractedText[billDetailsIndex].Trim();

                // Split the line by the "Billed Period: " prefix
                var readStartDatePart = dateRangeLine.Split(new[] { "Billed Period: " }, StringSplitOptions.None).Last().Trim();

                // Split the resulting string by " to " to get the start and end dates
                var dateParts = readStartDatePart.Split(new[] { " to " }, StringSplitOptions.None);

                if (dateParts.Length == 2)
                {
                    var startDateString = dateParts[0].Trim();
                    //var endDateString = dateParts[1].Trim();
                    //var endDateStringClean = endDateString.Split(' ')[0].Trim();
                    if (DateTime.TryParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                    {
                        var startFormatted = startDate.ToString("dd/MM/yyyy");
                        readStartDate = $"{startFormatted}";
                    }
                }
            }


            var readEndDate = string.Empty;
            billDetailsIndex = extractedText.FindIndex(s => s.Contains("Billed Period: "));

            if (billDetailsIndex != -1)
            {
                // Adjust to access the correct line with the billing period details
                var dateRangeLine = extractedText[billDetailsIndex].Trim();

                // Split the line by the "Billed Period: " prefix
                var readEndDatePart = dateRangeLine.Split(new[] { "Billed Period: " }, StringSplitOptions.None).Last().Trim();

                // Split the resulting string by " to " to get the start and end dates
                var dateParts = readEndDatePart.Split(new[] { " to " }, StringSplitOptions.None);

                if (dateParts.Length == 2)
                {

                    var endDateString = dateParts[1].Trim();
                    var endDateStringClean = endDateString.Split(' ')[0].Trim();
                    if (DateTime.TryParseExact(endDateStringClean, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                    {
                        var endFormatted = endDate.ToString("dd/MM/yyyy");
                        readEndDate = $"{endFormatted}";
                    }
                }
            }





            //Electricity charge

            // SERVICE DESCRIPTION


            string serviceDescription = string.Empty;

            // Loop through each line in the extracted text
            foreach (var line in extractedText)
            {
                // Check if the line contains the "ICP" keyword
                if (line.Contains("ICP"))
                {
                    // Split the line at the "ICP" keyword and take the part before it
                    var parts = line.Split(new[] { "ICP" }, StringSplitOptions.None);
                    if (parts.Length > 0)
                    {
                        // Trim the part before "ICP" to get the service description
                        serviceDescription = parts[0].Trim();
                    }
                    break; // Exit loop after finding and processing the description
                }
            }


            var addressPattern = @"^\d+[A-Za-z]?\s+\w+";
            var cityPattern = @"^[A-Z]+\s*\d{4}$";

            string billingAddress = string.Empty;
            bool addressStartFound = false;

            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i].Trim();

                if (!string.IsNullOrEmpty(line))
                {
                    // Check if this line is the start of the address
                    if (Regex.IsMatch(line, addressPattern, RegexOptions.IgnoreCase))
                    {
                        addressStartFound = true;
                        billingAddress = line;
                    }
                    // If we have started capturing the address, continue until we match the city pattern
                    else if (addressStartFound)
                    {
                        billingAddress += "\n" + line;
                        if (Regex.IsMatch(line, cityPattern, RegexOptions.IgnoreCase))
                        {
                            break;
                        }
                    }
                }
            }



            var icp = string.Empty;
            if (extractedText.Any(s => s.Contains("ICP ")))
            {
                var icpText = extractedText.FirstOrDefault(s => s.Contains("ICP "));
                icp = icpText.Split("ICP ").Last().Trim();
            }


            string fixedChargeQuantity = string.Empty;
            string fixedChargeQuantityPattern = @"Daily charge\s*\(\d+\.\d+\s*c\/day\s*x\s*(\d+)\s*days\)";

            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, fixedChargeQuantityPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    fixedChargeQuantity = match.Groups[1].Value;
                    break;
                }
            }


            string fixedChargeRatePattern = @"Daily charge\s*\(([\d\.]+)\s*c/day";

            // Initialize variable for storing the fixed charge rate
            string fixedChargeRate = string.Empty;

            // Loop through each line of extracted text
            foreach (var line in extractedText)
            {
                // Match fixed charge rate
                var fixedChargeRateMatch = Regex.Match(line, fixedChargeRatePattern, RegexOptions.IgnoreCase);
                if (fixedChargeRateMatch.Success)
                {
                    fixedChargeRate = fixedChargeRateMatch.Groups[1].Value;
                    break;
                }
            }


            string fixedChargeTotalPattern = @"Daily charge\s*\(\d+\.\d+\s*c\/day\s*x\s*\d+\s*days\)\s*\$([\d\.]+)";

            // Initialize variable
            string fixedChargeTotal = string.Empty;

            // Loop through each line of extracted text
            foreach (var line in extractedText)
            {
                var fixedChargeTotalMatch = Regex.Match(line, fixedChargeTotalPattern, RegexOptions.IgnoreCase);
                if (fixedChargeTotalMatch.Success)
                {
                    fixedChargeTotal = fixedChargeTotalMatch.Groups[1].Value;
                    break; // Stop after finding the fixed charge total
                }
            }


            string gstPattern = @"GST\s*@\s*\d+%\s*\$([\d\.]+)";
            string gst = string.Empty;

            foreach (var line in extractedText)
            {
                var gstMatch = Regex.Match(line, gstPattern, RegexOptions.IgnoreCase);
                if (gstMatch.Success)
                {
                    gst = gstMatch.Groups[1].Value;
                    break;
                }
            }



            //METER NUMBER

            var meterNumber = string.Empty;

            // Loop through each line in the extracted text with an index
            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];
                // Check if the line contains "Previous Reading" which indicates the presence of the meter number
                if (line.Contains("Previous Reading"))
                {
                    // Check if the next line exists
                    if (i + 1 < extractedText.Count)
                    {
                        // Get the next line which contains the meter number and other details
                        var nextLine = extractedText[i + 1].Trim();

                        // Split the next line at ':' to separate the meter number from other details
                        var meterNumberParts = nextLine.Split(':');

                        if (meterNumberParts.Length > 0)
                        {
                            // Assuming the first part before ':' is the meter number
                            meterNumber = meterNumberParts[0].Trim();
                        }
                    }
                    // Break the loop after finding and processing the line with "Previous Reading"
                    break;
                }
            }





            var multiplier = string.Empty;

            // Loop through each line in the extracted text with an index
            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];
                // Check if the line contains "Previous Reading" which indicates the presence of the meter number
                if (line.Contains("Previous Reading"))
                {
                    // Check if the next line exists
                    if (i + 1 < extractedText.Count)
                    {
                        // Get the next line which contains the multiplier
                        var nextLine = extractedText[i + 1].Trim();

                        // Split the next line at ':' to separate the meter number from other details
                        var parts = nextLine.Split(':');

                        if (parts.Length > 1)
                        {
                            // Assuming the first part before ':' is the meter number and the second part contains the multiplier
                            var potentialMultiplier = parts[1].Split(' ').FirstOrDefault();

                            if (int.TryParse(potentialMultiplier, out var numericMultiplier))
                            {
                                multiplier = numericMultiplier.ToString();
                            }
                        }
                    }
                    // Break the loop after finding and processing the line with "Previous Reading"
                    break;
                }
            }




            var type = string.Empty;

            // Loop through each line in the extracted text with an index
            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains "Previous Reading" to locate the meter details
                if (line.Contains("Previous Reading"))
                {
                    // Check if the next line exists
                    if (i + 1 < extractedText.Count)
                    {
                        // Get the next line which contains the meter details
                        var meterLine = extractedText[i + 1].Trim();

                        // Split the meter line at ':' to separate the meter number from other details
                        var parts = meterLine.Split(':');

                        if (parts.Length > 1)
                        {
                            // Split the part after ':' to isolate the type (assumed to be the first word after ':1')
                            var details = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            if (details.Length > 1)
                            {
                                // Assuming the type is the second element after the split
                                type = details[1].Trim();
                            }
                        }
                    }
                    // Break the loop after finding and processing the line with "Previous Reading"
                    break;
                }
            }




            var previousReading = string.Empty;

            // Loop through each line in the extracted text with an index
            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains "Previous Reading" to locate the relevant line
                if (line.Contains("Previous Reading"))
                {
                    // Check if the next line exists
                    if (i + 1 < extractedText.Count)
                    {
                        // Get the next line which contains the meter details
                        var meterLine = extractedText[i + 1].Trim();

                        // Split the meter line to isolate the previous reading
                        var parts = meterLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Assuming the previous reading is the third item in the line (based on the provided format)
                        if (parts.Length > 2)
                        {
                            previousReading = parts[2].Trim();
                        }
                    }
                    // Break the loop after finding and processing the line with "Previous Reading"
                    break;
                }
            }



            var currentReading = string.Empty;

            // Loop through each line in the extracted text with an index
            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains "Previous Reading" to locate the relevant line
                if (line.Contains("Previous Reading"))
                {
                    // Check if the next line exists
                    if (i + 1 < extractedText.Count)
                    {
                        // Get the next line which contains the meter details
                        var meterLine = extractedText[i + 1].Trim();

                        // Split the meter line to isolate the previous reading
                        var parts = meterLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Assuming the previous reading is the third item in the line (based on the provided format)
                        if (parts.Length > 3)
                        {
                            currentReading = parts[3].Trim();
                        }
                    }
                    // Break the loop after finding and processing the line with "Previous Reading"
                    break;
                }
            }



            var rate = string.Empty;

            // Loop through each line in the extracted text with an index
            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains "Previous Reading" to locate the relevant line
                if (line.Contains("BUSINESS EVERYDAY"))
                {
                    // Check if the next line exists
                    if (i + 1 < extractedText.Count)
                    {
                        // Get the next line which contains the meter details
                        var meterLine = extractedText[i + 1].Trim();

                        // Split the meter line to isolate the previous reading
                        var parts = meterLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Assuming the previous reading is the third item in the line (based on the provided format)
                        if (parts.Length > 7)
                        {
                            rate = parts[7].Trim();
                        }
                    }
                    // Break the loop after finding and processing the line with "Previous Reading"
                    break;
                }
            }


            var quantity = string.Empty;
            string quantityPattern = @"(\d+\.?\d*)\s*(kWh)";

            foreach (var line in extractedText)
            {
                // Match quantity
                var quantityMatch = Regex.Match(line, quantityPattern, RegexOptions.IgnoreCase);
                if (quantityMatch.Success)
                {
                    quantity = quantityMatch.Groups[1].Value;
                    break;
                }

            }


            var total = string.Empty;

            // Loop through each line in the extracted text with an index
            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains a keyword or pattern related to the total amount
                if (line.Contains("BUSINESS EVERYDAY"))
                {
                    // Check if the next line exists
                    if (i + 1 < extractedText.Count)
                    {
                        // Get the next line which contains the details including the total amount
                        var detailsLine = extractedText[i + 1].Trim();

                        // Split the line to isolate different parts
                        var parts = detailsLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Assuming the total amount is the last item in the line
                        if (parts.Length > 0)
                        {
                            // Extract the total amount part (it may include the $ symbol)
                            var amountPart = parts[parts.Length - 1].Trim();

                            // Remove the $ symbol if present
                            total = amountPart.Replace("$", string.Empty).Trim();
                        }
                    }
                    // Break the loop after finding and processing the line with the relevant details
                    break;
                }
            }





            var billMetadataList = new BillMetadata
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
                ICPCode = icp,
                BillingAddress = billingAddress,
                BillingPeriod = billingPeriod,
                ReadStartDate = readStartDate,
                ReadEndDate = readEndDate,
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
                TypeName = type,
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


            await _csvBillMapper.WriteToCsvAsync(billMetadataList);
        }

    }
}