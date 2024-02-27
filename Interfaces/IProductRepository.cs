using Microsoft.AspNetCore.Mvc;
using reactProjectFull.Dto;
using reactProjectFull.Models;

namespace reactProjectFull.Interfaces;

public interface IProductRepository
{
     Task<ICollection<Product>> GetProducts();
     Task<Product> GetProductAsync(int id);
     bool ProductExists(int id);
     Task<bool> CreateProductAsync(string userId, Product product, List<IFormFile> imageFiles);
     Task<bool> UpdateProduct(ProductDto productDto,List<IFormFile> imageFiles);
     Task<bool> DeleteProduct(int id);
     Task<bool> SaveAsync();
     
}