// Archivo: CementoTrazabilidad.API\Controllers\TestHashController.cs
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace CementoTrazabilidad.API.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestHashController : ControllerBase
    {
        [HttpGet("hash/{password}")]
        public IActionResult GetHash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            var hashBase64 = Convert.ToBase64String(hash);

            return Ok(new
            {
                password,
                hash = hashBase64,
                sqlUpdate = $"UPDATE Usuario SET PasswordHash = '{hashBase64}' WHERE Legajo = 'TU_LEGAJO';"
            });
        }

        [HttpGet("common-hashes")]
        public IActionResult GetCommonHashes()
        {
            var passwords = new[] { "operario123", "admin123", "supervisor123", "password", "123456" };
            var results = new Dictionary<string, string>();

            foreach (var pwd in passwords)
            {
                using var sha256 = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(pwd);
                var hash = sha256.ComputeHash(bytes);
                results[pwd] = Convert.ToBase64String(hash);
            }

            return Ok(results);
        }
    }
}