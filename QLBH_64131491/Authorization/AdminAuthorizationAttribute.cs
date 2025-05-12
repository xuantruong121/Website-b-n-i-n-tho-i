using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QLBH_64131491.Authorization
{
    public class AdminAuthorizationAttribute : TypeFilterAttribute
    {
        public AdminAuthorizationAttribute() : base(typeof(AdminAuthorizationFilter))
        {
        }

        private class AdminAuthorizationFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var isAdmin = context.HttpContext.Session.GetString("IsAdmin");
                if (string.IsNullOrEmpty(isAdmin) || isAdmin != "True")
                {
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                }
            }
        }
    }
} 