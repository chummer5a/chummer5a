using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChummerHub.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using System.IO;
using ChummerHub.Controllers;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.UI.Services;
using ChummerHub.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.Facebook;
using ChummerHub.Services.GoogleDrive;
using Microsoft.Extensions.Logging;
using ChummerHub.API;
using ChummerHub.Controllers.V1;

namespace ChummerHub
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;

        private static DriveHandler _gdrive = null;
        public static DriveHandler GDrive
        {
            get
            {
                return _gdrive;
            }
        }

        public Startup(ILogger<Startup> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
            if (_gdrive == null)
                _gdrive = new DriveHandler(logger, configuration);
        }

        public IConfiguration Configuration { get; }

        public IServiceCollection MyServices { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            MyServices = services;

           
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                
            });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"));
                
            });

            services.AddScoped<SignInManager<ApplicationUser>, SignInManager<ApplicationUser>>();

            //services.AddDefaultIdentity<ApplicationUser>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddSignInManager();

            //services.AddIdentity<ApplicationUser, IdentityRole>()
              services.AddDefaultIdentity<ApplicationUser>(options =>
              {
                  
              })
              .AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders()
              .AddSignInManager(); 

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddSingleton<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddRazorPagesOptions(options =>
                {
                    options.AllowAreas = true;
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
                });

            

            services.AddAuthentication()
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                    facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                    facebookOptions.BackchannelHttpHandler = new FacebookBackChannelHandler();
                    facebookOptions.UserInformationEndpoint = "https://graph.facebook.com/v2.8/me?fields=id,name,email,first_name,last_name";
                    //facebookOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                })
                .AddGoogle(options =>
                {
                    //options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";
#if DEBUG
                    options.Authority = "http://localhost:5000";
#else
                    options.Authority = "http://sinners.azurewebsites.net";
#endif
                    options.RequireHttpsMetadata = false;

                    options.ClientId = "mvc";
                    options.SaveTokens = true;
                });

            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie(options =>
            //    {
            //        options.ExpireTimeSpan = TimeSpan.MaxValue;
            //    });

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
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                //options.Cookie.HttpOnly = false;
                //options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                //options.LoginPath = "/Identity/Account/Login";
                //options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                //options.SlidingExpiration = true;

                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.Cookie.Name = "SINnersCookie";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.MaxValue;
                options.LoginPath = "/Identity/Account/Login";
                // ReturnUrlParameter requires 
                //using Microsoft.AspNetCore.Authentication.Cookies;
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            services.AddMvcCore()
                .AddVersionedApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    
                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                })
                .AddAuthorization()
                .AddJsonFormatters();

//            services.AddAuthentication("Bearer")
//                .AddIdentityServerAuthentication(options =>
//                {
//#if DEBUG
//                    options.Authority = "http://localhost:5000";
//#else
//                    options.Authority = "http://sinners.azurewebsites.net";
//#endif
//                    options.RequireHttpsMetadata = false;
//                    options.ApiName = "api1";
//                });

            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                //o.ApiVersionReader = new HeaderApiVersionReader("api-version");
                //o.Conventions.Controller<Controllers.V1.SINnerController>().HasApiVersion(new ApiVersion(1, 0));
                //o.Conventions.Controller<Controllers.V2.SINnerController>().HasApiVersion(new ApiVersion(2, 0));
            });


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
                    options.SwaggerDoc(description.GroupName, new Swashbuckle.AspNetCore.Swagger.Info
                    {
                        Version = description.GroupName,
                        Title = "ChummerHub",
                        Description = "Description for API " + description.GroupName + " to store and search Chummer Xml files",
                        TermsOfService = "None",
                        Contact = new Contact
                        {
                            Name = "Archon Megalon",
                            Email = "archon.megalon@gmail.com",
                        },
                        License = new License
                        {
                            Name = "License",
                            Url = "https://github.com/chummer5a/chummer5a/blob/master/LICENSE.txt",

                        }

                    });
                }

                options.OperationFilter<FileUploadOperation>();



                // add a custom operation filter which sets default values
                //options.OperationFilter<SwaggerDefaultValues>();

                options.DescribeAllEnumsAsStrings();

                options.ExampleFilters();

                options.OperationFilter<AddResponseHeadersFilter>();
                

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

            });

            services.AddSwaggerExamples();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

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
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            
            }

          

        }


        
    }
}
