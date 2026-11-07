using CambodiaDigitalBankingSystem.API.Middleware;
using CambodiaDigitalBankingSystem.SHARED.Base.Constant;
using CambodiaDigitalBankingSystem.SHARED.DataAccess;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<ISqlDataAccess, SqlDataAccess>();
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        // Model binding errors may contain attempted values: never reflect those values to consumers.
        var errors = context.ModelState.Where(pair => pair.Value?.Errors.Count > 0)
            .ToDictionary(pair => pair.Key, _ => new[] { MessageConstant.BadRequest });
        return new BadRequestObjectResult(ApiResponse<object?>.Fail(
            MessageConstant.ValidationError, errors, context.HttpContext.TraceIdentifier));
    };
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStatusCodePages(async statusContext =>
{
    var context = statusContext.HttpContext;
    var message = context.Response.StatusCode switch
    {
        400 => MessageConstant.BadRequest,
        401 => MessageConstant.Unauthorized,
        403 => MessageConstant.Forbidden,
        404 => MessageConstant.NotFound,
        409 => MessageConstant.Conflict,
        _ => MessageConstant.UnexpectedError
    };
    await context.Response.WriteAsJsonAsync(
        ApiResponse<object?>.Fail(message, traceId: context.TraceIdentifier), context.RequestAborted);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}