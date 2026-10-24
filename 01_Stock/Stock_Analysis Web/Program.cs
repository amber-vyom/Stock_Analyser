var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<Stock_Analysis.Engine.NSE.NseDownloader>();
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddPolicy("frontend", p => p
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
