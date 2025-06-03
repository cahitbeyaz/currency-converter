using System.Threading.Tasks;

namespace CurrencyConverter.Application.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with username and password
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>Authentication result with token if successful, null if unsuccessful</returns>
        Task<AuthResult> AuthenticateAsync(string username, string password);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public string[] Roles { get; set; }
        public string ErrorMessage { get; set; }
    }
}
