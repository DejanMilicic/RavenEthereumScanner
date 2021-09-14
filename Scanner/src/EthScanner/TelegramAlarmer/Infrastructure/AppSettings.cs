namespace TelegramAlarmer.Infrastructure
{
    public class AppSettings
    {
        public DatabaseSettings Database { get; set; }
        
        public class DatabaseSettings
        {
            public string[] Urls { get; set; }

            public string DatabaseName { get; set; }

            public string CertThumbprint { get; set; }

            public string CertPath { get; set; }

            public string CertPass { get; set; }
        }
    }
}
