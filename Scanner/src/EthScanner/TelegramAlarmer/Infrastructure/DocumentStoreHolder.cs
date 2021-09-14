using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using TelegramAlarmer;
using TelegramAlarmer.Infrastructure;

namespace Northwind
{
    public static class DocumentStoreHolder
    {
        private static readonly Lazy<IDocumentStore> LazyStore =
            new Lazy<IDocumentStore>(() =>
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                var config = builder.Build();

                var dbConfig = config.GetSection("Database").Get<AppSettings.DatabaseSettings>();

                var store = new DocumentStore
                {
                    Urls = dbConfig.Urls,
                    Database = dbConfig.DatabaseName,
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

                return store;
            });

        public static IDocumentStore Store => LazyStore.Value;
    }
}
