using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ShoppingCartWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CoupomController : ControllerBase
    {
        private readonly DataBaseHelper _dbHelper;
        public CoupomController(DataBaseHelper dbHelper) 
        {
            _dbHelper = dbHelper;
        }
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddCoumpom([FromQuery]int availableQuantity, int discountPercentage)
        {
            string query = "INSERT INTO Coupom (AvailableQuantity, DiscountPercentage) VALUES (@AvailableQuantity, @DiscountPercentage)";
            SqlParameter[] parameters =
            {
                new SqlParameter("@AvailableQuantity", availableQuantity),
                new SqlParameter("@DiscountPercentage", discountPercentage)
            };
            await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return Ok($"Coupom of {discountPercentage}% added successfully. Available Quantity: {availableQuantity}");
        }
    }
}
