using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
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
        [HttpPost]
        [Route("AddItemToCart")]
        public async Task<IActionResult> AddItemToCart([FromQuery] int cartId, int productId, int quantity)
        {
            var selectQuery = "SELECT Quantity FROM Cart_Has_Product WHERE CartId = @CartId AND ProductId = @ProductId";
            SqlParameter[] selectParameters =
            {
                new SqlParameter("@CartId", cartId),
                new SqlParameter("@ProductId", productId),
            };

            object result = await _dbHelper.ExecuteScalarAsync(selectQuery, selectParameters);
            int existingQuantity = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;

            if (existingQuantity > 0)
            {
                string updateQuery = "UPDATE Cart_Has_Product SET Quantity = Quantity + @Quantity WHERE CartId = @CartId AND ProductId = @ProductId";
                SqlParameter[] updateParameters =
                {
                   new SqlParameter("@CartId", cartId),
                   new SqlParameter("@ProductId", productId),
                   new SqlParameter("@Quantity", quantity + 1)
                };
                await _dbHelper.ExecuteNonQueryAsync(updateQuery, updateParameters);
                return Ok($"Item quantity updated: {existingQuantity} ➝ {existingQuantity + 1}");
            }
            else
            {
                string query = "INSERT INTO Cart_Has_Product (CartId, ProductId, Quantity) VALUES (@CartId, @ProductId, @Quantity)";
                SqlParameter[] parameters =
                {
                new SqlParameter("@CartId", cartId),
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@Quantity", quantity > 0 ? quantity : 1)
            };
                await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return Ok("New Item added to cart");
            }
        }
        [HttpDelete]
        [Route("DeleteItemFromCart")]
        public async Task<IActionResult> DeleteItemFromCart([FromQuery] int cartId, int productId)
        {
            var selectQuery = "SELECT Quantity FROM Cart_Has_Product WHERE CartId = @CartId AND ProductId = @ProductId";
            SqlParameter[] selectParameters =
            {
                new SqlParameter("@CartId", cartId),
                new SqlParameter("@ProductId", productId),
            };
            object result = await _dbHelper.ExecuteNonQueryAsync(selectQuery, selectParameters);
            int existingQuantity = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
            if(existingQuantity > 0)
            {
                string query = "DELETE FROM Cart_Has_Product WHERE CartId = @CartId AND ProductId = @ProductId";
                SqlParameter[] parameters =
                {
                new SqlParameter("@CartId", cartId),
                new SqlParameter("@ProductId", productId),
                };
                await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return Ok("Item deleted from cart");
            }
            else
            {
                return BadRequest("Item not found in cart");
            }
        }
    }
}