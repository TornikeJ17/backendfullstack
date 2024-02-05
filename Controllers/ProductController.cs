using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using reactProjectFull.Dto;
using reactProjectFull.Interfaces;
using reactProjectFull.Models;
using System.Security.Claims;

namespace reactProjectFull.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductController(IProductRepository productRepository, IMapper mapper)
    {
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
    public async Task<IActionResult> CreateProduct([FromForm] string userId, [FromForm] ProductDto productCreateDto, [FromForm] List<IFormFile> imageFiles)
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
        bool delete = await _productRepository.DeleteProduct(id);


        if (!delete)
        {
            // If the save fails, something went wrong with the database operation. Return a 500 error.
            return StatusCode(500, "A problem happened while handling your request.");
        }


        return await _productRepository.SaveAsync()
            ? Ok()
            : NoContent();

    }
}