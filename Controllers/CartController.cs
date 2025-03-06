using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;

namespace ShoppingCartWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly DataBaseHelper _dbHelper;
        public CartController(DataBaseHelper dbHelper) 
        {
            _dbHelper = dbHelper;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddCart([FromQuery] string userName)
        {
            string query = "INSERT INTO Cart (UserName) VALUES (@UserName)";
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserName", userName)
            };
            await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return Ok($"{userName} added to cart");
        }
    }
}
    