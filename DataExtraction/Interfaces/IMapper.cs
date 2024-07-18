using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataExtraction.Library.Interfaces
{
    public interface IMapper
    {
        Task ProcessAsync(string groupedText);
    }
}
