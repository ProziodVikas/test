﻿using DataExtraction.Library.Interfaces;
using DataExtraction.Library.Mappers.SuncorpMappers;
using DataExtraction.Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataExtraction.Library.Retailers
{
    public class SuncorpRetailer : IRetailer
    {
        public async Task ProcessAsync(string groupedText, List<string> extractedText, string billsFolderPath)
        {
            IMapper mapperInstance = null;
            if (groupedText != null)
            {
                if (groupedText.Contains("Electricity"))
                {
                    // Provide the path where you want to save the CSV file
                    var csvFilePath = "C:\\pdf\\Result.csv";

                    // Create an instance of CsvBillMapper
                    var csvBillMapper = new CsvBillMapper(csvFilePath);

                    // Pass the instance of CsvBillMapper to SuncorpElectricityMapper
                    mapperInstance = new SuncorpElectricityMapper(csvBillMapper);

                    if (mapperInstance != null)
                    {
                        await mapperInstance.ProcessAsync(groupedText, extractedText, billsFolderPath);
                    }
                }
            }
        }
    }
}
