using System.ComponentModel.DataAnnotations.Schema;

namespace reactProjectFull.Models;

public class Product
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
    public List<string> ImageUrls { get; set; }
    public ICollection<ProductUser> ProductUsers { get; set; }
}