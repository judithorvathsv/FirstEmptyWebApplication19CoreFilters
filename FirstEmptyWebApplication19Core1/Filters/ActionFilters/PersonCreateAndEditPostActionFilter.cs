using FirstEmptyWebApplication19Core1.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;


namespace FirstEmptyWebApplication19Core1.Filters.ActionFilters
{
    public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter
    {

        private readonly ICountriesService _countriesService;


        public PersonCreateAndEditPostActionFilter(ICountriesService countryService)
        {
            _countriesService = countryService;
        }


        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //before

            //csekkolni hogy personcontroller-e
            if (context.Controller is PersonsController personsController)
            {
                //csekkolni, hogy modelstate is valid. Ha nem, akkor ez lefut
                if (!personsController.ModelState.IsValid)
                {
                    List<CountryResponse> countries = await _countriesService.GetAllCountries();

                    personsController.ViewBag.Countries = countries.Select(temp =>
                                                 new SelectListItem() { 
                                                     Text = temp.CountryName, Value = temp.CountryID.ToString() 
                                                 });                    

                    personsController.ViewBag.Errors = personsController.ModelState.Values
                        .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();

                    //ugyanannak kell lennie mint a PersonCOntroller methodok (Create es Edit) argumentje neve: personRequest
                    var personRequest = context.ActionArguments["personRequest"];

                    context.Result = personsController.View(personRequest);  //short-circuit or skips the action
                }
                else //valid a model state
                {
                    await next(); //fut tovabb a program
                }
            }
            else //nem a PersonController fut
            {
                await next(); //fut tovabb a program
            }




            //after
            //_logger.LogInformation("In after logic of PersonsCreateAndEdit Action filter");
        }
    }
}
