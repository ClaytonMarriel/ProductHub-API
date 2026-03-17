using System.Net;
using ApiWeb.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ApiWeb.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for request {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/problem+json";

        var problem = ex switch
        {
            BusinessRuleException bre => new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "Regra de negócio inválida.",
                Status = StatusCodes.Status400BadRequest,
                Detail = bre.Message,
                Instance = context.Request.Path
            },

            NotFoundException nfe => new ProblemDetails
            {
                Type = "https://httpstatuses.com/404",
                Title = "Recurso não encontrado.",
                Status = StatusCodes.Status404NotFound,
                Detail = nfe.Message,
                Instance = context.Request.Path
            },

            _ => new ProblemDetails
            {
                Type = "https://httpstatuses.com/500",
                Title = "Ocorreu um erro inesperado.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "Please contact support if the problem persists.",
                Instance = context.Request.Path
            }
        };

        context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;
        await context.Response.WriteAsJsonAsync(problem);
    }
}