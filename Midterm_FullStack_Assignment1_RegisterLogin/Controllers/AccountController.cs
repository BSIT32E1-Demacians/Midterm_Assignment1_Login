using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Midterm_FullStack_Assignment1_RegisterLogin.Models;
using Domain;
using Service;
using Repository;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class AccountController : Controller
{
    private readonly IUserService _userService;
    private const int MaxLoginAttempts = 3;

    public AccountController(IUserService service)
    {
        _userService = service;
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
            var existingUser = _userService.GetByUsername(user.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Username already exists. Please choose a different one.");
                return View(user);
            }

            if (user.Username.Length < 6 || user.Username.Contains(" "))
            {
                ModelState.AddModelError("", "Username must be at least 6 characters long and cannot contain spaces.");
                return View(user);
            }

            if (!IsPasswordValid(user.Password))
            {
                ModelState.AddModelError("", "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
                return View(user);
            }

            user.Password = HashPassword(user.Password);

            _userService.Create(user); // Assuming CreateUser method exists in your ToDoService
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
        var loggedInUser = _userService.GetByUsername(user.Username);

        if (loggedInUser != null && VerifyPassword(user.Password, loggedInUser.Password))
        {
            HttpContext.Session.SetString("Username", loggedInUser.Username);
            HttpContext.Session.SetInt32("LoginAttempts", 0);
            return RedirectToAction("Index", "Home");
        }

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
}