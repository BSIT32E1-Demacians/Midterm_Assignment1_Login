using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Midterm_FullStack_Assignment1_RegisterLogin.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private const int MaxLoginAttempts = 3;

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

            // Validate username length and no spaces
            if (user.Username.Length < 6 || user.Username.Contains(" "))
            {
                ModelState.AddModelError("", "Username must be at least 6 characters long and cannot contain spaces.");
                return View(user);
            }

            // Validate password complexity
            if (!IsPasswordValid(user.Password))
            {
                ModelState.AddModelError("", "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
                return View(user);
            }

            // Hash the password
            user.Password = HashPassword(user.Password);

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
        // Retrieve the user from the database
        var loggedInUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);

        if (loggedInUser != null && VerifyPassword(user.Password, loggedInUser.Password))
        {
            // Successful login
            HttpContext.Session.SetString("Username", loggedInUser.Username); // Store username in session
            HttpContext.Session.SetInt32("LoginAttempts", 0); // Reset login attempts
            return RedirectToAction("Index", "Home");
        }

        // Invalid login attempt
        var remainingAttempts = MaxLoginAttempts - IncrementLoginAttempt();
        if (remainingAttempts > 0)
            ModelState.AddModelError("", $"Invalid username or password. {remainingAttempts} attempts remaining.");
        else
            ModelState.AddModelError("", $"You have exceeded the maximum number of login attempts. Please try again later or re-register.");

        return View(user);
    }

    private int IncrementLoginAttempt()
    {
        var attempts = HttpContext.Session.GetInt32("LoginAttempts");
        attempts = attempts.HasValue ? attempts + 1 : 1;
        HttpContext.Session.SetInt32("LoginAttempts", attempts.Value);
        return attempts.Value;
    }

    private bool IsPasswordValid(string password)
    {
        // Password complexity requirements
        var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
        return passwordRegex.IsMatch(password);
    }

    private string HashPassword(string password)
    {
        // Hash the password using PBKDF2 with a random salt
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);
        return Convert.ToBase64String(hashBytes);
    }

    private bool VerifyPassword(string enteredPassword, string hashedPassword)
    {
        // Verify the entered password against the hashed password
        byte[] hashBytes = Convert.FromBase64String(hashedPassword);
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);
        var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        for (int i = 0; i < 20; i++)
        {
            if (hashBytes[i + 16] != hash[i])
                return false;
        }
        return true;
    }
}