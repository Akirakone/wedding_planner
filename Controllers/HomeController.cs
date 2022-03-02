using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using wedding_planner.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace wedding_planner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private MyContext _context;

        public HomeController(ILogger<HomeController> logger, MyContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {

            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.email == newUser.email))
                {
                    ModelState.AddModelError("email", "Email is already in Use");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.password = Hasher.HashPassword(newUser, newUser.password);
                _context.Add(newUser);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("LoggedIn", newUser.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("login")]
        public IActionResult Login(Login loginUser)
        {
            if (ModelState.IsValid)
            {
                User userInDb = _context.Users.FirstOrDefault(u => u.email == loginUser.lEmail);
                if (userInDb == null)
                {
                    // If it's null then they are NOT in the database
                    ModelState.AddModelError("lemail", "Invalid login attempt");
                    return View("Index");
                }
                // If we are the correct email it's time to check the password
                // First we make another instance of the password hasher
                PasswordHasher<Login> Hasher = new PasswordHasher<Login>();
                PasswordVerificationResult result = Hasher.VerifyHashedPassword(loginUser, userInDb.password, loginUser.lPassword);

                if (result == 0)
                {
                    ModelState.AddModelError("lemail", "Invalid login attempt");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("LoggedIn", userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            int? loggedIn = HttpContext.Session.GetInt32("LoggedIn");
            if (loggedIn != null)
            {
                ViewBag.LoggedIn = _context.Users.FirstOrDefault(d => d.UserId == (int)loggedIn);
                ViewBag.AllWeddings = _context.Weddings.OrderBy(a => a.CreatedAt).Include(g => g.guestlist).ToList();
                return View("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("addWedding")]
        public IActionResult AddWedding()
        {
            int? LoggedIn = HttpContext.Session.GetInt32("LoggedIn");
            if (LoggedIn == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.UserId = Convert.ToInt32(LoggedIn);
            return View();
        }

        [HttpPost("planWedding")]
        public IActionResult PlanWedding(Wedding plannedWedding)
        {
            if (ModelState.IsValid)
            {
                if (plannedWedding.dateofWedding > DateTime.Now)
                {
                    _context.Add(plannedWedding);
                    _context.SaveChanges();
                    return Redirect("Dashboard");
                }
                else
                {
                    int? LoggedIn = HttpContext.Session.GetInt32("LoggedIn");
                    ViewBag.UserId = Convert.ToInt32(LoggedIn);
                    ModelState.AddModelError("dateofWedding", "Date should be in the future.");
                    return View("AddWedding");
                }
            }
            else
            {
                return View("AddWedding");
            }
        }

        [HttpGet("editform/{weddingid}")]
        public IActionResult EditWedding(int weddingid)
        {
            if (HttpContext.Session.GetInt32("LoggedIn") == null)
            {
                return RedirectToAction("Index");
            }
            Wedding weddingEdit = _context.Weddings.FirstOrDefault(d => d.WeddingId == weddingid);
            return View(weddingEdit);
        }

        [HttpPost("update/{weddingid}")]
        public IActionResult Update(int weddingid, Wedding WeddingUpdate)
        {
            Wedding weddingtoUpdate = _context.Weddings.FirstOrDefault(d => d.WeddingId == weddingid);
            if (ModelState.IsValid)
            {
                if (HttpContext.Session.GetInt32("LoggedIn") == null)
                {
                    return RedirectToAction("Index");
                }

                if (HttpContext.Session.GetInt32("LoggedIn") != weddingtoUpdate.UserId)
                {
                    return RedirectToAction("Logout");
                }

                weddingtoUpdate.Husband = WeddingUpdate.Husband;
                weddingtoUpdate.Wife = WeddingUpdate.Wife;
                weddingtoUpdate.dateofWedding = WeddingUpdate.dateofWedding;
                weddingtoUpdate.Address = weddingtoUpdate.Address;
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("EditWedding");
            }
        }

[HttpGet("RSVP/{weddingid}/{userid}")]
        public IActionResult RSVPwedding(int weddingid, int userid)
        {
            if (HttpContext.Session.GetInt32("LoggedIn") == null)
            {
                return RedirectToAction("Index");
            }
            if ((int)HttpContext.Session.GetInt32("LoggedIn") != userid)
            {
                return RedirectToAction("Logout");
            }
           
            RSVP newRSVP = new RSVP();
            newRSVP.WeddingId = weddingid;
            newRSVP.UserId = userid;
            _context.Add(newRSVP);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

[HttpGet("unRSVP/{weddingid}/{userid}")]
        public IActionResult unRSVPwedding(int weddingid, int userid)
        {
            if (HttpContext.Session.GetInt32("LoggedIn") == null)
            {
                return RedirectToAction("Index");
            }
            if ((int)HttpContext.Session.GetInt32("LoggedIn") != userid)
            {
                return RedirectToAction("Logout");
            }
            RSVP unRSVPwedd = _context.RSVPs.FirstOrDefault(f => f.WeddingId == weddingid && f.UserId == userid);
            _context.RSVPs.Remove(unRSVPwedd);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

 [HttpGet("showOne/{weddingid}")]
        public IActionResult Showone(int weddingid, int userid)
        {
            // if (HttpContext.Session.GetInt32("LoggedIn") == null)
             int? LoggedIn = HttpContext.Session.GetInt32("LoggedIn");
            if (LoggedIn == null)
            {
                return RedirectToAction("Index");
            }
         Wedding one = _context.Weddings.Include(c => c.guestlist).ThenInclude(ti => ti.User).FirstOrDefault(fd => fd.WeddingId == weddingid);
                ViewBag.AllUsers = _context.Users.OrderBy(u => u.firstName).ToList();
          return View(one);
        }
public IActionResult Delete(int weddingid)
{
    Wedding toDelete = _context.Weddings.SingleOrDefault(f => f.WeddingId == weddingid);
    if (HttpContext.Session.GetInt32("LoggedIn") != toDelete.UserId)
    {
        return RedirectToAction("Logout");
    }
    else
    {
        _context.Weddings.Remove(toDelete);
        _context.SaveChanges();
        return RedirectToAction("Dashboard");
    }
}
[HttpGet("logout")]
public IActionResult Logout()
{
    HttpContext.Session.Clear();
    return RedirectToAction("Index");
}

        }
        

    }



