namespace ShoppingCartWebAPI.Models;

public class CompleteCart
{
    public int CartId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Coupom Coupom { get; set; } = null!;
    public List<Product> Products { get; set; } = new List<Product>();
    public double Subtotal { get; set; }
    public double Total { get; set; }
}
