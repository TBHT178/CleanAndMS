using eCommerce.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace eCommerceSharedLibrary.Middleware
{
    public class GlobalException (RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            // Declare default variables
            string title = "Error";
            string message = "Sorry, internal server error occurred. Please try again";
            int statusCode = (int)HttpStatusCode.InternalServerError;

            try
            {
                await next(context);

                // Check if Response is Too Many Request // 429 status code
                if(context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = "Too Many Request";
                    statusCode = (int)HttpStatusCode.TooManyRequests;
                    await ModifyHeader(context, title, message, statusCode);
                }

                // Check if Response is UnAuthorized // 401 status code
                if(context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "You are not authorized to access";
                    statusCode = StatusCodes.Status401Unauthorized;
                    await ModifyHeader(context, title, message, statusCode);
                }

                // Check if Response is Forbidden // 403 status code
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Out of Access";
                    message = "You are not allowed to access";
                    statusCode = StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message, statusCode);
                }
            } catch (Exception e)
            {
                // Log Original Exceptions /File, Debugger, Console
                LogException.LogExceptions(e);

                // Check if Exception is Timeout // 408 request timeout
                if(e is TaskCanceledException || e is TimeoutException)
                {
                    title = "Out of Time";
                    message = "Request Time out";
                    statusCode = StatusCodes.Status408RequestTimeout;
                }

                // If exception is caught
                // If none of the exceptions then do the default
                await ModifyHeader(context, title, message, statusCode);
            }
        }

        private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            // Display scary-free message to client
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
            {
                Detail= message,
                Status = statusCode,
                Title = title
            }), CancellationToken.None);
            return;
        }
    }
}
