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
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["Message"] = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                TempData["MessageType"] = "error";
                return View();
            }

            // Tìm kiếm người dùng trong cơ sở dữ liệu
            var user = db.Students.FirstOrDefault(s => s.Username == username && s.Password == password);

            if (user != null)
            {
                // Lưu thông tin người dùng vào session
                Session["Username"] = user.Username;
                Session["FullName"] = user.FullName;
                Session["StudentId"] = user.StudentId;

                TempData["Message"] = "Đăng nhập thành công!";
                TempData["MessageType"] = "success";

                // Chuyển hướng đến dashboard của học viên
                return RedirectToAction("Dashboard", "Students");
            }

            TempData["Message"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
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
    }
}
