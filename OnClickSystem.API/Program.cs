using Microsoft.EntityFrameworkCore;
using OnClickSystem.Application.Services;
using OnClickSystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURAÇÃO DO BANCO DE DADOS (Entity Framework)
// Substitua "DefaultConnection" pelo nome exato que está no seu appsettings.json
builder.Services.AddDbContext<OnClickContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. INJEÇÃO DE DEPENDÊNCIA
// Sempre que a API pedir um IFinanceiroService, o sistema entrega um FinanceiroService
builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();

// 3. CONFIGURAÇÃO DE CORS (Essencial para Sistemas Web e Desktop)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirGeral", policy =>
    {
        policy.AllowAnyOrigin()  // Permite requisições de qualquer lugar (Web/Forms)
              .AllowAnyMethod()  // Permite GET, POST, PUT, DELETE
              .AllowAnyHeader(); // Permite envio de tokens e JSON
    });
});

builder.Services.AddControllers();

// Configuração do OpenAPI/Swagger (Ótimo para apresentar os endpoints para a banca)
builder.Services.AddOpenApi();

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Ativa a interface visual do Swagger para você testar a API no navegador
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OnClickSystem API v1");
    });
}

app.UseHttpsRedirection();

// 4. APLICA A POLÍTICA DE CORS ANTES DA AUTORIZAÇÃO
app.UseCors("PermitirGeral");

app.UseAuthorization();

app.MapControllers();

app.Run();