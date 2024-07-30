using System.Net;
using System.Text.Json;
using Ecommerce.SharedLibrary.Logs;
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
                    await ModifyHeader(context, title, message, statusCode);
                }
                // check for unauthorized request
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "You are not authorized to access";
                    await ModifyHeader(context, title, message, statusCode);
                }
                // forbiden
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Access Denail";
                    message = "You dont have access to this site";
                    statusCode = StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message, statusCode);
                }
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                // timeout exception
                if (ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = "Time Out";
                    message = "Request timeout...... Please try again";
                    statusCode = StatusCodes.Status408RequestTimeout;
                }
                await ModifyHeader(context, title, message, statusCode);
            }
        }

        private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            //display some massage
            context.Response.ContentType = "applicatin/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
            {
                Detail = message,
                Status = statusCode,
                Title = title,
            }), CancellationToken.None);
            return;
        }
    }
}