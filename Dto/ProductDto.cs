using reactProjectFull.Models;

namespace reactProjectFull.Dto;

public class ProductDto
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public int ProductCode { get; set; }
    public int ProductSKU { get; set; }
    public string ProductDescription { get; set; }
    public ProductPublish ProductPublish { get; set; }
    public ProductTags ProductTags { get; set; }
    public ProductCategory ProductCategory { get; set; }
    public bool inStock { get; set; }
    public ICollection<string> ImageUrls { get; set; }
}