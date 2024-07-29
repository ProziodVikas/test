using DataExtraction.Library.Interfaces;
using DataExtraction.Library.Mappers.AglMappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataExtraction.Library.Retailers
{
    public class AglRetailer : IRetailer
    {
        public async Task ProcessAsync(string groupedText, List<string> extractedText)
        {
            IMapper mapperInstance = null;
            if (groupedText != null)
            {
                if (groupedText.Contains("Electricity"))
                {
                    mapperInstance = new AglElectricityMapper();
                    mapperInstance?.ProcessAsync(groupedText, extractedText);
                }
                else
                {

                }
            }
        }
    }
}