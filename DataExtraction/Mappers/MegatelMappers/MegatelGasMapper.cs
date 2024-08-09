using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
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

namespace DataExtraction.Library.Mappers.MegatelMappers
{
    public class MegatelGasMapper : IMapper
    {
        private readonly CsvBillMapper _csvBillMapper;

        public MegatelGasMapper(CsvBillMapper csvBillMapper)
        {
            _csvBillMapper = csvBillMapper;
        }

        public async Task ProcessAsync(string groupedText, List<string> extractedText)
        {
            string combinedText = string.Join(Environment.NewLine, extractedText);



            ////Aspose.PDF AccountNumber
            var accountNumber = string.Empty;
            var accountNumberKeyword = new[] { "Your Account Number", "Account Number" };

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

                Console.WriteLine($"Account Number: {accountNumber}");
            }








            ////Aspose.PDF invoiceNumber
            var invoiceNumber = string.Empty;
            if (extractedText.Any(s => s.Contains("Tax Invoice ")))
            {
                var invoiceNumberText = extractedText.FirstOrDefault(s => s.Contains("Tax Invoice "));
                if (invoiceNumberText != null)
                {
                    // Determine the keyword to split on
                    var keyword = new[] { "Tax Invoice " }
                                  .FirstOrDefault(k => invoiceNumberText.Contains(k));

                    if (keyword != null)
                    {
                        // Split the text at the keyword and get the part after it
                        var parts = invoiceNumberText.Split(new[] { keyword }, StringSplitOptions.None);
                        if (parts.Length > 1)
                        {
                            // Extract the account number (the text after the keyword) and trim any remaining parts
                            invoiceNumber = new string(parts[1].Trim().TakeWhile(char.IsDigit).ToArray());

                            // Optionally, update the line to only contain the account number
                            var index = extractedText.IndexOf(invoiceNumberText);
                            if (index >= 0)
                            {
                                extractedText[index] = $"{keyword} {invoiceNumber}";
                            }
                        }
                    }
                }
            }













            ////Aspose.PDF issueDate
            var issueDate = string.Empty;

            // Loop through each line in the extracted text with an index
            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains "Billing Date"
                if (line.Contains("Billing Date") || line.Contains("Billing"))
                {
                    // Check if the next line exists
                    if (i + 1 < extractedText.Count)
                    {
                        // Get the next line which contains the date
                        var dateLine = extractedText[i + 1].Trim();

                        // Parse the date from the next line
                        if (DateTime.TryParseExact(dateLine, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                        {
                            issueDate = parsedDate.ToString("dd/MM/yyyy");
                        }
                        else if (DateTime.TryParseExact(dateLine, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate)) // Handle alternative format if necessary
                        {
                            issueDate = parsedDate.ToString("dd/MM/yyyy");
                        }
                    }
                    // Break the loop after finding and processing the line with "Billing Date"
                    break;
                }
            }





            ////Aspose.PDF dueDate

            var dueDate = string.Empty;

            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains "Billing Date"
                if (line.Contains("Due Date") || line.Contains("Due"))
                {
                    // Check if the next line exists
                    if (i + 1 < extractedText.Count)
                    {
                        // Get the next line which contains the date
                        var duedateLine = extractedText[i + 1].Trim();

                        // Parse the date from the next line
                        if (DateTime.TryParseExact(duedateLine, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                        {
                            dueDate = parsedDate.ToString("dd/MM/yyyy");
                        }
                        else if (DateTime.TryParseExact(duedateLine, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate)) // Handle alternative format if necessary
                        {
                            dueDate = parsedDate.ToString("dd/MM/yyyy");
                        }
                    }
                    // Break the loop after finding and processing the line with "Billing Date"
                    break;
                }
            }







            string totalAmountDue = string.Empty;

            if (extractedText.Any(s => s.Contains("the due date of ")))
            {
                var totalAmountDueText = extractedText.FirstOrDefault(s => s.Contains("the due date of "));

                if (totalAmountDueText != null)
                {
                    // Extract the part after "the due date of "
                    var amountText = totalAmountDueText.Split("the due date of ").Last().Trim();

                    // Remove the '$' symbol from the amount if present
                    amountText = amountText.Replace("$", string.Empty).Trim();

                    // Split by spaces and get the last part (assumes the amount is the last segment)
                    var amountParts = amountText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (amountParts.Length > 0)
                    {
                        totalAmountDue = amountParts.Last();
                    }
                }
            }






            // PAYMENT METHOD

            string paymentMethod = string.Empty;
            // Check if the line contains the "ICP" keyword
            if (extractedText.Any(s => s.Contains("Our Payment Method - ")))
            {
                var paymentMethodText = extractedText.FirstOrDefault(s => s.Contains("Our Payment Method - "));
                paymentMethod = paymentMethodText.Split("Our Payment Method - ").Last().Trim();
            }






            // OPENING BALANCE


            var openingBalance = string.Empty;
            string pattern = @"Opening balance\s*\$([\d,]+\.\d{2})";

            // Check if any line contains "Opening account balance"
            if (extractedText.Any(s => s.Contains("Opening balance ")))
            {
                // Find the line with the "Opening account balance" text
                var openingBalanceText = extractedText.FirstOrDefault(s => s.Contains("Opening balance "));

                // Extract the amount using regex
                var match = Regex.Match(openingBalanceText, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    openingBalance = match.Groups[1].Value.Replace(",", ""); // Remove comma if present
                }
            }





            var previousPayment = string.Empty;
            // Updated pattern to match amount with optional comma and dot
            string previouspaymentpattern = @"Account payment\s*\(\s*\$(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)\s*\)";

            // Check if any line contains "Account payment"
            if (extractedText.Any(s => s.Contains("Account payment ")))
            {
                // Find the line with the "Account payment" text
                var previousPaymentText = extractedText.FirstOrDefault(s => s.Contains("Account payment "));

                // Extract the amount using regex
                var match = Regex.Match(previousPaymentText, previouspaymentpattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    previousPayment = match.Groups[1].Value; // Extract the amount
                }
            }






            string phonePattern = @"\b\d{4}\s\d{3}\s\d{3}\b"; // Matches phone numbers like 0800 634 283
            string megatelKeyword = "www.megatel.co.nz/m/chat";
            var customerServiceContact = string.Empty;

            bool foundKeyword = false;

            foreach (var line in extractedText)
            {
                if (foundKeyword)
                {
                    var match = Regex.Match(line, phonePattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        customerServiceContact = match.Value; // Extract the phone number
                        break; // Stop after finding the match
                    }
                }

                if (line.Contains(megatelKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    foundKeyword = true; // Set flag to look for phone number in the next lines
                }
            }








            //CURRENT BILLING AMOUNT

            var currentBillAmount = string.Empty;

            if (extractedText.Any(s => s.Contains("Total current energy charges ")))
            {
                var currentBillAmountText = extractedText.FirstOrDefault(s => s.Contains("Total current energy charges "));
                // Extract the amount after "Total charges "
                currentBillAmount = currentBillAmountText.Split("Total current energy charges ").Last().Trim();

                // Remove the '$' symbol from the amount if present
                currentBillAmount = currentBillAmount.Replace("$", string.Empty).Trim();
            }

















            // ICP

            var icp = string.Empty;
            if (extractedText.Any(s => s.Contains("Your ICP Number ")))
            {
                var icpText = extractedText.FirstOrDefault(s => s.Contains("Your ICP Number "));
                icp = icpText.Split("Your ICP Number ").Last().Trim();
            }






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






            var addressPattern = @"\d{1,4}\s\w+\s(?:Street|St|Avenue|Ave|Road|Rd|Boulevard|Blvd|Lane|Ln|Drive|Dr)\b";
            var cityPattern = @"\b[A-Z][A-Z]+\b"; // Simplified city pattern

            var regexAddress = new Regex(addressPattern, RegexOptions.IgnoreCase);
            var regexCity = new Regex(cityPattern, RegexOptions.IgnoreCase);

            string billingAddress = string.Empty;

            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i].Trim(); // Trim spaces from the line
                if (regexAddress.IsMatch(line) && i + 1 < extractedText.Count)
                {
                    var cityLine = extractedText[i + 1].Trim(); // Trim spaces from the city line
                    var postalCodeLine = i + 2 < extractedText.Count ? extractedText[i + 2].Trim() : string.Empty; // Trim spaces from the postal code line

                    if (regexCity.IsMatch(cityLine))
                    {
                        // Construct the address with single commas and spaces
                        billingAddress = $"{line}, {cityLine}, {postalCodeLine}";
                        billingAddress = billingAddress.Trim();
                        break;
                    }
                }
            }




            //BILLING PERIOD

            var billingPeriod = string.Empty;
            var billDetailsIndex = extractedText.FindIndex(s => s.Contains("period : "));

            if (billDetailsIndex != -1)
            {
                var dateRangeLine = extractedText[billDetailsIndex].Trim();

                // Use regex to match the date range pattern
                var regex = new Regex(@"(\d{1,2}\s[A-Za-z]+\s\d{2})\s*to\s*(\d{1,2}\s[A-Za-z]+\s\d{2})");
                var match = regex.Match(dateRangeLine);

                if (match.Success)
                {
                    var startDateStr = match.Groups[1].Value;
                    var endDateStr = match.Groups[2].Value;

                    if (DateTime.TryParseExact(startDateStr, "dd MMM yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) &&
                        DateTime.TryParseExact(endDateStr, "dd MMM yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                    {
                        var startFormatted = startDate.ToString("dd/MM/yyyy");
                        var endFormatted = endDate.ToString("dd/MM/yyyy");
                        billingPeriod = $"{startFormatted} to {endFormatted}";
                    }
                }
            }





            //Aspose PeriodFrom
            var dateRangePattern = @"(\d{1,2}\s[A-Za-z]+\s\d{2})\s*to\s*(\d{1,2}\s[A-Za-z]+\s\d{2})";
            var readStartDate = string.Empty;

            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, dateRangePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string startDateStr = match.Groups[1].Value;
                    if (DateTime.TryParseExact(startDateStr, "dd MMM yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                    {
                        readStartDate = startDate.ToString("dd/MM/yyyy");
                    }
                    break;
                }
            }






            var readEndDate = string.Empty;
            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, dateRangePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string endDateStr = match.Groups[2].Value;
                    if (DateTime.TryParseExact(endDateStr, "dd MMM yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                    {
                        readEndDate = endDate.ToString("dd/MM/yyyy");
                    }
                    break;
                }
            }









            //METER NUMBER

            var meterNumber = string.Empty;
            string meterNumberPattern = @"Variable Charge\s+(\w+)";
            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, meterNumberPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    meterNumber = match.Groups[1].Value;
                    break;
                }
            }




            //FIXED CHARGE QUANTITY

            string fixedChargeQuantityPattern = @"Daily Charge\s+(\d+)\s+days";
            string fixedChargeQuantity = string.Empty;
            foreach (var line in extractedText)
            {
                // Match fixed charge quantity
                var fixedChargeQuantityMatch = Regex.Match(line, fixedChargeQuantityPattern, RegexOptions.IgnoreCase);
                if (fixedChargeQuantityMatch.Success)
                {
                    fixedChargeQuantity = fixedChargeQuantityMatch.Groups[1].Value;
                }
            }






            // FIXED CHARGE RATE

            string fixedChargeRatePattern = @"\s+([\d\.]+)\s+cents/day";
            string fixedChargeRate = string.Empty;
            foreach (var line in extractedText)
            {
                // Match fixed charge rate
                var fixedChargeRateMatch = Regex.Match(line, fixedChargeRatePattern, RegexOptions.IgnoreCase);
                if (fixedChargeRateMatch.Success)
                {
                    fixedChargeRate = fixedChargeRateMatch.Groups[1].Value;
                }
            }






            // FIXED CHARGE TOTAL

            string fixedChargeTotalPattern = @"\s+\$([\d\.]+)\s+\$([\d\.]+)";
            string fixedChargeTotal = string.Empty;
            foreach (var line in extractedText)
            {
                // Match fixed charge total
                var fixedChargeTotalMatch = Regex.Match(line, fixedChargeTotalPattern, RegexOptions.IgnoreCase);
                if (fixedChargeTotalMatch.Success)
                {
                    fixedChargeTotal = fixedChargeTotalMatch.Groups[2].Value;
                }
            }






            // GST


            string gstPattern = @"GST\s*\d+%\s*\$\s*([\d\.]+)";
            string gst = string.Empty;
            foreach (var line in extractedText)
            {
                // Match GST value
                var gstMatch = Regex.Match(line, gstPattern, RegexOptions.IgnoreCase);
                if (gstMatch.Success)
                {
                    gst = gstMatch.Groups[1].Value;
                }
            }






          

            var type = string.Empty;

            // Loop through each line in the extracted text with an index
            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains "Price Plan"
                if (line.Contains("Price Plan"))
                {
                    // Check if the next line exists
                    if (i + 1 < extractedText.Count)
                    {
                        // Get the next line which contains the type
                        var typeLine = extractedText[i + 1].Trim();

                        // Split the type line to isolate the type parts
                        var parts = typeLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Assuming the type is in the first two parts of the line
                        if (parts.Length >= 2)
                        {
                            type = $"{parts[0].Trim()} {parts[1].Trim()}";
                        }
                    }
                    // Break the loop after finding and processing the line with "Price Plan"
                    break;
                }
            }







            // Previous Reading

            var previousReading = string.Empty;
            string previousReadingPattern = @"\s+(\d+)\(Estimate\)";
            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, previousReadingPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    previousReading = match.Groups[1].Value;
                    break;
                }
            }





            //Current Reading

            var currentReading = string.Empty;
            string currentReadingPattern = @"Estimate\)\s+(\d+)\(Estimate\)";
            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, currentReadingPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    currentReading = match.Groups[1].Value;
                    break;
                }
            }






            // RATE

            var rate = string.Empty;
            string ratePattern = @"(\d+\.\d+)\s*cents/kWh";
            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, ratePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    rate = match.Groups[1].Value;
                    break;
                }
            }




            // QUANTITY

            var quantity = string.Empty;
            string quantityPattern = @"(\d+\.\d+)\s*kWh";
            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, quantityPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    quantity = match.Groups[1].Value;
                    break;
                }
            }





            // TOTAL

            var total = string.Empty;

            // Pattern to capture the last dollar amount in the line
            string totalPattern = @"\$(\d+\.\d+)\s*$";

            // Flag to indicate whether to start looking for the total
            bool foundPricePlan = false;

            // Loop through each line of extracted text
            foreach (var line in extractedText)
            {
                // Check if the line contains "Price Plan" to set the flag
                if (line.Contains("Price Plan"))
                {
                    foundPricePlan = true;
                    continue; // Skip the "Price Plan" line
                }

                // If we have passed the "Price Plan" line, look for the total
                if (foundPricePlan)
                {
                    // Match the total amount in the line
                    var match = Regex.Match(line, totalPattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        total = match.Groups[1].Value;
                        break; // Exit loop once the total is found
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



            await _csvBillMapper.WriteToCsvAsync(billMetadata);
        }
    }
}