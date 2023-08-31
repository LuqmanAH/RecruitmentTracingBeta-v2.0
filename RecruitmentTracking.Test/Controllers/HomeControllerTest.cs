using Microsoft.AspNetCore.Mvc.Testing;
using RecruitmentTracking.Controllers;
using RecruitmentTracking.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecruitmentTracking.Models;
using FluentAssertions;
using Xunit;
using Moq;


namespace RecruitmentTracking.Tests
{
	public class MockUserManager : UserManager<User>
	{
		public MockUserManager()
			: base(new Mock<IUserStore<User>>().Object,
				null, null, null, null, null, null, null, null)
		{
		}
	}

	public class HomeControllerTests : IClassFixture<WebApplicationFactory<Program>>
	{
		private readonly WebApplicationFactory<Program> _factory;

		public HomeControllerTests(WebApplicationFactory<Program> factory)
		{
			_factory = factory;
		}

		private ApplicationDbContext SetupInMemoryDbContext()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "RecruitmentTracking" + Guid.NewGuid())
				.Options;

			var context = new ApplicationDbContext(options);
			var department = new Department()
			{
				DepartmentId = 321,
				DepartmentName = "Engineering"
			};

			var job = new Job()
			{
				JobId = 123,
				JobTitle = "tester",
				JobDescription = "description",
				JobRequirement = "requirement",
				Location = "Semarang",
				EmploymentType = "Full-time",
				JobDepartment = "Engineering",
				JobExpiredDate = DateTime.Now.AddDays(30),
				JobMinEducation = "Bachelor's Degree",
				JobPostedDate = DateTime.Now,
				IsJobAvailable = true,
				Department = department
			};

			context.Departments.Add(department);
			context.Jobs.Add(job);
			context.SaveChanges();

			return context;
		}

		private HomeController SetupController(ApplicationDbContext context)
		{
			var loggerMock = new Mock<ILogger<HomeController>>();
			var configurationMock = new Mock<IConfiguration>();
			var userManagerMock = new Mock<MockUserManager>();

			return new HomeController(loggerMock.Object, configurationMock.Object, context, userManagerMock.Object);
		}

		[Fact]
		public async Task Index_ReturnsView()
		{
			// Arrange
			var context = SetupInMemoryDbContext();
			var controller = SetupController(context);

			//Act
			IActionResult result = await controller.Index(null, null, null);

			// Assert
			result.Should().BeOfType<ViewResult>();

		}

		[Fact]
		public async Task Index_ReturnsWithFilter()
		{
			// Arrange
			var context = SetupInMemoryDbContext();
			var controller = SetupController(context);

			//Act
			IActionResult result = await controller.Index("", "", "");

			// Assert
			var viewResult = (ViewResult)result;
			var listJob = (List<JobViewModel>)viewResult.Model;
			listJob.Should().NotBeNull();
			listJob.Should().HaveCountGreaterThan(0);
			listJob.First().JobTitle.Should().Be("tester");
		}

		[Fact]
		public void FilterByLocation_ReturnsResult()
		{
			// Arrange
			var context = SetupInMemoryDbContext();
			var controller = SetupController(context);

			var listJob = new List<JobViewModel>
			{
				new JobViewModel { Location = "Salatiga" },
				new JobViewModel { Location = "Yogyakarta" },
				new JobViewModel { Location = "Salatiga" },
			};

			var chosenLocation = "Salatiga";

			// Act
			var filteredJobs = controller.FilterByLocation(chosenLocation, listJob);

			// Assert
			filteredJobs.Should().NotBeNull();
			filteredJobs.Should().NotBeEmpty();
			filteredJobs.Should().OnlyContain(job => job.Location == chosenLocation);
		}

		[Fact]
		public void FilterByDepartment_ReturnsResult()
		{
			// Arrange
			var context = SetupInMemoryDbContext();
			var controller = SetupController(context);

			var listJob = new List<JobViewModel>
			{
				new JobViewModel { JobDepartment = "Engineering" },
				new JobViewModel { JobDepartment = "HR" },
				new JobViewModel { JobDepartment = "Engineering" },
			};

			var chosenDepartment = "Engineering";

			// Act
			var filteredJobs = controller.FilterByDepartment(chosenDepartment, listJob);

			// Assert
			filteredJobs.Should().NotBeNull();
			filteredJobs.Should().NotBeEmpty();
			filteredJobs.Should().OnlyContain(job => job.JobDepartment == chosenDepartment);
		}

		[Fact]
		public async Task DetailJob_ReturnsViewResultWithCorrectData()
		{
			// Arrange
			var context = SetupInMemoryDbContext();
			var controller = SetupController(context);

			var jobId = 123;

			var objJob = new Job
			{
				JobId = jobId,
				JobTitle = "tester",
				JobDescription = "description",
				JobRequirement = "requirement",
				Location = "Semarang",
				JobPostedDate = DateTime.Now,
				JobExpiredDate = DateTime.Now.AddDays(30)
			};


			// Act
			IActionResult result = controller.DetailJob(jobId);

			// Assert
			var viewResult = result.Should().BeOfType<ViewResult>().Subject;
			var data = viewResult.Model.Should().BeAssignableTo<JobViewModel>().Subject;

			data.JobId.Should().Be(objJob.JobId);
			data.JobTitle.Should().Be(objJob.JobTitle);
			data.JobDescription.Should().Be(objJob.JobDescription);
			data.JobRequirement.Should().Be(objJob.JobRequirement);
			data.Location.Should().Be(objJob.Location);
			data.JobPostedDate.Should().BeCloseTo(objJob.JobPostedDate.Value, TimeSpan.FromSeconds(2));
			data.JobExpiredDate.Should().BeCloseTo(objJob.JobExpiredDate.Value, TimeSpan.FromSeconds(2));
		}

	}
}
