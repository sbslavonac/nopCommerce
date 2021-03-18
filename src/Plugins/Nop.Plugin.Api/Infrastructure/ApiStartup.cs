using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Nop.Core.Infrastructure;
using Nop.Plugin.Api.Authorization.Policies;
using Nop.Plugin.Api.Authorization.Requirements;
using Nop.Plugin.Api.Configuration;
using Nop.Services.Configuration;
using Nop.Web.Framework.Infrastructure.Extensions;


namespace Nop.Plugin.Api.Infrastructure
{
    public class ApiStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(swagger =>
            {
                //This is to generate the Default UI of Swagger Documentation                
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "JWT Token Authentication API",
                    Description = "ASP.NET Core 5 Web API"
                });

                // To Enable authorization using Swagger (JWT)  
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                });
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}

                    }
                });
            });

            
            var apiConfigSection = configuration.GetSection("Api");
            

            if (apiConfigSection != null)
            {
                var apiConfig = new ApiConfiguration(); //services.AddConfig<ApiConfiguration>(apiConfigSection);

                //var apiConfig = services.ConfigureStartupConfig<ApiConfiguration>(apiConfigSection);

                if (!string.IsNullOrEmpty(apiConfig.SecurityKey))
                {
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                                          jwtBearerOptions =>
                                          {
                                              jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                                              {
                                                  ValidateIssuerSigningKey = true,
                                                  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiConfig.SecurityKey)),
                                                  ValidateIssuer = false, // ValidIssuer = "The name of the issuer",
                                                  ValidateAudience = false, // ValidAudience = "The name of the audience",
                                                  ValidateLifetime = true, // validate the expiration and not before values in the token
                                                  ClockSkew =
                                                                                                   TimeSpan.FromMinutes(apiConfig.AllowedClockSkewInMinutes)
                                              };
                                          });

                    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                    AddAuthorizationPipeline(services);
                }
            }

        }

        public void Configure(IApplicationBuilder app)
        {
            var rewriteOptions = new RewriteOptions()
                .AddRewrite("api/token", "/token", true);

            app.UseRewriter(rewriteOptions);

            app.UseCors(x => x.AllowAnyOrigin()
                             .AllowAnyMethod()
                             .AllowAnyHeader());

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ibis API V1");
                // c.RoutePrefix = string.Empty;
            });

            app.MapWhen(context => context.Request.Path.StartsWithSegments(new PathString("/api")),
                a =>
                {

                    a.Use(async (context, next) =>
                    {
                        Console.WriteLine("API Call");
                        context.Request.EnableBuffering();
                        await next();
                    });

                    //a.UseExceptionHandler("/api/error/500/Error");

                    a
                    .UseRouting()
                    .UseAuthentication()
                    .UseAuthorization()
                    .UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });

                }
            );
            app.UseDeveloperExceptionPage();
        }

        public int Order => 1;

        private static void AddAuthorizationPipeline(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            }).Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme,
                                  policy =>
                                  {
                                      policy.Requirements.Add(new ActiveApiPluginRequirement());
                                      policy.Requirements.Add(new AuthorizationSchemeRequirement());
                                      policy.Requirements.Add(new CustomerRoleRequirement());
                                      policy.RequireAuthenticatedUser();
                                  });
            });

            services.AddSingleton<IAuthorizationHandler, ActiveApiPluginAuthorizationPolicy>();
            services.AddSingleton<IAuthorizationHandler, ValidSchemeAuthorizationPolicy>();
            services.AddSingleton<IAuthorizationHandler, CustomerRoleAuthorizationPolicy>();

        }
    }
}
