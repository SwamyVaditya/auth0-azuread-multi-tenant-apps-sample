using System.Web.Mvc;

namespace Fabrikam.Timesheets.Mvc.Controllers
{
    [RoutePrefix("home")]
    public class HomeController : Controller
    {
        [Route("")]
        [Route("~/")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("about")]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        [Route("contact")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}