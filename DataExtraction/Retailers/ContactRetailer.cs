using DataExtraction.Library.Interfaces;
using DataExtraction.Library.Mappers;
using DataExtraction.Library.Mappers.ContactMappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DataExtraction.Library.Retailers
{
    public class ContactRetailer : IRetailer
    {
        public async Task ProcessAsync(string groupedText, List<string> extractedText, string billsFolderPath)
        {
            IMapper mapperInstance = null;
            if (groupedText != null)
            {
                if (groupedText.Contains("Electricity") || groupedText.Contains("kWh"))
                {
                    var jsonFilePath = "C:\\pdf\\Result.json";
                    var jsonBillMapper = new JsonBillMapper(jsonFilePath);

                    mapperInstance = new ContactElectricityMapper(jsonBillMapper);

                    if (mapperInstance != null)
                    {
                        await mapperInstance.ProcessAsync(groupedText, extractedText, billsFolderPath);
                    }
                }
            }
        }
    }

}

