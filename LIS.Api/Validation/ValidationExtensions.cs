using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LIS.Api.Validation;

public static class ValidationExtensions
{
    /// <summary>
    /// Copies FluentValidation failures into ModelState using camelCase property keys
    /// (e.g. "patientName"), so the resulting ValidationProblemDetails matches the
    /// JSON contract the frontend consumes.
    /// </summary>
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(ToCamelCase(error.PropertyName), error.ErrorMessage);
        }
    }

    private static string ToCamelCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName) || char.IsLower(propertyName[0]))
        {
            return propertyName;
        }

        return char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
    }
}
