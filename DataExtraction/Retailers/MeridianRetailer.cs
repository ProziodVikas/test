using DataExtraction.Library.Interfaces;
using DataExtraction.Library.Mappers.MeridianMappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataExtraction.Library.Retailers
{
    public class MeridianRetailer : IRetailer
    {
        public async Task ProcessAsync(string groupedText, List<string> extractedText)
        {
            IMapper mapperInstance = null;
            if (groupedText != null)
            {
                if (groupedText.Contains("Electricity") || groupedText.Contains("kWh"))
                {
                    // Provide the path where you want to save the JSON file
                    var jsonFilePath = "C:\\pdf\\Result.json";

                    // Create an instance of JsonBillMapper
                    var jsonBillMapper = new JsonBillMapper(jsonFilePath);

                    // Pass the instance of JsonBillMapper to MeridianElectricityMapper
                    mapperInstance = new MeridianElectricityMapper(jsonBillMapper);

                    if (mapperInstance != null)
                    {
                        await mapperInstance.ProcessAsync(groupedText, extractedText);
                    }
                }
            }
        }
    }
}
