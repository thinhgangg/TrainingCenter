using PagedList;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Models;

namespace TrainingCenter.Controllers
{
    public class EnrollmentsController : Controller
    {
        private TrainingCenterContext db = new TrainingCenterContext();

        private bool IsAdmin()
        {
            return Session["AdminId"] != null;
        }

        // GET: Enrollments
        public ActionResult Index(int? page)
        {
            if (!IsAdmin())
            {
                TempData["Message"] = "Vui lòng đăng nhập với tài khoản admin.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login", "Account");
            }

            var courses = db.Enrollments
                .Include(e => e.Course)
                .Select(e => e.Course)
                .Distinct()
                .ToList();

            foreach (var course in courses)
            {
                course.EnrolledCount = db.Enrollments.Count(e => e.CourseId == course.CourseId);
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var pagedCourses = courses
                .OrderBy(c => c.StartDate)
                .ToPagedList(pageNumber, pageSize);

            return View("Index", pagedCourses);
        }

        public ActionResult StudentsByCourse(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Message"] = "Không tìm thấy khóa học.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            var enrollments = db.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.CourseId == id.Value)
                .ToList()
                .OrderBy(e => e.Student.FullName.Split(' ').Last())
                .ToList();

            if (!enrollments.Any())
            {
                ViewBag.CourseName = db.Courses.Find(id.Value)?.CourseName ?? "Không xác định";
            }
            else
            {
                ViewBag.CourseName = enrollments.FirstOrDefault()?.Course.CourseName;
            }

            ViewBag.CourseId = id.Value;

            string returnUrl = Request.UrlReferrer?.AbsolutePath;
            if (returnUrl != null)
            {
                if (returnUrl.Contains("/Courses/Details"))
                {
                    ViewBag.ReturnAction = "Details";
                    ViewBag.ReturnController = "Courses";
                    ViewBag.ReturnId = id.Value;
                }
                else
                {
                    ViewBag.ReturnAction = "Index";
                    ViewBag.ReturnController = "Enrollments";
                }
            }
            else
            {
                ViewBag.ReturnAction = "Index";
                ViewBag.ReturnController = "Enrollments";
            }


            return View(enrollments);
        }

        public ActionResult CoursesByStudent(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Message"] = "Không tìm thấy học viên.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Student");
            }

            var enrollments = db.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.StudentId == id.Value)
                .ToList()
                .OrderBy(e => e.Course.CourseName)
                .ToList();

            if (!enrollments.Any())
            {
                ViewBag.StudentName = db.Students.Find(id.Value)?.FullName ?? "Không xác định";
            }
            else
            {
                ViewBag.StudentName = enrollments.FirstOrDefault()?.Student.FullName;
            }

            ViewBag.StudentId = id.Value;
            return View(enrollments);
        }


        // GET: Enrollments/Create
        public ActionResult Create(int? courseId, string returnUrl)
        {
            var students = db.Students
                .ToList()
                .OrderBy(s => s.FullName.Split(' ').Last())
                .ThenBy(s => s.FullName)
                .ToList();
            ViewBag.StudentId = new SelectList(students, "StudentId", "FullName");
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseName", courseId);
            ViewBag.ReturnUrl = returnUrl;

            // Nếu returnUrl không hợp lệ, đặt mặc định
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("StudentsByCourse") && courseId.HasValue)
            {
                ViewBag.ReturnUrl = Url.Action("StudentsByCourse", "Enrollments", new { id = courseId.Value });
            }

            var enrollment = new Enrollment
            {
                RegisterDate = DateTime.Now,
                CourseId = courseId ?? 0
            };

            return View(enrollment);
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EnrollmentId,StudentId,CourseId")] Enrollment enrollment, string returnUrl)
        {
            enrollment.RegisterDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                // Kiểm tra xem khóa học có tồn tại không
                var course = db.Courses.Find(enrollment.CourseId);
                if (course == null)
                {
                    TempData["Message"] = "Khóa học không tồn tại.";
                    TempData["MessageType"] = "error";
                    var students = db.Students
                        .ToList()
                        .OrderBy(s => s.FullName.Split(' ').Last())
                        .ThenBy(s => s.FullName)
                        .ToList();
                    ViewBag.StudentId = new SelectList(students, "StudentId", "FullName", enrollment.StudentId);
                    ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseName", enrollment.CourseId);
                    ViewBag.ReturnUrl = returnUrl;
                    return View(enrollment);
                }

                // Kiểm tra giới hạn MaxStudents
                var registeredCount = db.Enrollments.Count(e => e.CourseId == enrollment.CourseId);
                if (course.MaxStudents.HasValue && registeredCount >= course.MaxStudents.Value)
                {
                    TempData["Message"] = $"Đăng ký thất bại: Khóa học '{course.CourseName}' đã đầy!";
                    TempData["MessageType"] = "error";
                    var students = db.Students
                        .ToList()
                        .OrderBy(s => s.FullName.Split(' ').Last())
                        .ThenBy(s => s.FullName)
                        .ToList();
                    ViewBag.StudentId = new SelectList(students, "StudentId", "FullName", enrollment.StudentId);
                    ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseName", enrollment.CourseId);
                    ViewBag.ReturnUrl = returnUrl;
                    return View(enrollment);
                }

                // Kiểm tra xem học viên đã đăng ký khóa học này chưa
                var existingEnrollment = db.Enrollments
                    .FirstOrDefault(e => e.StudentId == enrollment.StudentId && e.CourseId == enrollment.CourseId);

                if (existingEnrollment != null)
                {
                    TempData["Message"] = "Đăng ký thất bại: Học viên đã đăng ký khóa học này.";
                    TempData["MessageType"] = "error";
                    var students = db.Students
                        .ToList()
                        .OrderBy(s => s.FullName.Split(' ').Last())
                        .ThenBy(s => s.FullName)
                        .ToList();
                    ViewBag.StudentId = new SelectList(students, "StudentId", "FullName", enrollment.StudentId);
                    ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseName", enrollment.CourseId);
                    ViewBag.ReturnUrl = returnUrl;
                    return View(enrollment);
                }

                // Thêm đăng ký mới
                db.Enrollments.Add(enrollment);
                db.SaveChanges();
                TempData["Message"] = "Đăng ký thành công!";
                TempData["MessageType"] = "success";

                // Reset form
                var newEnrollment = new Enrollment
                {
                    RegisterDate = DateTime.Now,
                    CourseId = enrollment.CourseId
                };

                var studentsReset = db.Students
                    .ToList()
                    .OrderBy(s => s.FullName.Split(' ').Last())
                    .ThenBy(s => s.FullName)
                    .ToList();
                ViewBag.StudentId = new SelectList(studentsReset, "StudentId", "FullName");
                ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseName", enrollment.CourseId);
                ViewBag.ReturnUrl = returnUrl;

                return View(newEnrollment);
            }

            // Nếu ModelState không hợp lệ
            var studentsForError = db.Students
                .ToList()
                .OrderBy(s => s.FullName.Split(' ').Last())
                .ThenBy(s => s.FullName)
                .ToList();
            ViewBag.StudentId = new SelectList(studentsForError, "StudentId", "FullName", enrollment.StudentId);
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseName", enrollment.CourseId);
            ViewBag.ReturnUrl = returnUrl;
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["Message"] = "Yêu cầu không hợp lệ.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            var enrollment = db.Enrollments
                .Include("Course")
                .Include("Student")
                .FirstOrDefault(e => e.EnrollmentId == id);

            if (enrollment == null)
            {
                TempData["Message"] = "Đăng ký không tồn tại hoặc đã bị xóa.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            ViewBag.CourseId = enrollment.CourseId;
            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                TempData["Message"] = "Đăng ký không tồn tại hoặc đã bị xóa.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            int courseId = enrollment.CourseId;
            try
            {
                db.Enrollments.Remove(enrollment);
                db.SaveChanges();
                TempData["Message"] = "Xóa đăng ký thành công!";
                TempData["MessageType"] = "success";

                // Kiểm tra CourseId có tồn tại trước khi chuyển hướng
                var course = db.Courses.Find(courseId);
                if (course == null)
                {
                    TempData["Message"] = "Khóa học không tồn tại.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Index");
                }

                return RedirectToAction("StudentsByCourse", new { id = courseId });
            }
            catch (Exception)
            {
                TempData["Message"] = "Xóa thất bại: Có lỗi xảy ra.";
                TempData["MessageType"] = "error";
                return View(enrollment);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(int courseId, int studentId)
        {
            var course = db.Courses.Find(courseId);
            if (course == null)
            {
                TempData["Message"] = "Khóa học không tồn tại.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Dashboard", "Students");
            }

            var student = db.Students.Find(studentId);
            if (student == null)
            {
                TempData["Message"] = "Học viên không tồn tại.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Dashboard", "Students");
            }

            // Check if course has started
            if (course.StartDate <= DateTime.Now)
            {
                TempData["Message"] = "Đăng ký thất bại: Khóa học đã bắt đầu.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Dashboard", "Students");
            }

            // Check enrollment limit
            var enrolledCount = db.Enrollments.Count(e => e.CourseId == courseId);
            if (course.MaxStudents.HasValue && enrolledCount >= course.MaxStudents.Value)
            {
                TempData["Message"] = $"Đăng ký thất bại: Khóa học '{course.CourseName}' đã đầy.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Dashboard", "Students");
            }

            // Check if already enrolled
            var existingEnrollment = db.Enrollments.FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);
            if (existingEnrollment != null)
            {
                TempData["Message"] = "Đăng ký thất bại: Bạn đã đăng ký khóa học này.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Dashboard", "Students");
            }

            // Create new enrollment
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                RegisterDate = DateTime.Now
            };

            try
            {
                db.Enrollments.Add(enrollment);
                db.SaveChanges();
                TempData["Message"] = $"Đăng ký khóa học '{course.CourseName}' thành công!";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Đăng ký thất bại: " + ex.Message;
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("Dashboard", "Students");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelEnrollment(int courseId, int studentId)
        {
            var enrollment = db.Enrollments.FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);
            if (enrollment == null)
            {
                TempData["Message"] = "Đăng ký không tồn tại.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Dashboard", "Students");
            }

            var course = db.Courses.Find(courseId);
            if (course == null)
            {
                TempData["Message"] = "Khóa học không tồn tại.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Dashboard", "Students");
            }

            try
            {
                db.Enrollments.Remove(enrollment);
                db.SaveChanges();
                TempData["Message"] = $"Hủy đăng ký khóa học '{course.CourseName}' thành công!";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Hủy đăng ký thất bại: " + ex.Message;
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("Dashboard", "Students");
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
