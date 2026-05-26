using EmployeeManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManagement.API.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _passwordHasher = new();

        public string Hash(string password)
        {
            return _passwordHasher.HashPassword(new object(), password);
        }

        public bool Verify(string password, string passwordHash)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                new object(),
                passwordHash,
                password);
            return result == PasswordVerificationResult.Success ||
                result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
