using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// 1. Banco de Dados
builder.Services.AddDbContext<OnClickContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Login (Autenticação)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/AcessoNegado";
        options.Cookie.Name = "OnClickSystem_Auth";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

// 3. Configuração de Sessão (NECESSÁRIO PARA O CARRINHO)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Carrinho dura 30min
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. Serviços de HttpContext (Para acessar a sessão no Controller/View)
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllersWithViews();

// 5. Minhas regras SERVICES
builder.Services.AddScoped<OnClickSystem.Application.Services.ComissaoService>();
builder.Services.AddScoped<OnClickSystem.Application.Services.RedeService>();
builder.Services.AddScoped<OnClickSystem.Application.Services.FinanceiroService>();
builder.Services.AddScoped<OnClickSystem.Application.Services.UsuarioService>();
builder.Services.AddScoped<OnClickSystem.Application.Services.AuthService>();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ATIVAR A SESSÃO (Deve vir antes do MapControllerRoute)
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();