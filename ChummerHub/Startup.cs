using System;
using System.IO;
using System.Reflection;
using ChummerHub.Controllers.V1;
using ChummerHub.Data;
using ChummerHub.Services;
using ChummerHub.Services.Application_Insights;
using ChummerHub.Services.GoogleDrive;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore;
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
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;

namespace ChummerHub
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Startup'
    public class Startup
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Startup'
    {



        private readonly ILogger<Startup> _logger;

        private static DriveHandler _gdrive;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Startup.GDrive'
        public static DriveHandler GDrive
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Startup.GDrive'
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



#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Startup.Startup(ILogger<Startup>, IConfiguration)'
        public Startup(ILogger<Startup> logger, IConfiguration configuration)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Startup.Startup(ILogger<Startup>, IConfiguration)'
        {
            _logger = logger;
            Configuration = configuration;
            if (_gdrive == null)
                _gdrive = new DriveHandler(logger, configuration);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Startup.Configuration'
        public IConfiguration Configuration { get; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Startup.Configuration'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Startup.MyServices'
        public IServiceCollection MyServices { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Startup.MyServices'

        //readonly string MyAllowAllOrigins = "AllowAllOrigins";

        // This method gets called by the runtime. Use this method to add services to the container.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Startup.ConfigureServices(IServiceCollection)'
        public void ConfigureServices(IServiceCollection services)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Startup.ConfigureServices(IServiceCollection)'
        {
            MyServices = services;

            ConnectionStringToMasterSqlDb = Configuration.GetConnectionString("MasterSqlConnection");
            ConnectionStringSinnersDb = Configuration.GetConnectionString("DefaultConnection");

            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            // Use this if MyCustomTelemetryInitializer can be constructed without DI injected parameters
            services.AddSingleton<ITelemetryInitializer>(new MyTelemetryInitializer());

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

            var tcbuilder = TelemetryConfiguration.Active.TelemetryProcessorChainBuilder;
            tcbuilder.Use(next => new GroupNotFoundFilter(next));

            // If you have more processors:
            tcbuilder.Use(next => new ExceptionDataProcessor(next));



            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

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
                    Configuration.GetConnectionString("DefaultConnection"),
                    builder =>
                    {
                        //builder.EnableRetryOnFailure(5);
                    });
                options.EnableDetailedErrors();
            });

            services.AddScoped<SignInManager<ApplicationUser>, SignInManager<ApplicationUser>>();


            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {

            })
              .AddRoleManager<RoleManager<ApplicationRole>>()
              .AddRoles<ApplicationRole>()
              .AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders()
              .AddSignInManager();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddSingleton<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);

          

            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                                 .RequireAuthenticatedUser()
                                 .Build();
                var filter = new AuthorizeFilter(policy);
                options.Filters.Add(filter);
                options.EnableEndpointRouting = false;
            }).SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddRazorPagesOptions(options =>
                {
                    //options.AllowAreas = true;
                    //options.Conventions.AuthorizePage("/Home/Contact");
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/ChummerLogin/Logout");
                })
                .AddJsonOptions(x =>
                {
                    //x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    //x.SerializerSettings.PreserveReferencesHandling =
                    //    PreserveReferencesHandling.Objects;
                }); 


            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                //.AddFacebook(facebookOptions =>
                //{
                //    facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                //    facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                //    facebookOptions.BackchannelHttpHandler = new FacebookBackChannelHandler();
                //    facebookOptions.UserInformationEndpoint = "https://graph.facebook.com/v2.8/me?fields=id,name,email,first_name,last_name";
                //})
                //.AddGoogle(options =>
                //{
                //    options.ClientId = Configuration["Authentication:Google:ClientId"];
                //    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                //    options.CallbackPath = new PathString("/ExternalLogin");
                //})
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = new PathString("/Identity/Account/Login");
                    options.AccessDeniedPath = new PathString("/Identity/Account/AccessDenied");
                    options.LogoutPath = "/Identity/Account/Logout";
                    options.Cookie.Name = "Cookies";
                    options.Cookie.HttpOnly = false;
                    options.ExpireTimeSpan = TimeSpan.FromDays(5 * 365);
                    options.LoginPath = "/Identity/Account/Login";
                    // ReturnUrlParameter requires
                    //using Microsoft.AspNetCore.Authentication.Cookies;
                    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                    options.SlidingExpiration = false;
                })
                ;

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

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.Cookie.Name = "Cookies";
                options.Cookie.HttpOnly = false;
                options.ExpireTimeSpan = TimeSpan.FromDays(5 * 365);
                options.LoginPath = "/Identity/Account/Login";
                // ReturnUrlParameter requires
                //using Microsoft.AspNetCore.Authentication.Cookies;
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = false;
                //options.Events.OnRedirectToAccessDenied = context => {

                //    // Your code here.
                //    // e.g.
                //    throw new Not();
                //};
            });
          


            services.AddVersionedApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                })
                .AddAuthorization();

            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                //o.ApiVersionReader = new HeaderApiVersionReader("api-version");
                //o.Conventions.Controller<Controllers.V1.SINnerController>().HasApiVersion(new ApiVersion(1, 0));
                //o.Conventions.Controller<Controllers.V2.SINnerController>().HasApiVersion(new ApiVersion(2, 0));
            });

            services.AddSwaggerExamples();

            services.AddSwaggerGen(options =>
            {
                //options.AddSecurityDefinition("Bearer",
                //    new ApiKeyScheme {
                //        In = "header",
                //        Description = "Please enter JWT with Bearer into field",
                //        Name = "Authorization",
                //        Type = "apiKey" });
                //options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                //{
                //    { "Bearer", Enumerable.Empty<string>() },
                //});
                // resolve the IApiVersionDescriptionProvider service
                // note: that we have to build a temporary service provider here because one has not been created yet
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

                //options.OperationFilter<FileUploadOperation>();

                // add a custom operation filter which sets default values
                //options.OperationFilter<SwaggerDefaultValues>();

                options.DescribeAllEnumsAsStrings();

                options.ExampleFilters();

                options.OperationFilter<AddResponseHeadersFilter>();


                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                //options.MapType<FileResult>(() => new Schema
                //{
                //    Type = "file",
                //});
                //options.MapType<FileStreamResult>(() => new Schema
                //{
                //    Type = "file",
                //});

            });
          
            
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



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Startup.Configure(IApplicationBuilder, IHostingEnvironment)'
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Startup.Configure(IApplicationBuilder, IHostingEnvironment)'
        {
            //app.UseSession();
            app.UseCors(options => options.AllowAnyOrigin());
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();

            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            //app.UseCookiePolicy();

            app.UseAuthentication();


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
                //dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

            }

            Seed(app);

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Program.Seed()'
        public static void Seed(IApplicationBuilder app)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Program.Seed()'
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
                try
                {
                    var env = services.GetService<IHostingEnvironment>();
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
