using System.Net;
using WebAPI.Controllers.Abstracts;
using WebAPI.Controllers.Attributes;

namespace WebAPI.Controllers;

public class HomeController : Controller
{
    [Route("/home", "GET")]
    public async Task<string> GetHome()
    {
        var responseMessage = "{\"message\":\"Welcome to Home!\"}";
        return responseMessage;    
    }
}
