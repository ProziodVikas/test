using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO.Enumeration;
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
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace DataExtraction.Library.Mappers.MeridianMappers
{
    public class MeridianElectricityMapper : IMapper
    {
        private readonly JsonBillMapper _jsonBillMapper;
        private readonly string _jsonFilePath;
        public MeridianElectricityMapper(JsonBillMapper jsonBillMapper)
        {
            _jsonBillMapper = jsonBillMapper;
        }

        public async Task ProcessAsync(string groupedText, List<string> extractedText, string billsFolderPath)
        {
            string combinedText = string.Join(Environment.NewLine, extractedText);

            //var country = Country.AU.ToString();
            var utilityType = UtilityType.Electricity.ToString();
            var supplier = Supplier.Meridian.ToString();
            var billingCurrency = BillingCurrency.NSD.ToString();
            var templateId = Guid.NewGuid().ToString();
            int templateVersion = 1;


            string fileName = string.Empty;
            string fileExtension = string.Empty;

            // Check if the directory exists
            if (Directory.Exists(billsFolderPath))
            {
                // Get the PDF files in the directory
                var pdfFiles = Directory.GetFiles(billsFolderPath, "*.pdf");

                // Process only the first PDF file found
                if (pdfFiles.Length > 0)
                {
                    var firstFilePath = pdfFiles.First();
                    fileName = System.IO.Path.GetFileNameWithoutExtension(firstFilePath);
                    fileExtension = System.IO.Path.GetExtension(firstFilePath);
                }
            }
            else
            {
                throw new DirectoryNotFoundException($"The directory '{billsFolderPath}' does not exist.");
            }


            var customerName = string.Empty;
            if (extractedText.Any(s => s.Contains("Name ")))
            {
                var customerNameText = extractedText.FirstOrDefault(s => s.Contains("Name "));
                customerName = customerNameText.Split("Name ").Last().Trim();
            }




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



            var datePattern = @"(\d{1,2}\s[A-Za-z]+\s\d{4})";
            DateTime issueDate = default; // Use DateTime default value to represent an uninitialized date

            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, datePattern);
                if (match.Success)
                {
                    string dateStr = match.Groups[1].Value;
                    if (DateTime.TryParseExact(dateStr, "d MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        issueDate = parsedDate; // Assign the parsed date to issueDate
                        break;
                    }
                }
            }



            DateTime dueDate = default;

            if (extractedText.Any(s => s.Contains("Due Date:") || s.Contains("Due")))
            {
                // Find the text line containing "Due Date:" or "Due"
                var dueDateText = extractedText.LastOrDefault(s => s.Contains("Due Date:") || s.Contains("Due"));

                if (dueDateText != null)
                {
                    // Extract the part of the text after "Due" and trim any whitespace
                    var datePart = dueDateText.Split("Due").Last().Trim();

                    // Parse the date string into a DateTime object
                    if (DateTime.TryParse(datePart, out DateTime parsedDate))
                    {
                        dueDate = parsedDate; // Assign the parsed DateTime
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
                        // The service description should be the part before "ICP"
                        serviceDescription = parts[0].Trim();
                    }
                    break; // Exit loop after finding and processing the description
                }
            }



            string totalAmountDueString = string.Empty;

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
                            totalAmountDueString = remainingText.Substring(dollarIndex + 1).Trim(); // Skip the dollar sign
                            break;
                        }
                    }
                }
            }

            // Convert the totalAmountDueString to decimal
            decimal totalAmountDue = 0m;
            if (decimal.TryParse(totalAmountDueString, NumberStyles.Currency, CultureInfo.InvariantCulture, out var parsedAmount))
            {
                totalAmountDue = parsedAmount;
            }




            // PAYMENT METHOD

            string paymentMethod = string.Empty;
            // Check if the line contains the "ICP" keyword
            if (extractedText.Any(s => s.Contains("Payment by ")))
            {
                var paymentMethodText = extractedText.FirstOrDefault(s => s.Contains("Payment by "));
                paymentMethod = paymentMethodText.Split("Payment by ").Last().Trim();
            }




            string pattern = @"Opening account balance\s*\$([\d,]+\.\d{2})";
            string openingBalanceString = string.Empty;

            // Check if any line contains "Opening account balance"
            if (extractedText.Any(s => s.Contains("Opening account balance")))
            {
                // Find the line with the "Opening account balance" text
                var openingBalanceText = extractedText.FirstOrDefault(s => s.Contains("Opening account balance"));

                // Extract the amount using regex
                var match = Regex.Match(openingBalanceText, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    openingBalanceString = match.Groups[1].Value.Replace(",", ""); // Remove comma if present
                }
            }

            // Convert string to decimal
            decimal openingBalanceDecimal = 0m;
            if (!decimal.TryParse(openingBalanceString, NumberStyles.Currency, CultureInfo.InvariantCulture, out openingBalanceDecimal))
            {
                // Handle the case where parsing fails, if needed
                Console.WriteLine($"Failed to parse opening balance '{openingBalanceString}' as decimal.");
            }






            var previousPayment = string.Empty;
            decimal previousPaymentDecimal = 0m;

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

                    // Convert to decimal
                    if (decimal.TryParse(previousPayment, NumberStyles.Currency, CultureInfo.InvariantCulture, out previousPaymentDecimal))
                    {
                        // Successfully parsed, previousPaymentDecimal now contains the decimal value
                    }
                    else
                    {
                        // Handle parsing failure if needed
                        Console.WriteLine($"Failed to parse previous payment amount '{previousPayment}' as decimal.");
                    }
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

            decimal currentBillAmountDecimal = 0m;

            if (extractedText.Any(s => s.Contains("Total charges ")))
            {
                var currentBillAmountText = extractedText.FirstOrDefault(s => s.Contains("Total charges "));
                if (currentBillAmountText != null)
                {
                    // Extract the amount after "Total charges "
                    var amountString = currentBillAmountText.Split("Total charges ").Last().Trim();

                    // Remove the '$' symbol and any other non-numeric characters if present
                    amountString = amountString.Replace("$", string.Empty).Trim();

                    // Convert the cleaned string to decimal
                    if (!decimal.TryParse(amountString, NumberStyles.Currency, CultureInfo.InvariantCulture, out currentBillAmountDecimal))
                    {
                        // Handle parsing failure if needed
                        Console.WriteLine($"Failed to parse amount '{amountString}' as decimal.");
                    }
                }
            }







            var icp = string.Empty;
            if (extractedText.Any(s => s.Contains("ICP ")))
            {
                var icpText = extractedText.FirstOrDefault(s => s.Contains("ICP "));
                icp = icpText.Split("ICP ").Last().Trim();
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







            var dateRangePattern = @"(\d{1,2} \w+ \d{4}) to (\d{1,2} \w+ \d{4})";
            DateTime readStartDate = default;

            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, dateRangePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string startDateStr = match.Groups[1].Value;

                    if (DateTime.TryParseExact(startDateStr, "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                    {
                        readStartDate = startDate;
                    }
                    break;
                }
            }





            DateTime readEndDate = default; // Initialize as DateTime

            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, dateRangePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string endDateStr = match.Groups[2].Value;
                    if (DateTime.TryParseExact(endDateStr, "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                    {
                        readEndDate = endDate; // Assign DateTime value directly
                    }
                    break;
                }
            }




            string fixedChargeQuantityString = string.Empty;
            string fixedChargeQuantityPattern = @"Daily charge\s*\(\d+\.\d+\s*c\/day\s*x\s*(\d+)\s*days\)";

            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, fixedChargeQuantityPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    fixedChargeQuantityString = match.Groups[1].Value;
                    break;
                }
            }

            // Convert fixedChargeQuantityString to decimal
            decimal fixedChargeQuantity = 0m;
            if (!decimal.TryParse(fixedChargeQuantityString, NumberStyles.Number, CultureInfo.InvariantCulture, out fixedChargeQuantity))
            {
                // Handle the case where parsing fails if needed
                Console.WriteLine($"Failed to parse fixedChargeQuantity '{fixedChargeQuantityString}' as decimal.");
            }





            string fixedChargeRatePattern = @"Daily charge\s*\(([\d\.]+)\s*c/day";

            // Initialize variable for storing the fixed charge rate as a string
            string fixedChargeRateString = string.Empty;

            // Loop through each line of extracted text
            foreach (var line in extractedText)
            {
                // Match fixed charge rate
                var fixedChargeRateMatch = Regex.Match(line, fixedChargeRatePattern, RegexOptions.IgnoreCase);
                if (fixedChargeRateMatch.Success)
                {
                    fixedChargeRateString = fixedChargeRateMatch.Groups[1].Value;
                    break;
                }
            }

            // Convert fixed charge rate to decimal
            decimal fixedChargeRate = 0m;
            if (!decimal.TryParse(fixedChargeRateString, NumberStyles.Float, CultureInfo.InvariantCulture, out fixedChargeRate))
            {
                // Handle the case where parsing fails, if needed
                Console.WriteLine($"Failed to parse fixed charge rate '{fixedChargeRateString}' as decimal.");
            }




            string fixedChargeTotalPattern = @"Daily charge\s*\(\d+\.\d+\s*c\/day\s*x\s*\d+\s*days\)\s*\$([\d\.]+)";

            // Initialize variable
            string fixedChargeTotalString = string.Empty;
            decimal fixedChargeTotal = 0m;

            // Loop through each line of extracted text
            foreach (var line in extractedText)
            {
                var fixedChargeTotalMatch = Regex.Match(line, fixedChargeTotalPattern, RegexOptions.IgnoreCase);
                if (fixedChargeTotalMatch.Success)
                {
                    fixedChargeTotalString = fixedChargeTotalMatch.Groups[1].Value;
                    break; // Stop after finding the fixed charge total
                }
            }

            // Convert the extracted string to decimal
            if (!decimal.TryParse(fixedChargeTotalString, NumberStyles.Currency, CultureInfo.InvariantCulture, out fixedChargeTotal))
            {
                // Handle the case where parsing fails, if needed
                Console.WriteLine($"Failed to parse fixed charge total '{fixedChargeTotalString}' as decimal.");
            }







            string gstPattern = @"GST\s*@\s*\d+%\s*\$([\d\.]+)";
            string gstString = string.Empty;

            foreach (var line in extractedText)
            {
                var gstMatch = Regex.Match(line, gstPattern, RegexOptions.IgnoreCase);
                if (gstMatch.Success)
                {
                    gstString = gstMatch.Groups[1].Value;
                    break;
                }
            }

            // Convert GST to decimal
            decimal gstDecimal = 0m;
            if (!decimal.TryParse(gstString, NumberStyles.Currency, CultureInfo.InvariantCulture, out gstDecimal))
            {
                // Handle the case where parsing fails, if needed
                Console.WriteLine($"Failed to parse GST '{gstString}' as decimal.");
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






            decimal multiplier = 0m;

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
                        // Get the next line which contains the meter number and multiplier
                        var nextLine = extractedText[i + 1].Trim();

                        // Split the next line at ':' to separate the meter number from other details
                        var parts = nextLine.Split(':');

                        if (parts.Length > 1)
                        {
                            // The multiplier is part of the text after the first ':', so we need to split it again by spaces
                            var potentialMultiplier = parts[1].Trim().Split(' ').FirstOrDefault();

                            // Try to convert the extracted part to decimal
                            if (decimal.TryParse(potentialMultiplier, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericMultiplier))
                            {
                                multiplier = numericMultiplier;
                            }
                            else
                            {
                                // Handle cases where parsing fails, if needed
                                Console.WriteLine($"Failed to parse multiplier '{potentialMultiplier}' as decimal.");
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





            var rateString = string.Empty;

            // Loop through each line in the extracted text with an index
            for (int i = 0; i < extractedText.Count; i++)
            {
                var line = extractedText[i];

                // Check if the line contains "BUSINESS EVERYDAY" to locate the relevant line
                if (line.Contains("BUSINESS EVERYDAY"))
                {
                    // Check if the next line exists
                    if (i + 1 < extractedText.Count)
                    {
                        // Get the next line which contains the meter details
                        var meterLine = extractedText[i + 1].Trim();

                        // Split the meter line to isolate the rate
                        var parts = meterLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Assuming the rate is the eighth item in the line (based on the provided format)
                        if (parts.Length > 7)
                        {
                            rateString = parts[7].Trim();
                        }
                    }
                    // Break the loop after finding and processing the line with "BUSINESS EVERYDAY"
                    break;
                }
            }

            // Convert the rate string to decimal
            decimal rateDecimal = 0m;
            if (!decimal.TryParse(rateString, NumberStyles.Currency, CultureInfo.InvariantCulture, out rateDecimal))
            {
                // Handle the case where parsing fails, if needed
                Console.WriteLine($"Failed to parse rate '{rateString}' as decimal.");
            }






            string quantityPattern = @"(\d+\.?\d*)\s*(kWh)";

            // Extract quantity
            string quantity = extractedText
                .Select(line => Regex.Match(line, quantityPattern, RegexOptions.IgnoreCase))
                .FirstOrDefault(match => match.Success)?.Groups[1].Value ?? string.Empty;

            // Convert quantity to decimal
            decimal quantityDecimal = 0m;
            if (!decimal.TryParse(quantity, NumberStyles.Float, CultureInfo.InvariantCulture, out quantityDecimal))
            {
                // Optionally handle the case where parsing fails
                quantityDecimal = 0m; // Default value if parsing fails
            }





            string totalString = string.Empty;
            decimal total = 0m;

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
                            totalString = parts[parts.Length - 1].Trim();

                            // Remove the $ symbol if present
                            totalString = totalString.Replace("$", string.Empty).Trim();

                            // Attempt to convert the total string to a decimal
                            if (!decimal.TryParse(totalString, NumberStyles.Currency, CultureInfo.InvariantCulture, out total))
                            {
                                Console.WriteLine($"Failed to parse total amount: {totalString}");
                                total = 0m; // Default value in case of parse failure
                            }
                        }
                    }
                    // Break the loop after finding and processing the line with the relevant details
                    break;
                }





                var billMetadata = new BillMetadata
                {
                    //BillIdentifier = billIdentifier,
                    billingCurrency = billingCurrency,
                    billingAddress = billingAddress,
                    totalAmountDue = totalAmountDue,
                    dueDate = dueDate,
                    customerServiceContact = customerServiceContact,
                    currentBillAmount = currentBillAmountDecimal,
                    accountNumber = accountNumber,
                    invoiceNumber = invoiceNumber,
                    invoiceDate = issueDate,
                    fixedChargeTotal = fixedChargeTotal,
                    ICP = icp,
                    billingPeriod = billingPeriod,
                    gst = gstDecimal,
                    fixedChargeQuantity = fixedChargeQuantity,
                    fixedChargeRate = fixedChargeRate,
                    paymentMethods = paymentMethod,
                    previousBalance = openingBalanceDecimal,
                    previousPayment = previousPaymentDecimal,
                    meterReadEndDate = readEndDate,
                    meterReadStartDate = readStartDate,


                    metersData = new List<metersData>
                {
                    new metersData
                    {
                meterNumber = meterNumber,
                meterMultiplier = multiplier,
                type = type,
                rate = rateDecimal,
                quantity = quantityDecimal,
                total = total,
                previousReading = previousReading,
                currentReading = currentReading
                    }
                },

                    templateId = templateId,
                    templateVersion = 1,
                    utilityType = utilityType,
                    supplierName = supplier,
                    customerName = customerName,
                    fileName = fileName,
                    fileExtension = fileExtension


                };




                await _jsonBillMapper.WriteToJsonAsync(billMetadata);
            }
        }
    }
}