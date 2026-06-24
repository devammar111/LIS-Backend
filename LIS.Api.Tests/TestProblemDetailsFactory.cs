using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LIS.Api.Tests;

/// <summary>
/// Minimal ProblemDetailsFactory so controllers can call ValidationProblem()/Problem()
/// in unit tests without a full HTTP host.
/// </summary>
public class TestProblemDetailsFactory : ProblemDetailsFactory
{
    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
        => new()
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance
        };

    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ModelStateDictionary modelStateDictionary,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
        => new(modelStateDictionary)
        {
            Status = statusCode ?? StatusCodes.Status400BadRequest,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance
        };
}
