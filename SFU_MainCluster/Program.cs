using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SFU_MainCluster.Domain;
using SFU_MainCluster.SFU.Main;
using Fleck;
using MessagesModels.Enums;
using SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers;
using SFU_MainCluster.SFU.Main.WebSocket;
using SFU_MainCluster.SFU.Server_Options;
using SFU_MainCluster.SFU.Tools;
using Coordinator = SFU_MainCluster.SFU.Main.Server.Coordinator.Coordinator;

namespace SFU_MainCluster
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var options = new WebApplicationOptions
            {
                Args = args,
                WebRootPath = "Web/wwwroot" 
            };

            var builder = WebApplication.CreateBuilder(options);
            
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfiguration configuration = configBuilder.Build();
            AspServerConfig config = configuration.GetSection("Project").Get<AspServerConfig>()!;
            builder.Services.AddControllersWithViews();
            builder.Services.AddHostedService<WebSocketHostedService>();
            builder.Services.AddDbContext<ServerDbContext>(x => x.UseSqlite(config.Database.ConnectionString));
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(opts =>
            {
                opts.Password.RequiredLength = 6;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
            }).AddEntityFrameworkStores<ServerDbContext>().AddDefaultTokenProviders();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "VideoServerCookie";
                options.Cookie.HttpOnly = true;
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });
            builder.Services.AddSingleton<Coordinator>();
            
            var app = builder.Build();
            
            // Configure the HTTP request pipeline.
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
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Download}/{id?}");
            
            await app.RunAsync();
        }

        public class WebSocketHostedService : BackgroundService
        {
            private WebSocketServer? _wsServer;
            private readonly Coordinator _coordinator;
            
            public WebSocketHostedService(Coordinator coordinator)
            {
                _coordinator = coordinator;
                _coordinator.RegisterHandler(MessageType.WebRTCInit, new WebRTCNegotiationMessagesHandler(_coordinator));
                _coordinator.RegisterHandler(MessageType.SessionInfoRequest, new InfoRequestMessagesHandler(_coordinator));
                _coordinator.RegisterHandler(MessageType.RoomRequest, new RoomActionsMessagesHandler(_coordinator));
            }
            
            protected override Task ExecuteAsync(CancellationToken stoppingToken)
            {
                var cert = X509CertificateLoader.LoadPkcs12FromFile("Certs/server.pfx", "MyPassword");
                Console.WriteLine(cert.NotAfter);
                _wsServer = new WebSocketServer("wss://0.0.0.0:26666");
                _wsServer.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                _wsServer.Certificate = cert;

                _wsServer.Start(socket =>
                {
                    var handler = new MainWebSocket(_coordinator, socket); 

                    socket.OnOpen = handler.OnOpen;
                    socket.OnClose = handler.OnClose;
                    socket.OnMessage = handler.OnMessage;
                    socket.OnError = handler.OnError;
                });                
                ServerTools.Logger.LogInformation("Coordinator accessible in ASP NET Core instance.");

                return Task.CompletedTask;
            }
        }
    }
}
