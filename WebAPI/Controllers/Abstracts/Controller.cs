using System.Net;

namespace WebAPI.Controllers.Abstracts;

public abstract class Controller
{
    protected HttpListenerContext Context { get; private set; } // TO DO: вынести в абстракцию 

    public void SetContext(HttpListenerContext context)
    {
        this.Context = context;
    }
}
