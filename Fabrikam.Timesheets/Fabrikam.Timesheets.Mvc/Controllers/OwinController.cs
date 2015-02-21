using System.Web;
using System.Web.Mvc;

using Microsoft.Owin.Security;

namespace Fabrikam.Timesheets.Mvc.Controllers
{
    public class OwinController : Controller
    {
        protected IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
    }
}