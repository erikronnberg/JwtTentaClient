namespace JwtTentaClient.Models.Entities
{
    public class Users
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
