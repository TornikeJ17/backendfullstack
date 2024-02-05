using System.ComponentModel.DataAnnotations;

namespace reactProjectFull.Dto;

public class UserRegisterDto
{
    [Required]
    public string Email { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    [Required]
    public string Password { get; set; }
}