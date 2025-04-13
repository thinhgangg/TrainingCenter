using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TrainingCenter.Models;

namespace TrainingCenter.Controllers
{
    public class StudentsController : Controller
    {
        private TrainingCenterContext db = new TrainingCenterContext();

        // GET: Students
        public ActionResult Index()
        {
            var students = db.Students
                    .ToList()
                    .OrderBy(s => s.FullName.Split(' ').Last())
                    .ToList();
            return View(students);
        }

        // GET: Students/Dashboard
        public ActionResult Dashboard()
        {
            if (Session["StudentId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int studentId = (int)Session["StudentId"];
            var student = db.Students.Find(studentId);

            if (student == null)
            {
                TempData["Message"] = "Không tìm thấy học viên.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login", "Account");
            }

            var model = new DashboardViewModel
            {
                Student = student,
                OpenCourses = db.Courses
                    .Where(c => c.StartDate > DateTime.Now)
                    .OrderBy(c => c.StartDate)
                    .Select(c => new CourseWithEnrollment
                    {
                        Course = c,
                        EnrolledCount = db.Enrollments.Count(e => e.CourseId == c.CourseId)
                    })
                    .ToList(),
                EnrolledCourses = db.Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.StudentId == studentId)
                    .Select(e => new CourseWithEnrollment
                    {
                        Course = e.Course,
                        EnrolledCount = db.Enrollments.Count(en => en.CourseId == e.CourseId)
                    })
                    .ToList()
            };

            return View(model);
        }


        // GET: Students/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // GET: Students/Create
        public ActionResult Create()
        {
            return View(new Student());
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "StudentId,FullName,Dob,PhoneNumber,Email,Username,Password")] Student student)
        {
            if (ModelState.IsValid)
            {
                if (db.Students.Any(s => s.PhoneNumber == student.PhoneNumber))
                {
                    TempData["Message"] = "Tạo thất bại: Số điện thoại đã được sử dụng.";
                    TempData["MessageType"] = "error";
                    return View(student);
                }
                if (db.Students.Any(s => s.Email == student.Email))
                {
                    TempData["Message"] = "Tạo thất bại: Email đã được sử dụng.";
                    TempData["MessageType"] = "error";
                    return View(student);
                }
                if (db.Students.Any(s => s.Username == student.Username))
                {
                    TempData["Message"] = "Tạo thất bại: Tên người dùng đã tồn tại.";
                    TempData["MessageType"] = "error";
                    return View(student);
                }

                try
                {
                    db.Students.Add(student);
                    db.SaveChanges();
                    TempData["Message"] = "Tạo học viên thành công!";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("Index"); // Chuyển hướng về Index
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Tạo thất bại: " + ex.Message;
                    TempData["MessageType"] = "error";
                    return View(student);
                }
            }

            // Lấy lỗi validation
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Message"] = "Dữ liệu không hợp lệ: " + string.Join("; ", errors);
            TempData["MessageType"] = "error";
            return View(student);
        }

        // GET: Students/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["Message"] = "Yêu cầu không hợp lệ.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            Student student = db.Students.Find(id);
            if (student == null)
            {
                TempData["Message"] = "Học viên không tồn tại.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "StudentId,FullName,Dob,PhoneNumber,Email,Username,Password")] Student student)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng Email hoặc Username (trừ học viên hiện tại)
                if (db.Students.Any(s => s.Email == student.Email && s.StudentId != student.StudentId))
                {
                    TempData["Message"] = "Cập nhật thất bại: Email đã được sử dụng.";
                    TempData["MessageType"] = "error";
                    return View(student);
                }
                if (db.Students.Any(s => s.PhoneNumber == student.PhoneNumber && s.StudentId != student.StudentId))
                {
                    TempData["Message"] = "Cập nhật thất bại: Số điện thoại đã được sử dụng.";
                    TempData["MessageType"] = "error";
                    return View(student);
                }
                if (db.Students.Any(s => s.Username == student.Username && s.StudentId != student.StudentId))
                {
                    TempData["Message"] = "Cập nhật thất bại: Tên người dùng đã tồn tại.";
                    TempData["MessageType"] = "error";
                    return View(student);
                }

                try
                {
                    db.Entry(student).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message"] = "Cập nhật học viên thành công!";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Cập nhật thất bại: " + ex.Message;
                    TempData["MessageType"] = "error";
                    return View(student);
                }
            }

            // Lấy lỗi validation
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Message"] = "Dữ liệu không hợp lệ: " + string.Join("; ", errors);
            TempData["MessageType"] = "error";
            return View(student);
        }

        // GET: Students/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["Message"] = "Yêu cầu không hợp lệ.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            Student student = db.Students.Find(id);
            if (student == null)
            {
                TempData["Message"] = "Học viên không tồn tại hoặc đã bị xóa.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Student student = db.Students.Find(id);
            if (student == null)
            {
                TempData["Message"] = "Học viên không tồn tại hoặc đã bị xóa.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            try
            {
                db.Students.Remove(student);
                db.SaveChanges();
                TempData["Message"] = "Xóa học viên thành công!";
                TempData["MessageType"] = "success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Xóa thất bại: " + (ex.InnerException?.Message ?? ex.Message);
                TempData["MessageType"] = "error";
                return View(student);
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


