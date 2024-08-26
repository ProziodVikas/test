using DataExtraction.Library.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataExtraction.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParserBaseController : ControllerBase
    {
        private readonly IExtractPdfService _extractPdfService;

        public ParserBaseController(IExtractPdfService extractPdfService)
        {
            _extractPdfService = extractPdfService;
        }

        [HttpPost("BillParser")]
        public async Task<ActionResult> BillParser()
        {
            var billsFolderPath = "C:\\pdf";

            if (!Directory.Exists(billsFolderPath))
            {
                return NotFound("Bills folder not found");
            }

            var files = Directory.GetFiles(billsFolderPath, "*.pdf");
            if (files.Length == 0)
            {
                return NotFound("No PDF bills found in the folder");
            }

            foreach (var filePath in files)
            {
                var extractedText = await _extractPdfService.ExtractTextFromPdf(filePath, billsFolderPath);
                var groupedText = string.Join(" ", extractedText);

            }

            return Ok("All bills processed successfully");
        }
    }
}
