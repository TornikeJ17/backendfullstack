using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using reactProjectFull.Data;
using reactProjectFull.Dto;
using reactProjectFull.Interfaces;
using reactProjectFull.Models;

namespace reactProjectFull.Repository;


public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public ICollection<User> GetUsers()
    {
        return _context.Users.ToList();
    }

    public ICollection<UserDetailsDto> GetUserDetails()
    {
        var users = _context.Users.ToList();

        // Use AutoMapper to map the list of User entities to a list of UserDetailsDto
        var userDetailsList = _mapper.Map<ICollection<UserDetailsDto>>(users);

        return userDetailsList;
    }
    public ICollection<Product> GetProductByUser(string userId)
    {
        var userWithProducts = _context.Users
            .Include(u => u.ProductUsers)
            .ThenInclude(pu => pu.Product)
            .SingleOrDefault(u => u.Id == userId);
        return userWithProducts?.ProductUsers.Select(pu => pu.Product).ToList() ?? new List<Product>();
    }

    public User GetUserById(string userId)
    {
       
        return _context.Users
            .AsNoTracking()
            .FirstOrDefault(u => u.Id == userId);
        
    }
}
