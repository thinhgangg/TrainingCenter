using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TrainingCenter.Models;

namespace TrainingCenter.Controllers
{
    public class CoursesController : Controller
    {
        private TrainingCenterContext db = new TrainingCenterContext();

        private bool IsAdmin()
        {
            return Session["AdminId"] != null;
        }

        private bool IsStudent()
        {
            return Session["StudentId"] != null;
        }

        // GET: Courses
        public ActionResult Index()
        {
            if (!IsAdmin())
            {
                TempData["Message"] = "Vui lòng đăng nhập với tài khoản admin.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login", "Account");
            }

            return View(db.Courses.ToList());
        }

        // GET: Courses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // GET: Courses/Create
        public ActionResult Create()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View(new Course());
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CourseId,CourseName,Instructor,StartDate,Fee,MaxStudents")] Course course)
        {
            if (ModelState.IsValid)
            {
                var existingCourse = db.Courses.FirstOrDefault(c => c.CourseName == course.CourseName && c.Instructor == course.Instructor);
                if (existingCourse != null)
                {
                    TempData["Message"] = "Tạo thất bại: Khóa học đã tồn tại.";
                    TempData["MessageType"] = "error";
                    return View(course); // Trả về form với dữ liệu hiện tại
                }
                else
                {
                    db.Courses.Add(course);
                    db.SaveChanges();
                    TempData["Message"] = "Tạo khóa học thành công!";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("Index");
                }
            }

            // Nếu ModelState không hợp lệ
            TempData["Message"] = "Dữ liệu không hợp lệ.";
            TempData["MessageType"] = "error";
            return View(course);
        }

        // GET: Courses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["Message"] = "Yêu cầu không hợp lệ.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            Course course = db.Courses.Find(id);
            if (course == null)
            {
                TempData["Message"] = "Khóa học không tồn tại.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            if (!IsAdmin())
            {
                TempData["Message"] = "Bạn không có quyền chỉnh sửa khóa học.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Courses");
            }

            string returnUrl = Request.UrlReferrer?.AbsolutePath;
            if (returnUrl != null)
            {
                if (returnUrl.Contains("/Courses/Details"))
                {
                    ViewBag.ReturnAction = "Details";
                    ViewBag.ReturnId = id;
                    TempData["ReturnAction"] = "Details";
                    TempData["ReturnId"] = id;
                }
                else
                {
                    ViewBag.ReturnAction = "Index";
                    TempData["ReturnAction"] = "Index";
                }
            }
            else
            {
                ViewBag.ReturnAction = "Index";
                TempData["ReturnAction"] = "Index";
            }

            return View(course);
        }


        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CourseId,CourseName,Instructor,StartDate,Fee,MaxStudents")] Course course)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng lặp CourseName và Instructor, ngoại trừ chính khóa học đang chỉnh sửa
                var existingCourse = db.Courses.FirstOrDefault(c => c.CourseName == course.CourseName && c.Instructor == course.Instructor && c.CourseId != course.CourseId);
                if (existingCourse != null)
                {
                    TempData["Message"] = "Cập nhật thất bại: Khóa học đã tồn tại.";
                    TempData["MessageType"] = "error";
                    return View(course);
                }

                // Cập nhật khóa học
                db.Entry(course).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Message"] = "Cập nhật khóa học thành công!";
                TempData["MessageType"] = "success";
                return View(course); // Ở lại trang Edit với dữ liệu hiện tại
            }

            // Nếu ModelState không hợp lệ
            TempData["Message"] = "Dữ liệu không hợp lệ.";
            TempData["MessageType"] = "error";
            return View(course);
        }

        // GET: Courses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["Message"] = "Yêu cầu không hợp lệ.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            Course course = db.Courses.Find(id);
            if (course == null)
            {
                TempData["Message"] = "Khóa học không tồn tại hoặc đã bị xóa.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                TempData["Message"] = "Bạn không có quyền xóa.";
                TempData["MessageType"] = "error";
                return RedirectToAction(IsStudent() ? "Dashboard" : "Login", IsStudent() ? "Students" : "Account");
            }

            Course course = db.Courses.Find(id);
            if (course == null)
            {
                TempData["Message"] = "Khóa học không tồn tại hoặc đã bị xóa.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            var hasEnrollments = db.Enrollments.Any(e => e.CourseId == id);
            if (hasEnrollments)
            {
                TempData["Message"] = "Không thể xóa vì khóa học đang được sử dụng.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            try
            {
                db.Courses.Remove(course);
                db.SaveChanges();
                TempData["Message"] = "Xóa khóa học thành công!";
                TempData["MessageType"] = "success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Xóa thất bại: " + (ex.InnerException?.Message ?? ex.Message);
                TempData["MessageType"] = "error";
                return View(course);
            }
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
