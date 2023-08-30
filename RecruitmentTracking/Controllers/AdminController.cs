using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using RecruitmentTracking.Models;
using RecruitmentTracking.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;

namespace RecruitmentTracking.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
	private readonly ApplicationDbContext _context;
	private readonly ILogger<HomeController> _logger;
	private readonly IConfiguration _configuration;
	private readonly UserManager<User> _userManager;

	public AdminController(ILogger<HomeController> logger, IConfiguration configuration, ApplicationDbContext context, UserManager<User> userManager)
	{
		_logger = logger;
		_configuration = configuration;
		_context = context;
		_userManager = userManager;
	}

	private List<SelectListItem> GetAvailableDepartments(ApplicationDbContext context)
	{
		List<SelectListItem> jobDepartments = new ();
		foreach (Department dept in context.Departments!.ToList())
		{
			var selectItem = new SelectListItem{Value = $"{dept.DepartmentName}", Text = $"{dept.DepartmentName}"};
			jobDepartments.Add(selectItem);
		}
		return jobDepartments;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		List<Job> listObjJob = await _context.Jobs!.Where(j => j.IsJobAvailable).ToListAsync();

		List<JobViewModel> listJobModel = new();
		foreach (Job job in listObjJob)
		{
			int candidateCount = _context.UserJobs!.Where(c => c.JobId == job.JobId).Count();
			job.CandidateCount = candidateCount;
			JobViewModel modelView = new()
			{
				JobId = job.JobId,
				JobTitle = job.JobTitle,
				JobDescription = job.JobDescription,
				JobRequirement = job.JobRequirement,
				Location = job.Location,
				JobPostedDate = job.JobPostedDate,
				JobExpiredDate = job.JobExpiredDate,
				CandidateCout = candidateCount,
			};
			listJobModel.Add(modelView);
		}
		await _context.SaveChangesAsync();

		return View(listJobModel);
		//return View(listJob);
	}

	// Admin/JobClosed
	[HttpGet]
	public async Task<IActionResult> JobClosed()
	{
		User user = (await _userManager.GetUserAsync(User))!;

		if (user == null)
		{
			return RedirectToAction("Login");
		}

		List<JobViewModel> listJob = new();
		foreach (Job job in _context.Jobs!.Where(j => !j.IsJobAvailable).ToList())
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
				CandidateCout = job.CandidateCount,
			};
			listJob.Add(data);
		}
		return View(listJob);
	}

	[HttpPost]
	public async Task<IActionResult> CloseTheJob(int id)
	{
		Job objJob = (await _context.Jobs!.FindAsync(id))!;

		objJob.IsJobAvailable = false;
		await _context.SaveChangesAsync();

		TempData["success"] = "Successfully Close a Job";
		return Redirect("/Admin");
	}

	[HttpPost]
	public async Task<IActionResult> ActivateTheJob(int id)
	{
		Job objJob = _context.Jobs!.Find(id)!;

		objJob.IsJobAvailable = true;
		await _context.SaveChangesAsync();

		TempData["success"] = "Successfully Activate a Job";
		return Redirect("/Admin/JobClosed");
	}

	// Add Feature, if candidate apply job > 0, job can't be closed
	[HttpPost]
	public async Task<IActionResult> DeleteJob(int id)
	{
		if ((await _context.UserJobs!.Where(cj => cj.JobId == id).AnyAsync())!)
		{
			TempData["warning"] = "Job can't be closed because there are candidates who have applied for this job.";
			//return Redirect("/Admin");
			return Redirect("/Admin/JobClosed");
		}

		Job objJob = _context.Jobs!.Find(id)!;
		_context.Jobs.Remove(objJob);
		await _context.SaveChangesAsync();

		TempData["success"] = "Successfully Delete a Job";
		return Redirect("/Admin/JobClosed");
	}

	//UI added
	[HttpGet]
	public async Task<IActionResult> CreateJob()
	{
		User user = (await _userManager.GetUserAsync(User))!;

		if (user == null)
		{
			return RedirectToAction("Login");
		}

		ViewBag.AdminName = user.Name;
		ViewBag.Departments = GetAvailableDepartments(_context);
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> CreateJobs(JobCreate objJob)
	{
		User user = (await _userManager.GetUserAsync(User))!;

		if (user == null)
		{
			return RedirectToAction("Login");
		}

		Job newJob = new()
		{
			JobTitle = objJob.JobTitle,
			JobDescription = objJob.JobDescription,
			JobExpiredDate = objJob.JobExpiredDate,
			JobRequirement = objJob.JobRequirement!.Replace("\r\n", "\n"),
			JobDepartment = objJob.JobDepartment,
			Department = _context.Departments.FirstOrDefault(x => x.DepartmentName == objJob.JobDepartment),
			JobMinEducation = objJob.JobMinEducation,
			EmploymentType = objJob.EmploymentType,
			JobPostedDate = DateTime.Now,
			Location = objJob.Location,
			IsJobAvailable = true,
			User = user,
		};

		// newJob.Department = _context.Departments.FirstOrDefault(x => x.)

		_context.Jobs!.Add(newJob);
		await _context.SaveChangesAsync();

		TempData["success"] = "Successfully Created a Job";
		return Redirect("/Admin");
	}

	[HttpGet]
	public async Task<IActionResult> EditJob(int id)
	{
		User user = (await _userManager.GetUserAsync(User))!;

		if (user == null)
		{
			return RedirectToAction("Login");
		}

		ViewBag.AdminName = user.Name;
		ViewBag.Departments = GetAvailableDepartments(_context);

		Job objJob = (await _context.Jobs!.FindAsync(id))!;

		JobViewModel data = new()
		{
			JobId = objJob.JobId,
			JobTitle = objJob.JobTitle,
			JobDescription = objJob.JobDescription,
			JobRequirement = objJob.JobRequirement!.Replace("\r\n", "\n"),
			JobDepartment = objJob.JobDepartment,
			JobMinEducation = objJob.JobMinEducation,
			EmploymentType = objJob.EmploymentType,
			Location = objJob.Location,
			JobPostedDate = objJob.JobPostedDate,
			JobExpiredDate = objJob.JobExpiredDate,
		};

		return View(data);
	}

	[HttpPost]
	public async Task<IActionResult> EditJobs(JobCreate objJob)
	{
		User user = (await _userManager.GetUserAsync(User))!;

		if (user == null)
		{
			return RedirectToAction("Login");
		}

		Job updateJob = (await _context.Jobs!.FindAsync(objJob.JobId))!;
		updateJob.JobTitle = objJob.JobTitle;
		updateJob.JobDescription = objJob.JobDescription;
		updateJob.JobExpiredDate = objJob.JobExpiredDate;
		updateJob.JobRequirement = objJob.JobRequirement;
		updateJob.JobDepartment = objJob.JobDepartment;
		updateJob.Department = _context.Departments.FirstOrDefault(x => x.DepartmentName == objJob.JobDepartment);
		updateJob.JobMinEducation = objJob.JobMinEducation;
		updateJob.EmploymentType = objJob.EmploymentType;
		updateJob.Location = objJob.Location;

		await _context.SaveChangesAsync();

		TempData["success"] = "Successfully Edit a Job";
		return Redirect("/Admin");
	}

	[HttpGet]
	public async Task<IActionResult> RecruitmentProcess(int id)
	{
		User user = (await _userManager.GetUserAsync(User))!;

		if (user == null)
		{
			return RedirectToAction("Login");
		}

		ViewBag.AdminName = user.Name;
		ViewBag.jobId = id;

		Job objJob = (await _context.Jobs!.FindAsync(id))!;
		ViewBag.JobTitle = objJob.JobTitle;
		ViewBag.EmailUser = objJob.EmailUser;

		List<User> listObjUser = (await _context.UserJobs!
										.Where(cj => cj.JobId == id)
										.Select(cj => cj.User)
										.ToListAsync())!;

		List<DataCandidateJobs> listAdministration = new();
		foreach (User objUser in listObjUser)
		{
			string statusInJob = (await _context.UserJobs!.FirstOrDefaultAsync(uj => uj.JobId == id && uj.UserId == objUser.Id))!.StatusInJob!;
			if (statusInJob == "Administration")
			{
				UserJob cj = _context.UserJobs!.FirstOrDefault(cj => cj.UserId == objUser.Id)!;
				Candidate objCandidate = (await _context.Candidates.FirstOrDefaultAsync(c => c.UserId == objUser.Id))!;
				DataCandidateJobs dataCandidate = new()
				{
					UserId = objUser.Id,
					Name = objUser.Name,
					Email = objUser.Email,
					Phone = objCandidate.Phone,
					LastEducation = objCandidate.LastEducation,
					GPA = objCandidate.GPA,
					CV = cj.CV,
					DateHRInterview = cj.DateHRInterview,
					TimeHRInterview = cj.TimeHRInterview,
					LocationHRInterview = cj.LocationHRInterview,
				};
				listAdministration.Add(dataCandidate);
			}
		}

		List<DataCandidateJobs> listHRInterview = new();
		foreach (User objUser in listObjUser)
		{
			string statusInJob = (await _context.UserJobs!.FirstOrDefaultAsync(uj => uj.JobId == id && uj.UserId == objUser.Id))!.StatusInJob!;
			if (statusInJob == "HRInterview")
			{
				UserJob cj = _context.UserJobs!.FirstOrDefault(cj => cj.UserId == objUser.Id)!;
				Candidate objCandidate = (await _context.Candidates.FirstOrDefaultAsync(c => c.UserId == objUser.Id))!;
				DataCandidateJobs dataCandidate = new()
				{
					UserId = objUser.Id,
					Name = objUser.Name,
					Email = objUser.Email,
					Phone = objCandidate.Phone,
					LastEducation = objCandidate.LastEducation,
					GPA = objCandidate.GPA,
					CV = cj.CV,
					DateHRInterview = cj.DateHRInterview,
					TimeHRInterview = cj.TimeHRInterview,
					LocationHRInterview = cj.LocationHRInterview,
				};
				listHRInterview.Add(dataCandidate);
			}
		}

		List<DataCandidateJobs> listUserInterview = new();
		foreach (User objUser in listObjUser)
		{
			string statusInJob = (await _context.UserJobs!.FirstOrDefaultAsync(uj => uj.JobId == id && uj.UserId == objUser.Id))!.StatusInJob!;
			if (statusInJob == "UserInterview")
			{
				UserJob cj = _context.UserJobs!.FirstOrDefault(cj => cj.UserId == objUser.Id)!;
				Candidate objCandidate = (await _context.Candidates.FirstOrDefaultAsync(c => c.UserId == objUser.Id))!;
				DataCandidateJobs dataCandidate = new()
				{
					UserId = objUser.Id,
					Name = objUser.Name,
					Email = objUser.Email,
					Phone = objCandidate.Phone,
					LastEducation = objCandidate.LastEducation,
					GPA = objCandidate.GPA,
					CV = cj.CV,
					Salary = objCandidate.Salary,
					DateHRInterview = cj.DateHRInterview,
					TimeHRInterview = cj.TimeHRInterview,
					LocationHRInterview = cj.LocationHRInterview,
				};
				listUserInterview.Add(dataCandidate);
			}
		}

		List<DataCandidateJobs> listOffering = new();
		foreach (User objUser in listObjUser)
		{
			string statusInJob = (await _context.UserJobs!.FirstOrDefaultAsync(uj => uj.JobId == id && uj.UserId == objUser.Id))!.StatusInJob!;
			if (statusInJob == "Offering")
			{
				UserJob cj = _context.UserJobs!.FirstOrDefault(cj => cj.UserId == objUser.Id)!;
				Candidate objCandidate = (await _context.Candidates.FirstOrDefaultAsync(c => c.UserId == objUser.Id))!;
				DataCandidateJobs dataCandidate = new()
				{
					UserId = objUser.Id,
					Name = objUser.Name,
					Email = objUser.Email,
					Phone = objCandidate.Phone,
					LastEducation = objCandidate.LastEducation,
					GPA = objCandidate.GPA,
					CV = cj.CV,
					Salary = objCandidate.Salary,
					DateHRInterview = cj.DateHRInterview,
					TimeHRInterview = cj.TimeHRInterview,
					LocationHRInterview = cj.LocationHRInterview,
				};
				listOffering.Add(dataCandidate);
			}
		}

		ProcessViewModel viewModel = new()
		{
			Administration = listAdministration,
			HRInterview = listHRInterview,
			UserInterview = listUserInterview,
			Offering = listOffering,
		};

		return View(viewModel);
	}

	[HttpGet("Admin/TemplateEmail/{id}")]
	public async Task<IActionResult> TemplateEmail(int id)
	{
		ViewBag.JobId = id;

		Job objJob = (await _context.Jobs!.FindAsync(id))!;
		EmailTemplate email = new()
		{
			JobId = id,
			EmailHR = objJob.EmailHR,
			EmailUser = objJob.EmailUser,
			EmailReject = objJob.EmailReject,
			EmailOffering = objJob.EmailOffering,
		};

		return View(email);
	}

	[HttpPost]
	public async Task<IActionResult> SaveTemplateEmail(EmailTemplate objEmailTemplate)
	{
		Job objJob = (await _context.Jobs!.FindAsync(objEmailTemplate.JobId))!;

		objJob.EmailHR = objEmailTemplate.EmailHR;
		objJob.EmailUser = objEmailTemplate.EmailUser;
		objJob.EmailReject = objEmailTemplate.EmailReject;
		objJob.EmailOffering = objEmailTemplate.EmailOffering;

		await _context.SaveChangesAsync();

		return Redirect($"/Admin/RecruitmentProcess/{objEmailTemplate.JobId}");
	}

	[HttpPost]
	public async Task<IActionResult> SaveRejectionEmail (EmailTemplate objEmailTemplate)
	{
		Job objJob = (await _context.Jobs!.FindAsync(objEmailTemplate.JobId))!;
		objJob.EmailReject = objEmailTemplate.EmailReject;

		await _context.SaveChangesAsync();

		TempData["success"] = "Rejection Email Template Saved";
		return Redirect($"/Admin/TemplateEmail/{objEmailTemplate.JobId}");
	}

	[HttpPost]
	public async Task<IActionResult> SaveHREmail (EmailTemplate objEmailTemplate)
	{
		Job objJob = (await _context.Jobs!.FindAsync(objEmailTemplate.JobId))!;
		objJob.EmailHR = objEmailTemplate.EmailHR;

		await _context.SaveChangesAsync();

		TempData["success"] = "HR Interview Email Template Saved";
		return Redirect($"/Admin/TemplateEmail/{objEmailTemplate.JobId}");
	}

	[HttpPost]
	public async Task<IActionResult> SaveUserEmail (EmailTemplate objEmailTemplate)
	{
		Job objJob = (await _context.Jobs!.FindAsync(objEmailTemplate.JobId))!;
		objJob.EmailUser = objEmailTemplate.EmailUser;

		await _context.SaveChangesAsync();

		TempData["success"] = "User Interview Email Template Saved";
		return Redirect($"/Admin/TemplateEmail/{objEmailTemplate.JobId}");
	}
	
	[HttpPost]
	public async Task<IActionResult> SaveOfferEmail (EmailTemplate objEmailTemplate)
	{
		Job objJob = (await _context.Jobs!.FindAsync(objEmailTemplate.JobId))!;
		objJob.EmailOffering = objEmailTemplate.EmailOffering;

		await _context.SaveChangesAsync();

		TempData["success"] = "Offering Email Template Saved";
		return Redirect($"/Admin/TemplateEmail/{objEmailTemplate.JobId}");
	}

	[HttpPost]
	public async Task<IActionResult> DownloadCV(string UserId, int JobId)
	{
		UserJob CJ = (await _context.UserJobs!
							.FirstOrDefaultAsync(cj => cj.JobId == JobId && cj.UserId == UserId))!;

		string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "DataCV", CJ.CV!);

		return PhysicalFile(filePath, "application/force-download", Path.GetFileName(filePath));
	}

	[HttpPost]
	public async Task<IActionResult> Accept(string UserId, int JobId)
	{
		UserJob UJ = (await _context.UserJobs!
							.FirstOrDefaultAsync(cj => cj.JobId == JobId && cj.UserId == UserId))!;

		User objUser = (await _context.Users!.FindAsync(UJ.UserId))!;
		Job objJob = (await _context.Jobs!.FindAsync(UJ.JobId))!;

		int statusInJob = (int)Enum.Parse(typeof(ProcessType), UJ.StatusInJob!);
		statusInJob++;
		UJ.StatusInJob = $"{(ProcessType)statusInJob}";
		// SendEmailRejection(objUser, UJ, objJob.JobTitle!);

		await _context.SaveChangesAsync();

		TempData["success"] = "Candidate Accepted";
		return Redirect($"/Admin/RecruitmentProcess/{JobId}");
	}

	[HttpPost]
	public async Task<IActionResult> SaveEmailUser(int JobId, string emailUser)
	{
		Job objJob = (await _context.Jobs!.FindAsync(JobId))!;
		objJob.UserEmailInterview = emailUser;
		await _context.SaveChangesAsync();

		TempData["success"] = "Email User Saved";
		return Redirect($"/Admin/RecruitmentProcess/{JobId}");
	}

	[HttpPost]
	public async Task<IActionResult> Rejected(string UserId, int JobId)
	{
		UserJob UJ = (await _context.UserJobs!
							 .FirstOrDefaultAsync(cj => cj.JobId == JobId && cj.UserId == UserId))!;

		User objUser = (await _context.Users!.FindAsync(UJ.UserId))!;
		Job objJob = (await _context.Jobs!.FindAsync(UJ.JobId))!;

		UJ.StatusInJob = $"{ProcessType.Rejected}";

		SendEmailRejection(objUser, UJ, objJob.JobTitle!);

		await _context.SaveChangesAsync();

		TempData["success"] = "Candidate Rejected";
		return Redirect($"/Admin/RecruitmentProcess/{JobId}");
	}

	[HttpPost]
	public async Task<ActionResult> SendHRInterview(string UserId, int JobId, DateTime date, DateTime time, string location)
	{
		string myEmailAccount = "projectadmreruiter@gmail.com";
		string myEmailPassword = "qucnsmdddmobmnfo";

		UserJob UJ = (await _context.UserJobs!
							 .FirstOrDefaultAsync(cj => cj.JobId == JobId && cj.UserId == UserId))!;

		UJ.DateHRInterview = date;
		UJ.TimeHRInterview = time;
		UJ.LocationHRInterview = location;

		User objUser = (await _context.Users!.FindAsync(UJ.UserId))!;
		Job objJob = (await _context.Jobs!.FindAsync(UJ.JobId))!;

		string emailTemplate = UJ.Job!.EmailHR!;
		string emailBody = emailTemplate
				.Replace("[Applicant's Name]", objUser.Name)
				.Replace("[Job Title]", objJob.JobTitle)
				.Replace("[Date]", UJ.DateHRInterview?.ToString("dddd, dd MMM yyy"))
				.Replace("[Time]", UJ.TimeHRInterview?.ToString("HH:mm"))
				.Replace("[Location]", UJ.LocationHRInterview);

		//make instance message
		MailMessage message = new MailMessage
		{
			From = new MailAddress(myEmailAccount)
		};

		//add recipient
		message.To.Add(new MailAddress($"{objUser.Email}"));
		// message.To.Add(new MailAddress("ignatius.c.k@gmail.com"));

		// add subject and body
		message.Subject = $"UPDATE Recruitment for {objJob.JobTitle}";
		message.Body = emailBody;

		var SmtpClient = new SmtpClient("smtp.gmail.com")
		{
			Port = 587,
			Credentials = new NetworkCredential(myEmailAccount, myEmailPassword),
			EnableSsl = true,
		};

		//send message
		SmtpClient.Send(message);
		await _context.SaveChangesAsync();

		TempData["success"] = "Email sent";
		return Redirect($"/Admin/RecruitmentProcess/{JobId}");
	}

	[HttpPost]
	public async Task<ActionResult> SendUserInterview(string UserId, int JobId, DateTime date, DateTime time, string location)
	{
		string myEmailAccount = "projectadmreruiter@gmail.com";
		string myEmailPassword = "qucnsmdddmobmnfo";

		UserJob UJ = (await _context.UserJobs!
							 .FirstOrDefaultAsync(cj => cj.JobId == JobId && cj.UserId == UserId))!;

		UJ.DateUserInterview = date;
		UJ.TimeUserInterview = time;
		UJ.LocationUserInterview = location;

		User objUser = (await _context.Users!.FindAsync(UJ.UserId))!;
		Job objJob = (await _context.Jobs!.FindAsync(UJ.JobId))!;
		Candidate objCandidate = (await _context.Candidates!.FirstOrDefaultAsync(c => c.UserId == objUser.Id))!;

		string emailTemplate = UJ.Job!.EmailUser!;
		string emailBodyCandidate = emailTemplate
				.Replace("[Applicant's Name]", objUser.Name)
				.Replace("[Job Title]", objJob.JobTitle)
				.Replace("[Date]", UJ.DateUserInterview?.ToString("dddd, dd MMM yyy"))
				.Replace("[Time]", UJ.TimeUserInterview?.ToString("HH:mm"))
				.Replace("[Location]", UJ.LocationUserInterview);

		string emailBodyUser =
		"Interview User \n \n" +
		"Details  \n" +
		$"Name      : {objUser.Name} \n" +
		$"Edu       : {objCandidate.LastEducation} \n" +
		$"GPA       : {objCandidate.GPA} \n" +
		$"Time      : {UJ.TimeUserInterview} \n" +
		$"Date      : {UJ.DateUserInterview} \n" +
		$"Location  : {UJ.LocationUserInterview} \n";

		//make instance message
		MailMessage messageCandidate = new MailMessage
		{
			From = new MailAddress(myEmailAccount)
		};

		MailMessage messageUser = new MailMessage
		{
			From = new MailAddress(myEmailAccount)
		};

		//add recipient
		// messageCandidate.To.Add(new MailAddress("ignatius.c.k@gmail.com"));
		// messageUser.To.Add(new MailAddress("ignatius.adse@gmail.com"));
		messageCandidate.To.Add(new MailAddress($"{objUser.Email}"));
		messageUser.To.Add(new MailAddress("projectadmreruiter@gmail.com"));
		// message.To.Add(new MailAddress("ignatius.c.k@gmail.com"));

		// add subject and body
		messageCandidate.Subject = $"User Interview : {objJob.JobTitle}";
		messageCandidate.Body = emailBodyCandidate;

		messageUser.Subject = $"User Interview : {objJob.JobTitle}";
		messageUser.Body = emailBodyUser;

		var smtpClient = new SmtpClient("smtp.gmail.com")
		{
			Port = 587,
			Credentials = new NetworkCredential(myEmailAccount, myEmailPassword),
			EnableSsl = true,
		};

		//send message
		smtpClient.Send(messageCandidate);
		smtpClient.Send(messageUser);

		await _context.SaveChangesAsync();

		TempData["success"] = "Email sent";
		return Redirect($"/Admin/RecruitmentProcess/{JobId}");
	}

	[HttpPost]
	public async Task<ActionResult> SendOffering(string UserId, int JobId)
	{
		string myEmailAccount = "projectadmreruiter@gmail.com";
		string myEmailPassword = "qucnsmdddmobmnfo";

		UserJob UJ = (await _context.UserJobs!
							 .FirstOrDefaultAsync(cj => cj.JobId == JobId && cj.UserId == UserId))!;

		User objUser = (await _context.Users!.FindAsync(UJ.UserId))!;
		Job objJob = (await _context.Jobs!.FindAsync(UJ.JobId))!;

		string emailTemplate = UJ.Job!.EmailOffering!;
		string emailBodyCandidate = emailTemplate
				.Replace("[Applicant's Name]", objUser.Name)
				.Replace("[Job Title]", objJob.JobTitle);

		//make instance message
		MailMessage messageCandidate = new MailMessage
		{
			From = new MailAddress(myEmailAccount)
		};
		
		MailMessage messageUser = new MailMessage
		{
			From = new MailAddress(myEmailAccount)
		};

		//add recipient
		messageCandidate.To.Add(new MailAddress($"{objUser.Email}"));
		// message.To.Add(new MailAddress("ignatius.c.k@gmail.com"));
		messageUser.To.Add(new MailAddress("projectadmreruiter@gmail.com"));
		// add subject and body
		messageCandidate.Subject = $"Offering Job : {objJob.JobTitle}";
		messageCandidate.Body = emailBodyCandidate;

		var smtpClient = new SmtpClient("smtp.gmail.com")
		{
			Port = 587,
			Credentials = new NetworkCredential(myEmailAccount, myEmailPassword),
			EnableSsl = true,
		};

		//send message
		smtpClient.Send(messageCandidate);
		smtpClient.Send(messageUser);
		await _context.SaveChangesAsync();

		TempData["success"] = "Email sent";
		return Redirect($"/Admin/RecruitmentProcess/{JobId}");
	}
	
	private void SendEmailRejection(User objUser, UserJob UJ, string jobTitle)
	{
		string myEmailAccount = "projectadmreruiter@gmail.com";
		string myEmailPassword = "qucnsmdddmobmnfo";

		string emailTemplate = UJ.Job!.EmailReject!;
		string emailBody = emailTemplate
				.Replace("[Applicant's Name]", objUser.Name)
				.Replace("[Job Title]", jobTitle);

		//make instance message
		MailMessage message = new MailMessage
		{
			From = new MailAddress(myEmailAccount)
		};

		//add recipient
		message.To.Add(new MailAddress($"{objUser.Email}"));

		// add subject and body
		message.Subject = $"UPDATE Recruitment for {jobTitle}";
		message.Body = emailBody;

		var SmtpClient = new SmtpClient("smtp.gmail.com")
		{
			Port = 587,
			Credentials = new NetworkCredential(myEmailAccount, myEmailPassword),
			EnableSsl = true,
		};

		//send message
		SmtpClient.Send(message);
	}

	// [HttpPost]
	// public async Task<IActionResult> TemplateEmail(EmailTemplate email)
	// {
	//     Job objJob = (await _db.Jobs!.FindAsync(email.JobId))!;
	//     objJob.EmailHR = email.EmailHR;
	//     await _db.SaveChangesAsync();

	//     return View(email);
	// }
}
