using JwtTentaClient.Data;
using JwtTentaClient.Models;
using JwtTentaClient.Models.Entities;
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
            try
            {
                Console.ReadKey();
                Console.WriteLine("Hello World!");
                //var resp = await PostUser();
                //var resp = await AuthUser();
                var resp = await GetAllUsers("eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiZXJpa3Jvbm5iZXJnIiwianRpIjoiMGU2NGQyZmItOGMzNC00MDMxLWIwMmMtNjliOWZlMWMyZDFjIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjpbIkVtcGxveWVlIiwiQWRtaW4iXSwiZXhwIjoxNjAwNzgyODc2LCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjYxOTU1IiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo0MjAwIn0.F_ee0knE6eBAeYWmGMkoq9hfFabinPELMTY5Pl30a1kk-v7YsZb3IuKp-9_DG0TVBVSYjKscr736SBARuGY62w");

                foreach (var x in resp)
                {
                    Console.WriteLine(x.Username + "\n");
                }
                //Console.WriteLine(resp.ToString());
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        public static async Task<AccountResponse> PostUser()
        {
            var user = new RegisterRequest();
            user.Email = "test@test.com";
            user.Password = "!Grodan1234";
            user.Username = "erik1";
            user.EmployeeID = 2;

            var endpoint = "user/register";
            var jsonRequest = JsonConvert.SerializeObject(user);
            var postRequest = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url + endpoint, postRequest);
                if (response.IsSuccessStatusCode == false)
                    return null;

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AccountResponse>(jsonResult);
                return result;
            }
        }

        public static async Task<AuthenticateResponse> AuthUser()
        {
            var user = new AuthenticateRequest { Email = "erik.rnnberg@gmail.com", Password = "!Groda1234567" };
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