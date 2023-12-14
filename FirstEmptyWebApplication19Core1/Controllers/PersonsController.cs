using FirstEmptyWebApplication19Core1.Filters.ActionFilters;
using FirstEmptyWebApplication19Core1.Filters.AuthorizationFilter;
using FirstEmptyWebApplication19Core1.Filters.ExceptionFilters;
using FirstEmptyWebApplication19Core1.Filters.ResourceFilters;
using FirstEmptyWebApplication19Core1.Filters.ResultFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace FirstEmptyWebApplication19Core1.Controllers
{

    [TypeFilter(typeof(HandleExceptionFilter))]

    //IOrderedFilter nelkul: (ResponseHeaderFilterben)
    /*[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "My-Key-From-Controller", "My-Value-From-Controller" }, 
        Order =2)]
    */
    //IOrderedFilter-rel: (ResponseHeaderFilterben)
    [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "My-Key-From-Controller", "My-Value-From-Controller",3 },
        Order = 3)]
    //[Route("[controller]")]   -> ez is jo! 
    [Route("persons")]
    public class PersonsController : Controller
    {

        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;


        public PersonsController(IPersonsService personsService, ICountriesService countriesService, ILogger<PersonsController> logger)
        {
            _personsService = personsService;
            _countriesService = countriesService;
            _logger = logger;
        }


        [Route("index")]
        //[Route("[action]")]   -> ez is jo! 
        [Route("/")]

        //IOrderedFilter nincs benne, es nem is akarok ordert adni neki:
        //[TypeFilter(typeof(PersonsListActionFilter))]
        //Nincs IOrderedFilter A PersonListActionFilterbe, de akarok ordert adni:
        [TypeFilter(typeof(PersonsListActionFilter), Order = 4)]


        //IOrderedFilter nelkul:
        //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Custom-Key","Custom-Value"}, Order =1)]
        //IOrderedFilter-rel:
        [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Custom-Key", "Custom-Value" , 1}, Order = 1)]


        [TypeFilter(typeof(PersonsListResultFilter))]


        public async Task<IActionResult> Index(string? searchBy, string? searchString, 
            string sortBy = nameof(PersonResponse.PersonName),SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {

            _logger.LogInformation("Index action method of PersonController");

            _logger.LogDebug($"search by: {searchString}, sortby: {sortBy}, sortOrder: {sortOrder} ");

            //search dropdown
            /*
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                 { nameof(PersonResponse.PersonName), "Person Name" },
                 { nameof(PersonResponse.Email), "Email" },
                 { nameof(PersonResponse.DateOfBirth), "Date Of Birth" },
                 { nameof(PersonResponse.Gender), "Gender" },
                 { nameof(PersonResponse.Country), "Country" },
                 { nameof(PersonResponse.Address), "Address" }
            };
            */

            //search
            List<PersonResponse> persons = await _personsService.GetFilteredPersons(searchBy, searchString);
            //ViewBag.CurrentSearchBy = searchBy;
            //ViewBag.CurrentSearchString = searchString;

            //sort
            List<PersonResponse> sortedPersons = await _personsService.GetSortedPersons(persons, sortBy, sortOrder);
            //ViewBag.CurrentSortBy = sortBy;
            //ViewBag.CurrentSortOrder = sortOrder.ToString();


            return View(sortedPersons);
        }


        [Route("create")]
        [HttpGet]
        [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "my-key", "my-value", 4 })]
        public async Task<IActionResult> Create()
        {
            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(c =>
                                                    new SelectListItem()
                                                    {
                                                        Text = c.CountryName,
                                                        Value = c.CountryID.ToString()
                                                    }
            );

            return View();
        }




        [Route("create")]
        [HttpPost]
        [TypeFilter(typeof( PersonCreateAndEditPostActionFilter))]

        [TypeFilter(typeof(FeatureDisableResourceFilter), Arguments = new object[] { false })]
        public async Task<IActionResult> Create(PersonAddRequest personRequest)
        {

            PersonResponse personResponse = await _personsService.AddPerson(personRequest);

            return RedirectToAction("Index", "Persons");
        }


        //Route-ba nem kell a method neve, eleg a [action].
        [Route("[action]")]
        public IActionResult SomeNewMethod()
        {
            return View();
        }


        [HttpGet]
        [Route("[action]/{personID}")]

        [TypeFilter(typeof(TokenResultFilter))]

        public async Task<IActionResult> Edit(Guid personID)
        {
            PersonResponse? response = await _personsService.GetPersonByPersonID(personID);

            if (response == null)
            {
                return RedirectToAction("Index");
            }

            PersonUpdateRequest personUpdateRequest = response.ToPersonUpdateRequest();
            //personUpdateRequest.Country = await _countriesService.GetCountryByCountryId(personUpdateRequest.CountryID)?.CountryName;

            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(c =>
                                                    new SelectListItem()
                                                    {
                                                        Text = c.CountryName,
                                                        Value = c.CountryID.ToString()
                                                    }
            );

            return View(personUpdateRequest);
        }


        [HttpPost]
        [Route("[action]/{personID}")]
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]

        [TypeFilter(typeof(TokenAuthorizationFilter))]

        public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)            
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personRequest.PersonID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            //ezt kivettem az if(ModelState.isValidbol):
            PersonResponse updatedPerson = await _personsService.UpdatePerson(personRequest);
            return RedirectToAction("Index");

            //ez a resz nem kell mert van PersonCreateAndEditPostActionFilter ami elvegzi ugyanezt:
            /*
            if (ModelState.IsValid)
            {
                PersonResponse updatedPerson = await _personsService.UpdatePerson(personRequest);
                return RedirectToAction("Index");
            }         
            else
            {
                List<CountryResponse> countries = await _countriesService.GetAllCountries();
                ViewBag.Countries = countries;

                ViewBag.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();

                return View(personResponse.ToPersonUpdateRequest());
            }
            */
        }


        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(Guid? personID)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }
            return View(personResponse);
        }


        [HttpPost]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            await _personsService.DeletePerson(personUpdateRequest.PersonID);

            return RedirectToAction("Index");
        }


        [Route("PersonsPDF")]
        public async Task<IActionResult> PersonsPDF()
        {
            //get list of persons
            List<PersonResponse> persons = await _personsService.GetAllPersons();

            //return view as pdf
            return new ViewAsPdf("PersonPDF", persons, ViewData) {
                PageMargins = new Rotativa.AspNetCore.Options.Margins()
                {
                    Top = 20, Right = 20, Bottom = 20, Left = 20
                },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }


        [Route("PersonsCSV")]
        public async Task<IActionResult> PersonsCSV()
        {
            MemoryStream memoryStream = await _personsService.GetPersonsCSV();

            return File(memoryStream, "application/octet-stream", "persons.csv");
        }



        [Route("PersonsExcel")]
        public async Task<IActionResult> PersonsExcel()
        {
            MemoryStream memoryStream = await _personsService.GetPersonsExcel();

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }

    }
}
