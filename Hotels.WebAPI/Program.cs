using System.Text;
using Hotels.WebAPI.Api;
using Hotels.WebAPI.Data;
using Hotels.WebAPI.Services;
using Hotels.WebAPI.Interfaces;
using Hotels.WebAPI.Services.Repository;
using Hotels.WebAPI.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
RegisterServices(builder.Services);

WebApplication app = builder.Build();
Configure(app);
RegisterApis();

app.Run();

void RegisterServices(IServiceCollection services)
{
    services.AddSwaggerGen();
    services.AddAuthorization();
    services.AddEndpointsApiExplorer();
    services.AddSingleton<ITokenService, TokenService>();
    services.AddScoped<IHotelRepository, HotelRepository>();
    services.AddSingleton<IUserRepository, UserRepository>();
    services.AddTransient<IApi, HotelApi>();
    services.AddTransient<IApi, AuthApi>();
    services.AddDbContext<HotelDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
    });
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        string key = builder.Configuration["Jwt:Key"] ?? "KitM4-1111111111111111111111111111111111111111111111111111111111111111111";
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });
}

void Configure(WebApplication app)
{
    app.UseAuthorization();
    app.UseAuthentication();
    app.UseHttpsRedirection();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        using IServiceScope scopeService = app.Services.CreateScope();
        HotelDbContext dbContext = scopeService.ServiceProvider.GetRequiredService<HotelDbContext>();
        dbContext.Database.EnsureCreated();
    }
}

void RegisterApis()
{
    IEnumerable<IApi> apis = app.Services.GetServices<IApi>();
    foreach (IApi api in apis)
    {
        if (api is null) throw new InvalidProgramException("Api not found");
        api.Register(app);
    }
}