using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace FirstEmptyWebApplication19Core1.Controllers
{
    [Route("[controller]")]
    public class CountriesController : Controller
    {

        private readonly ICountriesService _countriesService;

        public CountriesController(ICountriesService countriesService)
        {
            _countriesService = countriesService;
        }


        [Route("UpploadFromExcel")]
        public IActionResult UpploadFromExcel()
        {
            return View();
        }

        //excelfile: u.a.-nak kell lennie mint a view-ban az input name.
        [HttpPost]
        [Route("UpploadFromExcel")]
        public async Task<IActionResult> UpploadFromExcel(IFormFile excelfile)
        {
            if(excelfile == null || excelfile.Length == 0)
            {
                ViewBag.ErrorMessage = "Please select an xlsx file";
                return View();
            }

            if(!Path.GetExtension(excelfile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = "Unsupported file. 'xlsx' file is expected";
                return View();
            }

            int countriesCountInserted = await _countriesService.UploadCountriesFromExcelFile(excelfile);

            ViewBag.Message = $"{countriesCountInserted} Countries Uploaded";
            return View();
        }
    }
}
