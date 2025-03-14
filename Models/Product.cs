namespace ShoppingCartWebAPI.Models;

public class Product
{
    public string ProductName { get; set; } = string.Empty;
    public double Price { get; set; }
    public int Quantity { get; set; }
}
