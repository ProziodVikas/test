using DataExtraction.Library.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DataExtraction.Library.Mappers.MeridianMappers
{
    public class JsonBillMapper : IMapper
    {
        private readonly string _jsonFilePath;

        public JsonBillMapper(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath;
        }

        public async Task WriteToJsonAsync(BillMetadata billMetadata)
        {
            var jsonContent = JsonConvert.SerializeObject(billMetadata, Formatting.Indented);
            await File.WriteAllTextAsync(_jsonFilePath, jsonContent);
        }

        public async Task ProcessAsync(string groupedText, List<string> extractedText, string billsFolderPath)
        {
            // Create an object to hold the data
            var billData = new
            {
                GroupedText = groupedText,
                ExtractedText = extractedText
            };

            // Serialize the object to JSON
           // var jsonContent = JsonConvert.SerializeObject(billData, Formatting.Indented);

            var jsonContent = JsonConvert.SerializeObject(billData, Formatting.Indented);
            await File.WriteAllTextAsync(_jsonFilePath, jsonContent);
            // Write the JSON content to the file
           // await File.WriteAllTextAsync(_jsonFilePath, jsonContent);
        }
    }
}
