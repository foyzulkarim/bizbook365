using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.IdentityModel.Tokens;
using OcelotApiGw.Utilities;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace OcelotApiGw
{
    public class Startup
    {
        const string BizBook365Com = "bizbook365.com";
        private const string SecretKey = "a55ae165-912d-4052-b61e-aeeb6d6d2c07";
        private readonly SymmetricSecurityKey SigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Constants>(new Constants(Configuration));
            services.AddOcelot(Configuration);
            // services.AddAuthentication();
            // services.AddTokenValidation(BizBook365Com, BizBook365Com, SigningKey);
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(SecretKey, x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = SigningKey,
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OcelotApiGw", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OcelotApiGw v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            OcelotPipelineConfiguration ocelotConfig = new OcelotPipelineConfiguration();

            // ocelotConfig.AuthenticationMiddleware = async (ctx, next) =>
            // {
            //     Console.WriteLine("AuthenticationMiddleware", ctx);
            //     await next();
            // };

            ocelotConfig.AuthorisationMiddleware = async (ctx, next) =>
            {
                var context = ctx as DefaultHttpContext;
                var resource = context.HttpContext.Request.Path.Value;
                var token = context.HttpContext.Request.Headers["Authorization"].ToString();
                HttpAuthorization(resource, token);
                await next();
            };

            app.UseOcelot(ocelotConfig);
        }

        private static string HttpAuthorization(string resource, string token)
        {
            var client = Factory.HttpClient;
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
            var url = $"{Constants.AuthServer}/api/AuthorizeToken?resource={resource}";
            var httpResponseMessage = client.GetAsync(url).GetAwaiter().GetResult();
            httpResponseMessage.EnsureSuccessStatusCode();
            return token;
        }
    }

    public class Constants
    {
        public static string AuthServer { get; private set; }

        public Constants(IConfiguration config)
        {
            var s = config["AuthServer"];
            AuthServer = s;
        }
    }

    public static class Factory
    {
        //private static Greeter.GreeterClient _greeterClient;

        private static HttpClient _httpClient;

        // public static Greeter.GreeterClient GreeterClient
        // {
        //     get
        //     {
        //         if (_greeterClient == null)
        //         {
        //             GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:5005");
        //             _greeterClient = new Greeter.GreeterClient(channel);
        //         }

        //         return _greeterClient;
        //     }
        // }

        public static HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                }

                return _httpClient;
            }
        }
    }
}
