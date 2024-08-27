using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataExtraction.Library.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DataExtraction.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParserBaseController : ControllerBase
    {
        private readonly IExtractPdfService _extractPdfService;

        public ParserBaseController(IExtractPdfService extractPdfService)
        {
            _extractPdfService = extractPdfService;
        }

        [HttpPost("ProcessBills")]
        public async Task<ActionResult> ProcessBills()
        {
            var incomingBillsFolderPath = "C:\\pdf\\IncomingBills";
            var parsedBillsFolderPath = "C:\\pdf\\ParsedBill";
            var unparsedBillsFolderPath = "C:\\pdf\\UnparsedBill";

            if (!Directory.Exists(incomingBillsFolderPath))
            {
                return NotFound("Incoming bills folder not found");
            }

            if (!Directory.Exists(parsedBillsFolderPath))
            {
                Directory.CreateDirectory(parsedBillsFolderPath);
            }

            if (!Directory.Exists(unparsedBillsFolderPath))
            {
                Directory.CreateDirectory(unparsedBillsFolderPath);
            }

            var files = Directory.GetFiles(incomingBillsFolderPath, "*.pdf");
            if (files.Length == 0)
            {
                return NotFound("No PDF bills found in the incoming folder");
            }

            foreach (var filePath in files)
            {
                var extractedText = await _extractPdfService.ExtractTextFromPdf(filePath, incomingBillsFolderPath);

                // Assuming extractedText is a List<string> or similar
                var customerName = GetCustomerName(extractedText);

                var targetFolder = string.IsNullOrEmpty(customerName)
                    ? unparsedBillsFolderPath
                    : parsedBillsFolderPath;

                var fileName = Path.GetFileName(filePath);
                var destinationPath = Path.Combine(targetFolder, fileName);

                // Move the file to the appropriate folder
                System.IO.File.Move(filePath, destinationPath);
            }

            return Ok("All bills processed successfully");
        }

        private string GetCustomerName(IEnumerable<string> extractedText)
        {
            string customerName = string.Empty;

            // Use LINQ to find the first line that contains "LIMITED" or "T/A"
            var customerNameLine = extractedText
                .FirstOrDefault(line => line.Contains("LIMITED", StringComparison.OrdinalIgnoreCase) || line.Contains("T/A", StringComparison.OrdinalIgnoreCase));

            // If a valid line is found, extract the customer name
            if (customerNameLine != null)
            {
                // Remove only the brackets using a regular expression
                customerNameLine = Regex.Replace(customerNameLine, @"[()]", "");

                // Split by known delimiters or words and take the first part (before "T/A")
                var splitByTA = customerNameLine.Split(new[] { "T/A" }, StringSplitOptions.None);

                if (splitByTA.Length > 0)
                {
                    // Clean and normalize the name
                    var rawName = splitByTA[0].Trim();

                    // Split by spaces, capitalize the first letter of each word, and join without spaces
                    customerName = string.Join("", rawName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                         .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
                }
            }

            return customerName;
        }
    }
}