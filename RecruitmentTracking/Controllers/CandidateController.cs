using Microsoft.AspNetCore.Mvc;

using RecruitmentTracking.Models;
using RecruitmentTracking.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace RecruitmentTracking.Controllers;

[Authorize]
public class CandidateController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _server;
    private readonly UserManager<User> _userManager;

    public CandidateController(ILogger<HomeController> logger, IWebHostEnvironment server, IConfiguration configuration, ApplicationDbContext context, UserManager<User> userManager)
    {
        _logger = logger;
        _server = server;
        _configuration = configuration;
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("/Profile")]
    public async Task<IActionResult> Profile()
    {
        User user = (await _userManager.GetUserAsync(User))!;

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        Candidate objCandidate = (await _context.Candidates!.FirstOrDefaultAsync(c => c.UserId == user.Id))!;

        if (objCandidate == null)
        {
            Candidate newCandidate = new()
            {
                User = user,
            };
            _context.Candidates?.Add(newCandidate);
            await _context.SaveChangesAsync();

            UserEditProfile newProfile = new UserEditProfile()
            {
                Name = user.Name,
                Email = user.Name,
                Phone = newCandidate.Phone,
                LastEducation = newCandidate.LastEducation,
                GPA = newCandidate.GPA,
            };

            return View(newProfile);
        }

        UserEditProfile profile = new UserEditProfile()
        {
            Name = user.Name,
            Phone = objCandidate.Phone,
            LastEducation = objCandidate.LastEducation,
            GPA = objCandidate.GPA,
        };

        return View(profile);
    }

    [HttpPost("/EditProfile")]
    public async Task<IActionResult> EditProfile(UserEditProfile profile)
    {
        User user = (await _userManager.GetUserAsync(User))!;

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        Candidate candidate = (await _context.Candidates!.FirstOrDefaultAsync(c => c.UserId == user.Id))!;
        user.Name = profile.Name;
        candidate.Phone = profile.Phone;
        candidate.LastEducation = profile.LastEducation;
        candidate.GPA = profile.GPA;

        await _context.SaveChangesAsync();

        TempData["success"] = "Successfully Update Profile";
        return Redirect("/");
    }


    [HttpGet("/Home/ApplyJob/{id}")]
    public async Task<IActionResult> ApplyJob(int id)
    {
        User user = (await _userManager.GetUserAsync(User))!;

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        Job objJob = (await _context.Jobs!.FindAsync(id))!;

        ViewBag.JobId = id;

        Candidate objCandidate = _context.Candidates!.FirstOrDefault(c => c.UserId == user.Id)!;

        UserEditProfile data = new()
        {
            Name = user.Name,
            Phone = objCandidate.Phone,
            LastEducation = objCandidate.LastEducation,
            GPA = objCandidate.GPA,
        };

        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> ApplyJobs(int JobId, UserEditProfile updateCandidate)
    {

        if (updateCandidate.FileCV?.Length < 0)
        {
            TempData["warning"] = "Please select a CV file";
            return Redirect($"/Home/ApplyJob/{JobId}");
        }

        User user = (await _userManager.GetUserAsync(User))!;

        UserJob UJ = (await _context.UserJobs!.FirstOrDefaultAsync(cj => cj.JobId == JobId && cj.UserId == user.Id))!;
        if (UJ != null)
        {
            TempData["error"] = "You have applied for the job";
            return Redirect($"/Home/ApplyJob/{JobId}");
        }

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateCandidate.FileCV!.FileName);

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data", "DataCV");

        Directory.CreateDirectory(uploadsFolder);

        string filePath = Path.Combine(uploadsFolder, fileName);

        Candidate objCandidate = (await _context.Candidates!.FirstOrDefaultAsync(c => c.UserId == user.Id))!;
        user.Name = updateCandidate.Name;
        objCandidate.Phone = updateCandidate.Phone;
        objCandidate.LastEducation = updateCandidate.LastEducation;
        objCandidate.GPA = updateCandidate.GPA;
        objCandidate.Salary = updateCandidate.Salary;

        Job objJob = (await _context.Jobs!.FindAsync(JobId))!;

        UserJob objCJ = new()
        {
            UserId = user.Id,
            Job = objJob,
            CV = fileName,
            StatusInJob = $"{ProcessType.Administration}",
        };

        objJob.CandidateCount++;
        await _context.UserJobs!.AddAsync(objCJ);
        await _context.SaveChangesAsync();

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            updateCandidate.FileCV.CopyTo(stream);
        }

        TempData["success"] = "Application Successfully Sent";
        return Redirect("/TrackJob");
    }

    [HttpGet("/TrackJob")]
    public async Task<IActionResult> TrackJob()
    {
        User user = (await _userManager.GetUserAsync(User))!;

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        List<Job> listJobCandidate = _context.UserJobs!
                            .Where(c => c.UserId == user.Id)!
                            .Select(c => c.Job)
                            .ToList()!;

        List<CandidateJobStatus> listData = new();
        foreach (Job job in listJobCandidate)
        {
            UserJob UJ = (await _context.UserJobs!.FirstOrDefaultAsync(cj => cj.JobId == job.JobId && cj.UserId == user.Id))!;
            CandidateJobStatus status = new()
            {
                CandidateStatus = GetStatusApplication(UJ.StatusInJob!).Split(' '), // need migrate db to CandidateJobStatus for status in Job
                JobTitle = job.JobTitle,
            };
            listData.Add(status);
        }
        return View(listData);
    }

    [HttpGet("/Jobs")]
    public async Task<IEnumerable<Job>> Job()
    {
        return await _context.Jobs!.Where(Job => Job.IsJobAvailable).ToListAsync();
    }

    [HttpPatch("/EditProfile")]
    public async Task<IActionResult> EditProfile(Candidate objCandidate)
    {
        _context.Entry(objCandidate).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(objCandidate);
    }

    private string GetStatusApplication(string status)
    {
        List<string> process = new()
        {
            "current-item none none none",
            "none current-item none none",
            "none none current-item none",
            "none none none current-item"
        };

        int step = (int)Enum.Parse(typeof(ProcessType), status);

        if (step == 7) return "Rejected";

        else if (step == 5) return "Accepted";

        return process[step - 1];
    }
}
