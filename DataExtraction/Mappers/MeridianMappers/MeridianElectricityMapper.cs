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

namespace DataExtraction.Library.Mappers.SuncorpMappers
{
    public class MeridianElectricityMapper : IMapper
    {
        private readonly CsvBillMapper _csvBillMapper;

        public MeridianElectricityMapper(CsvBillMapper csvBillMapper)
        {
            _csvBillMapper = csvBillMapper;
        }

        public async Task ProcessAsync(string groupedText, List<string> extractedText)
        {
            string combinedText = string.Join(Environment.NewLine, extractedText);

            //var country = Country.AU.ToString();
            //var commodity = Commodity.Electricity.ToString();
            //var retailerShortName = RetailerShortName.Suncorp.ToString();







            //var billIdentifier = BillIdentifier.ICP.ToString();
            //if (extractedText.Any(s => s.Contains("Customer No:")))
            //{
            //    var billIdentifierText = extractedText.FirstOrDefault(s => s.Contains("Customer No:"));
            //    billIdentifier = billIdentifierText.Split(":").Last().Trim();
            //}









            ////Aspose.PDF AccountNumber
            var accountNumber = string.Empty;
            if (extractedText.Any(s => s.Contains("Customer No:") || s.Contains("Customer Number:") || s.Contains("Account number ")))
            {
                var accountNumberText = extractedText.FirstOrDefault(s => s.Contains("Customer No:") || s.Contains("Customer Number:") || s.Contains("Account number "));
                if (accountNumberText != null)
                {
                    // Determine the keyword to split on
                    var keyword = new[] { "Customer No:", "Customer Number:", "Account number " }
                                  .FirstOrDefault(k => accountNumberText.Contains(k));

                    if (keyword != null)
                    {
                        // Split the text at the keyword and get the part after it
                        var parts = accountNumberText.Split(new[] { keyword }, StringSplitOptions.None);
                        if (parts.Length > 1)
                        {
                            // Extract the account number (the text after the keyword) and trim any remaining parts
                            accountNumber = new string(parts[1].Trim().TakeWhile(char.IsDigit).ToArray());

                            // Optionally, update the line to only contain the account number
                            var index = extractedText.IndexOf(accountNumberText);
                            if (index >= 0)
                            {
                                extractedText[index] = $"{keyword} {accountNumber}";
                            }
                        }
                    }
                }
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














            var billingPeriod = string.Empty;
            var billDetailsIndex = extractedText.FindIndex(s => s.Contains("Your bill details"));

            if (billDetailsIndex != -1 && billDetailsIndex + 1 < extractedText.Count)
            {
                var dateRangeLine = extractedText[billDetailsIndex + 1].Trim();

                // Use regex to match the date range pattern
                var regex = new Regex(@"(\d{1,2} \w+ \d{4}) to (\d{1,2} \w+ \d{4})");
                var match = regex.Match(dateRangeLine);

                if (match.Success)
                {
                    var startDateStr = match.Groups[1].Value;
                    var endDateStr = match.Groups[2].Value;

                    if (DateTime.TryParseExact(startDateStr, "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) &&
                        DateTime.TryParseExact(endDateStr, "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                    {
                        var startFormatted = startDate.ToString("dd/MM/yyyy");
                        var endFormatted = endDate.ToString("dd/MM/yyyy");
                        billingPeriod = $"{startFormatted} to {endFormatted}";
                    }
                }
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




















            string totalAmountDue = string.Empty;

            // Check each line for the phrase "Total amount due"
            foreach (var line in extractedText)
            {
                if (line.Contains("Total amount due", StringComparison.OrdinalIgnoreCase))
                {
                    // Split the line at "Total amount due" and take the second part if it exists
                    var parts = line.Split(new string[] { "Total amount due" }, StringSplitOptions.None);
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
           // Check if the line contains the "ICP" keyword
                if (extractedText.Any(s => s.Contains("Payment by ")))
                {
                    var paymentMethodText = extractedText.FirstOrDefault(s => s.Contains("Payment by "));
                paymentMethod = paymentMethodText.Split("Payment by ").Last().Trim();
                }

    
















            var openingBalance = string.Empty;
            string pattern = @"Opening account balance\s*\$([\d,]+\.\d{2})";

            // Check if any line contains "Opening account balance"
            if (extractedText.Any(s => s.Contains("Opening account balance")))
            {
                // Find the line with the "Opening account balance" text
                var openingBalanceText = extractedText.FirstOrDefault(s => s.Contains("Opening account balance"));

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














            string phonePattern = @"(\b\d{4}\s\d{3}\s\d{3}\b)"; // Matches phone numbers like 0800 496 777

            // Initialize the customer service contact variable
            var customerServiceContact = string.Empty;

            // Find the line containing the customer service contact
            foreach (var line in extractedText)
            {
                // Match phone number
                var match = Regex.Match(line, phonePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    customerServiceContact = match.Groups[1].Value; // Extract the phone number
                    break; // Stop after finding the first match
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











            //CURRENT BILLING AMOUNT

            var currentBillAmount = string.Empty;

            if (extractedText.Any(s => s.Contains("Total charges ")))
            {
                var currentBillAmountText = extractedText.FirstOrDefault(s => s.Contains("Total charges "));
                // Extract the amount after "Total charges "
                currentBillAmount = currentBillAmountText.Split("Total charges ").Last().Trim();

                // Remove the '$' symbol from the amount if present
                currentBillAmount = currentBillAmount.Replace("$", string.Empty).Trim();
            }

















            var icp = string.Empty;
            if (extractedText.Any(s => s.Contains("ICP ")))
            {
                var icpText = extractedText.FirstOrDefault(s => s.Contains("ICP "));
                icp = icpText.Split("ICP ").Last().Trim();
            }













            //Aspose PeriodFrom
            var dateRangePattern = @"(\d{1,2}\s[A-Za-z]+\s\d{4})\s*to\s*(\d{1,2}\s[A-Za-z]+\s\d{4})";
            var readStartDate = string.Empty;

            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, dateRangePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string startDateStr = match.Groups[1].Value;
           
                    if (DateTime.TryParseExact(startDateStr, "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        readStartDate = parsedDate.ToString("dd/MM/yyyy");
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
                    if (DateTime.TryParseExact(endDateStr, "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        readEndDate = parsedDate.ToString("dd/MM/yyyy");
                    }
                    break;
                }
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





















            ////Aspose.PDF issueDate
            var datePattern = @"(\d{1,2}\s[A-Za-z]+\s\d{4})";
            DateTime? issueDate = null;

            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, datePattern);
                if (match.Success)
                {
                    string dateStr = match.Groups[1].Value;
                    if (DateTime.TryParseExact(dateStr, "d MMMM yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    {
                        issueDate = parsedDate;
                        break;
                    }
                }
            }

            









            ////Aspose.PDF dueDate
            DateTime? dueDate = null;
            if (extractedText.Any(s => s.Contains("Due Date:")|| s.Contains("Due")))
            {
                var dueDateText = extractedText.LastOrDefault(s => s.Contains("Due Date:")|| s.Contains("Due"));
                dueDate = Convert.ToDateTime(dueDateText.Split("Due").Last().Trim());
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








































            ////PdfPig 
            //DateTime? startDate = null;
            //DateTime? endDate = null;
            //bool isBillingPeriodPresent = combinedText.Contains("Metered Electricity");
            //if (isBillingPeriodPresent)
            //{
            //    startDate = issueDate;
            //    endDate = issueDate;
            //}







            //PdfPig
            //string chargeName = "B8478 - Metered Electricity Jan to Mar 2024";
            //decimal price = 14023.07m;
            ////decimal quantity = 1m;
            //string quantityUnit = "Unit";
            //string priceUnit = "/Unit";
            //decimal cost = 14023.07m;








            var billMetadata = new BillMetadata
            {
                //BillIdentifier = billIdentifier,
                AccountNumber = accountNumber,
                InvoiceNumber = invoiceNumber,
                BillingPeriod = billingPeriod,
                ServiceDescription = serviceDescription,
                TotalAmountDue = totalAmountDue,
                PaymentMethod = paymentMethod,
                OpeningBalance = openingBalance,
                PreviousPayment = previousPayment,
                CustomerServiceContact = customerServiceContact,
                BillingAddress = billingAddress,
                CurrentBillAmount = currentBillAmount,
                IssueDate = issueDate,
                DueDate = dueDate,
                ICP = icp,
                ReadStartDate = readStartDate,
                ReadEndDate = readEndDate,
                FixedChargeQuantity = fixedChargeQuantity,
                FixedChargeRate = fixedChargeRate,
                FixedChargeTotal = fixedChargeTotal,
                GST = gst,
                MeterNumber = meterNumber,
                Multiplier = multiplier,
                Type = type,
                PreviousReading = previousReading,
                CurrentReading = currentReading,
                Rate = rate,
                Quantity = quantity,
                Total = total

            };






            //billMetadata.Charges.Add(new Charge
            //{
            //    ICP = icp,
            //    ReadStartDate = readStartDate,
            //    ReadEndDate = readEndDate,
            //    FixedChargeQuantity = fixedChargeQuantity,
            //    FixedChargeRate = fixedChargeRate,
            //    FixedChargeTotal = fixedChargeTotal,
            //    GST = gst,
            //});








            // Add total
            //billMetadata.Finaltotal.Add(new FinalTotal
            //{
            //    MeterNumber = meterNumber,
            //    Multiplier = multiplier,
            //    Type = type,
            //    PreviousReading = previousReading,
            //    CurrentReading = currentReading,
            //    Rate = rate,
            //    Quantity = quantity,
            //    Total = total
            //});

        await _csvBillMapper.WriteToCsvAsync(billMetadata);
        }
    }
}