using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CementoTrazabilidad.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioID.ToString()),
                new Claim(ClaimTypes.Name, usuario.Legajo),
                new Claim(ClaimTypes.GivenName, usuario.Personal?.Nombre ?? ""),
                new Claim(ClaimTypes.Role, usuario.RolSistema),
                new Claim("PersonalID", usuario.PersonalID.ToString()),
                new Claim("Legajo", usuario.Legajo)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "DefaultSecretKeyForTesting123456789"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "CementoTrazabilidad",
                audience: _configuration["Jwt:Audience"] ?? "CementoTrazabilidadApp",
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKeyForTesting123456789");

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"] ?? "CementoTrazabilidad",
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"] ?? "CementoTrazabilidadApp",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}