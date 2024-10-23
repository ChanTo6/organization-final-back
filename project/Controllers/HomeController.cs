using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using project.packages;
using project.Model;
using System.Data;
using project.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        IPKG_TO_DO package;
        private readonly IPKG_TO_DO _package;
        private IConfiguration _configuration;

        public HomeController(IPKG_TO_DO package, IConfiguration configuration)
        {
            _package = package;
            _configuration = configuration;
        }


        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser(UserData userData)
        {
            try
            {

                await _package.CreateUser(userData.Email, userData.EmployeeLastName, userData.EmployeeName, userData.OrganizationAddress, userData.OrganizationName, userData.Password, userData.personId, userData.PhoneNumber, userData.Role);


                return Ok(new { message = "User created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPost("UpdateUserByPersonId")]
        public async Task<IActionResult> UpdateUserByPersonId([FromBody] Update request)
        {
            Console.WriteLine(request);
            try
            {
                await _package.UpdateUserByPersonId(
            request.PersonId,
            request.EmployeeName,
            request.EmployeeSurname,
            request.Password,
            request.Role,
            request.Telephone,
            request.OrgName,
            request.Warehouse
                );
                return Ok(new { message = "User updated successfully" });
            }
            catch (OracleException ex)
            {
                return BadRequest(new { message = $"Oracle error occurred: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] int userId)
        {
            try
            {
                Console.WriteLine(userId);
                string result = await _package.DeleteUser(userId);
                return Ok(result); // Return the success message
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred while deleting the user."); 
            }
        }



        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct([FromBody] ProductUpdate productDto)
        {
            if (productDto == null)
            {
                return BadRequest(new { message = "Invalid product data." });
            }

            try
            {
              
                if (string.IsNullOrWhiteSpace(productDto.warehouse))
                {
                    return BadRequest(new { message = "Warehouse name is required." });
                }

               
                await _package.AddProductToWarehouse(
                    productDto.ProductName,
                    productDto.quantity,
                    productDto.userId,
                    productDto.warehouse, 
                    productDto.Location
                );
            
                return Ok(new { message = "Product added successfully." });
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPost("EditProduct")]
        public async Task<IActionResult> EditProduct([FromBody] Product productDto)
        {
            if (productDto == null)
            {
                return BadRequest(new { message = "Invalid product data." });
            }

            try
            {
                await _package.EditProductInWarehouse(
                    productDto.ProductName,
                    productDto.barcode,
                    productDto.ProductId,
                    productDto.quantity,
                    productDto.userId
                );

                return Ok(new { message = "Product updated successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpGet("FetchProducts")]
        public async Task<IActionResult> FetchProducts()
        {
            try
            {
                var products = await _package.FetchProducts();

                if (products == null || !products.Any())
                {
                    return NotFound(new { message = "No products found." });
                }

                return Ok(products);
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpGet("GetRemovedProducts")]
        public async Task<IActionResult> GetRemovedProducts()
        {
            try
            {
                var products = await _package.GetRemovedProducts();

                if (products == null || !products.Any())
                {
                    return NotFound(new { message = "No products found." });
                }

                return Ok(products);
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPost("GetRemovedProductsByUserId")]
        public async Task<IActionResult> GetRemovedProductsByUserId([FromBody] int userId)
        {

            try
            {
                var products = await _package.GetRemovedProductsByUserId(userId);

                if (products == null || !products.Any())
                {
                    return NotFound(new { message = "No products found for the specified user." });
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred: {ex.Message}" });
            }
        }


        //  [Authorize(Roles = "operator")]
        [HttpGet("FetchProductbyuserId/{userId}")]
        public async Task<IActionResult> FetchProductbyuserId(int userId)
        {
            Console.WriteLine(userId);
            try
            {
                var products = await _package.FetchProductbyuserId(userId);

                if (products == null || !products.Any())
                {
                    return NotFound(new { message = "No products found for the specified user." });
                }

                return Ok(products);
            }
            catch (OracleException oracleEx)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Database error: {oracleEx.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred: {ex.Message}" });
            }
        }


        //  [Authorize(Roles = "manager")]
        [HttpPost]
        [Route("RemoveProduct")]
        public async Task<IActionResult> RemoveProductAsync([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Product data is required.");
            }

            try
            {
                await _package.RemoveProductAsync(product.userId, product.barcode, product.quantity);
                return Ok();
            }
            catch (Exception ex)
            {

                return BadRequest($"Error: {ex.Message}");
            }
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request." });
            }

            try
            {
                var loginResponse = await _package.LoginUser(request.UserName, request.Password);

                if (loginResponse != null)
                {
                    return Ok(new
                    {
                        token = loginResponse.Token,
                        role = loginResponse.Role,
                        userId = loginResponse.UserId
                    });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid username or password." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred: {ex.Message}" });
            }
        }


        public static UserData user = new UserData();


        //   [Authorize(Roles = "admin")]
        [HttpGet("GetAllProjectUsersAsync")]
        public async Task<IActionResult> GetAllProjectUsersAsync()
        {


            var users = await _package.GetAllProjectUsersAsync();


            return Ok(users);
        }


        [HttpPost("UpdateUserStatus")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UpdateUserStatusRequest request)
        {
            try
            {
                await _package.UpdateUserStatus(request.UserId, request.Status);
                return Ok(new { message = "User status updated successfully." });
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred: {ex.Message}" });
            }
        }


        //    [Authorize(Roles = "admin")]
        [HttpGet("GetAllOrganizationNamesAsync")]
        public async Task<IActionResult> GetAllOrganizationNamesAsync()
        {
            try
            {
                var orgNames = await _package.GetAllOrganizationNamesAsync();
                return Ok(orgNames);
            }
            catch (Exception ex)
            {


                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [HttpGet("check-free-seats/{userId}")]
        public async Task<IActionResult> CheckFreeSeatsByUserIdAsync(int userId)
        {
            try
            {
                var warehouseList = await _package.CheckFreeSeatsByUserIdAsync(userId);

                if (warehouseList == null || !warehouseList.Any())
                {
                    return NotFound($"No warehouses found for user ID: {userId}");
                }

                return Ok(warehouseList);
            }
            catch (Exception ex)
            {
                // Log the exception (using a logging framework is recommended)
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [HttpGet("check-free-seats")]
        public async Task<IActionResult> CheckFreeSeatsAllWarehousesAsync()
        {
            try
            {
                var warehouseList = await _package.CheckFreeSeatsAllWarehousesAsync();

                if (warehouseList == null || !warehouseList.Any())
                {
                    return NotFound("No warehouses found.");
                }

                return Ok(warehouseList);
            }
            catch (Exception ex)
            {
                // Log the exception (using a logging framework is recommended)
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [HttpGet("FetchNameAndLocation")]
        public async Task<IActionResult> FetchNameAndLocation()
        {
            try
            {
                var warehouses = await _package.FetchNameAndLocation();

                if (warehouses == null || !warehouses.Any())
                {
                    return NotFound(new { message = "No warehouses found." });
                }

                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPost("AddWarehouse")]
        public async Task<IActionResult> AddWarehouse([FromBody] WarehouseInfo warehouseData)
        {
            if (warehouseData == null)
            {
                return BadRequest(new { message = "Warehouse data is required" });
            }

            try
            {
                await _package.AddWarehouse(
                    warehouseData.UserId,
                    warehouseData.WarehouseName,
                    warehouseData.Location
                );

                return Ok(new { message = "Warehouse added successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
