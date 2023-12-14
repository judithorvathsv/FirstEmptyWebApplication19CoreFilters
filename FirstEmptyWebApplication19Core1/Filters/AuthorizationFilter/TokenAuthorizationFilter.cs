using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FirstEmptyWebApplication19Core1.Filters.AuthorizationFilter
{
    public class TokenAuthorizationFilter : IAuthorizationFilter
    {


        public void OnAuthorization(AuthorizationFilterContext context)
        {

            //csekk all cookiest ami a browserben van, utana csekkolni hogy van-e a koztuk Auth-key cookie
            //cookie is not submitted:
            if (context.HttpContext.Request.Cookies.ContainsKey("Auth-Key") == false)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized); //return 401
                return;
            }

            //csaekkolni hogy a cookie value == A100 ?
            //cookie is submitted but not A100:
            if (context.HttpContext.Request.Cookies["Auth-Key"] != "A100")
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);       //return 401     
            }

        }
    }
}
