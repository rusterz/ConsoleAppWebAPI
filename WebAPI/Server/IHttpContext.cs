using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Server
{
    public interface IHttpContext
    {
        IRequestContext Request { get; }
        IResponseContext Response { get; }
    }

    public interface IRequestContext
    {
        Stream InputStream { get; }
        NameValueCollection QueryString { get; }
        Uri Url { get; }
        NameValueCollection Headers { get; }
        Encoding ContentEncoding { get; }  
        string HttpMethod { get; }

    }

    public interface IResponseContext
    {
        void SetContentAndWriteAsync(string content, string contentType, HttpStatusCode statusCode);
        public void CloseOutputStream();
    }

    public class RequestContext : IRequestContext
    {
        private HttpListenerRequest _request;

        public RequestContext(HttpListenerRequest request)
        {
            _request = request;
        }

        public Stream InputStream => _request.InputStream;
        public NameValueCollection QueryString => _request.QueryString;
        public Uri Url => _request.Url;
        public NameValueCollection Headers => _request.Headers;
        public Encoding ContentEncoding => _request.ContentEncoding;
        public string HttpMethod => _request.HttpMethod;
    }

    public class ResponseContext : IResponseContext
    {
        private HttpListenerResponse _response;

        public ResponseContext(HttpListenerResponse response)
        {
            _response = response;
        }

        public void SetContentAndWriteAsync(string content, string contentType, HttpStatusCode statusCode)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            _response.ContentType = contentType;
            _response.ContentLength64 = buffer.Length;
            _response.StatusCode = (int)statusCode;
            _response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        public void CloseOutputStream()
        {
            _response.OutputStream.Close();
        }
    }

    public class HttpContext : IHttpContext
    {
        public IRequestContext Request { get; }
        public IResponseContext Response { get; }

        public HttpContext(HttpListenerContext context)
        {
            Request = new RequestContext(context.Request);
            Response = new ResponseContext(context.Response);
        }
    }
}
