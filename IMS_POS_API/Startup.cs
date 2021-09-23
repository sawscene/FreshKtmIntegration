using IMS_POS_API.DAL;
using IMS_POS_API.Helper;
using IMS_POS_API.Model;
using IMS_POS_API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

namespace IMS_POS_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            ConnectionModel.KEY = Configuration.GetValue<string>("KEY");
            services.AddAuthentication("BasicAuthentication")
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            services.AddTransient<ILoginDataAccess, LoginDataAccess>();
            services.AddTransient<ItemSaveDataAccess, ItemSaveDataAccess>();
            services.AddTransient<SalesService, SalesService>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",new OpenApiInfo{Title = "IMS API",Version = "v1"});
                options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
                {
                    Description = "Basic auth added to authorizaion header",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Scheme = "basic",
                    Type = SecuritySchemeType.Http
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Basic" }
                        },
                        new List<string>()
                    }
                });
            });
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
               builder.SetIsOriginAllowed(_ => true)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()

               );

                options.AddPolicy("CorsPolicy",
                    builder => builder
                        //.SetIsOriginAllowed(_ => true)
                        //.SetIsOriginAllowedToAllowWildcardSubdomains()
                        //.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed((host) => true)
                        .AllowCredentials()

                        );
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/Swagger/v1/swagger.json", "Insert Demo API");
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
