using System;

namespace JwtTentaClient.Models
{
    public class AccountResponse
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phonenumber { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        public override string ToString()
        {
            return "Username: " + Username + " Email: " + Email;
        }
    }
}