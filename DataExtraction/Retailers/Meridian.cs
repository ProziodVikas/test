using DataExtraction.Library.Interfaces;
using DataExtraction.Library.Mappers.SuncorpMappers;
using DataExtraction.Library.Services;
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
                    // Provide the path where you want to save the CSV file
                    var csvFilePath = "C:\\pdf\\Result.csv";

                    // Create an instance of CsvBillMapper
                    var csvBillMapper = new CsvBillMapper(csvFilePath);

                    // Pass the instance of CsvBillMapper to SuncorpElectricityMapper
                    mapperInstance = new MeridianElectricityMapper(csvBillMapper);

                    if (mapperInstance != null)
                    {
                        await mapperInstance.ProcessAsync(groupedText, extractedText);
                    }
                }
            }
        }
    }
}
