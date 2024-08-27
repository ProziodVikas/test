using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DataExtraction.Library.Interfaces;

namespace DataExtraction.Library.Mappers
{
    public class JsonBillMapper : IMapper
    {
        private readonly string _apiUrlInsert = "https://api.billportal.io/api/ParseData/InsertParseData/";
        private readonly string _apiUrlUpload = "https://api.billportal.io/api/BillData/UploadParsedBills";
        private string jsonFilePath;

        public JsonBillMapper(string jsonFilePath)
        {
            this.jsonFilePath = jsonFilePath;
        }

        public Task ProcessAsync(string groupedText, List<string> extractedText, string billsFolderPath)
        {
            throw new NotImplementedException();
        }

        public async Task WriteToJsonAsync(BillMetadata billMetadata)
        {
            var jsonContent = JsonConvert.SerializeObject(billMetadata, Formatting.Indented);
            File.WriteAllText(jsonFilePath, jsonContent);
            await SendJsonToApiAsync(jsonContent, billMetadata);
        }

        private async Task SendJsonToApiAsync(string jsonContent, BillMetadata billMetadata)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(_apiUrlInsert, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to send JSON data. Status Code: {response.StatusCode}, Response: {responseContent}");
                    }
                    else
                    {
                        Console.WriteLine("JSON data sent successfully.");
                        // After successful insertion, upload parsed data
                        await UploadParsedDataAsync(billMetadata);
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
        }

        private async Task UploadParsedDataAsync(BillMetadata billMetadata)
        {
            using (var client = new HttpClient())
            {
                using (var form = new MultipartFormDataContent())
                {
                    // Ensure the correct names are used for each part, matching the API expectations
                    AddFormField(form, "accountNumber", billMetadata.accountNumber);
                    AddFormField(form, "customer", billMetadata.customerName);
                    AddFormField(form, "supplier", billMetadata.supplierName);
                    AddFormField(form, "utilityType", billMetadata.utilityType);
                    AddFormField(form, "invoiceNumber", billMetadata.invoiceNumber);
                    AddFormField(form, "invoiceDate", billMetadata.invoiceDate.ToString("yyyy-MM-dd"));

                    // Add file content as a part
                    try
                    {
                        var fileContent = new ByteArrayContent(File.ReadAllBytes(jsonFilePath));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        form.Add(fileContent, "file", Path.GetFileName(jsonFilePath));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading the file: {ex.Message}");
                        return;
                    }

                    try
                    {
                        var response = await client.PostAsync(_apiUrlUpload, form);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Failed to upload parsed bill. Status Code: {response.StatusCode}, Response: {responseContent}");
                        }
                        else
                        {
                            Console.WriteLine("Parsed bill uploaded successfully.");
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine($"Request error during upload: {e.Message}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Unexpected error: {e.Message}");
                    }
                }
            }
        }

        private void AddFormField(MultipartFormDataContent form, string fieldName, string fieldValue)
        {
            if (!string.IsNullOrEmpty(fieldValue))
            {
                form.Add(new StringContent(fieldValue), fieldName);
            }
            else
            {
                Console.WriteLine($"Warning: {fieldName} is null or empty.");
            }
        }
    }
}
