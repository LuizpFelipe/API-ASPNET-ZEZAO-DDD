namespace Domain.Interfaces
{
    public interface ISecurityService
    {
        string HashPassword(string password);
    }
}
