using Newtonsoft.Json;
using System;
using System.Net;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
    public class RequestError
    {
        [JsonConstructor]
        public RequestError()
        {

        }

        public RequestError(int statusCode, string message)
        {
            this.StatusCode = (HttpStatusCode)statusCode;
            this.Message = message;
        }

        public RequestError(Exception ex)
        {
            this.Exception = ex;
            this.ExceptionType = ex.GetType();
            this.Message = ex.Message;
        }

        public HttpStatusCode? StatusCode { get; set; }
        public Exception Exception { get; set; }

        public Type ExceptionType { get; set; }

        public string Message { get; set; }
    }
}
