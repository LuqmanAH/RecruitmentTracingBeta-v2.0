using System.ComponentModel.DataAnnotations;

namespace RecruitmentTracking.Models;

public class JobCreate
{
    public int JobId { get; set; }
    [Required(ErrorMessage = "Please fill in the required information.")]

    public string? JobTitle { get; set; }
    [Required(ErrorMessage = "Please fill in the required information.")]

    public string? JobDescription { get; set; }
    [Required(ErrorMessage = "Please fill in the required information.")]

    public string? JobRequirement { get; set; }
    public bool IsJobAvailable { get; set; }
    [Required(ErrorMessage = "Please fill in the required information.")]

    public string? JobMinEducation {get; set; }
    [Required(ErrorMessage = "Please fill in the required information.")]
    public string? JobDepartment {get; set; }
    [Required(ErrorMessage = "Please fill in the required information.")]
    public string? EmploymentType {get; set; }
    [Required(ErrorMessage = "Please fill in the required information.")]
    

    public string? Location { get; set; }
    public DateTime? JobExpiredDate { get; set; }

}
