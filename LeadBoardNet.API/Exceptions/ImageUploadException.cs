using System.Net;

namespace LeadBoard.Shared;

public class ImageUploadException : Exception
{
    public HttpStatusCode StatusCode { get; set; }
    public ImageUploadException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    } 
}