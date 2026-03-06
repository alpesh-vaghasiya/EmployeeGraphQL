using FluentValidation;
using FluentValidation.Results;
using System.Text.Json;

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _services;

    public ValidationMiddleware(RequestDelegate next, IServiceProvider services)
    {
        _next = next;
        _services = services;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/graphql"))
        {
            await _next(context);
            return;
        }

        // 🚀 Skip validation for file uploads (multipart/form-data)
        if (context.Request.ContentType != null &&
            context.Request.ContentType.Contains("multipart/form-data"))
        {
            await _next(context);
            return;
        }

        // Enable request body rewind
        context.Request.EnableBuffering();

        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        context.Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            await _next(context);
            return;
        }

        JsonElement payload;

        try
        {
            payload = JsonSerializer.Deserialize<JsonElement>(body);
        }
        catch
        {
            // 🚀 If body is not JSON → skip validation instead of throwing error
            await _next(context);
            return;
        }

        if (payload.TryGetProperty("variables", out var vars) &&
            vars.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in vars.EnumerateObject())
            {
                var variableValue = property.Value;

                if (variableValue.ValueKind == JsonValueKind.Null)
                    continue;

                var modelObj = variableValue.Deserialize<object>();
                if (modelObj == null)
                    continue;

                var modelType = modelObj.GetType();
                var validatorType = typeof(IValidator<>).MakeGenericType(modelType);

                var validator = _services.GetService(validatorType);

                if (validator != null)
                {
                    var validateMethod = validatorType.GetMethod("Validate",
                        new[] { modelType });

                    var result = (ValidationResult)validateMethod.Invoke(validator, new[] { modelObj });

                    if (!result.IsValid)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            errors = result.Errors.Select(e => new
                            {
                                field = e.PropertyName,
                                message = e.ErrorMessage
                            })
                        });
                        return;
                    }
                }
            }
        }

        await _next(context);
    }
}