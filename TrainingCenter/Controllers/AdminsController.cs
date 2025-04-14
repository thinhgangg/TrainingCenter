using System;
using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Models;

namespace TrainingCenter.Controllers
{
    public class AdminsController : Controller
    {
        private TrainingCenterContext db = new TrainingCenterContext();

        private bool IsAdmin()
        {
            return Session["AdminId"] != null;
        }

        // GET: Admins/Dashboard
        public ActionResult Dashboard()
        {
            if (!IsAdmin())
            {
                TempData["Message"] = "Vui lòng đăng nhập với tài khoản admin.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login", "Account");
            }

            var model = new AdminDashboardViewModel
            {
                StudentCount = db.Students.Count(),
                CourseCount = db.Courses.Count(),
                EnrollmentCount = db.Enrollments.Count(),
                OpenCourses = db.Courses.Count(c => c.StartDate > DateTime.Now)
            };
            return View("Dashboard", model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}