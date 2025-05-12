using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QLBH_64131491.Authorization
{
    public class UserAuthorizationAttribute : TypeFilterAttribute
    {
        public UserAuthorizationAttribute() : base(typeof(UserAuthorizationFilter))
        {
        }

        private class UserAuthorizationFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var userId = context.HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                }
            }
        }
    }
} 