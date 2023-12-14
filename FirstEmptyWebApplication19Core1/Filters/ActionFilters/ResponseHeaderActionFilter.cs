using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FirstEmptyWebApplication19Core1.Filters.ActionFilters
{
    //Sync methodba:

    /*
 public class ResponseHeaderActionFilter : IActionFilter, IOrderedFilter
 {
     ILogger<ResponseHeaderActionFilter> _logger;
     private readonly string Key;
     private readonly string Value;

     public int Order { get; set; }


     public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger, string key, string value, int order)
     {
         _logger = logger;
         Key = key;
         Value = value;
         Order = order;
     }

     //after
     public void OnActionExecuted(ActionExecutedContext context)
     {
         _logger.LogInformation("{FilterName}.{MethodName} method", nameof(ResponseHeaderActionFilter), nameof(OnActionExecuted));
         context.HttpContext.Response.Headers[Key] = Value;
     }


     //before
     public void OnActionExecuting(ActionExecutingContext context)
     {
         _logger.LogInformation("{FilterName}.{MethodName} method", nameof(ResponseHeaderActionFilter), nameof(OnActionExecuting));
     }
  
}
  */

    //Async methodba:
    public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
    {
        ILogger<ResponseHeaderActionFilter> _logger;
        private readonly string _key;
        private readonly string _value;

        public int Order { get; set; }


        public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger, string key, string value, int order)
        {
            _logger = logger;
            _key = key;
            _value = value;
            Order = order;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //before
            _logger.LogInformation("{FilterName}.{MethodName} before method", 
                nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));

            await next();

            //after
            _logger.LogInformation("{FilterName}.{MethodName} after method", 
                nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));
            context.HttpContext.Response.Headers[_key] = _value;
        }



    }
}
