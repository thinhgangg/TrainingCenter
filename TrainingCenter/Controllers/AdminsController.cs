using System;
using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Models;
using TrainingCenter.Models.ViewModels;

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
                OpenCourses = db.Courses.Count(c => c.StartDate > DateTime.Now),
                ClosedCourses = db.Courses.Count(c => c.StartDate <= DateTime.Now)
            };
            return View("Dashboard", model);
        }

        // Thống kê doanh thu khóa học
        public ActionResult RevenueStatistics()
        {
            if (!IsAdmin())
            {
                TempData["Message"] = "Vui lòng đăng nhập với tài khoản admin.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login", "Account");
            }

            var data = db.Courses
                .Select(course => new RevenueStatisticViewModel
                {
                    CourseName = course.CourseName,
                    Fee = course.Fee,
                    StudentCount = course.Enrollments.Count()
                })
                .ToList();

            ViewBag.TotalRevenue = data.Sum(d => d.Revenue);

            return View(data);
        }

        // Thống kê số lượng học viên theo từng khóa học
        public ActionResult StudentCountStatistics()
        {
            if (!IsAdmin())
            {
                TempData["Message"] = "Vui lòng đăng nhập với tài khoản admin.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login", "Account");
            }

            var data = db.Courses
                .Select(course => new StudentCountStatisticViewModel
                {
                    CourseName = course.CourseName,
                    Instructor = course.Instructor,
                    StartDate = course.StartDate,
                    MaxStudents = course.MaxStudents,
                    StudentCount = course.Enrollments.Count()
                })
                .OrderByDescending(c => c.StudentCount)
                .ToList();

            return View(data);
        }

        // Lọc thống kê theo tháng/năm
        public ActionResult RevenueByMonth(int? month, int? year)
        {
            if (!IsAdmin())
            {
                TempData["Message"] = "Vui lòng đăng nhập với tài khoản admin.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login", "Account");
            }

            if (!month.HasValue)
                month = DateTime.Now.Month;

            if (!year.HasValue)
                year = DateTime.Now.Year;

            var data = db.Enrollments
                .Where(e => e.Course.StartDate.Month == month && e.Course.StartDate.Year == year)
                .GroupBy(e => e.Course)
                .Select(g => new RevenueFilterViewModel
                {
                    CourseName = g.Key.CourseName,
                    Fee = g.Key.Fee,
                    StudentCount = g.Count()
                })
                .ToList();

            ViewBag.SelectedMonth = month;
            ViewBag.SelectedYear = year;
            ViewBag.TotalRevenue = data.Sum(d => d.Revenue);

            return View(data);
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