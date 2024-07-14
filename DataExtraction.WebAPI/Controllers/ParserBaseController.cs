using DataExtraction.Library.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataExtraction.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParserBaseController : ControllerBase
    {
        private readonly string billsFolderPath = "C:/InvoiceStore/IncomingBills";
        private readonly IExtractPdfService _extractPdfService;

        public ParserBaseController(IExtractPdfService extractPdfService) 
        {
            _extractPdfService = extractPdfService;
        }

        [HttpGet("BillParser")]
        public async Task<ActionResult> BillParser()
        {
            if (!Directory.Exists(billsFolderPath))
            {
                return NotFound("Bills folder not found");
            }
            var files = Directory.GetFiles(billsFolderPath);
            if (files.Length == 0)
            {
                return NotFound("Bills not found in the folder");
            }
            foreach(var filePath in files)
            {
                var extension = Path.GetExtension(filePath);
                if (extension == ".pdf")
                {
                    var fileName = Path.GetFileName(filePath);
                    var extractedText = await _extractPdfService.ExtractTextFromPdf(filePath);
                }
            }
            return Ok();
        }
    }
}
