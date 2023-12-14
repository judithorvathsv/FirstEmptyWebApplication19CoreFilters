using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FirstEmptyWebApplication19Core1.Filters.ResourceFilters
{
    public class FeatureDisableResourceFilter : IAsyncResourceFilter
    {

        private readonly ILogger<FeatureDisableResourceFilter> _logger;
        private readonly bool _isDisabled;

        public FeatureDisableResourceFilter(ILogger<FeatureDisableResourceFilter> logger, bool isDisabled=false)
        {
            _logger = logger;
            _isDisabled = isDisabled;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {

            //before logic
            _logger.LogInformation("{FilterName}.{MethodName} - before", nameof(FeatureDisableResourceFilter), nameof(OnResourceExecutionAsync));

            if (_isDisabled)
            {
                //ezt akkor hasznaljuk, ha veglegesen nem fog a create method mukodni tovabb
                //context.Result = new NotFoundResult(); //404 - Not found

                //ezt akkor hasznaljuk, ha temporary nem mukodik a create method
                context.Result = new StatusCodeResult(501); //501 - Not implemented
            }
            else
            {
                await next();
            }           
            

            //after logic:
            _logger.LogInformation("{FilterName}.{MethodName} - after", nameof(FeatureDisableResourceFilter), nameof(OnResourceExecutionAsync));
        }
    }
}
