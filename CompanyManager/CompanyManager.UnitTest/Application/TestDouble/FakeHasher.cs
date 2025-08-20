using CompanyManager.Domain.Interfaces;

namespace CompanyManager.UnitTest.Application.TestDouble
{


    internal sealed class FakeHasher : IPasswordHasher
    {
        public string Hash(string plainTextPassword) => $"HASH({plainTextPassword})";
        public bool Verify(string hash, string plainTextPassword) => hash == Hash(plainTextPassword);
    }

}
