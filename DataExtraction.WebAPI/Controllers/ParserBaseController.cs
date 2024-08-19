using DataExtraction.Library.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataExtraction.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParserBaseController : ControllerBase
    {
        private readonly string billsFolderPath = "C:\\pdf";
        private readonly IExtractPdfService _extractPdfService;

        public ParserBaseController(IExtractPdfService extractPdfService)
        {
            _extractPdfService = extractPdfService;
        }

        [HttpGet("BillParser")]
        public async Task<ActionResult> BillParser()
        {
            if (!Directory.Exists(billsFolderPath))
            {
                return NotFound("Bills folder not found");
            }

            var files = Directory.GetFiles(billsFolderPath, "*.pdf");
            if (files.Length == 0)
            {
                return NotFound("No PDF bills found in the folder");
            }

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                var extractedText = await _extractPdfService.ExtractTextFromPdf(filePath);

                // Map the extracted text to the BillMetadata model
                var billMetadata = MapExtractedTextToBillMetadata(extractedText);

                // Serialize the bill metadata to JSON
                var json = JsonSerializer.Serialize(billMetadata);

                // Log the JSON payload for debugging
                Console.WriteLine("Sending JSON Payload: " + json);

                // Send the JSON data to the external API
                var response = await SendJsonToApiAsync(json);

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Log detailed error message
                    Console.WriteLine($"Failed to insert data for {fileName}. Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}, Content: {responseContent}");
                    return StatusCode((int)response.StatusCode, $"Failed to insert data for {fileName}. Details: {responseContent}");
                }
            }

            return Ok("All bills processed successfully");
        }

        private BillMetadata MapExtractedTextToBillMetadata(List<string> extractedText)
        {
            var billMetadata = new BillMetadata
            {
                supplierName = ExtractFieldValue(extractedText, "Supplier:"),
                accountNumber = ExtractFieldValue(extractedText, "Account Number:"),
                invoiceNumber = ExtractFieldValue(extractedText, "Invoice Number:"),
                invoiceDate = ExtractFieldValue(extractedText, "Issue Date:"),
                dueDate = ExtractFieldValue(extractedText, "Due Date:"),
                totalAmountDue = ExtractFieldValue(extractedText, "Total Amount Due:"),
                paymentMethod = ExtractFieldValue(extractedText, "Payment Method:"),
                openingBalance = ExtractFieldValue(extractedText, "Opening Balance:"),
                previousPayment = ExtractFieldValue(extractedText, "Previous Payment:"),
                customerServiceContact = ExtractFieldValue(extractedText, "Customer Service Contact:"),
                currentBillAmount = ExtractFieldValue(extractedText, "Current Bill Amount:"),
                discountAmount = ExtractFieldValue(extractedText, "Discount Amount:"),
                ICPS = ExtractICPs(extractedText) // Ensure this is correctly populated
            };

            // Log the bill metadata for debugging
            Console.WriteLine("BillMetadata: " + JsonSerializer.Serialize(billMetadata));

            return billMetadata;
        }

        private string ExtractFieldValue(List<string> lines, string fieldName)
        {
            var matchingLine = lines.FirstOrDefault(line => line.Contains(fieldName));
            return matchingLine != null ? matchingLine.Substring(matchingLine.IndexOf(fieldName) + fieldName.Length).Trim() : string.Empty;
        }

        private string ExtractFieldValueFromLine(string line, string fieldName)
        {
            if (line.Contains(fieldName))
            {
                var startIndex = line.IndexOf(fieldName) + fieldName.Length;
                return line.Substring(startIndex).Trim();
            }
            return string.Empty;
        }

        private List<ICP> ExtractICPs(List<string> lines)
        {
            var icps = new List<ICP>();

            // Example parsing logic
            foreach (var line in lines)
            {
                if (line.Contains("ICP"))
                {
                    var icp = new ICP
                    {
                        utilityType = ExtractFieldValueFromLine(line, "UtilityType"),
                        ICPCode = ExtractFieldValueFromLine(line, "ICPCode"),
                        serviceDescription = ExtractFieldValueFromLine(line, "ServiceDescription"),
                        billingAddress = ExtractFieldValueFromLine(line, "BillingAddress"),
                        billingPeriod = ExtractFieldValueFromLine(line, "BillingPeriod"),
                        meterReadStartDate = ExtractFieldValueFromLine(line, "ReadStartDate"),
                        meterReadEndDate = ExtractFieldValueFromLine(line, "ReadEndDate"),
                        Meters = ExtractMeters(line) // Implement ExtractMeters to parse meters
                    };

                    icps.Add(icp);
                }
            }

            return icps;
        }

        private List<Meter> ExtractMeters(string line)
        {
            var meters = new List<Meter>();

            // Example logic to extract meters
            // Parse meters based on the structure of your lines

            return meters;
        }

        private async Task<HttpResponseMessage> SendJsonToApiAsync(string json)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;

                try
                {
                    Console.WriteLine("Sending JSON Payload: " + json); // Debugging

                    response = await client.PostAsync("https://api.billportal.io/api/ParseData/InsertParseData", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to insert data. Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}, Content: {responseContent}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                }

                return response;
            }
        }
    }
}
