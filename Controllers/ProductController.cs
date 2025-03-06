using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ShoppingCartWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly DataBaseHelper _dbHelper;
        public ProductController(DataBaseHelper dbHelper) 
        {
            _dbHelper = dbHelper;
        }
        public async Task<IActionResult> AddProduct([FromQuery] string productName, double price)
        {
            string query = "INSERT INTO Product (ProductName, Price) VALUES (@ProductName, @Price)";
            SqlParameter[] parameters =
            {
                new SqlParameter("@ProductName", productName),
                new SqlParameter("@Price", price)
            };
            await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return Ok($"{productName} added to cart");
        }
    }
}
