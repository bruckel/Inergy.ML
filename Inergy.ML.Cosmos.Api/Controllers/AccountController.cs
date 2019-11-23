using Inergy.ML.Cosmos.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Inergy.ML.Cosmos.Api.Controllers
{
    /// <summary>
    /// Controlador de autorización y autentificación de la web api
    /// </summary>
    [ApiController]
    public class AccountController : Controller
    {
        /// <summary>
        /// Gestor de usuarios Identity
        /// </summary>
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ILogger log;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userManager">Gestor de usuarios Identity</param>
        /// <param name="log">Gestor de log</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger log, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.log = log;
            this.configuration = configuration;
        }

        /// <summary>
        /// Login de usuario de la api web
        /// </summary>
        /// <param name="model">Modelo con el usurio y la contraseña</param>
        /// <returns>Token de autentificación Jwt</returns>
        [HttpPost("api/account/login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                //* Obtener usuario por nombre *//
                var user = await userManager.FindByNameAsync(model.Username);

                //* Verificar contraseña *//
                if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
                {
                    //* Obtener Claims definidos para el usuario con Identity y almacenadso en la BB.DD. *//
                    var identityClaims = await userManager.GetClaimsAsync(user);

                    //* DEfinir claims del Token y unirlos a los de Identity *//
                    var authClaims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    }
                    .Union(identityClaims);

                    var token = new JwtSecurityToken(
                        issuer: configuration.GetSection("TokenSettings:Issuer").Value,
                        audience: configuration.GetSection("TokenSettings:Audience").Value,
                        expires: DateTime.Now.AddHours(1),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("TokenSettings:Key").Value)), SecurityAlgorithms.HmacSha256)
                    );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }
                else
                {
                    if (!(await roleManager.RoleExistsAsync("Admin")))
                    {
                        await roleManager.CreateAsync(new IdentityRole("Admin"));
                    }

                    user = new ApplicationUser()
                    {
                        UserName = "admin",
                        Email = "tic@inergybcn.com"
                    };

                    var userResult = await userManager.CreateAsync(user, "In@rgy_2019");
                    var roleResult = await userManager.AddToRoleAsync(user, "Admin");
                    var claimResult = await userManager.AddClaimAsync(user, new Claim("SuperUser", "True"));

                    if (!userResult.Succeeded || !roleResult.Succeeded || !claimResult.Succeeded)
                    {
                        throw new InvalidOperationException("Failed to build user and roles");
                    }
                }
            }
            catch (Exception exception)
            {
                log.Error($"Exception thrown while creating JWT: {exception}");
            }

            return BadRequest("Failed to generate token");
        }
    }
}