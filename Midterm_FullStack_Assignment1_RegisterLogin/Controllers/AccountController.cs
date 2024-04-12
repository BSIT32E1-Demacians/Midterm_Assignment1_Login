using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Midterm_FullStack_Assignment1_RegisterLogin.Models;
using System.Linq;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(User user)
    {
        if (ModelState.IsValid)
        {
            // Check if the username already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Username already exists. Please choose a different one.");
                return View(user);
            }
            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Login");
        }
        return View(user);
    }
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(User user)
    {
        var loggedInUser = _context.Users.FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);
        if (loggedInUser != null)
        {
            // You can implement authentication here (e.g., set a cookie)
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError("", "Invalid username or password.");
        return View(user);
    }
}
