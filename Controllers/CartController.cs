using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using ShoppingCartWebAPI.Models;
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
            try
            {
                string query = "INSERT INTO Cart (UserName) VALUES (@UserName)";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@UserName", userName)
                };
                await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return Ok($"{userName} added to cart");
            }
            catch(SqlException)
            {
                return StatusCode(500, "Database error");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPost]
        [Route("AddItemToCart")]
        public async Task<IActionResult> AddItemToCart([FromQuery] int cartId, int productId, int quantity)
        {
            try
            {
                string checkCartQuery = "SELECT COUNT(1) FROM Cart WHERE CartId = @CartId";
                string checkProductQuery = "SELECT COUNT(1) FROM Product WHERE ProductId = @ProductId";

                SqlParameter[] checkCartParameters = { new SqlParameter("@CartId", cartId) };
                SqlParameter[] checkProductParameters = { new SqlParameter("@ProductId", productId) };
                
                object cartExists = await _dbHelper.ExecuteScalarAsync(checkCartQuery, checkCartParameters);
                object productExists = await _dbHelper.ExecuteScalarAsync(checkProductQuery, checkProductParameters);

                if (cartExists == null || cartExists == DBNull.Value || Convert.ToInt32(cartExists) == 0)
                {
                    return BadRequest("Cart not found");
                }
                if (productExists == null || productExists == DBNull.Value || Convert.ToInt32(productExists) == 0)
                {
                    return BadRequest("Product not found");
                }

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
            catch(SqlException)
            {
                return StatusCode(500, "Database error.");
            }
            catch(Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpDelete]
        [Route("DeleteItemFromCart")]
        public async Task<IActionResult> DeleteItemFromCart([FromQuery] int cartId, int productId)
        {
            try
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
            catch (SqlException)
            {
                return StatusCode(500, "Database error");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpDelete]
        [Route("ClearCart")]
        public async Task<IActionResult> ClearCart([FromQuery] int cartId)
        {
            try
            {
                string selectQuery = "SELECT COUNT(*) FROM Cart_Has_Product WHERE CartId = @CartId";
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@CartId", cartId)
                };
                object result = await _dbHelper.ExecuteScalarAsync(selectQuery, sqlParameters);
                int existingQuantity = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;

                if (existingQuantity > 0)
                {
                    string query = "DELETE FROM Cart_Has_Product WHERE CartId = @CartId";
                    SqlParameter[] parameters =
                    {
                        new SqlParameter("@CartId", cartId)
                    };
                    await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                    return Ok("Cart cleared");
                }
                else
                {
                    return BadRequest("Cart is empty");
                }
            }
            catch (SqlException)
            {
                return StatusCode(500, "Database error.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPut]
        [Route("AddCouponToCart")]
        public async Task<IActionResult> AddCouponToCart([FromQuery] int cartId, int coupomId)
        {
            try
            {
                string selectQuery = "SELECT AvailableQuantity FROM Coupom WHERE CoupomId = @CoupomId";
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@CoupomId", coupomId)
                };
                object result = await _dbHelper.ExecuteScalarAsync(selectQuery, sqlParameters);
                int availableQuantity = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;

                if (availableQuantity > 0)
                {
                    using (var transaction = await _dbHelper.BeginTransactionAsync())
                    {
                        try
                        {
                            string updateQuery = "UPDATE Coupom SET AvailableQuantity = @AvailableQuantity WHERE CoupomId = @CoupomId";
                            SqlParameter[] sqlUpdateParameters =
                            {
                                new SqlParameter("@AvailableQuantity", availableQuantity - 1),
                                new SqlParameter("@CoupomId", coupomId)
                            };
                            await _dbHelper.ExecuteNonQueryAsync(updateQuery, sqlUpdateParameters, transaction);

                            string cartQuery = "UPDATE Cart SET CoupomId = @CoupomId WHERE CartId = @CartId";
                            SqlParameter[] cartParameters =
                            {
                                new SqlParameter("@CoupomId", coupomId),
                                new SqlParameter("@CartId", cartId)
                            };
                            await _dbHelper.ExecuteNonQueryAsync(cartQuery, cartParameters, transaction);

                            await transaction.CommitAsync();
                            return Ok("Coupom added to cart");
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            return StatusCode(500, "Internal server error");
                        }
                    }
                }
                else
                {
                    return BadRequest("Coupom is not available");
                }
            }
            catch (SqlException)
            {
                return StatusCode(500, "Database error.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet]
        [Route("CompleteCart")]
        public async Task<IActionResult> CompleteCart([FromQuery] int cartId)
        {
            try
            {
                string query = "SELECT c.CartId, c.UserName, c.CoupomId, p.ProductName, p.Price, cp.Quantity, co.DiscountPercentage " +
                               "FROM Cart_Has_Product cp " +
                               "INNER JOIN Cart c ON cp.CartId = c.CartId " +
                               "INNER JOIN Product p ON cp.ProductId = p.ProductId " +
                               "LEFT JOIN Coupom co ON c.CoupomId = co.CoupomId " +
                               "WHERE c.CartId = @CartId";
                
                SqlParameter[] parameter =
                {
            new SqlParameter("@CartId", cartId)
        };

                using (var reader = await _dbHelper.ExecuteReaderAsync(query, parameter))
                {
                    if (!await reader.ReadAsync())
                    {
                        return NotFound("Cart not found");
                    }

                    var completeCart = new CompleteCart()
                    {
                        CartId = reader.GetInt32(0),
                        UserName = reader.GetString(1),
                        Coupom = new Coupom()
                        {
                            DiscountPercentage = reader.IsDBNull(6) ? 0 : reader.GetInt32(6)
                        },
                        Products = new List<Product>()
                    };

                    do
                    {
                        string productName = reader.GetString(3);
                        double price = (double)reader.GetDecimal(4);
                        int quantity = reader.GetInt32(5);

                        var product = new Product()
                        {
                            ProductName = productName,
                            Price = price,
                            Quantity = quantity
                        };
                        completeCart.Products.Add(product);

                    } while (await reader.ReadAsync());

                    completeCart.Subtotal = completeCart.Products.Sum(p => p.Price * p.Quantity);
                    completeCart.Total = completeCart.Subtotal - ((completeCart.Subtotal * completeCart.Coupom.DiscountPercentage) / 100);
                    return Ok(completeCart);
                }
            }
            catch (SqlException)
            {
                return StatusCode(500, "Database error.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

    }
}