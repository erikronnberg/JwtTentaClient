using System;

namespace JwtTentaClient.Models
{
    public class AuthenticateResponse
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            return "Username: " + Username + "\n\nToken: " + JwtToken + "\n\nRefreshToken: " + RefreshToken;
        }
    }
}