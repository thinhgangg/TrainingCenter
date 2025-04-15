using System.Web.Mvc;

namespace TrainingCenter.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}