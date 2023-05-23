using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using log4net;

using RecruitmentTracking.Models;
using IndexDb;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace RecruitmentTracking.Controllers;

public class AuthenticatorController : Controller
{
    private readonly DataContex _db = new();
    private readonly ILog _log;
    private readonly IConfiguration _configuration;

    public AuthenticatorController(IConfiguration configuration)
    {
        _log = LogManager.GetLogger(typeof(AuthenticatorController));
        _configuration = configuration;
    }

    [HttpGet("/Login")]
    public IActionResult Login()
    {
        ViewBag.IsAuth = Request.Cookies["ActionLogin"]! != null;

        return View();
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("ActionLogin");

        TempData["success"] = "Successfully Logout";
        return RedirectToAction("Index", "Candidate");
    }

    [HttpGet("/Signup")]
    public IActionResult Signup()
    {
        ViewBag.IsAuth = Request.Cookies["ActionLogin"]! != null;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Signups(CandidateSignup request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            TempData["warning"] = "passwords you entered do not match. ";
            return Redirect("/Signup");
        }
        if (request.Password!.Length < 6 || request.Password! == null)
        {
            TempData["warning"] = "Password must be at least 6 characters";
            return Redirect("/Signup");
        }

        if (_db.Candidates!.Any(a => a.Email == request.Email))
        {
            TempData["warning"] = "Email is already in use.";
            return Redirect("/Signup");
        }

        string HassedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        Candidate objCandidate = new()
        {
            Name = request.Name,
            Email = request.Email,
            Password = HassedPassword,
        };

        _db.Candidates!.Add(objCandidate);
        await _db.SaveChangesAsync();

        TempData["success"] = "Successfully Sign Up";
        return RedirectToAction("Login");
    }

    [HttpPost]
    public async Task<IActionResult> Logins(LoginRequest request)
    {
        Candidate objCandidate = (await _db.Candidates!.FirstOrDefaultAsync(a => a.Email == request.Email))!;
        if (objCandidate != null && BCrypt.Net.BCrypt.Verify(request.Password, objCandidate.Password))
        {
            string token = CreateTokenCandidate(objCandidate!);

            CookieOptions Options = new()
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.Now.AddHours(1),
            };

            Response.Cookies.Append("ActionLogin", token, Options);

            TempData["success"] = "Successfully Login";
            return RedirectToAction("Index", "Candidate");
        }

        Admin objAdmin = (await _db.Admins!.FirstOrDefaultAsync(a => a.Email == request.Email))!;
        if (objAdmin != null && BCrypt.Net.BCrypt.Verify(request.Password, objAdmin.Password))
        {
            string token = CreateTokenAdmin(objAdmin!);

            CookieOptions Options = new()
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.Now.AddHours(1),
            };

            Response.Cookies.Append("ActionLogin", token, Options);

            TempData["success"] = "Successfully Login";
            return RedirectToAction("Index", "Admin");
        }

        TempData["warning"] = "User not found";
        return Redirect("/Login");
    }

    private string CreateTokenAdmin(Admin objAdmin)
    {
        List<Claim> claims = new(){
            new Claim(ClaimTypes.Email, objAdmin.Email!),
            new Claim(ClaimTypes.Name, objAdmin.Name!),
        };

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(
            _configuration.GetSection("AppSettings:TokenAdmin").Value!
        ));

        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha512Signature);

        JwtSecurityToken token = new(
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string CreateTokenCandidate(Candidate objCandidate)
    {
        List<Claim> claims = new(){
            new Claim(ClaimTypes.Email, objCandidate.Email!),
        };

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(
            _configuration.GetSection("AppSettings:TokenCandidate").Value!
        ));

        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha512Signature);

        JwtSecurityToken token = new(
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("admin/signup")]
    public async Task<IActionResult> SignupAdmin(Admin request)
    {
        if (_db.Admins!.Any(a => a.Email == request.Email))
        {
            return BadRequest("Email is already in use.");
        }

        string HassedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        Admin objAdmin = new()
        {
            Name = request.Name,
            Email = request.Email,
            Password = HassedPassword,
        };

        _db.Admins!.Add(objAdmin);
        await _db.SaveChangesAsync();

        return Ok(objAdmin);
    }
}
