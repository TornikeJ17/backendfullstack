using System.ComponentModel.DataAnnotations;

namespace reactProjectFull.Dto;

public class UserDto
{
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }
}