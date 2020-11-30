using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OcelotApiGw
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder builder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
            builder.ConfigureServices(x => x.AddSingleton(builder)).ConfigureAppConfiguration(x =>
                x.AddJsonFile(@"C:\github\my\bizbook365\src\ApiGw\OcelotApiGw\OcelotApiGw\bin\Debug\net5.0\configuration\configuration.json").AddJsonFile(@"C:\github\my\bizbook365\src\ApiGw\OcelotApiGw\OcelotApiGw\bin\Debug\net5.0\appsettings.json")); // todo: need to configure this path later

            return builder;
        }
    }
}
