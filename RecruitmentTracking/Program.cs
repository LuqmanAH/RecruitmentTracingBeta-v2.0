using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecruitmentTracking.Data;
using RecruitmentTracking.Models;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore.Sqlite;
using System.Configuration;
//using RPauth.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RecruitmentTracking.Areas.Identity.Services;
using RecruitmentTracking;

// using Google.Apis.Auth.OAuth2;
// using Google.Apis.Gmail.v1;
// using Google.Apis.Gmail.v1.Data;
// using Google.Apis.Services;
// using Google.Apis.Util.Store;


var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

builder.Services.AddControllersWithViews();
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication()
.AddCookie(options => {
	options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
})
.AddGoogle(options =>
{
	IConfigurationSection googleAuthNSection =
	config.GetSection("Authentication:Google");
	options.ClientId = googleAuthNSection["ClientId"];
	options.ClientSecret = googleAuthNSection["ClientSecret"];
	options.CorrelationCookie.SameSite = SameSiteMode.None;
});

// Konfigurasi MailSettings dari appsettings.json
builder.Services.Configure<MailSettings>(config.GetSection("MailSettings"));

// Registrasi layanan email
builder.Services.AddTransient<IEmailSender, MailtrapEmailSender>();


var app = builder.Build();

SetRoleUser.CreateRoles(app);
SetAdmin.CreateAdminData(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseCookiePolicy(new CookiePolicyOptions
{
	MinimumSameSitePolicy = SameSiteMode.None,
	HttpOnly = HttpOnlyPolicy.Always,
	Secure = CookieSecurePolicy.Always
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
