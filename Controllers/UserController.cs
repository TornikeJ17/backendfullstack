using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using reactProjectFull.Dto;
using reactProjectFull.Interfaces;
using reactProjectFull.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace reactProjectFull.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public UserController(IUserRepository userRepository, IMapper mapper, UserManager<User> userManager, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpGet("Avatars")]
    public async Task<ActionResult> GetAvatars()
    {
        var avatarDirectory = Path.Combine("wwwroot", "Avatars");
        var avatarFiles = Directory.GetFiles(avatarDirectory)
            .Select(Path.GetFileName)
            .ToList();
        avatarFiles.RemoveAll(i => i == ".DS_Store");
        return Ok(avatarFiles);
    }
    [HttpGet]
    public async Task<ActionResult> GetUsers()
    {
        var users = _mapper.Map<List<UserDto>>(_userRepository.GetUsers());
            
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        return Ok(users);
    }

    [HttpGet("userDetails")]
    public async Task<ActionResult> GetUserDetails()
    {
        var users = _mapper.Map<List<UserDetailsDto>>(_userRepository.GetUserDetails());
            
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        return Ok(users);
    }
    [HttpGet("userDetails/{id}")]
    public async Task<ActionResult> GetUserDetailsById(string id)
    {
        try
        {
            var user = _userRepository.GetUserById(id); // This method should return a User object, not just the ID
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userDetailsDto = _mapper.Map<UserDetailsDto>(user); // Map the User object to UserDetailsDto
            return Ok(userDetailsDto); // Return the mapped DTO
        }
        catch (Exception ex)
        {
            // Use a proper logging framework to log the exception
            // For example: _logger.LogError(ex, "An error occurred while processing the GetUserDetailsById request.");

            // Return a generic error message to the client
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
  
    [HttpGet("{userId}/product")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ProductDto>))]
    [ProducesResponseType(400)]
    public IActionResult GetProductByUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return NotFound("User not found.");

        // Map the products to DTOs
        var products = _userRepository.GetProductByUser(userId);
        var productDtos = _mapper.Map<List<ProductDto>>(products);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return Ok(productDtos);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto userDto)
    {
        var userExists = await _userManager.FindByEmailAsync(userDto.Email);
        if (userExists != null)
        {
            return BadRequest("User already exists!");
        }

        User user = new User
        {
            UserName = userDto.Email,
            Email = userDto.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            FirstName = userDto.FirstName, 
            LastName = userDto.LastName,
        };

        var result = await _userManager.CreateAsync(user, userDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok("User created successfully!");
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserDto userDto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(userDto.Email);

            if (user == null)
            {
                return NotFound("User not found.");
            }
            if (user != null && await _userManager.CheckPasswordAsync(user, userDto.Password))
            {
                var authClaims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                };

                // Ensure this line is inside the method scope
                var secret = _configuration["JwtSettings:Secret"];
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

                var token = new JwtSecurityToken(
                    issuer: null, 
                    audience: null, 
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
                var userDetailsDto = _mapper.Map<UserDetailsDto>(user);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token).ToString(),
                    expiration = token.ValidTo,
                    userDetails = userDetailsDto
                });
            }
            return Unauthorized();
        }
        catch (Exception ex)
        {
            // Optional: Log the exception
            return StatusCode(500, "Internal Server Error: " + ex.Message); 
        }
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromForm] UserUpdateDto userUpdateDto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Use AutoMapper to update the User entity with the fields from UserUpdateDto
        _mapper.Map(userUpdateDto, user);
    
        // Save changes made by AutoMapper
        var updateUserResult = await _userManager.UpdateAsync(user);
        if (!updateUserResult.Succeeded)
        {
            return BadRequest(updateUserResult.Errors);
        }

        if (!string.IsNullOrEmpty(userUpdateDto.ImageUrls))
        {
            string filePath = $"/Avatars/{userUpdateDto.ImageUrls}";
            user.ImageUrls = filePath; // Assuming the ImageUrls field stores the path to the avatar
        }

        return NoContent(); // 204 No Content response
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
          
            var userWithProducts = _userRepository.GetProductByUser(id);
            if (userWithProducts.Count > 0)
            {
                // Prevent deletion if user has products
                return BadRequest("User with products cannot be deleted.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                // No user found with the given id
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                // Deletion failed, return a server error
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting user");
            }

            // Successfully deleted the user
            return Ok(user);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // Log the exception e using a logging framework
            // Return a server error
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while processing your request.");
        }
    }
}