using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Aspose.Pdf.Operators;
using DataExtraction.Library.Enums;
using DataExtraction.Library.Interfaces;
using DataExtraction.Library.Services;

namespace DataExtraction.Library.Mappers.SuncorpMappers
{
    public class SuncorpElectricityMapper : IMapper
    {
        private readonly CsvBillMapper _csvBillMapper;

        public SuncorpElectricityMapper(CsvBillMapper csvBillMapper)
        {
            _csvBillMapper = csvBillMapper;
        }

        public async Task ProcessAsync(string groupedText, List<string> extractedText, string billsFolderPath)
        {
            //string combinedText = string.Join(Environment.NewLine, extractedText);

            ////var country = Country.AU.ToString();
            ////var commodity = Commodity.Electricity.ToString();
            ////var retailerShortName = RetailerShortName.Suncorp.ToString();



            //////PdfPig AccountNumber

            ////var accountNumber = string.Empty;
            ////bool isAccountNumberPresent = combinedText.Contains("Customer No:");
            ////if (isAccountNumberPresent)
            ////{
            ////    var accountNumberText = extractedText.FirstOrDefault(s => s.StartsWith("Customer No:"));
            ////    var index = extractedText.IndexOf(accountNumberText);
            ////    accountNumber = extractedText[index + 3];
            ////}


            //////Aspose.PDF AccountNumber
            //var accountNumber = string.Empty;
            //if (extractedText.Any(s => s.Contains("Customer No:")))
            //{
            //    var accountNumberText = extractedText.FirstOrDefault(s => s.Contains("Customer No:"));
            //    accountNumber = accountNumberText.Split(":").Last().Trim();
            //}








            //////PdfPig invoiceNumber
            ////var invoiceNumber = string.Empty;
            ////bool isInvoiceNumberPresent = combinedText.Contains("Invoice No:");
            ////if (isInvoiceNumberPresent)
            ////{
            ////    var invoiceNumberText = extractedText.FirstOrDefault(s => s.Contains("Invoice No:")).Split("Invoice No:").First();
            ////    invoiceNumber = invoiceNumberText.Split(":").Last().Trim();
            ////}


            //////Aspose.PDF invoiceNumber
            //var invoiceNumber = string.Empty;
            //if (extractedText.Any(s => s.Contains("Invoice No:")))
            //{
            //    var invoiceNumberText = extractedText.FirstOrDefault(s => s.Contains("Invoice No:"));
            //    invoiceNumber = invoiceNumberText.Split("Invoice No:").Last().Trim();
            //}








            ////PdfPig address
            ////var address = string.Empty;
            ////bool isAddressPresent = combinedText.Contains("Address:");
            ////if (isAddressPresent)
            ////{
            ////    var addressText = extractedText.FirstOrDefault(s => s.StartsWith("Address:"));
            ////    var index = extractedText.IndexOf(addressText);
            ////    accountNumber = extractedText[index + 3];
            ////}













            ////Aspose Address
            //var addressPattern = @"\d{1,4}\s\w+\s(?:Street|St|Avenue|Ave|Road|Rd|Boulevard|Blvd|Lane|Ln|Drive|Dr)\b";
            //var cityPattern = @"\b[A-Z]{2,}\s+\w{2,4}\s+\d{4}\b";
            //var regexAddress = new Regex(addressPattern, RegexOptions.IgnoreCase);
            //var regexCity = new Regex(cityPattern, RegexOptions.IgnoreCase);

            //string address = string.Empty;

            //for (int i = 0; i < extractedText.Count; i++)
            //{

            //    var line = extractedText[i];
            //    if (!string.IsNullOrEmpty(line))
            //    {
            //        var trimIndex = Math.Max(line.LastIndexOf("                 "), line.LastIndexOf("                  "));
            //        line = trimIndex >= 0 ? line.Substring(0, trimIndex).Trim() : line;
            //        extractedText.Add($"Extracted line: {line}");
            //    }

            //    if (regexAddress.IsMatch(line) && i + 1 < extractedText.Count && regexCity.IsMatch(extractedText[i + 1]))
            //    {
            //        address = $"{line}\n{extractedText[i + 1]}";
            //        address = line + " " + extractedText[i + 1];
            //        break;
            //    }

            //}

            //// Remove unwanted parts by trimming after the last space or line break
            //if (!string.IsNullOrEmpty(address))
            //{
            //    var trimIndex = Math.Max(address.LastIndexOf("                 "), address.LastIndexOf("                "));
            //    address = trimIndex >= 0 ? address.Substring(0, trimIndex).Trim() : address;
            //    extractedText.Add($"Extracted Address: {address}");
            //}















            ////Aspose City
            //cityPattern = @"\b[A-Z]{2,}\s+\w{2,4}\s+\d{4}\b";
            //regexCity = new Regex(cityPattern, RegexOptions.IgnoreCase);

            //string city = string.Empty;

            //for (int i = 0; i < extractedText.Count; i++)
            //{

            //    var line = extractedText[i];
            //    if (regexCity.IsMatch(line) && i < extractedText.Count)
            //    {
            //        city = line;
            //        break;
            //    }

            //}

            //// Remove unwanted parts by trimming after the last space or line break
            //if (!string.IsNullOrEmpty(city))
            //{
            //    var trimIndex = Math.Min(city.LastIndexOf("                                    "), city.LastIndexOf("                                    "));
            //    city = trimIndex >= 0 ? city.Substring(0, trimIndex).Trim() : city;
            //    extractedText.Add($"Extracted Address: {city}");
            //}
















            ////Aspose Postcode
            //var postcodePattern = @"\w{2,4}\s+\d{4}\b";
            //var regexPostcode = new Regex(postcodePattern, RegexOptions.IgnoreCase);

            //string postcode = string.Empty;

            //for (int i = 0; i < extractedText.Count; i++)
            //{
            //    var line = extractedText[i];

            //    // Find the index of unwanted parts and trim the line
            //    var trimIndex = Math.Min(line.IndexOf("              "), line.IndexOf("             "));
            //    line = trimIndex >= 0 ? line.Substring(0, trimIndex).Trim() : line;

            //    // Check if the trimmed line matches the postcode pattern
            //    if (regexPostcode.IsMatch(line))
            //    {
            //        postcode = line;
            //        break;
            //    }
            //}

            //// Further trim the postcode to remove unwanted parts after a specific index
            //if (!string.IsNullOrEmpty(postcode))
            //{
            //    // Find the first occurrence of unwanted spaces
            //    var trimIndex = postcode.IndexOf(" ");
            //    if (trimIndex >= 0)
            //    {
            //        // Find the next occurrence of unwanted spaces
            //        var secondTrimIndex = postcode.IndexOf(" ", trimIndex + 2);
            //        if (secondTrimIndex >= 0)
            //        {
            //            postcode = postcode.Substring(secondTrimIndex + 1).Trim();
            //        }
            //    }

            //    extractedText.Add($"Extracted Address: {postcode}");
            //}



















            ////Aspose PeriodFrom
            //var dateRangePattern = @"\b([A-Za-z]{3})\s*to\s*([A-Za-z]{3})\s*(\d{4})\b";
            //DateOnly? periodFrom = null;
            //foreach (var line in extractedText)
            //{
            //    var match = Regex.Match(line, dateRangePattern, RegexOptions.IgnoreCase);
            //    if (match.Success)
            //    {
            //        string startMonth = match.Groups[1].Value;
            //        string year = match.Groups[3].Value;
            //        if (DateTime.TryParseExact($"{startMonth} {year}", "MMM yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            //        {
            //            periodFrom = DateOnly.FromDateTime(parsedDate);
            //        }
            //        break;
            //    }
            //}

            //if (periodFrom.HasValue)
            //{
            //    Console.WriteLine($"Match found: {periodFrom}");
            //}














            ////Aspose PeriodTo
            //DateOnly? periodTo = null;
            //foreach (var line in extractedText)
            //{
            //    var match = Regex.Match(line, dateRangePattern, RegexOptions.IgnoreCase);
            //    if (match.Success)
            //    {
            //        string endMonth = match.Groups[2].Value;
            //        string year = match.Groups[3].Value;
            //        if (DateTime.TryParseExact($"{endMonth} {year}","MMM yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            //        {
            //            periodTo = DateOnly.FromDateTime(parsedDate);
            //        }
            //        break;
            //    }
            //}














            //////PdfPig issueDate
            ////DateTime? issueDate = null;
            ////bool isIssueDatePresent = combinedText.Contains("Date:");
            ////if (isIssueDatePresent)
            ////{
            ////    var issueDateText = extractedText.FirstOrDefault(s => s.StartsWith("Date:"));
            ////    issueDate = Convert.ToDateTime(issueDateText.Split("Date:").Last());
            ////}


            //////Aspose.PDF issueDate
            //var issueDate = string.Empty;
            //if (extractedText.Any(s => s.Contains("Date:")))
            //{
            //    var issueDateText = extractedText.FirstOrDefault(s => s.Contains("Date:"));
            //    issueDate = Convert.ToString(issueDateText.Split("Date:").Last().Trim());
            //}









            //////PdfPig dueDate
            ////DateTime? dueDate = null;
            ////bool isDueDatePresent = combinedText.Contains("Due Date:");
            ////if (isDueDatePresent)
            ////{
            ////    var dueDateText = extractedText.LastOrDefault(s => s.Contains("Due Date:"));
            ////    dueDate = Convert.ToDateTime(dueDateText.Split("Due").First());
            ////}


            //////Aspose.PDF dueDate
            //var dueDate = string.Empty;
            //if (extractedText.Any(s => s.Contains("Due Date:")))
            //{
            //    var dueDateText = extractedText.LastOrDefault(s => s.Contains("Due Date:"));
            //    dueDate = Convert.ToString(dueDateText.Split("Due Date:").Last().Trim());
            //}










            //////PdfPig meterNumber
            //var meterNumber = string.Empty;
            //bool isMeterNumberPresent = combinedText.Contains("Metered number");
            //if (isMeterNumberPresent)
            //{
            //    var meterNumberText = extractedText.FirstOrDefault(s => s.Contains("Metered Electricity"));
            //    meterNumber = meterNumberText.Split("-").First().Trim();
            //}










            //////PdfPig 
            //var startDate = string.Empty;
            //var endDate = string.Empty;
            //bool isBillingPeriodPresent = combinedText.Contains("Metered Electricity");
            //if (isBillingPeriodPresent)
            //{
            //    startDate = issueDate;
            //    endDate = issueDate;
            //}







            ////PdfPig
            //string chargeName = "B8478 - Metered Electricity Jan to Mar 2024";
            //decimal price = 14023.07m;
            //decimal quantity = 1m;
            //string quantityUnit = "Unit";
            //string priceUnit = "/Unit";
            //decimal cost = 14023.07m;








            //var billMetadata = new BillMetadata
            //{
            //    AccountNumber = accountNumber,
            //    InvoiceNumber = invoiceNumber,
            //    //Country = country,
            //    //Commodity = commodity,
            //    //RetailerShortName = retailerShortName,
            //    //Address = address,
            //    //City = city,
            //    //Postcode = postcode,
            //    //PeriodFrom = periodFrom, 
            //    //PeriodTo = periodTo,
            //    IssueDate = issueDate,
            //    DueDate = dueDate,
            //};






            ////billMetadata.Charges.Add(new Charge
            ////{
            ////    ChargeName = chargeName,
            ////    Quantity = (int)quantity,
            ////    Price = price,
            ////    Cost = cost
            ////});








            //// Add total
            ////billMetadata.Total = new Total
            ////{
            ////    Quantity = 1,
            ////    Price = 1,
            ////    Cost = cost
            ////};

          //  await _csvBillMapper.WriteToCsvAsync(billMetadata);
        }
    }
}