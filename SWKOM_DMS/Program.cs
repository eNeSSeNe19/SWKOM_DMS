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

            // AutoMapper setup
            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            // Database Context
            builder.Services.AddDbContext<DocumentDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

            // Register RabbitMQService
            builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>(); // Singleton

            // Set up URLs
            builder.WebHost.UseUrls("http://*:80");

            // CORS Policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Add services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Register Elasticsearch client provider
            builder.Services.AddSingleton<IElasticsearchClientProvider>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var elasticUri = configuration["ElasticsearchConfig:Uri"];
                return new ElasticsearchClientProvider(elasticUri);
            });



            // Configure log4net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "logging", "log4net.config");
            XmlConfigurator.Configure(logRepository, new FileInfo(logFilePath));

            // Register ILoggerWrapper
            builder.Services.AddSingleton<ILoggerWrapper>(provider => CustomLoggerFactory.GetLogger());

            // Log the startup of the app
            var app = builder.Build();

            // Use the logger from DI container
            var logger = app.Services.GetRequiredService<ILoggerWrapper>();
            logger.Info("Application started - test log");

            // Call RabbitMQ consume method
            using (var scope = app.Services.CreateScope())
            {
                var rabbitMQService = scope.ServiceProvider.GetRequiredService<IRabbitMQService>();
                rabbitMQService.ConsumeOcrResults();
            }


            // Enable CORS
            app.UseCors("AllowAll");

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
