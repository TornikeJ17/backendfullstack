using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using reactProjectFull.Data;
using reactProjectFull.Dto;
using reactProjectFull.Interfaces;
using reactProjectFull.Models;
using System.Collections;
using System.Security.Claims;

namespace reactProjectFull.Repository;

public class ProductRepository : IProductRepository
{
    private readonly DataContext _context;

    public ProductRepository(DataContext context)
    {
        _context = context;
    }
    public async Task<ICollection<Product>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product> GetProductAsync(int id)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
    }

    public bool ProductExists(int id)
    {
        return _context.Products.Any(p => p.Id == id);
    }

    public async Task<bool> CreateProductAsync(string userId, Product product, List<IFormFile> imageFiles)
    {
        var productUser = new ProductUser()
        {
            Product = product,
            UserId = userId
        };
        if (imageFiles != null && imageFiles.Count > 0)
        {
            product.ImageUrls = new List<string>();
            foreach (var file in imageFiles)
            {
                if (file.Length > 0)
                {
                    string uploadsFolder = Path.Combine("Resources", "Images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    string imageUrl = $"/Resources/Images/{uniqueFileName}";
                    product.ImageUrls.Add(imageUrl);
                }
            }
        }
        

        _context.Products.Add(product);
        _context.Add(productUser);
        return await SaveAsync();
    }
    

    public bool UpdateProduct(Product product)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteProduct(int id)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);
        return await SaveAsync();
    }

    public async Task<bool> SaveAsync()
    {
        return (await _context.SaveChangesAsync()) >= 0;
    }
}