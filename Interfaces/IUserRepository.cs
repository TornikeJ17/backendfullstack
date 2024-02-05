using reactProjectFull.Dto;
using reactProjectFull.Models;

namespace reactProjectFull.Interfaces;

public interface IUserRepository
{
     ICollection<User> GetUsers();
     ICollection<UserDetailsDto> GetUserDetails();
     ICollection<Product> GetProductByUser(string userId);
     User GetUserById(string userId);
}