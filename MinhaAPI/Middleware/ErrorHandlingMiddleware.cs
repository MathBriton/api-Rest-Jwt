using System.Net;
using System.Text.Json;
using MinhaAPI.Models;
using MinhaAPI.Exceptions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MinhaAPI.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorResponse = new ErrorResponse
        {
            Path = context.Request.Path
        };

        switch (exception)
        {
            case NotFoundException notFoundEx:
                errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = notFoundEx.Message;
                Log.Warning(notFoundEx, "Recurso não encontrado: {Path}", context.Request.Path);
                break;

            case BadRequestException badRequestEx:
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = badRequestEx.Message;
                Log.Warning(badRequestEx, "Requisição inválida: {Path}", context.Request.Path);
                break;

            case UnauthorizedException unauthorizedEx:
                errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = unauthorizedEx.Message;
                Log.Warning(unauthorizedEx, "Acesso não autorizado: {Path}", context.Request.Path);
                break;

            case ForbiddenException forbiddenEx:
                errorResponse.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Message = forbiddenEx.Message;
                Log.Warning(forbiddenEx, "Acesso proibido: {Path}", context.Request.Path);
                break;

            case ValidationException validationEx:
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = validationEx.Message;
                errorResponse.Errors = validationEx.Errors;
                Log.Warning(validationEx, "Erro de validação: {Path}", context.Request.Path);
                break;

            case DbUpdateException dbEx:
                errorResponse.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.Message = "Erro ao atualizar o banco de dados";
                errorResponse.Details = "Pode haver um conflito com dados existentes";
                Log.Error(dbEx, "Erro de banco de dados: {Path}", context.Request.Path);
                break;

            case UnauthorizedAccessException:
                errorResponse.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Message = "Você não tem permissão para acessar este recurso";
                Log.Warning(exception, "Acesso negado: {Path}", context.Request.Path);
                break;

            default:
                errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "Ocorreu um erro interno no servidor";
                errorResponse.Details = exception.Message;
                Log.Error(exception, "Erro interno: {Path}", context.Request.Path);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        return context.Response.WriteAsync(json);
    }
}