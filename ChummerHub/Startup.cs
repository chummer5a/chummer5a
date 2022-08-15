/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.IO;
using System.Reflection;
using System.Configuration;
using ChummerHub.Data;
using ChummerHub.Services;
using ChummerHub.Services.Application_Insights;
using ChummerHub.Services.GoogleDrive;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Newtonsoft.Json.Converters;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.Tracing;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using ChummerHub.Services.JwT;
using System.Text;

using Microsoft.Net.Http.Headers;
using Duende.IdentityServer.Configuration;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Duende.IdentityServer.Models;

namespace ChummerHub
{
    public class Startup
    {



        private readonly ILogger<Startup> _logger;

        private static DriveHandler _gdrive;
        public static DriveHandler GDrive
        {
            get
            {
                return _gdrive;
            }
        }

        /// <summary>
        /// This leads to the master-azure-db to create/edit/delete users
        /// </summary>
        public static string ConnectionStringToMasterSqlDb { get; set; }

        /// <summary>
        /// This leads to the master-azure-db to create/edit/delete users
        /// </summary>
        public static string ConnectionStringSinnersDb { get; set; }



        public Startup(ILogger<Startup> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
            AppSettings = System.Configuration.ConfigurationManager.AppSettings;
            if (_gdrive == null)
                _gdrive = new DriveHandler(logger, configuration);

            //Microsoft.IdentityModel.Logging.IdentityModelEventSource.Logger.LogLevel = System.Diagnostics.Tracing.EventLevel.Verbose;
            //Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            //Microsoft.IdentityModel.Logging.IdentityModelEventSource.Logger
            //var listener = new EventListener();
            //listener.EnableEvents(Microsoft.IdentityModel.Logging.IdentityModelEventSource.Logger, EventLevel.LogAlways);
            //listener.EventWritten += Listener_EventWritten;
            //Microsoft.IdentityModel.Logging.IdentityModelEventSource.Logger.WriteVerbose("A test message.");

        }

        //private void Listener_EventWritten(object sender, EventWrittenEventArgs e)
        //{
        //    foreach (object payload in e.Payload)
        //    {
        //        Debug.WriteLine($"[{e.EventName}] {e.Message} | {payload}");
        //    }
        //}

        public IConfiguration Configuration { get; }
        public static System.Collections.Specialized.NameValueCollection AppSettings { get; set; }

        public IServiceCollection MyServices { get; set; }

        //readonly string MyAllowAllOrigins = "AllowAllOrigins";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            MyServices = services;

            //services.AddSingleton<Diagnostics>();

            ConnectionStringToMasterSqlDb = Configuration.GetConnectionString("MasterSqlConnection");
            ConnectionStringSinnersDb = Configuration.GetConnectionString("DefaultConnection");
            var keys = new KeyVault(_logger);

            //ConnectionStringToMasterSqlDb = keys.GetSecret("MasterSqlConnection");
            //ConnectionStringSinnersDb = keys.GetSecret("DefaultConnection");

          

            // Use this if MyCustomTelemetryInitializer can be constructed without DI injected parameters
            services.AddSingleton<ITelemetryInitializer>(new MyTelemetryInitializer());

            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            //services.AddSingleton<ICookieManager, SystemWebCookieManager>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .SetIsOriginAllowedToAllowWildcardSubdomains();
                    });
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("https://www.shadowsprawl.com");
                    });
            });

            // Configure SnapshotCollector from application settings
            //services.Configure<SnapshotCollectorConfiguration>(Configuration.GetSection(nameof(SnapshotCollectorConfiguration)));

            // Add SnapshotCollector telemetry processor.
            //services.AddSingleton<ITelemetryProcessorFactory>(sp => new SnapshotCollectorTelemetryProcessorFactory(sp));
            Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions aiOptions
                = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions();
            aiOptions.InstrumentationKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
            // Disables adaptive sampling.
            aiOptions.EnableAdaptiveSampling = false;

            // Disables QuickPulse (Live Metrics stream).
            aiOptions.EnableQuickPulseMetricStream = true;

            services.AddApplicationInsightsTelemetry(aiOptions);
            
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                //options.Providers.Add<CustomCompressionProvider>();
                //options.MimeTypes =
                //    ResponseCompressionDefaults.MimeTypes.Concat(
                //        new[] { "image/svg+xml" });
            });


            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 100000000;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    //Configuration.GetConnectionString("DefaultConnection"),
                    ConnectionStringSinnersDb,
                    builder =>
                    {
                        //builder.EnableRetryOnFailure(5);
                    });
                options.EnableDetailedErrors();
            });
            var helper = new JwtHelper(Program.logger, Configuration);
            services.AddAuthentication(options =>
            {
                // custom scheme defined in .AddPolicyScheme() below
                options.DefaultScheme = "JWT_OR_COOKIE";
                options.DefaultChallengeScheme = "JWT_OR_COOKIE";
                options.DefaultAuthenticateScheme = "JWT_OR_COOKIE";
            })
            .AddCookie(options =>
            {
                
                options.LoginPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.ClaimsIssuer = helper.jwtToken.Issuer;
                
            })
            .AddJwtBearer(options =>
            {
                var helper = new JwtHelper(Program.logger, Configuration);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = helper.jwtToken.Issuer,
                    ValidateAudience = false,
                    ValidAudience = helper.jwtToken.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(helper.jwtToken.SigningKey)),
                    SaveSigninToken = true,
                };
                options.SaveToken = true;
                options.ClaimsIssuer = helper.jwtToken.Issuer;
                options.RequireHttpsMetadata = false;
            })
            // this is the key piece!
            .AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    string authorization = context.Request.Headers[HeaderNames.Authorization];
                    if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                        return JwtBearerDefaults.AuthenticationScheme;

                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            });

            #region OLDAUTH

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders()
                    .AddRoles<ApplicationRole>()
                    .AddRoleManager<RoleManager<ApplicationRole>>()
                    .AddSignInManager()
                    .AddClaimsPrincipalFactory<ClaimsPrincipalFactory>()
                    .AddDefaultUI();

            services.AddIdentityServer(options =>
            {

                //options.Authentication.CookieAuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //options.Authentication.CookieLifetime = TimeSpan.FromDays(365);
                //options.Authentication.CookieSlidingExpiration = true;
                options.UserInteraction = new UserInteractionOptions()
                {
                    LogoutUrl = "/Identity/Account/Logout",
                    LoginUrl = "/Identity/Account/Login",
                    LoginReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter,
                };
                if (Debugger.IsAttached)
                {
                    options.IssuerUri = "https://localhost:64939"; //configuration.GetRequiredValue<string>("Oidc:IdentityServer:IssuerUri");
                }
                else
                {
                    options.IssuerUri = Configuration["JWTIssuer"]; //Configuration.GetValue("Oidc:IdentityServer:IssuerUri");
                }
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.EmitStaticAudienceClaim = true;

            }).AddInMemoryIdentityResources(Config.IdentityResources)
              .AddInMemoryApiScopes(Config.ApiScopes)
              .AddInMemoryClients(Config.Clients)
              .AddAspNetIdentity<ApplicationUser>()
              //.AddCookieAuthentication();
              //.AddJwtBearerClientAuthentication();
            ;

            //services.AddAuthentication()
            //.AddCookie("Cookies", options =>
            //{
            //    options.LoginPath = new PathString("/Identity/Account/Login");
            //    options.AccessDeniedPath = new PathString("/Identity/Account/AccessDenied");
            //    options.LogoutPath = "/Identity/Account/Logout";
            //                //options.Cookie.Name = "Cookies";
            //    options.Cookie.HttpOnly = false;
            //    options.ExpireTimeSpan = TimeSpan.FromDays(365);
            //    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            //    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            //    options.LoginPath = "/Identity/Account/Login";
            //                // ReturnUrlParameter requires
            //                //using Microsoft.AspNetCore.Authentication.Cookies;
            //    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
            //    options.SlidingExpiration = true;
            //                //options.Events = new CookieAuthenticationEvents();
            //    options.Events.OnRedirectToLogin = (context) =>
            //    {

            //        context.HttpContext.Response.Redirect(context.RedirectUri);// "https://externaldomain.com/login");
            //        return Task.CompletedTask;
            //    };

            //    options.Events.OnSigningIn = (context) =>
            //    {
            //        context.CookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(30);
            //        return Task.CompletedTask;
            //    };
            //})
            //            .AddJwtBearer(options =>
            //            {
            //                options.Authority = "https://localhost:64939";
            //                options.TokenValidationParameters.ValidateAudience = false;
            //                options.RequireHttpsMetadata = false;
            //                options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
            //            })
            //        .AddOpenIdConnect("oidc", options =>
            //        {
            //            options.Authority = "https://localhost:64939";
            //            options.SignInScheme = "Cookies";
            //            options.RequireHttpsMetadata = false;
            //            options.SaveTokens = true;
            //            options.ClientId = "interactive.public";
            //                //options.ClientSecret = "secret";
            //                //options.ResponseType = "code";

            //            options.Scope.Clear();
            //            options.Scope.Add("openid");
            //            options.Scope.Add("profile");
            //            options.Scope.Add("api");
            //            options.Scope.Add("verification");
            //            options.Scope.Add("offline_access");
            //        });
            //            ;

            //            //services.AddAuthentication("Bearer")
            //            //     .AddIdentityServerAuthentication("Bearer", options =>
            //            //     {
            //            //         // required audience of access tokens
            //            //         options.ApiName = "api1";

            //            //         // auth server base endpoint (this will be used to search for disco doc)
            //            //         options.Authority = "https://localhost:5000";
            //            //     });


            #endregion

            services.AddScoped<SignInManager<ApplicationUser>, SignInManager<ApplicationUser>>();


            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);

        
            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                                 .RequireAuthenticatedUser()
                                 .Build();
                var filter = new AuthorizeFilter(policy);
                options.Filters.Add(filter);
                options.EnableEndpointRouting = false;
            })//.SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddRazorPagesOptions(options =>
                {
                    //options.AllowAreas = true;
                    //options.Conventions.AuthorizePage("/Home/Contact");
                    //options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    //options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
                    //options.Conventions.AuthorizeAreaPage("Identity", "/Account/ChummerLogin/Logout");
                })
                .AddNewtonsoftJson(x =>
                {
                    x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    x.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
                    x.SerializerSettings.Converters.Add(new StringEnumConverter());
                    x.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            
                });
            // order is vital, this *must* be called *after* AddNewtonsoftJson()
            services.AddSwaggerGenNewtonsoftSupport();

            //services.AddDataProtection()
            //    .PersistKeysToFileSystem(PersistKeysLocation.GetKeyRingDirInfo()).SetApplicationName(Config.ApplicationName);

        


            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 0;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
#if DEBUG
                options.SignIn.RequireConfirmedEmail = false;
#else
                options.SignIn.RequireConfirmedEmail = true;
#endif
                options.SignIn.RequireConfirmedPhoneNumber = false;

                
                
            });

          
            services.AddVersionedApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                })
                //.AddAuthorization()
                ;
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                //o.ApiVersionReader = new HeaderApiVersionReader("api-version");
                //o.Conventions.Controller<Controllers.V1.SINnerController>().HasApiVersion(new ApiVersion(1, 0));
                //o.Conventions.Controller<Controllers.V2.SINnerController>().HasApiVersion(new ApiVersion(2, 0));
            });

            services.AddSwaggerExamplesFromAssemblyOf<Startup>();

            

            services.AddSwaggerGen(options =>
            {
                options.UseAllOfToExtendReferenceSchemas();
                
                AddSwaggerApiVersionDescriptions(services, options);
              
                options.ExampleFilters();

                options.OperationFilter<AddResponseHeadersFilter>();


                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                options.AddSecurityDefinition(JwtAuthenticationDefaults.AuthenticationScheme,
                new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = JwtAuthenticationDefaults.HeaderName, // Authorization
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    //Flows = new OpenApiOAuthFlows() { new AuthorizationCode }
                }) ;


                //options.AddSecurityDefinition(jwtSecurityScheme.Scheme, jwtSecurityScheme);


                //OpenApiSecurityRequirement requirement = new OpenApiSecurityRequirement()
                //{
                //    { jwtSecurityScheme, new List<string>() }
                //};

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtAuthenticationDefaults.AuthenticationScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });
            services.AddAzureAppConfiguration();


            //services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
            //services.AddSession();

            //services.AddHttpsRedirection(options =>
            //{
            //    options.HttpsPort = 443;
            //});

            //services.Configure<ApiBehaviorOptions>(options =>
            //{
            //    options.SuppressModelStateInvalidFilter = true;
            //});


        }

     

        private static void AddSwaggerApiVersionDescriptions(IServiceCollection services, Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
        {
            var provider = services.BuildServiceProvider()
                .GetRequiredService<IApiVersionDescriptionProvider>();

            // add a swagger document for each discovered API version
            // note: you might choose to skip or document deprecated API versions differently

            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo()
                {
                    Version = description.GroupName,
                    Title = "ChummerHub",
                    Description = "Description for API " + description.GroupName + " to store and search Chummer Xml files",
                    Contact = new OpenApiContact
                    {
                        Name = "Archon Megalon",
                        Email = "archon.megalon@gmail.com",
                    },
                    License = new OpenApiLicense
                    {
                        Name = "License",
                        Url = new Uri("https://github.com/chummer5a/chummer5a/blob/master/LICENSE.txt"),

                    }

                });
            }
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            //app.UseSession();
            app.UseCors(options => options.AllowAnyOrigin());
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseMigrationsEndPoint();

            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
         

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            try
            {
                app.UseAzureAppConfiguration();
            }
            catch(Exception e)
            {
                _logger?.LogWarning(e, e.Message);
            }
            app.UseRouting();
            app.UseAuthentication();
            app.UseIdentityServer();
           
           
            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax, Secure = CookieSecurePolicy.Always });
            
            app.UseAuthorization();

            app.UseMvc(routes =>
            {

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}"
                    );
                //routes.MapRoute(
                //    name: "Identity",
                //    template: "Identity/{controller=IdentityHome}/{action=Index}/{id?}"
                //    );
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // resolve the IApiVersionDescriptionProvider service
            // note: that we have to build a temporary service provider here because one has not been created yet
            var provider = MyServices.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options =>
            {
                //options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                //c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "V1");
                // build a swagger endpoint for each discovered API version
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using (var serviceScope = serviceScopeFactory.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dbContext != null)
                {
                    try
                    {
                        dbContext.Database.EnsureCreated();
                        try
                        {
                            using (var dbContextTransaction = dbContext.Database.BeginTransaction())
                            {
                                dbContext.Database.ExecuteSqlRaw(
                                    @"CREATE VIEW View_SINnerUserRights AS 
        SELECT        dbo.SINners.Alias, dbo.UserRights.EMail, dbo.SINners.Id, dbo.UserRights.CanEdit, dbo.SINners.GoogleDriveFileId, dbo.SINners.MyGroupId, dbo.SINners.LastChange
                         
FROM            dbo.SINners INNER JOIN
                         dbo.SINnerMetaData ON dbo.SINners.SINnerMetaDataId = dbo.SINnerMetaData.Id INNER JOIN
                         dbo.SINnerVisibility ON dbo.SINnerMetaData.VisibilityId = dbo.SINnerVisibility.Id INNER JOIN
                         dbo.UserRights ON dbo.SINnerVisibility.Id = dbo.UserRights.SINnerVisibilityId"
                                );
                                dbContextTransaction.Commit();
                            }
                        }

                        catch (SqlException e)
                        {
                            if (!e.Message.StartsWith("There is already an object"))
                                throw;
                        }
                    }
                    catch(SqlException e)
                    {

                        if(e.Message?.Contains("already exists") == false && e.Message.Contains("is already an object") == false)

                        {
                            throw;
                        }
                    }
                    
                }
            }

            Seed(app);
        }

        public static void Seed(IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();
                try
                {
                    context.Database.Migrate();
                }
                catch(SqlException e)
                {

                    if (e.Message.Contains("already exists") == false && e.Message.Contains("is already an object") == false)

                        throw;
                    //logger.LogWarning(e, e.Message);
                }
                catch (Exception e)
                {
                    try
                    {
                        var tc = new TelemetryClient();
                        var telemetry = new ExceptionTelemetry(e);
                        tc.TrackException(telemetry);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.ToString());
                    }
                    logger.LogError(e.Message, "An error occurred migrating the DB: " + e);
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }
                // requires using Microsoft.Extensions.Configuration;
                var config = services.GetRequiredService<IConfiguration>();
                // Set password with the Secret Manager tool.
                // dotnet user-secrets set SeedUserPW <pw>
                var testUserPw = config["SeedUserPW"];
                if (String.IsNullOrEmpty(testUserPw))
                {
                    var keys = new KeyVault(logger);
                    testUserPw = keys.GetSecret("SeedUserPW");
                }
                try

                {
                    var env = services.GetService<IHostEnvironment>();
                    SeedData.Initialize(services, testUserPw, env).Wait();
                }
                catch (Exception ex)
                {
                    try
                    {
                        var tc = new TelemetryClient();
                        var telemetry = new ExceptionTelemetry(ex);
                        tc.TrackException(telemetry);
                    }
                    catch (Exception e1)
                    {
                        logger.LogError(e1.ToString());
                    }
                    logger.LogError(ex.Message, "An error occurred seeding the DB: " + ex);
                }
            }

        }
    }
}
