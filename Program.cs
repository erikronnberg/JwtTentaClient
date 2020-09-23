using JwtTenta.Models.Response;
using JwtTentaClient.Data;
using JwtTentaClient.Models;
using JwtTentaClient.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                            var resPost = await PostUser();
                            Console.WriteLine(resPost.ToString() + "\n" + "\n");

                            var resAuth = await AuthUser();
                            Console.WriteLine(resAuth.ToString() + "\n" + "\n");

                            var resGetMy = await GetMyOrders(resAuth.JwtToken, 1);
                            foreach (var x in resGetMy)
                            {
                                Console.WriteLine(x.OrderDate + x.ShipCountry + "\n");
                            }

                            var resGetAll = await GetAllOrders(resAuth.JwtToken);
                            foreach (var x in resGetAll)
                            {
                                Console.WriteLine(x.OrderDate + x.ShipCountry + "\n");
                            }

                            var resGetCountry = await GetCountryOrders(resAuth.JwtToken, "USA");
                            foreach (var x in resGetCountry)
                            {
                                Console.WriteLine(x.OrderDate + x.ShipCountry + "\n");
                            }

                            var resUpdate = await UpdateUser(resAuth.JwtToken);
                            Console.WriteLine(resUpdate.ToString() + "\n" + "\n");

                            var resGetAllUsers = await GetAllUsers(resAuth.JwtToken);
                            foreach (var x in resGetAllUsers)
                            {
                                Console.WriteLine(x.Username + "\n");
                            }

                            var resDelete = await DeleteUser(resAuth.JwtToken, "test1");
                            Console.WriteLine(resDelete.ToString() + "\n" + "\n");
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

        public static async Task<string> PostUser()
        {
            var user = new RegisterRequest { Email = "test1@test.com", Password = "!Password1", Username = "test1", EmployeeID = 2, };

            var endpoint = "user/register";
            var jsonRequest = JsonConvert.SerializeObject(user);
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
        public static async Task<string> UpdateUser(string token)
        {
            var user = new UpdateRequest { Email = "test@test.com", Phonenumber = "0987654321", Username = "test"};

            var endpoint = "user/update";
            var jsonRequest = JsonConvert.SerializeObject(user);
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

        public static async Task<AuthenticateResponse> AuthUser()
        {
            var user = new AuthenticateRequest { Email = "test@test.com", Password = "!Password1" };
            var endpoint = "user/login";
            var jsonRequest = JsonConvert.SerializeObject(user);
            var postRequest = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url + endpoint, postRequest);
                if (response.IsSuccessStatusCode == false)
                    return null;

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

                await context.SaveChangesAsync();

                return result;
            }
        }
        public static async Task<IEnumerable<AccountResponse>> GetAllUsers(string token)
        {
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