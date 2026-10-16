using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ECommerce.Api.Filters;

/// <summary>
/// Runs the FluentValidation validator (if one is registered) for every action argument
/// and short-circuits with a 400 ValidationProblemDetails response on failure.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;
        var modelState = new ModelStateDictionary();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument), context.HttpContext.RequestAborted);

            foreach (var error in result.Errors)
                modelState.AddModelError(ToCamelCasePath(error.PropertyName), error.ErrorMessage);
        }

        if (!modelState.IsValid)
        {
            var problem = services.GetRequiredService<ProblemDetailsFactory>()
                .CreateValidationProblemDetails(context.HttpContext, modelState, StatusCodes.Status400BadRequest);
            context.Result = new BadRequestObjectResult(problem);
            return;
        }

        await next();
    }

    // "Items[0].Quantity" -> "items[0].quantity" to match the JSON property names
    internal static string ToCamelCasePath(string path)
        => string.Join('.', path.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
