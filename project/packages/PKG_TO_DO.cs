using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using project.Model;
using project.Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace project.packages
{

    public interface IPKG_TO_DO
    {
        public Task CreateUser(string email, string employeeLastName, string employeeName, string organizationAddress, string organizationName, string password, int personId, int? phoneNumber, string role);
        public Task<LoginResponse> LoginUser(string username, string password);
        public Task UpdateUserByPersonId(int personId, string employeeName, string employeeSurname, string password, string role, string telephone, string orgName,string Warehouse);
        public Task<string> DeleteUser(int userId);
        public Task<int> RegisterOrganizationAsync(string orgName, string address, string email, string telephone);
        public Task OrgLoginAsync(string username, string password);
        public Task RemoveProductAsync(int userId, string barcode, int quantity);
        public Task<List<Balance>> ViewBalances(int orgId);
        public Task<List<ProjectUser>> GetAllProjectUsersAsync();
        public Task AddProductToWarehouse(string productName, int quantity, int userId, string warehouseName, string location);
        public Task EditProductInWarehouse(string ProductName, string Barcode, int ProductId, int Quantity, int UserId);
        public  Task<List<Product>> FetchProductbyuserId(int userId);
        public  Task<List<Product>> FetchProducts();
        public Task<List<Product>> GetRemovedProducts();
        public Task UpdateUserStatus(int userId, int status);
        public Task<List<string>> GetAllOrganizationNamesAsync();
        public Task<List<Product>> GetRemovedProductsByUserId(int userId);
        public Task<List<WarehouseInfo>> CheckFreeSeatsByUserIdAsync(int userId);
        public Task<List<WarehouseInfo>> CheckFreeSeatsAllWarehousesAsync();
        public Task<List<WarehouseInfo>> FetchNameAndLocation();
        public Task AddWarehouse(int userId, string warehouseName, string location);
    }
    public class PKG_TO_DO : PKG_BASE, IPKG_TO_DO
    {
        private readonly IConfiguration _configuration;

        public PKG_TO_DO(IConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;
        }
        UserData user = new UserData();
        public async Task CreateUser(string email, string employeeLastName, string employeeName, string organizationAddress, string organizationName, string password, int personId, int? phoneNumber, string role)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            try
            {
                using (OracleConnection conn = new OracleConnection(Connstr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("PROJECT_pkg.create_user", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = email;
                        cmd.Parameters.Add("p_employee_lastname", OracleDbType.Varchar2).Value = employeeLastName;
                        cmd.Parameters.Add("p_employee_name", OracleDbType.Varchar2).Value = employeeName;
                        cmd.Parameters.Add("p_organizationAddress", OracleDbType.Varchar2).Value = organizationAddress;
                        cmd.Parameters.Add("p_org_name", OracleDbType.Varchar2).Value = organizationName;
                        cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = hashedPassword;
                        cmd.Parameters.Add("p_telephone", OracleDbType.Int32).Value = phoneNumber;
                        cmd.Parameters.Add("p_role", OracleDbType.Varchar2).Value = role;
                        cmd.Parameters.Add("p_person_id", OracleDbType.Int32).Value = personId;


                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("OracleException: " + ex.Message);
                Console.WriteLine("Error Code: " + ex.ErrorCode);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
            }
        }

        public async Task<List<ProjectUser>> GetAllProjectUsersAsync()
        {
            var users = new List<ProjectUser>();

            try
            {
                using (OracleConnection conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();
                    Console.WriteLine("Database connection opened.");

                    using (OracleCommand cmd = new OracleCommand("PROJECT_pkg.GET_ALL_PROJECT_USERS", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        Console.WriteLine("Executing stored procedure.");

                        using (OracleDataReader reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                        {
                            Console.WriteLine("Stored procedure executed. Reading data.");

                            while (await reader.ReadAsync())
                            {
                                var user = new ProjectUser
                                {
                                    UserId = reader.IsDBNull(reader.GetOrdinal("user_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("user_id")),
                                    Password = reader.IsDBNull(reader.GetOrdinal("password")) ? string.Empty : reader.GetString(reader.GetOrdinal("password")),
                                    Role = reader.IsDBNull(reader.GetOrdinal("role")) ? string.Empty : reader.GetString(reader.GetOrdinal("role")),
                                    EmployeeName = reader.IsDBNull(reader.GetOrdinal("employee_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("employee_name")),
                                    EmployeeSurname = reader.IsDBNull(reader.GetOrdinal("employee_surname")) ? string.Empty : reader.GetString(reader.GetOrdinal("employee_surname")),
                                    PersonId = reader.IsDBNull(reader.GetOrdinal("person_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("person_id")),
                                    Telephone = reader.IsDBNull(reader.GetOrdinal("user_telephone")) ? string.Empty : reader.GetString(reader.GetOrdinal("user_telephone")),
                                    OrgName = reader.IsDBNull(reader.GetOrdinal("org_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("org_name")),
                                    OrgEmail = reader.IsDBNull(reader.GetOrdinal("org_email")) ? string.Empty : reader.GetString(reader.GetOrdinal("org_email")),
                                    OrgTelephone = reader.IsDBNull(reader.GetOrdinal("org_telephone")) ? string.Empty : reader.GetString(reader.GetOrdinal("org_telephone")),
                                    IsActive = reader.IsDBNull(reader.GetOrdinal("is_active")) ? false : reader.GetInt32(reader.GetOrdinal("is_active")) == 1,
                                };

                                users.Add(user);
                                Console.WriteLine($"User added: {user.EmployeeName} {user.EmployeeSurname}");
                            }

                            Console.WriteLine($"Total users retrieved: {users.Count}");
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}");
                Console.WriteLine($"Error Code: {ex.ErrorCode}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            return users;
        }

        public async Task UpdateUserByPersonId(int personId, string employeeName, string employeeSurname,string password, string role, string telephone, string orgName,string warehouse)
        {
            string hashedPassword = string.IsNullOrEmpty(password) ? password : BCrypt.Net.BCrypt.HashPassword(password);

            try
            {
                int telephoneInt;
                if (!int.TryParse(telephone, out telephoneInt))
                {
                    throw new ArgumentException("Telephone must be a valid integer.");
                }

                using (OracleConnection conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();

                    using (OracleCommand cmd = new OracleCommand("PROJECT_pkg.UPDATE_USER_BY_PERSON_ID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_person_id", OracleDbType.Int32).Value = personId;
                        cmd.Parameters.Add("p_employee_name", OracleDbType.Varchar2).Value = employeeName;
                        cmd.Parameters.Add("p_employee_surname", OracleDbType.Varchar2).Value = employeeSurname;
                        cmd.Parameters.Add("p_role", OracleDbType.Varchar2).Value = role;
                        cmd.Parameters.Add("p_org_name", OracleDbType.Varchar2).Value = orgName;
                        cmd.Parameters.Add("p_telephone", OracleDbType.Int32).Value = telephoneInt; 
                        cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = hashedPassword;
                        cmd.Parameters.Add("p_warehouse", OracleDbType.Varchar2).Value = warehouse;
                        Console.WriteLine($"Updating user: {employeeName} {employeeSurname}, Role: {role}, Org: {orgName}, Telephone: {telephoneInt}, Warehouse: {warehouse}");
                        await cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<string> DeleteUser(int userId)
        {
            string message;
            using (OracleConnection conn = new OracleConnection(Connstr))
            {
                await conn.OpenAsync();
                using (OracleCommand cmd = new OracleCommand("PROJECT_pkg.delete_user", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_person_id", OracleDbType.Int32).Value = userId;
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            message = "User deleted successfully";
            return message;
        }


        public async Task<List<Balance>> ViewBalances(int orgId)
        {
            var balances = new List<Balance>();

            try
            {
                using (OracleConnection conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();

                    using (OracleCommand cmd = new OracleCommand("PROJECT_pkg.view_balances", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_org_id", OracleDbType.Int32).Value = orgId;

                        await cmd.ExecuteNonQueryAsync();
                    }

                    using (OracleCommand queryCmd = new OracleCommand("SELECT product_name, quantity FROM PROJECT_WAREHOUSE WHERE org_id = :org_id", conn))
                    {
                        queryCmd.Parameters.Add("org_id", OracleDbType.Int32).Value = orgId;

                        using (OracleDataReader reader = (OracleDataReader)await queryCmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string productName = reader.GetString(0);
                                int quantity = reader.GetInt32(1);

                                balances.Add(new Balance { ProductName = productName, Quantity = quantity });
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }

            return balances;
        }
        private string CreateToken(UserData user)
        {
            List<Claim> claims = new List<Claim>
    {
        new Claim(ClaimTypes.Role, user.Role ?? "User"),
        new Claim(ClaimTypes.NameIdentifier, user.personId.ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
        public async Task<LoginResponse> LoginUser(string username, string password)
        {
            UserData userData = null;

            try
            {
                using (var conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();

                    using (var cmd = new OracleCommand("PROJECT_pkg.login_user", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_username", OracleDbType.Varchar2).Value = username;
                        var pUserData = new OracleParameter("p_user_data", OracleDbType.RefCursor)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(pUserData);

                        await cmd.ExecuteNonQueryAsync();
                        using (var reader = ((OracleRefCursor)pUserData.Value).GetDataReader())
                        {
                            if (await reader.ReadAsync())
                            {
                                var hashedPassword = reader.GetString(4);

                                if (BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                                {
                                    userData = new UserData
                                    {
                                        Role = reader.GetString(3),
                                        personId = reader.GetInt32(0)
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return null;
            }

            if (userData == null)
            {
                return null;
            }

            var token = CreateToken(userData);
            return new LoginResponse
            {
                Token = token,
                Role = userData.Role,
                UserId = userData.personId
            };
        }
        public async Task EditProductInWarehouse(string ProductName, string Barcode, int ProductId, int Quantity, int UserId)
        {
            try
            {
                using (var conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();

                    using (var cmd = new OracleCommand("PROJECT_pkg.EDIT_PRODUCT_IN_WAREHOUSE", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_product_id", OracleDbType.Int32).Value = ProductId;
                        cmd.Parameters.Add("p_product_name", OracleDbType.Varchar2).Value = ProductName;
                        cmd.Parameters.Add("p_quantity", OracleDbType.Int32).Value = Quantity;
                        cmd.Parameters.Add("p_barcode", OracleDbType.Varchar2).Value = Barcode;
                        cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = UserId;

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }
        public async Task<List<Product>> FetchProductbyuserId(int userId)
        {
            var products = new List<Product>();

            try
            {
                using (var conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();
                    using (var cmd = new OracleCommand("PROJECT_pkg.FetchProductsByUserId", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        var pUserId = new OracleParameter("p_user_id", OracleDbType.Int32)
                        {
                            Value = userId
                        };
                        cmd.Parameters.Add(pUserId);

                        var pResultSet = new OracleParameter("p_result_set", OracleDbType.RefCursor)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(pResultSet);

                        await cmd.ExecuteNonQueryAsync();

                        using (var reader = ((OracleRefCursor)pResultSet.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                products.Add(new Product
                                {
                                    ProductId = reader.GetInt32(0),
                                    userId = reader.GetInt32(1),
                                    ProductName = reader.GetString(2),
                                    quantity = reader.GetInt32(3),
                                    barcode = reader.GetString(4),
                                    WarehouseName = reader.GetString(5)
                                });
                            }
                        }
                    }
                }
            }
            catch (OracleException oracleEx)
            {
                // Log the Oracle exception
                Console.WriteLine($"Oracle Exception: {oracleEx.Message}, Code: {oracleEx.ErrorCode}");
                throw; // Re-throw to maintain original stack trace
            }
            catch (Exception ex)
            {
                // Log the general exception
                Console.WriteLine($"General Exception: {ex.Message}");
                throw; // Re-throw to maintain original stack trace
            }
            return products;
        }

        public async Task<List<Product>> FetchProducts()
        {
            var products = new List<Product>();
            try
            {
                using (var conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();
                    using (var cmd = new OracleCommand("PROJECT_pkg.FETCHALLPRODUCTS", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        var pResultSet = new OracleParameter("p_result_set", OracleDbType.RefCursor)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(pResultSet);

                        await cmd.ExecuteNonQueryAsync();

                        using (var reader = ((OracleRefCursor)pResultSet.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                products.Add(new Product
                                {
                                    ProductId = reader.GetInt32(0),
                                    userId = reader.GetInt32(1),
                                    ProductName = reader.GetString(2),
                                    quantity = reader.GetInt32(3),
                                    barcode = reader.GetString(4),
                                    Role = reader.GetString(5)
                                    
                                });
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
            return products;
        }
        public async Task<List<Product>> GetRemovedProducts()
        {
            var products = new List<Product>();
            try
            {
                using (var conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();

                    using (var cmd = new OracleCommand("PROJECT_pkg.GetRemovedProducts", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        var pResultSet = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(pResultSet);

                        await cmd.ExecuteNonQueryAsync();

                        using (var reader = ((OracleRefCursor)pResultSet.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                products.Add(new Product
                                {
                                    ProductId = reader.GetInt32(0),
                                    ProductName = reader.GetString(1),
                                    quantity = reader.GetInt32(2),
                                    barcode = reader.GetString(3),
                                    userId = reader.GetInt32(4),
                                    WarehouseName = reader.GetString(5)

                                });
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }

            return products;
        }

        public async Task<List<Product>> GetRemovedProductsByUserId(int userId)
        {
            var products = new List<Product>();
            try
            {
                using (var conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();

                    using (var cmd = new OracleCommand("PROJECT_pkg.GetRemovedProductsByUserId", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add the userId parameter (Input)
                        var pUserId = new OracleParameter("p_user_id", OracleDbType.Int32)
                        {
                            Value = userId,
                            Direction = ParameterDirection.Input
                        };
                        cmd.Parameters.Add(pUserId);  // Added first as it is an input parameter

                        // Add the cursor parameter (Output)
                        var pResultSet = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(pResultSet);  // Added second as it is an output parameter

                        // Execute the stored procedure
                        await cmd.ExecuteNonQueryAsync();

                        // Use the REF CURSOR to read the data
                        using (var reader = ((OracleRefCursor)pResultSet.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                products.Add(new Product
                                {
                                    ProductId = reader.GetInt32(0),
                                    ProductName = reader.GetString(1),
                                    quantity = reader.GetInt32(2),
                                    barcode = reader.GetString(3),
                                    userId = reader.GetInt32(4),
                                    WarehouseName = reader.GetString(5),
                                });
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }

            return products;
        }


        public async Task AddProductToWarehouse(string productName, int quantity, int userId, string warehouseName, string location)
        {
            string barcode = GenerateUniqueBarcode(4);

            do
            {
                barcode = GenerateUniqueBarcode(6);
            } while (await BarcodeExistsAsync(barcode));

            try
            {
                using (var conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();

                    using (var cmd = new OracleCommand("PROJECT_pkg.ADD_PRODUCT_TO_WAREHOUSE", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Adding parameters for the stored procedure
                        cmd.Parameters.Add("p_product_name", OracleDbType.Varchar2).Value = productName;
                        cmd.Parameters.Add("p_quantity", OracleDbType.Int32).Value = quantity;
                        cmd.Parameters.Add("p_barcode", OracleDbType.Varchar2).Value = barcode;
                        cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
                        cmd.Parameters.Add("p_warehouse_name", OracleDbType.Varchar2).Value = warehouseName;
                        cmd.Parameters.Add("p_location", OracleDbType.Varchar2).Value = location; // Added location parameter

                        // Executing the stored procedure
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }




        public string GenerateUniqueBarcode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder result = new StringBuilder("#");

            Random random = new Random();

            for (int i = 1; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }
        private async Task<bool> BarcodeExistsAsync(string barcode)
        {
            try
            {
                using (var conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();

                    using (var cmd = new OracleCommand("PROJECT_pkg.Check_Barcode_Exists", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        var existsParam = new OracleParameter("p_barcode", OracleDbType.Varchar2)
                        {
                            Value = barcode
                        };
                        cmd.Parameters.Add(existsParam);
                        var existsOutput = new OracleParameter("exists", OracleDbType.Int32)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(existsOutput);

                        await cmd.ExecuteNonQueryAsync();

                        return existsOutput.Value.ToString() == "1";
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return false;
            }
        }
        public async Task<int> RegisterOrganizationAsync(string orgName, string address, string email, string telephone)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();

                    using (OracleCommand cmd = new OracleCommand("PROJECT_pkg.Register_Organization", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_org_name", OracleDbType.Varchar2).Value = orgName;
                        cmd.Parameters.Add("p_address", OracleDbType.Varchar2).Value = address;
                        cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = email;
                        cmd.Parameters.Add("p_telephone", OracleDbType.Varchar2).Value = telephone;
                        OracleParameter orgIdParam = new OracleParameter("p_org_id", OracleDbType.Int32);
                        orgIdParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(orgIdParam);

                        await cmd.ExecuteNonQueryAsync();
                        int orgId = Convert.ToInt32(orgIdParam.Value);

                        return orgId;
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
                throw;
            }
        }
        public async Task OrgLoginAsync(string username, string password)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();

                    using (OracleCommand cmd = new OracleCommand("PROJECT_pkg.Org_Login", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_username", OracleDbType.Varchar2).Value = username;
                        cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = password;
                        OracleParameter orgCursor = new OracleParameter("p_org_cursor", OracleDbType.RefCursor)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(orgCursor);

                        OracleParameter employeeCursor = new OracleParameter("p_employee_cursor", OracleDbType.RefCursor)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(employeeCursor);
                        await cmd.ExecuteNonQueryAsync();
                        using (OracleDataReader orgReader = (OracleDataReader)cmd.Parameters["p_org_cursor"].Value)
                        {
                            Console.WriteLine("Organization Details:");
                            while (await orgReader.ReadAsync())
                            {
                                string orgName = orgReader.GetString(0);
                                string address = orgReader.GetString(1);
                                string email = orgReader.GetString(2);
                                string telephone = orgReader.GetString(3);
                                DateTime createdAt = orgReader.GetDateTime(4);

                                Console.WriteLine($"Name: {orgName}, Address: {address}, Email: {email}, Telephone: {telephone}, Created At: {createdAt}");
                            }
                        }

                        using (OracleDataReader empReader = (OracleDataReader)cmd.Parameters["p_employee_cursor"].Value)
                        {
                            Console.WriteLine("Employee Details:");
                            while (await empReader.ReadAsync())
                            {
                                string empUsername = empReader.GetString(0);
                                string role = empReader.GetString(1);
                                string employeeName = empReader.GetString(2);
                                DateTime empCreatedAt = empReader.GetDateTime(3);

                                Console.WriteLine($"Username: {empUsername}, Role: {role}, Employee Name: {employeeName}, Created At: {empCreatedAt}");
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }
        public async Task RemoveProductAsync(int userId, string barcode, int quantity)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();

                    using (OracleCommand cmd = new OracleCommand("PROJECT_pkg.Remove_Product", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
                        cmd.Parameters.Add("p_barcode", OracleDbType.Varchar2).Value = barcode;
                        cmd.Parameters.Add("p_quantity", OracleDbType.Int32).Value = quantity;
                        await cmd.ExecuteNonQueryAsync();

                        Console.WriteLine("Product removed successfully.");
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }
        public async Task UpdateUserStatus(int userId, int status)
        {
            try
            {
                using (var conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();
                    using (var cmd = new OracleCommand("PROJECT_pkg.UPDATE_USER_STATUS", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
                        cmd.Parameters.Add("p_is_active", OracleDbType.Int32).Value = status;
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating user status", ex);
            }
        }
        public async Task<List<string>> GetAllOrganizationNamesAsync()
        {
            var orgNames = new List<string>();

            try
            {
                using (OracleConnection conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();
                    using (OracleCommand cmd = new OracleCommand("PROJECT_pkg.get_org_names", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_refcursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var orgName = reader.IsDBNull(reader.GetOrdinal("org_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("org_name"));
                                orgNames.Add(orgName);
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {

            }

            return orgNames;
        }

        public async Task<List<WarehouseInfo>> CheckFreeSeatsByUserIdAsync(int userId)
        {
            var warehouseList = new List<WarehouseInfo>();

            try
            {
                using (OracleConnection conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();
                    using (OracleCommand cmd = new OracleCommand("PROJECT_pkg.CHECK_FREE_SEATS_BY_USER_ID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
                        cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var warehouse = new WarehouseInfo
                                {
                                    WarehouseName = reader.IsDBNull(reader.GetOrdinal("warehouse_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("warehouse_name")),
                                    TotalQuantity = reader.IsDBNull(reader.GetOrdinal("total_quantity")) ? 0 : reader.GetInt32(reader.GetOrdinal("total_quantity")),
                                    FreeSeats = reader.IsDBNull(reader.GetOrdinal("free_seats")) ? 0 : reader.GetInt32(reader.GetOrdinal("free_seats"))
                                };

                                warehouseList.Add(warehouse);
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }

            return warehouseList;
        }


        public async Task<List<WarehouseInfo>> CheckFreeSeatsAllWarehousesAsync()
        {
            var warehouseList = new List<WarehouseInfo>();

            try
            {
                using (OracleConnection conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();
                    using (OracleCommand cmd = new OracleCommand("PROJECT_pkg.CHECK_FREE_SEATS_ALL_WAREHOUSES", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                        using (OracleDataReader reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var warehouse = new WarehouseInfo
                                {
                                    WarehouseName = reader.IsDBNull(reader.GetOrdinal("warehouse_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("warehouse_name")),
                                    TotalQuantity = reader.IsDBNull(reader.GetOrdinal("total_quantity")) ? 0 : reader.GetInt32(reader.GetOrdinal("total_quantity")),
                                    FreeSeats = reader.IsDBNull(reader.GetOrdinal("free_seats")) ? 0 : reader.GetInt32(reader.GetOrdinal("free_seats"))
                                };

                                warehouseList.Add(warehouse);
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }

            return warehouseList;
        }


        public async Task<List<WarehouseInfo>> FetchNameAndLocation()
        {
            var warehouses = new List<WarehouseInfo>();

            try
            {
                using (var conn = new OracleConnection(Connstr))
                {
                    await conn.OpenAsync();
                    using (var cmd = new OracleCommand("PROJECT_pkg.FetchNameAndLocation", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        var pResultSet = new OracleParameter("p_result_set", OracleDbType.RefCursor)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(pResultSet);

                        await cmd.ExecuteNonQueryAsync();

                        using (var reader = ((OracleRefCursor)pResultSet.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                warehouses.Add(new WarehouseInfo
                                {
                                    WarehouseName = reader.GetString(0),
                                    Location = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"OracleException: {ex.Message}\nError Code: {ex.ErrorCode}\nStack Trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }

            return warehouses;
        }

        public async Task AddWarehouse(int userId, string warehouseName, string location)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(Connstr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("PROJECT_pkg.ADD_WAREHOUSE", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add parameters for the stored procedure
                        cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
                        cmd.Parameters.Add("p_warehouse_name", OracleDbType.Varchar2).Value = warehouseName;
                        cmd.Parameters.Add("p_location", OracleDbType.Varchar2).Value = location;

                        // Execute the procedure asynchronously
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("OracleException: " + ex.Message);
                Console.WriteLine("Error Code: " + ex.ErrorCode);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
            }
        }


    }
}
