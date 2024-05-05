using System.Net;
using System.Text.Json;
using WebAPI.Controllers.Abstracts;
using WebAPI.Controllers.Attributes;
using WebAPI.DTOs;

namespace WebAPI.Controllers;

public class HomeController : Controller
{
    [Route("/home", "GET")]
    public async Task<string> Home(HttpListenerContext context)
    {
        var responseMessage = "{\"message\":\"Welcome to Home!\"}";
        return responseMessage;
    }
    
    [Route("/home", "POST")]
    public async Task<string> Echo(HttpListenerContext context, KeyValueDto dto)
    {
        return JsonSerializer.Serialize(dto);
    }
}
