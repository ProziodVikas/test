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
    public class GenesisElectricityMapper : IMapper
    {
        private readonly CsvBillMapper _csvBillMapper;

        public GenesisElectricityMapper(CsvBillMapper csvBillMapper)
        {
            _csvBillMapper = csvBillMapper;
        }

        public async Task ProcessAsync(string groupedText, List<string> extractedText, string billsFolderPath)
        {
           



        }
    }
}