using System.Text;
using System.Text.Json;

namespace EmployeeGraphQL.Infrastructure.Middleware;

public sealed class GraphQLExceptionStatusCodeMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/graphql", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        try
        {
            await next(context);

            responseBuffer.Position = 0;
            var responsePayload = await new StreamReader(responseBuffer, Encoding.UTF8).ReadToEndAsync();
            var inferredStatusCode = InferStatusCode(responsePayload);

            if (inferredStatusCode.HasValue)
            {
                context.Response.StatusCode = inferredStatusCode.Value;
            }

            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalBody);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static int? InferStatusCode(string responsePayload)
    {
        if (string.IsNullOrWhiteSpace(responsePayload))
        {
            return null;
        }

        try
        {
            var graphQlResponse = JsonSerializer.Deserialize<GraphQlErrorEnvelope>(responsePayload, JsonOptions);
            if (graphQlResponse?.Errors is null || graphQlResponse.Errors.Count == 0)
            {
                return null;
            }

            foreach (var error in graphQlResponse.Errors)
            {
                var statusCode = InferStatusCodeFromError(error);
                if (statusCode.HasValue)
                {
                    return statusCode;
                }
            }

            return StatusCodes.Status500InternalServerError;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static int? InferStatusCodeFromError(GraphQlError error)
    {
        if (error.Extensions is not null)
        {
            if (TryReadHttpStatus(error.Extensions, out var explicitHttpStatus))
            {
                return explicitHttpStatus;
            }

            if (error.Extensions.TryGetValue("code", out var codeElement))
            {
                var code = codeElement.GetString();
                if (string.Equals(code, "ASM_FORBIDDEN", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCodes.Status403Forbidden;
                }

                if (string.Equals(code, "AUTH_NOT_AUTHENTICATED", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCodes.Status401Unauthorized;
                }

                if (string.Equals(code, "VALIDATION_ERROR", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCodes.Status400BadRequest;
                }
            }
        }

        var message = error.Message;
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        if (message.Contains("schema", StringComparison.OrdinalIgnoreCase)
            && message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCodes.Status404NotFound;
        }

        if (message.Contains("Syntax Error", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Cannot query field", StringComparison.OrdinalIgnoreCase)
            || message.Contains("validation", StringComparison.OrdinalIgnoreCase)
            || message.Contains("variable", StringComparison.OrdinalIgnoreCase)
            || message.Contains("required", StringComparison.OrdinalIgnoreCase)
            || message.Contains("invalid", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCodes.Status400BadRequest;
        }

        return null;
    }

    private static bool TryReadHttpStatus(Dictionary<string, JsonElement> extensions, out int statusCode)
    {
        statusCode = default;
        if (!extensions.TryGetValue("httpStatus", out var statusElement))
        {
            return false;
        }

        return statusElement.ValueKind switch
        {
            JsonValueKind.Number => statusElement.TryGetInt32(out statusCode),
            JsonValueKind.String => int.TryParse(statusElement.GetString(), out statusCode),
            _ => false
        };
    }

    private sealed class GraphQlErrorEnvelope
    {
        public List<GraphQlError>? Errors { get; init; }
    }

    private sealed class GraphQlError
    {
        public string? Message { get; init; }

        public Dictionary<string, JsonElement>? Extensions { get; init; }
    }
}
