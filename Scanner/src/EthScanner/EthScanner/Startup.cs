using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using EthScanner.Infrastructure;
using EthScanner.Subscriptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using System;
using EthScanner.Features;

namespace EthScanner
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton<IDocumentStore>(ctx =>
            {
                var dbConfig = Configuration.GetSection("Database").Get<Settings.DatabaseSettings>();

                var store = new DocumentStore
                {
                    Urls = dbConfig.Urls,
                    Database = dbConfig.DatabaseName
                };
                
                if (!string.IsNullOrWhiteSpace(dbConfig.CertThumbprint))
                {
                    // Try to open the store.
                    using var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    certStore.Open(OpenFlags.ReadOnly);

                    // Find the certificate that matches the thumbprint.
                    var certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, dbConfig.CertThumbprint, false);                    
                    certStore.Close();

                    store.Certificate = certCollection[0];
                }
                else if (!string.IsNullOrWhiteSpace(dbConfig.CertPath))
                    store.Certificate = new X509Certificate2(dbConfig.CertPath, dbConfig.CertPass);

                store.Initialize();

                IndexCreation.CreateIndexes(typeof(Startup).Assembly, store);

                return store;
            });

            services.AddMediatR(typeof(Startup));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDocumentStore store)
        {
            var telegramSettings = Configuration.GetSection("Telegram").Get<Settings.TelegramSettings>();

            var helper = new TelegramRateLimiter(telegramSettings.Token);
            var singleWhale = new SingleWhaleTransaction(store).Create(helper);
            var dailyWhale = new DailyWhales(store).Create(helper);
            var monthlyWhale = new MonthlyWhales(store).Create(helper);
            var telegramTask = helper.RunAsync();

            Task.WhenAll(singleWhale, dailyWhale, monthlyWhale).ConfigureAwait(false);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });

                endpoints.MapControllers();
            });
        }
    }
}
