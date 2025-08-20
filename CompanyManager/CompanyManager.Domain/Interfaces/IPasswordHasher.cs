namespace CompanyManager.Domain.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string plainTextPassword);
        bool Verify(string plainTextPassword, string hash);
    }
}
