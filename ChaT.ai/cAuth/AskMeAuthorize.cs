using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChaT.ai.cAuth
{
    public class AskMeAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool disableAuthentication = false;

            #if DEBUG
                        //disableAuthentication = true;
            #endif

            if (disableAuthentication)
                return true;

            if (httpContext.Session["loginsuccess"] != null)
            {
                if (httpContext.Session["loginsuccess"].ToString() == "yes")
                {
                    return true;
                }
            }
            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new ViewResult { ViewName = "~/Views/Shared/Unauthorized.cshtml"};
            }
            else
            {

            }
        }

    }
}