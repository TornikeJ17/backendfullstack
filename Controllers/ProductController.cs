using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using reactProjectFull.Data;
using reactProjectFull.Dto;
using reactProjectFull.Interfaces;
using reactProjectFull.Models;
using System.Security.Claims;

namespace reactProjectFull.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductController(DataContext context,IProductRepository productRepository, IMapper mapper)
    {
        _context = context;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult> GetProducts()
    {
        try
        {
            var productEntities = await _productRepository.GetProducts(); // Use the async method

            if (productEntities == null)
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var products = _mapper.Map<List<ProductDto>>(productEntities);

            return Ok(products);
        }
        catch (Exception ex)
        {
            // Log the exception
            // Return an appropriate error response
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        if (!_productRepository.ProductExists(id))
            return NotFound();

        var productEntity = await _productRepository.GetProductAsync(id);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = _mapper.Map<ProductDto>(productEntity);

        return Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateProduct([FromForm] string userId, [FromForm] ProductDto productCreateDto, [FromForm] List<IFormFile> imageFiles = null)
    {
        if (productCreateDto == null)
            return BadRequest(ModelState);

        if (_productRepository.ProductExists(productCreateDto.Id))
        {
            ModelState.AddModelError("", "Product already exists");
            return StatusCode(422, ModelState);
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var productEntity = _mapper.Map<Product>(productCreateDto);

        var result = await _productRepository.CreateProductAsync(userId, productEntity, imageFiles);
        if (!result)
        {
            ModelState.AddModelError("", "Something went wrong saving the product");
            return StatusCode(500, ModelState);
        }

        var createdProductDto = _mapper.Map<ProductDto>(productEntity);
        return CreatedAtAction(nameof(GetProduct), new { id = createdProductDto.Id }, createdProductDto);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        if (!_productRepository.ProductExists(id))
            return NotFound();

        if (!ModelState.IsValid)
            return BadRequest();

        var result = await _productRepository.DeleteProduct(id);

        // Return NoContent for successful deletion, otherwise return appropriate error.
        return result ? NoContent() : StatusCode(500, "Error when trying to delete the product");
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDto productUpdateDto, [FromForm] List<IFormFile>? imageFiles = null)
    {
        
        // Check if the product exists
        if (!_productRepository.ProductExists(id))
            return NotFound();

        // Check if the ID from the path matches the ID in the DTO
        if (id != productUpdateDto.Id)
        {
            return BadRequest("Mismatched product ID");
        }

        // Validate the DTO
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        Console.WriteLine($"Received update for Product ID {id}");
        Console.WriteLine($"Name: {productUpdateDto.ProductName}, Price: {productUpdateDto.ProductPrice}");


        bool result = await _productRepository.UpdateProduct(productUpdateDto, imageFiles ?? Enumerable.Empty<IFormFile>().ToList()); // Assuming imageFiles is obtained correctly

        if (result)
        {
            var updatedProduct = await _context.Products.FindAsync(id);
            return Ok(updatedProduct);
        }
        else
        {
            return BadRequest("Failed to update product.");
        }
    }
}