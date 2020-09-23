using JwtTentaClient.Data;
using JwtTentaClient.Models;
using JwtTentaClient.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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
                            //var resAuth = await AuthUser();
                            //var resGetAll = await GetAllUsers(resAuth.JwtToken);

                            Console.WriteLine(resPost.ToString() + "\n" + "\n");
                            //Console.WriteLine(resAuth.ToString() + "\n" + "\n");

                            //foreach (var x in resGetAll)
                            //{
                            //    Console.WriteLine(x.Username + "\n");
                            //}
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
            var user = new RegisterRequest { Email = "test@test.com", Password = "!Password1", Username = "test", EmployeeID = 1, };

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

                var account = new Users { RefreshToken = result.RefreshToken, Username = result.Username, JwtToken = result.JwtToken };

                await context.Users.AddAsync(account);
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
    }
}