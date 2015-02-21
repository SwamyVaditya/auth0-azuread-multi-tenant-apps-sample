using System.Web.Mvc;

namespace Fabrikam.Timesheets.Mvc.Controllers
{
    [RoutePrefix("profile")]
    public class ProfileController : Controller
    {
        [Route("me")]
        public ActionResult Index()
        {
            return View();
        }
    }
}