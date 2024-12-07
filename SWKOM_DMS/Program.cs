using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWKOM_DMS.Services;
using log4net;
using log4net.Config;
using System.IO;
using System.Reflection;
using SWKOM_DMS.logging;
using CustomLoggerFactory = SWKOM_DMS.logging.LoggerFactory;

namespace SWKOM_DMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.AddDbContext<DocumentDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

            // Ensure the application listens on port 80 inside the container
            builder.WebHost.UseUrls("http://*:80");

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure log4net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("C:\\Users\\eNeSSeNe\\Desktop\\SWKOM_DMS\\SWKOM_DMS\\logging\\log4net.config")); // Path to your config file

            // Register RabbitMQService with ILoggerWrapper
            builder.Services.AddSingleton<ILoggerWrapper>(CustomLoggerFactory.GetLogger());
            builder.Services.AddSingleton<RabbitMQService>();


            var logger = new Log4NetWrapper(); // assuming `Log4NetWrapper` has been properly configured
            logger.Info("Application started - test log");


            var app = builder.Build();

            // Enable CORS
            app.UseCors("AllowAll");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
