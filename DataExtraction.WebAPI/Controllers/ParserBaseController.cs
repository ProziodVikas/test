using Aspose.Pdf.AI;
using DataExtraction.Library.Enums;
using DataExtraction.Library.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        [HttpPost("BillParser")]
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
                var fileExtension = Path.GetFileName(filePath);
                var extractedText = await _extractPdfService.ExtractTextFromPdf(filePath, billsFolderPath);

                // Map the extracted text to the BillMetadata model
                var billMetadata = MapExtractedTextToBillMetadata(extractedText, fileName, fileExtension);

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

        private BillMetadata MapExtractedTextToBillMetadata(List<string> extractedText, string fileName, string fileExtension)
        {
            var billMetadata = new BillMetadata
            {
                billingCurrency = ExtractFieldValue(extractedText, "billingCurrency:"),
                billingAddress = ExtractFieldValue(extractedText, "billingAddress"),
                totalAmountDue = ConvertToDecimal(ExtractFieldValue(extractedText, "totalAmountDue")),
                dueDate = ConvertToDate(ExtractFieldValue(extractedText, "dueDate")),
                customerServiceContact = ExtractFieldValue(extractedText, "customerServiceContact"),
                currentBillAmount = ConvertToDecimal(ExtractFieldValue(extractedText, "currentBillAmount")),
                accountNumber = ExtractFieldValue(extractedText, "accountNumber"),
                invoiceNumber = ExtractFieldValue(extractedText, "invoiceNumber"),
                invoiceDate = ConvertToDate(ExtractFieldValue(extractedText, "invoiceDate")),
                fixedChargeTotal = ConvertToDecimal(ExtractFieldValue(extractedText, "fixedChargeTotal")),
                ICP = ExtractFieldValue(extractedText, "ICP"),
                billingPeriod = ExtractFieldValue(extractedText, "billingPeriod"),
                gst = ConvertToDecimal(ExtractFieldValue(extractedText, "gst")),
                fixedChargeQuantity = ConvertToDecimal(ExtractFieldValue(extractedText, "fixedChargeQuantity")),
                fixedChargeRate = ConvertToDecimal(ExtractFieldValue(extractedText, "fixedChargeRate")),
                paymentMethods = ExtractFieldValue(extractedText, "paymentMethods"),
                previousBalance = ConvertToDecimal(ExtractFieldValue(extractedText, "previousBalance")),
                previousPayment = ConvertToDecimal(ExtractFieldValue(extractedText, "previousPayment")),
                meterReadEndDate = ConvertToDate(ExtractFieldValue(extractedText, "meterReadEndDate")),
                meterReadStartDate = ConvertToDate(ExtractFieldValue(extractedText, "meterReadStartDate")),
                metersData = ExtractMeters(extractedText),
                templateId = Guid.NewGuid().ToString(),
                templateVersion = 1,
                utilityType = ExtractFieldValue(extractedText, "utilityType"),
                supplierName = ExtractFieldValue(extractedText, "supplierName"),
                customerName = ExtractFieldValue(extractedText, "customerName"),
                fileName = fileName,
                fileExtension = fileExtension
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


        private decimal ConvertToDecimal(string value)
        {
            if (decimal.TryParse(value, NumberStyles.Currency, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return 0m;
        }

        private DateTime ConvertToDate(string value)
        {
            if (DateTime.TryParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return default;
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
        private List<metersData> ExtractMeters(List<string> lines)
        {
            var meters = new List<metersData>();
            foreach (var line in lines.Where(line => line.Contains("meter")))
            {
                var metersData = new metersData
                    {

                        meterNumber = ExtractFieldValueFromLine(line, "meterNumber"),
                        meterMultiplier = ConvertToDecimal(ExtractFieldValueFromLine(line, "meterMultiplier")),
                        type = ExtractFieldValueFromLine(line, "type"),
                        rate = ConvertToDecimal(ExtractFieldValueFromLine(line, "rate")),
                        quantity = ConvertToDecimal(ExtractFieldValueFromLine(line, "quantity")),
                        total = ConvertToDecimal(ExtractFieldValueFromLine(line, "total")),
                        previousReading = ExtractFieldValueFromLine(line, "previousReading"),
                        currentReading = ExtractFieldValueFromLine(line, "currentReading")

                    };
                    meters.Add(metersData);
                }
            
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
