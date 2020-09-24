using JwtTenta.Models.Response;
using JwtTentaClient.Data;
using JwtTentaClient.Models;
using JwtTentaClient.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace JwtTentaClient
{
    class Program
    {
        protected static readonly string url = "https://localhost:5001/api/";
        protected static readonly DataContext context = new DataContext();
        static async Task Main(string[] args)
        {
            RegisterRequest registerRequest1 = new RegisterRequest { Email = "test1@test.com", Password = "!Password1", Username = "test1", EmployeeID = 1, };
            RegisterRequest registerRequest2 = new RegisterRequest { Email = "test2@test.com", Password = "!Password2", Username = "test2", EmployeeID = 2, };
            RegisterRequest registerRequest3 = new RegisterRequest { Email = "test3@test.com", Password = "!Password3", Username = "test3", EmployeeID = 3, };
            AuthenticateRequest authenticateRequest = new AuthenticateRequest { Email = "test1@test.com", Password = "!Password1" };
            UpdateRequest updateRequest = new UpdateRequest { Username = "test2", Email = "test2@test.com", Phonenumber = "0987654321", Role = "CountryManager" };
            AuthenticateRequest authenticateRequest2 = new AuthenticateRequest { Email = "test2@test.com", Password = "!Password2" };
            UpdateRequest updateRequest2 = new UpdateRequest { Username = "test2", Email = "test2@test.com", Phonenumber = "0987654321", Role = "Admin" };
            string userToDelete = "test3";

            bool menuBool = true;

            while (menuBool)
            {
                Console.Clear();
                Console.WriteLine("Northwind API");
                Console.WriteLine("==================================");
                Console.WriteLine("Do stuff (s)");
                Console.WriteLine("Exit (e)");

                string menu = Console.ReadLine();

                switch (menu)
                {
                    case "s":
                        try
                        {
                            var resPost1 = await PostUser(registerRequest1);
                            Console.WriteLine(resPost1 + "\n" + "\n");
                            var resPost2 = await PostUser(registerRequest2);
                            Console.WriteLine(resPost2 + "\n" + "\n");
                            var resPost3 = await PostUser(registerRequest3);
                            Console.WriteLine(resPost3 + "\n" + "\n");

                            var resAuth = await AuthUser(authenticateRequest);
                            if (!resAuth.Success)
                                Console.WriteLine(resAuth.ErrorMessage + "\n" + "\n");

                            Console.WriteLine(resAuth.ToString() + "\n" + "\n");

                            var resAuth2 = await AuthUser(authenticateRequest2);
                            if (!resAuth2.Success)
                                Console.WriteLine(resAuth2.ErrorMessage + "\n" + "\n");

                            Console.WriteLine(resAuth2.ToString() + "\n" + "\n");

                            var resUpdate = await UpdateUser(resAuth.JwtToken, updateRequest);
                            Console.WriteLine(resUpdate + "\n" + "\n");

                            var resUpdate2 = await UpdateUser(resAuth2.JwtToken, updateRequest2);
                            Console.WriteLine(resUpdate2 + "\n" + "\n");

                            var resGetAllUsers = await GetAllUsers(resAuth.JwtToken);
                            if (resGetAllUsers != null)
                            {
                                foreach (var x in resGetAllUsers)
                                {
                                    Console.WriteLine("Username: " + x.Username + " Created: " + x.Created + "\n");
                                }
                            }
                            else Console.WriteLine("No objects found or token is expired\n");

                            var resDelete = await DeleteUser(resAuth.JwtToken, userToDelete);
                            Console.WriteLine(resDelete + "\n" + "\n");

                            var resGetMy = await GetMyOrders(resAuth.JwtToken, 1);
                            if (resGetMy != null)
                            {
                                foreach (var x in resGetMy)
                                {
                                    Console.WriteLine(x.OrderDate + x.ShipCountry + "\n");
                                }
                            }
                            else Console.WriteLine("No objects found or token is expired\n");


                            var resGetAll = await GetAllOrders(resAuth.JwtToken);
                            if (resGetAll != null)
                            {
                                foreach (var x in resGetAll)
                                {
                                    Console.WriteLine(x.OrderDate + x.ShipCountry + "\n");
                                }
                            }
                            else Console.WriteLine("No objects found or token is expired\n");

                            var resGetCountry = await GetCountryOrders(resAuth.JwtToken, "USA");
                            if (resGetCountry != null)
                            {
                                foreach (var x in resGetCountry)
                                {
                                    Console.WriteLine(x.OrderDate + x.ShipCountry + "\n");
                                }
                            }
                            else Console.WriteLine("No objects found or token is expired\n");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Console.ReadKey();
                            break;
                        }

                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;

                    case "e":
                    default:
                        menuBool = false;
                        break;
                }
            }
        }

        public static bool TokenValidation(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.ReadJwtToken(token);
            if (DateTime.UtcNow > securityToken.ValidTo)
                return false;
            else
                return true;
        }

        public static async Task<string> PostUser(RegisterRequest request)
        {
            var endpoint = "user/register";
            var jsonRequest = JsonConvert.SerializeObject(request);
            var postRequest = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url + endpoint, postRequest);
                if (response.IsSuccessStatusCode == false)
                    return response.StatusCode.ToString();

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AccountResponse>(jsonResult);
                return result.ToString();
            }
        }
        public static async Task<string> PostAdmin(string token)
        {
            if (!TokenValidation(token))
                return "Expired token";

            var user = new RegisterRequest { Email = "test@test.com", Password = "!Password1", Username = "test", EmployeeID = 1, };

            var endpoint = "user/register-admin";
            var jsonRequest = JsonConvert.SerializeObject(user);
            var postRequest = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.PostAsync(url + endpoint, postRequest);
                if (response.IsSuccessStatusCode == false)
                    return response.StatusCode.ToString();

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AccountResponse>(jsonResult);
                return result.ToString();
            }
        }
        public static async Task<string> UpdateUser(string token, UpdateRequest request)
        {
            if (!TokenValidation(token))
                return "Expired token";

            var endpoint = "user/update";
            var jsonRequest = JsonConvert.SerializeObject(request);
            var postRequest = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.PatchAsync(url + endpoint, postRequest);
                if (response.IsSuccessStatusCode == false)
                    return response.StatusCode.ToString();

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AccountResponse>(jsonResult);
                return result.ToString();
            }
        }
        public static async Task<string> DeleteUser(string token, string userName)
        {
            if (!TokenValidation(token))
                return "Expired token";

            var endpoint = "user/delete";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var uriBuilder = new UriBuilder(url + endpoint);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["username"] = userName;
                uriBuilder.Query = query.ToString();
                var response = await client.DeleteAsync(uriBuilder.Uri);
                if (response.IsSuccessStatusCode == false)
                    return response.StatusCode.ToString();

                return response.StatusCode.ToString();
            }
        }

        public static async Task<AuthenticateResponse> AuthUser(AuthenticateRequest request)
        {
            var endpoint = "user/login";
            var jsonRequest = JsonConvert.SerializeObject(request);
            var postRequest = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url + endpoint, postRequest);
                if (response.IsSuccessStatusCode == false)
                    return new AuthenticateResponse { ErrorMessage = response.StatusCode.ToString(), Success = false };

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AuthenticateResponse>(jsonResult);

                var exist = await context.Users.Where(x => x.Username == result.Username).FirstOrDefaultAsync();
                if (exist == null)
                {
                    var account = new Users { RefreshToken = result.RefreshToken, Username = result.Username, JwtToken = result.JwtToken };
                    await context.Users.AddAsync(account);
                }
                else
                {
                    exist.JwtToken = result.JwtToken;
                    exist.RefreshToken = result.RefreshToken;
                }

                var success = await context.SaveChangesAsync();
                if (success == 0)
                    return new AuthenticateResponse { ErrorMessage = "Could not save user localy", Success = false };

                result.Success = true;
                return result;
            }
        }
        public static async Task<IEnumerable<AccountResponse>> GetAllUsers(string token)
        {
            if (!TokenValidation(token))
                return null;

            var endpoint = "user/get-all-users";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync(url + endpoint);
                if (response.IsSuccessStatusCode == false)
                    return null;

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<AccountResponse>>(jsonResult);
                return result;
            }
        }
        public static async Task<IEnumerable<OrderResponse>> GetMyOrders(string token, int id)
        {
            if (!TokenValidation(token))
                return null;

            var endpoint = "order/get-my-orders";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var uriBuilder = new UriBuilder(url + endpoint);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["id"] = id.ToString();
                uriBuilder.Query = query.ToString();
                var response = await client.GetAsync(uriBuilder.Uri);
                if (response.IsSuccessStatusCode == false)
                    return null;

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<OrderResponse>>(jsonResult);
                return result;
            }
        }
        public static async Task<IEnumerable<OrderResponse>> GetCountryOrders(string token, string country)
        {
            if (!TokenValidation(token))
                return null;

            var endpoint = "order/get-orders-by-country";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var uriBuilder = new UriBuilder(url + endpoint);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["country"] = country;
                uriBuilder.Query = query.ToString();
                var response = await client.GetAsync(uriBuilder.Uri);
                if (response.IsSuccessStatusCode == false)
                    return null;

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<OrderResponse>>(jsonResult);
                return result;
            }
        }
        public static async Task<IEnumerable<OrderResponse>> GetAllOrders(string token)
        {
            if (!TokenValidation(token))
                return null;

            var endpoint = "order/get-all-orders";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var uriBuilder = new UriBuilder(url + endpoint);
                var response = await client.GetAsync(uriBuilder.Uri);
                if (response.IsSuccessStatusCode == false)
                    return null;

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<OrderResponse>>(jsonResult);
                return result;
            }
        }
    }
}