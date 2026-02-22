using Microsoft.EntityFrameworkCore;
using SneakerWebAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using SneakerWebAPI.Services.UserService;
using SneakerWebAPI.Services.CardService;
using SneakerWebAPI.Services.SneakerService;
using SneakerWebAPI.Services.TokenService;
using SneakerWebAPI.Services.NewsService;
using SneakerWebAPI.Services.SneakerReleaseService;
using Quartz;
using SneakerWebAPI.ScheduledTasks;
using SneakerWebAPI.Services.GameService;
using SneakerWebAPI.Services.FunkoPopService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ISneakerService, SneakerService>();
builder.Services.AddScoped<IFunkoPopService, FunkoPopService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<ISneakerReleaseService, SneakerReleaseService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization bheader using the bearer scheme (\"bearer {token}\")",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddCors(options => options.AddPolicy(name: "NgOrigins", 
    policy => 
    {
        policy.WithOrigins("https://sneaker-45ef9.web.app","http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
     }
    ));
builder.Services.AddQuartz(q =>
{
    q.ScheduleJob<SneakerPricePoster>(trigger => trigger
       .WithIdentity("SneakerPricePosterTrigger")
       .StartNow()
       .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever())
   );

    q.ScheduleJob<GamePricePoster>(trigger => trigger
       .WithIdentity("GamePricePosterTrigger")
       .StartNow()
       .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever())
   );

    q.ScheduleJob<CardPricePoster>(trigger => trigger
       .WithIdentity("CardPricePosterTrigger")
       .StartNow()
       .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever())
   );

    q.ScheduleJob<FunkoPopPricePoster>(trigger => trigger
       .WithIdentity("FunkoPopPricePosterTrigger")
       .StartNow()
       .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever())
   );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
var app = builder.Build();

//var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
//app.Urls.Add($"http://*:{port}");
//app.MapGet("/", () => "Hello from render");
//Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
    app.UseSwaggerUI();
//}
app.UseCors("NgOrigins");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

