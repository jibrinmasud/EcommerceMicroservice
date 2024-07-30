using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace Ecommerce.SharedLibrary.Middelware
{
    public class GlobalException(RequestDelegate next)
    {

        public async Task InvokeAsync(HttpContext context)
        {
            // Declare Variables
            string message = "Sorry, internal server error try again";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";

            try
            {
                await next(context);

                // check for too many request 429 statuscode
                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = "Too many Requset";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    await ModifyHeader(context, title,message, statusCode);
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        private async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            //display some massage
            context.Response.ContentType ="applicatin/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails(){
                Detail = message,
                Status = statusCode,
                Title = title,
            }));
        }
    }
}