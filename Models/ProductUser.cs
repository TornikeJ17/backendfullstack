namespace reactProjectFull.Models;

public class ProductUser
{
    public int ProductId { get; set; }
    public string UserId { get; set; }
    public Product Product { get; set; }
    public User User { get; set; }
}