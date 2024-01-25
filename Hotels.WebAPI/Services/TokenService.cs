using System.Text;
using System.Security.Claims;
using Hotels.WebAPI.Models;
using Hotels.WebAPI.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Hotels.WebAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly TimeSpan _expiryDuration = new(0, 30, 0);

        public string BuildToken(string key, string issuer, User user)
        {
            Claim[] claims =
            [
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            ];

            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(key));
            SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature);
            JwtSecurityToken tokenDescriptor = new(issuer, issuer, claims, expires: DateTime.Now.Add(_expiryDuration), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}