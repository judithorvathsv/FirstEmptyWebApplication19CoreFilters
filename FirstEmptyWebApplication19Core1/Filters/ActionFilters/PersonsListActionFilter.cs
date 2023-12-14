using FirstEmptyWebApplication19Core1.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceContracts.DTO;

namespace FirstEmptyWebApplication19Core1.Filters.ActionFilters
{
    //personscontroller index methodja elott es mogott kell futnia
    public class PersonsListActionFilter : IActionFilter
    {
        private readonly ILogger<PersonsListActionFilter> _logger;

        public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
        {
            _logger = logger;
        }

        //OnActionExecuted (after index method) es OnActionExecuting (before) methodok vannak



        public void OnActionExecuted(ActionExecutedContext context)
        {
           // _logger.LogInformation("PersonsListActionFilter.OnActionExecuted method");
            _logger.LogInformation("{FilterName}.{MethodName}OnActionExecuted method",
                nameof(PersonsListActionFilter), nameof(OnActionExecuted));

            PersonsController personsController = (PersonsController) context.Controller;
            
            IDictionary<string, object?>? parameters = 
                (IDictionary<string, object?>?)context.HttpContext.Items["arguments"];

            if(parameters != null)
            {
                if (parameters.ContainsKey("searchBy"))
                {
                    personsController.ViewData["CurrentSearchBy"] = Convert.ToString(parameters["searchBy"]);
                }
                if (parameters.ContainsKey("searchString"))
                {
                    personsController.ViewData["CurrentSearchString"] = Convert.ToString(parameters["searchString"]);
                }
                if (parameters.ContainsKey("sortBy"))
                {
                    personsController.ViewData["CurrentSortBy"] = Convert.ToString(parameters["sortBy"]);
                }
                if (parameters.ContainsKey("sortOrder"))
                {
                    personsController.ViewData["CurrentSortOrder"] = Convert.ToString(parameters["sortOrder"]);
                }
            }

            personsController.ViewBag.SearchFields = new Dictionary<string, string>()
            {
                 { nameof(PersonResponse.PersonName), "Person Name" },
                 { nameof(PersonResponse.Email), "Email" },
                 { nameof(PersonResponse.DateOfBirth), "Date Of Birth" },
                 { nameof(PersonResponse.Gender), "Gender" },
                 { nameof(PersonResponse.Country), "Country" },
                 { nameof(PersonResponse.Address), "Address" }
            };

        }



        public void OnActionExecuting(ActionExecutingContext context)
        {            
            //_logger.LogInformation("PersonsListActionFilter.OnActionExecuting method");

            _logger.LogInformation("{FilterName}.{MethodName}OnActionExecuted method",
                            nameof(PersonsListActionFilter), nameof(OnActionExecuting));

            context.HttpContext.Items["arguments"] = context.ActionArguments;

            if (context.ActionArguments.ContainsKey("searchBy"))
            {
                string? searchBy = Convert.ToString(context.ActionArguments["searchBy"]);

                //validating searchBy parameter value
                if (!string.IsNullOrEmpty(searchBy))
                {
                    //csak a nevet csekkolja, a valuet nem
                    var searchByOptions = new List<string>()
                    {
                        nameof(PersonResponse.PersonName),
                        nameof(PersonResponse.Email),
                        nameof(PersonResponse.DateOfBirth),
                        nameof(PersonResponse.Gender),
                        nameof(PersonResponse.CountryID),
                        nameof(PersonResponse.Address),
                    };

                    //ha semmire se keresunk, vagy olyanra keresunk ami nincs a listaban, pl url-be beirunk
                    //akkor irja ki, hogy mire kerestunk,
                    //utana tegyen ugy, mintha a PersonName-re kerestunk volna,
                    //irja ki.
                    if (searchByOptions.Any(t => t == searchBy) == false){
                        _logger.LogInformation("searchBy actual value: {searchBy}", searchBy);
                        context.ActionArguments["searchBy"] = nameof(PersonResponse.PersonName);
                        _logger.LogInformation("searchBy updated value: {searchBy}", context.ActionArguments["searchBy"]);
                    }
                }
            }
        }
    }
}
