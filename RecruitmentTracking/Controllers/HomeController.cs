using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentTracking.Data;
using RecruitmentTracking.Models;

namespace RecruitmentTracking.Controllers;

public class HomeController : Controller
{
	private readonly ApplicationDbContext _context;
	private readonly ILogger<HomeController> _logger;
	private readonly IConfiguration _configuration;
	private readonly UserManager<User> _userManager;

	public HomeController(ILogger<HomeController> logger, IConfiguration configuration, ApplicationDbContext context, UserManager<User> userManager)
	{
		_logger = logger;
		_configuration = configuration;
		_context = context;
		_userManager = userManager;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		User user = (await _userManager.GetUserAsync(User))!;

		if (user != null)
		{
			if (await _userManager.IsInRoleAsync(user, "Admin")) return Redirect("/Admin");
			Candidate objCandidate = (await _context.Candidates.FirstOrDefaultAsync(c => c.UserId == user.Id))!;
			if (objCandidate == null)
			{
				TempData["warning"] = "Please complete your data";
				return Redirect("/Profile");
			}
		}

		List<JobViewModel> listJob = new();
		foreach (Job job in _context.Jobs!.Where(j => j.IsJobAvailable).ToList())
		{
			JobViewModel data = new()
			{
				JobId = job.JobId,
				JobTitle = job.JobTitle,
				JobDescription = job.JobDescription,
				JobRequirement = job.JobRequirement,
				Location = job.Location,
				JobPostedDate = job.JobPostedDate,
				JobExpiredDate = job.JobExpiredDate,
				CandidateCout = job.candidateCount,
			};

			listJob.Add(data);
		}
		return View(listJob);
	}

	[HttpPost]
	public async Task<IActionResult> Index(string searchstring)
	{
		var jobs = from j in _context.Jobs select j;

		List<JobViewModel> listJob = new();

		if (!string.IsNullOrEmpty(searchstring))
		{
			var filteredjobs = jobs.ToList().Where(j => j.JobTitle != null && j.IsJobAvailable && j.JobTitle.Contains(searchstring, StringComparison.OrdinalIgnoreCase));
			foreach (var job in filteredjobs)
			{
				JobViewModel data = new()
				{
					JobId = job.JobId,
					JobTitle = job.JobTitle,
					JobDescription = job.JobDescription,
					JobRequirement = job.JobRequirement,
					Location = job.Location,
					CandidateCout = job.candidateCount,
				};

				listJob.Add(data);
			}
		}
		else 
		{
			// jika seacrhstring kosong, setiap pekerjaan ditampilkan
			foreach (Job job in _context.Jobs!.Where(j => j.IsJobAvailable).ToList())
			{
				JobViewModel viewModel = new()
				{
					JobId = job.JobId,
					JobTitle = job.JobTitle,
					JobDescription = job.JobDescription,
					JobRequirement = job.JobRequirement,
					Location = job.Location,
					CandidateCout = job.candidateCount,
				};
				
				listJob.Add(viewModel);
			}
		}
		return View(listJob);
	}

	[HttpGet("/DetailJob/{id}")]
	public IActionResult DetailJob(int id)
	{
		Job objJob = _context.Jobs!.Find(id)!;

		JobViewModel data = new()
		{
			JobId = objJob.JobId,
			JobTitle = objJob.JobTitle,
			JobDescription = objJob.JobDescription,
			JobRequirement = objJob.JobRequirement,
			Location = objJob.Location,
			JobPostedDate = objJob.JobPostedDate,
			JobExpiredDate = objJob.JobExpiredDate,
		};

		return View(data);
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
}
