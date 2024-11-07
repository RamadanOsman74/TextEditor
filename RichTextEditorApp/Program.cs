
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RichTextEditorApp.Models;
using System;

namespace RichTextEditorApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                       options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB limit
            }); 
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("cs"));
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseStaticFiles();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("Content-Security-Policy",
                    "default-src 'self'; script-src 'self'; style-src 'self' " +
                    "'unsafe-inline'; img-src 'self' data:; object-src 'none';");

                await next();
            });

            app.MapControllers();

            app.Run();
        }
    }
}
