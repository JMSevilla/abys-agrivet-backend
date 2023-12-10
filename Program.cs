using System.Text;
using abys_agrivet_backend.Authentication;
using abys_agrivet_backend.BusinessLayer.Constructors;
using abys_agrivet_backend.DB;
using abys_agrivet_backend.Helper.JWT;
using abys_agrivet_backend.Helper.MailSettings;
using abys_agrivet_backend.Repository.ThirdPartyServices;
using MailKit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MailService = abys_agrivet_backend.Services.MailService;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
builder.Services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
builder.Services.AddDbContext<APIDBContext>(options => 
    options.UseSqlServer(configuration["connectionStrings:prodenv"],
        providerOptions => providerOptions.EnableRetryOnFailure())
);

var passwordOptions = new PasswordOptions
{
    RequireDigit = false,
    RequiredLength = 6,
    RequireLowercase = false,
    RequireNonAlphanumeric = false,
    RequireUppercase = false
};
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password = passwordOptions;
});
builder.Services.AddIdentity<JWTIdentity, IdentityRole>()
    .AddEntityFrameworkStores<APIDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = configuration["JWT:ValidAudience"],
        ValidIssuer = configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:IssuerSigningKey"]))
    };
});
var myappOrigins = "_myAppOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(myappOrigins, policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddControllers();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
});
Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseUrls("http://localhost:5240");
        webBuilder.UseStartup<WebApplication>();
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 5001;
});
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "The API key to access API's",
        Type = SecuritySchemeType.ApiKey,
        Name = "x-api-key",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });
    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        },
        In = ParameterLocation.Header
    };
    var requirement = new OpenApiSecurityRequirement{
        { scheme, new List<string>() }
    };
    c.AddSecurityRequirement(requirement);
});
builder.Services.AddScoped<BaseConstructorUsers>();
builder.Services.AddScoped<BaseConstructorBranches>();
builder.Services.AddScoped<BaseConstructorVerification>();
builder.Services.AddScoped<BaseConstructorServices>();
builder.Services.AddScoped<BaseConstructorAppointment>();
builder.Services.AddScoped<ApiKeyAuthFilter>();
builder.Services.AddTransient<IMailServices, MailService>();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();
app.UseRouting();
app.UseCors(myappOrigins);

//app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();