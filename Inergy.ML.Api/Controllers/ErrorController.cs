using Microsoft.AspNetCore.Mvc;

namespace Imergy.ML.Cosmos.Api.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public IActionResult Error() => Problem();
    }
}
