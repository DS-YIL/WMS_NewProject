using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WMS.Common
{
    public class Configurations
    {
        public readonly string _SQLconnectionString = string.Empty;
        public readonly string _PostgreconnectionString = string.Empty;
        public Configurations()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var root = configurationBuilder.Build();
            _SQLconnectionString = root.GetSection("ConnectionStrings").GetSection("DataConnection").Value;

            _PostgreconnectionString = root.GetSection("ConnectionStrings").GetSection("PostgresConnection").Value;

            var appSetting = root.GetSection("ApplicationSettings");

        }

        public string SQLConnectionString
        {
            get => _SQLconnectionString;
        }

        public string PostgresConnectionString
        {
            get => _PostgreconnectionString;
        }

    }
}
