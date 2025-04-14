using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Models;

namespace TrainingCenter.Controllers
{
    public class AccountController : Controller
    {
        private TrainingCenterContext db = new TrainingCenterContext();

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            // Kiểm tra bảng admins
            var admin = db.Admins.FirstOrDefault(a => a.Username == username && a.Password == password);
            if (admin != null)
            {
                Session["AdminId"] = admin.AdminId;
                TempData["Message"] = "Đăng nhập thành công.";
                TempData["MessageType"] = "success";
                return RedirectToAction("Dashboard", "Admins");
            }

            // Kiểm tra bảng students
            var student = db.Students.FirstOrDefault(s => s.Username == username && s.Password == password);
            if (student != null)
            {
                Session["StudentId"] = student.StudentId;
                TempData["Message"] = "Đăng nhập thành công.";
                TempData["MessageType"] = "success";
                return RedirectToAction("Dashboard", "Students");
            }

            TempData["Message"] = "Tên người dùng hoặc mật khẩu không đúng.";
            TempData["MessageType"] = "error";
            return View();
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            return View(new Student());
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Student student)
        {
            if (ModelState.IsValid)
            {
                if (db.Students.Any(s => s.Username == student.Username))
                {
                    TempData["Message"] = "Tên đăng nhập đã tồn tại.";
                    TempData["MessageType"] = "error";
                    return View(student);
                }
                if (db.Students.Any(s => s.Email == student.Email))
                {
                    TempData["Message"] = "Email đã được sử dụng.";
                    TempData["MessageType"] = "error";
                    return View(student);
                }
                if (db.Students.Any(s => s.PhoneNumber == student.PhoneNumber))
                {
                    TempData["Message"] = "Số điện thoại đã được sử dụng.";
                    TempData["MessageType"] = "error";
                    return View(student);
                }

                // Không mã hóa mật khẩu nữa, lưu trực tiếp
                db.Students.Add(student);
                db.SaveChanges();

                TempData["Message"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                TempData["MessageType"] = "success";
                return RedirectToAction("Login");
            }

            TempData["Message"] = "Dữ liệu không hợp lệ.";
            TempData["MessageType"] = "error";
            return View(student);
        }


        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            TempData["Message"] = "Bạn đã đăng xuất.";
            TempData["MessageType"] = "success";
            return RedirectToAction("Login");
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
