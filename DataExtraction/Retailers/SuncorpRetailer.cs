
using DataExtraction.Library.Interfaces;
using DataExtraction.Library.Mappers.SuncorpMappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataExtraction.Library.Retailers
{
    public class SuncorpRetailer : IRetailer
    {
        public async Task ProcessAsync(string groupedText)
        {
            IMapper mapperInstance = null;
            if (groupedText != null)
            {
                if (groupedText.Contains("Electricity"))
                {
                    mapperInstance = new SuncorpElectricityMapper();
                    mapperInstance?.ProcessAsync(groupedText);
                }
            }
        }
    }
}