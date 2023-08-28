using System.ComponentModel;
using System.Diagnostics;
using Google.Apis.Gmail.v1.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
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

		ViewBag.Subtitle = "Opportunities";
		ViewBag.Message = "See our available opportunities below";

		List<Department> jobDepartments = new List<Department>();
		foreach (Department department in _context.Departments!.ToList())
		{
			Department dept = new()
			{
				DepartmentId = department.DepartmentId,
				DepartmentName = department.DepartmentName
			};
			jobDepartments.Add(dept);
		}

		ViewBag.Departments = jobDepartments;

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
				JobDepartment = job.JobDepartment,
				JobMinEducation = job.JobMinEducation,
				EmploymentType = job.EmploymentType,
				JobPostedDate = job.JobPostedDate,
				JobExpiredDate = job.JobExpiredDate,
				Department = job.Department,
				CandidateCout = job.CandidateCount,
			};

			listJob.Add(data);
		}
		return View(listJob);
	}

	[HttpPost]
	public async Task<IActionResult> Index(string searchString, string? chosenLocation = null, string? chosenDepartment = null)
	{
		Console.WriteLine("\n\nLOCATION CHOSEN: " + chosenLocation);
		Console.WriteLine("\n\nDEPARTMENT CHOSEN: " + chosenDepartment);
		var jobs = from j in _context.Jobs select j;
		List<JobViewModel> listJob = new();

		List<Department> jobDepartments = new List<Department>();
		foreach (Department department in _context.Departments!.ToList())
		{
			Department dept = new()
			{
				DepartmentId = department.DepartmentId,
				DepartmentName = department.DepartmentName
			};
			jobDepartments.Add(dept);
		}

		ViewBag.Departments = jobDepartments;

		if (!string.IsNullOrEmpty(searchString))
		{
			ViewBag.Subtitle = "Search results";
			ViewBag.Message = $"Viewing jobs for \"{searchString}\"";

			var filteredjobs = jobs.ToList().Where(j => j.JobTitle != null && j.IsJobAvailable && j.JobTitle.Contains(searchString, StringComparison.OrdinalIgnoreCase));
			foreach (var job in filteredjobs)
			{
				JobViewModel data = new()
				{
					JobId = job.JobId,
					JobTitle = job.JobTitle,
					JobDescription = job.JobDescription,
					JobRequirement = job.JobRequirement,
					Location = job.Location,
					JobDepartment = job.JobDepartment,
					JobMinEducation = job.JobMinEducation,
					EmploymentType = job.EmploymentType,
					JobPostedDate = job.JobPostedDate,
					JobExpiredDate = job.JobExpiredDate,
					Department = job.Department,
					CandidateCout = job.CandidateCount,
				};

				listJob.Add(data);
			}
			if (!string.IsNullOrEmpty(chosenLocation) && !string.IsNullOrEmpty(chosenDepartment))
			{
				ViewBag.Message += $" in {chosenLocation} and {chosenDepartment} Department";
				listJob = FilterByDepartment(chosenDepartment, FilterByLocation(chosenLocation, listJob));
			}
			else if (!string.IsNullOrEmpty(chosenLocation))
			{
				ViewBag.Message += $" in {chosenLocation}";
				listJob = FilterByLocation(chosenLocation, listJob);
			}
			else if (!string.IsNullOrEmpty(chosenDepartment))
			{
				ViewBag.Message = $" in {chosenDepartment} Department";
				listJob = FilterByDepartment(chosenDepartment, listJob);
			}
		}
		else 
		{
			// jika seacrhstring kosong, setiap pekerjaan ditampilkan
			ViewBag.Subtitle = "Opportunities";
			ViewBag.Message = "See our available opportunities below";
			foreach (Job job in _context.Jobs!.Where(j => j.IsJobAvailable).ToList())
			{
				JobViewModel viewModel = new()
				{
					JobId = job.JobId,
					JobTitle = job.JobTitle,
					JobDescription = job.JobDescription,
					JobRequirement = job.JobRequirement,
					Location = job.Location,
					JobDepartment = job.JobDepartment,
					JobMinEducation = job.JobMinEducation,
					EmploymentType = job.EmploymentType,
					JobPostedDate = job.JobPostedDate,
					JobExpiredDate = job.JobExpiredDate,
					Department = job.Department,
					CandidateCout = job.CandidateCount,
				};
				
				listJob.Add(viewModel);
			}
			if (!string.IsNullOrEmpty(chosenLocation) && !string.IsNullOrEmpty(chosenDepartment))
			{
				ViewBag.Subtitle = "Search results";
				ViewBag.Message = $"Filtered jobs in: {chosenLocation} and {chosenDepartment} Department";
				listJob = FilterByDepartment(chosenDepartment, FilterByLocation(chosenLocation, listJob));
			}
			else if (!string.IsNullOrEmpty(chosenLocation))
			{
				ViewBag.Subtitle = "Search results";
				ViewBag.Message = $"Filtered jobs in: {chosenLocation}";
				listJob = FilterByLocation(chosenLocation, listJob);
			}
			else if (!string.IsNullOrEmpty(chosenDepartment))
			{
				ViewBag.Subtitle = "Search results";
				ViewBag.Message = $"Filtered jobs in: {chosenDepartment} Department";
				listJob = FilterByDepartment(chosenDepartment, listJob);
			}
		}
		if (listJob.Count == 0)
		{
			if (string.IsNullOrEmpty(searchString))
			{
				ViewBag.Message = $"no results found for the chosen categories";
			}
			else
			{
				ViewBag.Message = $"no results found for \"{searchString}\"";
			}
		}
		return View(listJob);
	}

	private List<JobViewModel> FilterByLocation (string chosenLocation, List<JobViewModel> listJob)
	{
		List<JobViewModel> filterByLocation = new List<JobViewModel>();
		foreach (var job in listJob)
		{
			filterByLocation = listJob.Where(j => j.Location == chosenLocation).ToList();
		}
		return filterByLocation;
	}

	private List<JobViewModel> FilterByDepartment (string chosenDepartment, List<JobViewModel> listJob)
	{
		List<JobViewModel> filterByDepartment = new List<JobViewModel>();
		foreach (var job in listJob)
		{
			filterByDepartment = listJob.Where(j => j.JobDepartment == chosenDepartment).ToList();
		}
		return filterByDepartment;
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
