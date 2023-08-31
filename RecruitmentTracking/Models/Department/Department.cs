using System.ComponentModel.DataAnnotations;

namespace RecruitmentTracking.Models;

public class Department
{
    [Key]
    public int DepartmentId { get; set; }
    
    public string? DepartmentName { get; set; }
}