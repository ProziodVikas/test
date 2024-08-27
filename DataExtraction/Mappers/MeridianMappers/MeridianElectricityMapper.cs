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
using System.Diagnostics.Metrics;

namespace DataExtraction.Library.Mappers.MeridianMappers
{
    public class MeridianElectricityMapper : IMapper
    {
        private readonly JsonBillMapper _jsonBillMapper;
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
            var billingCurrency = BillingCurrency.NZD.ToString();
            var templateId = Guid.NewGuid().ToString();
            int templateVersion = 1;


           




            string customerName = string.Empty;
            // Assuming the customer name follows a specific known structure
            var potentialCustomerNameIndex = extractedText.FindIndex(line =>
                line.Contains("LIMITED") || line.Contains("T/A"));

            // If a valid index is found, extract the customer name
            if (potentialCustomerNameIndex != -1)
            {
                var customerNameLine = extractedText[potentialCustomerNameIndex];

                customerNameLine = Regex.Replace(customerNameLine, @"[()]", "");
                // Split by known delimiters or words and take the first part (before "T/A")
                var splitByTA = customerNameLine.Split(new[] { "T/A" }, StringSplitOptions.None);

                if (splitByTA.Length > 0)
                {
                    // Clean and normalize the name
                    var rawName = splitByTA[0].Trim();

                    // Split by spaces, capitalize first letter of each word, and join without spaces
                    customerName = string.Join("", rawName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                         .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
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
                        billingAddress = line;  // Start capturing the address
                    }
                    // If we have started capturing the address, continue until we match the city pattern
                    else if (addressStartFound)
                    {
                        billingAddress += " " + line;  // Append the line with a space
                        if (Regex.IsMatch(line, cityPattern, RegexOptions.IgnoreCase))
                        {
                            break;  // Stop once the city pattern is matched
                        }
                    }
                }
            }
            // Trim any leading or trailing whitespace
            billingAddress = billingAddress.Trim();



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






            DateOnly dueDate = default;

            if (extractedText.Any(s => s.Contains("Due Date:") || s.Contains("Due")))
            {
                // Find the text line containing "Due Date:" or "Due"
                var dueDateText = extractedText.LastOrDefault(s => s.Contains("Due Date:") || s.Contains("Due"));

                if (dueDateText != null)
                {
                    // Extract the part of the text after "Due" and trim any whitespace
                    var datePart = dueDateText.Split("Due").Last().Trim();

                    // Parse the date string into a DateTime object
                    if (DateTime.TryParse(datePart, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {

                        dueDate = DateOnly.FromDateTime(parsedDate);
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







            //CURRENT BILLING AMOUNT

            string currentBillAmount = string.Empty;

            if (extractedText.Any(s => s.Contains("Total charges ")))
            {
                var currentBillAmountText = extractedText.FirstOrDefault(s => s.Contains("Total charges "));
                if (currentBillAmountText != null)
                {
                    // Extract the amount after "Total charges "
                    var amountString = currentBillAmountText.Split("Total charges ").Last().Trim();

                    // Remove the '$' symbol and any other non-numeric characters if present
                    amountString = amountString.Replace("$", string.Empty).Trim();

                    // Store the cleaned string as it is
                    currentBillAmount = amountString;
                }
            }





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



            DateOnly invoiceDate = default;
            var datePattern = @"\b\d{1,2}\s[A-Za-z]+\s\d{4}\b";

            // Define multiple date formats to match various possible representations of the date
            string[] dateFormats = { "d MMM yyyy", "dd MMM yyyy", "d MMMM yyyy", "dd MMMM yyyy" };

            foreach (var line in extractedText)
            {
                // Use regex to find a date pattern in the line
                var match = Regex.Match(line, datePattern);

                if (match.Success)
                {
                    string dateStr = match.Value; // Directly use the matched value

                    // Attempt to parse the date string with any of the specified date formats
                    if (DateTime.TryParseExact(dateStr, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        // Create a DateOnly instance from the parsed DateTime
                        invoiceDate = DateOnly.FromDateTime(parsedDate);

                        break; // Exit the loop once a valid date is found
                    }
                }
            }



            string fixedChargeTotalPattern = @"Daily charge\s*\(\d+\.\d+\s*c\/day\s*x\s*\d+\s*days\)\s*\$([\d\.]+)";

            // Initialize variable
            string fixedChargeTotal = string.Empty;

            // Loop through each line of extracted text
            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, fixedChargeTotalPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    fixedChargeTotal = match.Groups[1].Value;
                    break; // Stop after finding the fixed charge total
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





            string gstPattern = @"GST\s*@\s*\d+%\s*\$([\d\.]+)";
            string gst = string.Empty;

            foreach (var line in extractedText)
            {
                var match = Regex.Match(line, gstPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    gst = match.Groups[1].Value;
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

            // Initialize variable for storing the fixed charge rate as a string
            string fixedChargeRate = string.Empty;

            // Loop through each line of extracted text
            foreach (var line in extractedText)
            {
                // Match fixed charge rate
                var match = Regex.Match(line, fixedChargeRatePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    fixedChargeRate = match.Groups[1].Value;
                    break;
                }
            }



            // PAYMENT METHOD
            string paymentMethod = string.Empty;

            // Check if the line contains either "Payment by" or "payment method to a" keyword
            if (extractedText.Any(s => s.Contains("Payment by ")) || extractedText.Any(s => s.Contains("payment method to a ")))
            {
                var paymentMethodText = extractedText.FirstOrDefault(s => s.Contains("Payment by ") || s.Contains("payment method to a "));

                if (paymentMethodText != null)
                {
                    // Adjusting extraction logic based on the keyword
                    if (paymentMethodText.Contains("Payment by "))
                    {
                        paymentMethod = paymentMethodText.Split(new string[] { "Payment by " }, StringSplitOptions.None).Last().Trim();
                    }
                    else if (paymentMethodText.Contains("payment method to a "))
                    {
                        paymentMethod = paymentMethodText.Split(new string[] { "payment method to a " }, StringSplitOptions.None).Last().Trim();
                    }

                    // Remove any trailing special characters such as '-'
                    paymentMethod = paymentMethod.TrimEnd('-', '.'); // Add more characters if needed
                }
            }




            string pattern = @"Opening account balance\s*\$([\d,]+\.\d{2})";
            string openingBalance = string.Empty;

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
            string previousPaymentPattern = @"Account payment\s*\(\s*\$(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)\s*\)";

            // Check if any line contains "Account payment"
            if (extractedText.Any(s => s.Contains("Account payment ")))
            {
                // Find the line with the "Account payment" text
                var previousPaymentText = extractedText.FirstOrDefault(s => s.Contains("Account payment "));

                // Extract the amount using regex
                var match = Regex.Match(previousPaymentText, previousPaymentPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    previousPayment = match.Groups[1].Value; // Extract the amount

                    // No need to convert to decimal, just handle the string
                    // previousPayment now contains the extracted amount as a string
                }
            }







            var dateRangePattern = @"(\d{1,2} \w+ \d{4}) to (\d{1,2} \w+ \d{4})";
            DateOnly readStartDate = default;

            string[] startDateFormats = { "d MMM yyyy", "dd MMM yyyy", "d MMMM yyyy", "dd MMMM yyyy" };

            foreach (var line in extractedText)
            {
                // Use regex to find a date pattern in the line
                var match = Regex.Match(line, dateRangePattern);

                if (match.Success)
                {
                    string startDateStr = match.Groups[1].Value; // Directly use the matched value

                    // Attempt to parse the date string with any of the specified date formats
                    if (DateTime.TryParseExact(startDateStr, startDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        readStartDate = DateOnly.FromDateTime(parsedDate);

                        break; // Exit the loop once a valid date is found
                    }
                }
            }





            DateOnly readEndDate = default; // Initialize as DateTime

            string[] endDateFormats = { "d MMM yyyy", "dd MMM yyyy", "d MMMM yyyy", "dd MMMM yyyy" };

            foreach (var line in extractedText)
            {
                // Use regex to find a date pattern in the line
                var match = Regex.Match(line, dateRangePattern);

                if (match.Success)
                {
                    string endDateStr = match.Groups[2].Value; // Directly use the matched value

                    // Attempt to parse the date string with any of the specified date formats
                    if (DateTime.TryParseExact(endDateStr, endDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        readEndDate = DateOnly.FromDateTime(parsedDate);
                        break; // Exit the loop once a valid date is found
                    }
                }
            }


            var meters = new List<metersData>();
            metersData currentMeter = null;


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
                    
                    if (currentMeter != null)
                    {
                        meters.Add(currentMeter);
                    }

                    // Start a new meter
                    currentMeter = new metersData();
                    currentMeter.meterTypes = new List<meterType>();

                    // Get the meter number
                    if (i + 1 < extractedText.Count)
                    {
                        var nextLine = extractedText[i + 1].Trim();
                        var meterNumberParts = nextLine.Split(':');
                        if (meterNumberParts.Length > 0)
                        {
                            currentMeter.meterNumber = meterNumberParts[0].Trim();
                        }
                    }
                }


                var type = string.Empty;

                if (currentMeter != null && line.Contains(":1")) // Pattern for detecting types
                {
                    var parts = line.Split(':');
                    if (parts.Length > 1)
                    {
                        var details = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (details.Length > 0)
                        {
                            var newType = new meterType();



                            if (details.Length > 1)
                            {
                                newType.type = details[1].Trim();
                            }


                            for (int k = 0; k < extractedText.Count; k++)
                            {
                                line = extractedText[k];

                                // Check if the line contains "Previous Reading" to locate the meter details
                                if (line.Contains("Previous Reading"))
                                {
                                    // Check if the next line exists
                                    if (k + 1 < extractedText.Count)
                                    {
                                        // Get the next line which contains the meter details
                                        var meterLine = extractedText[k + 1].Trim();

                                        // Split the meter line at ':' to separate the meter number from other details
                                        parts = meterLine.Split(':');

                                        if (parts.Length > 1)
                                        {
                                            // Split the part after ':' to isolate the type (assumed to be the first word after ':1')
                                            details = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                            if (details.Length > 1)
                                            {
                                                // Assuming the type is the second element after the split
                                                newType.type = details[1].Trim();
                                            }
                                        }
                                    }
                                    // Break the loop after finding and processing the line with "Previous Reading"
                                    break;
                                }
                            }





                            string meterMultiplier = null;

                            // Loop through each line in the extracted text with an index
                            for (int k = 0; k < extractedText.Count; k++)
                            {
                                line = extractedText[i];

                                // Check if the line contains "Previous Reading" which indicates the presence of the meter number
                                if (line.Contains("Previous Reading"))
                                {
                                    // Check if the next line exists
                                    if (k + 1 < extractedText.Count)
                                    {
                                        // Get the next line which contains the meter number and multiplier
                                        var nextLine = extractedText[k + 1].Trim();

                                        // Split the next line at ':' to separate the meter number from other details
                                        parts = nextLine.Split(':');

                                        if (parts.Length > 1)
                                        {
                                            // The multiplier is part of the text after the first ':', so we need to split it again by spaces
                                            newType.meterMultiplier = parts[1].Trim().Split(' ').FirstOrDefault();
                                        }
                                    }
                                    // Break the loop after finding and processing the line with "Previous Reading"
                                    break;
                                }
                            }




                            var rate = string.Empty;

                            // Loop through each line in the extracted text with an index
                            for (int k = 0; k < extractedText.Count; k++)
                            {
                                line = extractedText[k];

                                // Check if the line contains "BUSINESS EVERYDAY" to locate the relevant line
                                if (line.Contains("BUSINESS EVERYDAY"))
                                {
                                    // Check if the next line exists
                                    if (k + 1 < extractedText.Count)
                                    {
                                        // Get the next line which contains the meter details
                                        var meterLine = extractedText[k + 1].Trim();

                                        // Split the meter line to isolate the rate
                                        parts = meterLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        // Assuming the rate is the eighth item in the line (based on the provided format)
                                        if (parts.Length > 7)
                                        {
                                            newType.rate = parts[7].Trim();
                                        }
                                    }
                                    // Break the loop after finding and processing the line with "BUSINESS EVERYDAY"
                                    break;
                                }
                            }







                            string quantityPattern = @"(\d+\.?\d*)\s*(kWh)";
                            var quantity = string.Empty;
                            // Extract quantity
                            newType.quantity = extractedText
                                .Select(line => Regex.Match(line, quantityPattern, RegexOptions.IgnoreCase))
                                .FirstOrDefault(match => match.Success)?.Groups[1].Value ?? string.Empty;




                            string total = string.Empty;

                            // Loop through each line in the extracted text with an index
                            for (int k = 0; k < extractedText.Count; k++)
                            {
                                line = extractedText[k];

                                // Check if the line contains a keyword or pattern related to the total amount
                                if (line.Contains("BUSINESS EVERYDAY"))
                                {
                                    // Check if the next line exists
                                    if (k + 1 < extractedText.Count)
                                    {
                                        // Get the next line which contains the details including the total amount
                                        var detailsLine = extractedText[k + 1].Trim();

                                        // Split the line to isolate different parts
                                        parts = detailsLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        // Assuming the total amount is the last item in the line
                                        if (parts.Length > 0)
                                        {
                                            // Extract the total amount part (it may include the $ symbol)
                                            total = parts[parts.Length - 1].Trim();

                                            // Remove the $ symbol if present
                                            newType.total = total.Replace("$", string.Empty).Trim();
                                        }
                                    }
                                    // Break the loop after finding and processing the line with the relevant details
                                    break;
                                }
                            }





                            var previousReading = string.Empty;

                            // Loop through each line in the extracted text with an index
                            for (int k = 0; k < extractedText.Count; k++)
                            {
                                line = extractedText[k];

                                // Check if the line contains "Previous Reading" to locate the relevant line
                                if (line.Contains("Previous Reading"))
                                {
                                    // Check if the next line exists
                                    if (k + 1 < extractedText.Count)
                                    {
                                        // Get the next line which contains the meter details
                                        var meterLine = extractedText[k + 1].Trim();

                                        // Split the meter line to isolate the previous reading
                                        parts = meterLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        // Assuming the previous reading is the third item in the line (based on the provided format)
                                        if (parts.Length > 2)
                                        {
                                            newType.previousReading = parts[2].Trim();
                                        }
                                    }
                                    // Break the loop after finding and processing the line with "Previous Reading"
                                    break;
                                }
                            }







                            var currentReading = string.Empty;

                            // Loop through each line in the extracted text with an index
                            for (int k = 0; k < extractedText.Count; k++)
                            {
                                line = extractedText[k];

                                // Check if the line contains "Previous Reading" to locate the relevant line
                                if (line.Contains("Previous Reading"))
                                {
                                    // Check if the next line exists
                                    if (k + 1 < extractedText.Count)
                                    {
                                        // Get the next line which contains the meter details
                                        var meterLine = extractedText[k + 1].Trim();

                                        // Split the meter line to isolate the previous reading
                                        parts = meterLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        // Assuming the previous reading is the third item in the line (based on the provided format)
                                        if (parts.Length > 3)
                                        {
                                            newType.currentReading = parts[3].Trim();
                                        }
                                    }
                                    // Break the loop after finding and processing the line with "Previous Reading"
                                    break;
                                };

                            }

                            currentMeter.meterTypes.Add(newType);
                        }
                    }
                }
            }

            // Add the last meter if it exists
            if (currentMeter != null)
            {
                meters.Add(currentMeter);
            }


            string fileName = string.Empty;
            string fileExtension = string.Empty;
            string newFileName = string.Empty; // Declare newFileName outside the if block

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

                    // Construct the new filename using string interpolation
                    newFileName = $"{customerName}_{supplier}_{accountNumber}_{utilityType}_{invoiceNumber}_{invoiceDate}{fileExtension}";
                    fileName = newFileName; // Update fileName if necessary

                    // Combine with the directory path to create the full path
                    string newFilePath = System.IO.Path.Combine(billsFolderPath, newFileName);

                    // Optionally, rename the file (uncomment the line below to actually rename)
                    // File.Move(firstFilePath, newFilePath);

                    // Output the new file name
                    Console.WriteLine($"New file name: {newFileName}");
                }
            }
            else
            {
                throw new DirectoryNotFoundException($"The directory '{billsFolderPath}' does not exist.");
            }



            var billMetadata = new BillMetadata
            {
                billingCurrency = billingCurrency,
                billingAddress = billingAddress,
                totalAmountDue = totalAmountDue,
                dueDate = dueDate,
                customerServiceContact = customerServiceContact,
                currentBillAmount = currentBillAmount,
                accountNumber = accountNumber,
                invoiceNumber = invoiceNumber,
                invoiceDate = invoiceDate,
                fixedChargeTotal = fixedChargeTotal,
                ICP = icp,
                billingPeriod = billingPeriod,
                gst = gst,
                fixedChargeQuantity = fixedChargeQuantity,
                fixedChargeRate = fixedChargeRate,
                paymentMethods = paymentMethod,
                previousBalance = openingBalance,
                previousPayment = previousPayment,
                meterReadEndDate = readEndDate,
                meterReadStartDate = readStartDate,
                metersData = meters, // Use the adjusted list with multiple meter types
                templateId = templateId,
                templateVersion = templateVersion,
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
