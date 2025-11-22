using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime;
using System.Security.Claims;
using System.Text;
using WebAppDoCongNghe.Models.ApiRespone;

namespace WebAppDoCongNghe.Service
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        public JwtService(IConfiguration configuration , IOptions<AppSettings> appSettings)
        {
            _configuration = configuration;
            _appSettings = appSettings.Value;
        }
        // sinh token
        public string GenerateToken(string Id, string role)
        {
            var claim = new[]
            {
                   new Claim(ClaimTypes.Name, Id),
                   new Claim(ClaimTypes.Role, role),
                   new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString())
               };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            var token = new JwtSecurityToken(
               issuer: _appSettings.Issuer,
                 audience: _appSettings.Audience,
                   claims: claim,
                   expires: DateTime.Now.AddHours(3),
                      signingCredentials: creds

                      );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
