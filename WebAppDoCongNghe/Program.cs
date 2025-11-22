
using BanDoCongNghe.Services.VnpayServices;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entity;
using WebAppDoCongNghe.Service;

namespace WebAppDoCongNghe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // kết nối Sql
            builder.Services.AddDbContext<WebAppDoCongNgheContext>(options =>
            {
                var conn = builder.Configuration.GetConnectionString("MyDb");
                options.UseSqlServer(conn);
            });

            // đăng ký publicService
            builder.Services.AddScoped<PublicService>();

            // Lấy phần AppSettings từ appsettings.json
            builder.Services.Configure<AppSettings>(
                builder.Configuration.GetSection("AppSettings"));

            // đăng ký jwt
            builder.Services.AddSingleton<JwtService>();

            // JWT cấu hình

            var jwtKey = builder.Configuration["AppSettings:Key"];
            var jwtIssuer = builder.Configuration["AppSettings:Issuer"];
            var jwtAudience = builder.Configuration["AppSettings:Audience"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                 .AddJwtBearer(options =>
                 {
                     options.RequireHttpsMetadata = false;
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,
                         ValidIssuer = jwtIssuer,
                         ValidAudience = jwtAudience,
                         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                         ClockSkew = TimeSpan.Zero
                     };


                 });

            // Cấu hình Cloudinary
            builder.Services.Configure<CloudinarySettings>(
            builder.Configuration.GetSection("CloudinarySettings"));
            builder.Services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<
                    Microsoft.Extensions.Options.IOptions<CloudinarySettings>>().Value;

                var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
                return new Cloudinary(account);
            });

            // Đăng ký CloudinaryService
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();


            // cấu hình Email
            builder.Services.Configure<EmailSettings>(
            builder.Configuration.GetSection("EmailSettings"));

            // Đăng ký VnPay
            builder.Services.Configure<VNPayConfig>(builder.Configuration.GetSection("VNPay"));
            builder.Services.AddScoped<IVnpay, Vnpay>();
            builder.Services.AddSignalR();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.WithOrigins(("http://localhost:5173"))
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials() //  Quan trọng cho SignalR
                        .SetIsOriginAllowed(_ => true); // Cho phép tất cả domain 

                    });
            });


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();


            app.Run();
        }
    }
}
