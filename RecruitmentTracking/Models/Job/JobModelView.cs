namespace RecruitmentTracking.Models;

public class JobViewModel
{
    public int JobId { get; set; }
    public string? JobTitle { get; set; }
    public string? JobDescription { get; set; }
    public string? JobRequirement { get; set; }
    public string? Location { get; set; }
    public string? JobMinEducation { get; set; }
    public string? JobDepartment { get; set; }
    public string? EmploymentType { get; set; }
    public DateTime? JobPostedDate { get; set; }
    public DateTime? JobExpiredDate { get; set; }
    public Department? Department{ get; set; }
    public int CandidateCout { get; set; }
}
