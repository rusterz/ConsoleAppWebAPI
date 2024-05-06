using System.Net;
using System.Text.Json;
using WebAPI.Controllers.Abstracts;
using WebAPI.Controllers.Attributes;
using WebAPI.DTOs;
using WebAPI.Server;

namespace WebAPI.Controllers;

public class HomeController : Controller
{
    [Route("/home", "GET")]
    public async Task<string> Home(IHttpContext context)
    {
        var responseMessage = "{\"message\":\"Welcome to Home!\"}";
        return responseMessage;
    }

    [Route("/home/val", "GET")]
    public async Task<string> Home(IHttpContext context, int value)
    {
        var responseMessage = $"Your value is {value}";
        return responseMessage;
    }

    [Route("/home", "POST")]
    public async Task<string> Echo(IHttpContext context, KeyValueDto dto)
    {
        return JsonSerializer.Serialize(dto);
    }
}
