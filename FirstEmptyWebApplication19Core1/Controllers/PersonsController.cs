using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace FirstEmptyWebApplication19Core1.Controllers
{
    //[Route("[controller]")]   -> ez is jo! 
    [Route("persons")]
    public class PersonsController : Controller
    {

        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;


        public PersonsController(IPersonsService personsService, ICountriesService countriesService)
        {
            _personsService = personsService;
            _countriesService = countriesService;
        }


        [Route("index")]
        //[Route("[action]")]   -> ez is jo! 
        [Route("/")]
        public async Task<IActionResult> Index(
            string? searchBy, string? searchString,
            string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            //search dropdown
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                 { nameof(PersonResponse.PersonName), "Person Name" },
                 { nameof(PersonResponse.Email), "Email" },
                 { nameof(PersonResponse.DateOfBirth), "Date Of Birth" },
                 { nameof(PersonResponse.Gender), "Gender" },
                 { nameof(PersonResponse.Country), "Country" },
                 { nameof(PersonResponse.Address), "Address" }
            };

            //search
            List<PersonResponse> persons = await _personsService.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            //sort
            List<PersonResponse> sortedPersons = await _personsService.GetSortedPersons(persons, sortBy, sortOrder);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder.ToString();


            return View(sortedPersons);
        }


        [Route("create")]
        [HttpGet]
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
        public async Task<IActionResult> Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = await _countriesService.GetAllCountries();
                ViewBag.Countries = countries;

                ViewBag.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();

                return View(personAddRequest);
            }

            PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);

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
        public async Task<IActionResult> Edit(PersonUpdateRequest personUpdateRequest)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                PersonResponse updatedPerson = await _personsService.UpdatePerson(personUpdateRequest);
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
