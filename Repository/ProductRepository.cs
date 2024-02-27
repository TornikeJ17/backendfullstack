using AutoMapper;
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
    private readonly IMapper _mapper;

    public ProductRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

    public async Task<bool> CreateProductAsync(string userId, Product product, List<IFormFile> imageFiles = null)
    {
        var productUser = new ProductUser()
        {
            Product = product,
            UserId = userId
        };
        var imageFile = product.ImageUrls;
        if (imageFile != null && imageFile.Count > 0)
        {
            product.ImageUrls = new List<string>();
            foreach (var file in imageFile)
            {
                if (file.Length > 0)
                {
                    string uploadsFolder = Path.Combine("Resources", "Images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Path.GetFileNameWithoutExtension(file) + "_" + Guid.NewGuid() + Path.GetExtension(file);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
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


    
   public async Task<bool> UpdateProduct(ProductDto productDto, List<IFormFile>? imageFiles = null)
{
    
    var product = await _context.Products
        .FirstOrDefaultAsync(p => p.Id == productDto.Id);

    if (product == null)
    {
        Console.WriteLine($"Product with ID: {productDto.Id} not found.");
        return false;
    }
    var imageFile = _context.Products.FirstAsync(find => find.ImageUrls == productDto.ImageUrls);
    
    // Update properties manually
    product.ProductName = productDto.ProductName;
    product.ProductPrice = productDto.ProductPrice;
    product.ProductCode = productDto.ProductCode;
    product.ProductSKU = productDto.ProductSKU;
    product.ProductDescription = productDto.ProductDescription;
    product.ProductPublish = productDto.ProductPublish;
    product.ProductTags = productDto.ProductTags;
    product.ProductCategory = productDto.ProductCategory;
    product.inStock = productDto.inStock;



    if (imageFiles is not null)
    {

        // Handle image removal
        var currentImageUrls = product.ImageUrls ?? new List<string>();
        var updatedImageUrls = productDto.ImageUrls ?? new List<string>();

        var imagesToRemove = currentImageUrls.Except(updatedImageUrls).ToList();
        foreach (var imageUrl in imagesToRemove)
        {
            // Delete the actual image file if needed
            var imagePath = Path.Combine("Resources", "Images");
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }

            // Remove from the product's image URL list
            product.ImageUrls.Remove(imageUrl);
        }

        // Handle new image uploads
        if (imageFiles != null && imageFiles.Count > 0)
        {
            foreach (var file in imageFiles)
            {
                if (file.Length > 0)
                {
                    string uploadsFolder = Path.Combine("Resources", "Images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder); // Now checks and creates if doesn't exist.
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    string imageUrl = $"/Resources/Images/{uniqueFileName}";
                    product.ImageUrls.Add(imageUrl); // Add new image URL to the product's image URL list
                }
            }
        }
    }


    try
    {
        await _context.SaveChangesAsync();
        return true;
    }
    catch (Exception ex)
    {
        // Log the exception to help diagnose the issue
        // Log.Error(ex, "An error occurred while updating the product with ID {ProductId}", productDto.Id);
        // For simplicity, we'll just write to the console. Replace this with proper logging.
        Console.WriteLine($"An error occurred while updating the product with ID {productDto.Id}: {ex.Message}");
        return false;
    }
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