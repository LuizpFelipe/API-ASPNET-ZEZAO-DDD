using Domain.Interfaces;

namespace Infrastructure.Security
{
    public class BCryptoSecurityService : ISecurityService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
