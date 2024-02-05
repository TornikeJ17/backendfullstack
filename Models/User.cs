using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace reactProjectFull.Models;

public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Company { get; set; }
    public int? ContactNumber { get; set; }
    public string? CompanySite { get; set; }
    public string? Country { get; set; }
    public string? Address { get; set; }
    public string? ImageUrls { get; set; }
    public ICollection<ProductUser> ProductUsers { get; set; }
}