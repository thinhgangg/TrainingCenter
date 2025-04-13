using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TrainingCenter.Models;

namespace TrainingCenter.Controllers
{
    public class UsersController : Controller
    {
        private TrainingCenterContext db = new TrainingCenterContext();

        // GET: Users
        public ActionResult Index()
        {
            var users = db.Users
                    .ToList()
                    .OrderBy(s => s.FullName.Split(' ').Last())
                    .ToList();
            return View(users);
        }

        // GET: Users/Dashboard
        public ActionResult Dashboard()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);

            if (user == null)
            {
                TempData["Message"] = "Không tìm thấy học viên.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login", "Account");
            }

            var model = new DashboardViewModel
            {
                User = user,
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
                    .Where(e => e.UserId == userId)
                    .Select(e => new CourseWithEnrollment
                    {
                        Course = e.Course,
                        EnrolledCount = db.Enrollments.Count(en => en.CourseId == e.CourseId)
                    })
                    .ToList()
            };

            return View(model);
        }


        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View(new User());
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "userId,FullName,Dob,PhoneNumber,Email,Username,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(s => s.PhoneNumber == user.PhoneNumber))
                {
                    TempData["Message"] = "Tạo thất bại: Số điện thoại đã được sử dụng.";
                    TempData["MessageType"] = "error";
                    return View(user);
                }
                if (db.Users.Any(s => s.Email == user.Email))
                {
                    TempData["Message"] = "Tạo thất bại: Email đã được sử dụng.";
                    TempData["MessageType"] = "error";
                    return View(user);
                }
                if (db.Users.Any(s => s.Username == user.Username))
                {
                    TempData["Message"] = "Tạo thất bại: Tên người dùng đã tồn tại.";
                    TempData["MessageType"] = "error";
                    return View(user);
                }

                try
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    TempData["Message"] = "Tạo học viên thành công!";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("Index"); // Chuyển hướng về Index
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Tạo thất bại: " + ex.Message;
                    TempData["MessageType"] = "error";
                    return View(user);
                }
            }

            // Lấy lỗi validation
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Message"] = "Dữ liệu không hợp lệ: " + string.Join("; ", errors);
            TempData["MessageType"] = "error";
            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["Message"] = "Yêu cầu không hợp lệ.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            User user = db.Users.Find(id);
            if (user == null)
            {
                TempData["Message"] = "Học viên không tồn tại.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            // Lưu trữ URL trang trước đó
            string returnUrl = Request.UrlReferrer?.AbsolutePath;
            if (returnUrl != null)
            {
                if (returnUrl.Contains("/Users/Details"))
                {
                    ViewBag.ReturnAction = "Details";
                    ViewBag.ReturnId = id;
                    TempData["ReturnAction"] = "Details";
                    TempData["ReturnId"] = id;
                }
                else if (returnUrl.Contains("/Users/Dashboard"))
                {
                    ViewBag.ReturnAction = "Dashboard";
                    TempData["ReturnAction"] = "Dashboard";
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

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "userId,FullName,Dob,PhoneNumber,Email,Username,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng Email hoặc Username (trừ học viên hiện tại)
                if (db.Users.Any(s => s.Email == user.Email && s.UserId != user.UserId))
                {
                    TempData["Message"] = "Cập nhật thất bại: Email đã được sử dụng.";
                    TempData["MessageType"] = "error";
                    return View(user);
                }
                if (db.Users.Any(s => s.PhoneNumber == user.PhoneNumber && s.UserId != user.UserId))
                {
                    TempData["Message"] = "Cập nhật thất bại: Số điện thoại đã được sử dụng.";
                    TempData["MessageType"] = "error";
                    return View(user);
                }
                if (db.Users.Any(s => s.Username == user.Username && s.UserId != user.UserId))
                {
                    TempData["Message"] = "Cập nhật thất bại: Tên người dùng đã tồn tại.";
                    TempData["MessageType"] = "error";
                    return View(user);
                }

                try
                {
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message"] = "Cập nhật học viên thành công!";
                    TempData["MessageType"] = "success";

                    // Chuyển hướng dựa trên trang trước đó
                    string returnAction = TempData["ReturnAction"]?.ToString() ?? "Index";
                    if (returnAction == "Details")
                    {
                        int? returnId = TempData["ReturnId"] as int?;
                        return RedirectToAction("Details", new { id = returnId });
                    }
                    else if (returnAction == "Dashboard")
                    {
                        return RedirectToAction("Dashboard");
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Cập nhật thất bại: " + ex.Message;
                    TempData["MessageType"] = "error";
                    return View(user);
                }
            }

            // Lấy lỗi validation
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Message"] = "Dữ liệu không hợp lệ: " + string.Join("; ", errors);
            TempData["MessageType"] = "error";
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["Message"] = "Yêu cầu không hợp lệ.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            User user = db.Users.Find(id);
            if (user == null)
            {
                TempData["Message"] = "Học viên không tồn tại hoặc đã bị xóa.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                TempData["Message"] = "Học viên không tồn tại hoặc đã bị xóa.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            try
            {
                db.Users.Remove(user);
                db.SaveChanges();
                TempData["Message"] = "Xóa học viên thành công!";
                TempData["MessageType"] = "success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Xóa thất bại: " + (ex.InnerException?.Message ?? ex.Message);
                TempData["MessageType"] = "error";
                return View(user);
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


