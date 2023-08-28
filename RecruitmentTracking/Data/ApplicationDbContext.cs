using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecruitmentTracking.Models;

namespace RecruitmentTracking.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
	public ApplicationDbContext()
	{
	}

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
	: base(options)
	{
	}

	//public DbSet<User>? Users { get; set; } = default;
	public DbSet<Job>? Jobs { get; set; }
	public DbSet<UserJob>? UserJobs { get; set; }
	public DbSet<Candidate> Candidates { get; set; }
	public DbSet<Department> Departments { get; set; }
}
