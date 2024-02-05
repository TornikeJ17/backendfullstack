using AutoMapper;
using reactProjectFull.Models;
using reactProjectFull.Dto;

namespace reactProjectFull.Helper;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        //Creating User
        CreateMap<User, UserDto>();
        CreateMap<UserDto, User>();
        
        //User Details and Update User
        CreateMap<User,UserDetailsDto>();
        CreateMap<UserUpdateDto,User>();
        
        //Creating Product
        CreateMap<Product, ProductDto>();
        CreateMap<ProductDto, Product>();
        
    }
    
}