using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Preserve property names as defined in C# classes (no camelCase conversion)
        options.JsonSerializerOptions.PropertyNamingPolicy = null;

        // Ensure enums are handled as their string values (not numeric) for Swagger + runtime binding
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "BETA",
        Title = "HDB Data Services API",
        Description = "This web page serves as the main interface, documentation, and testing service for the available HDB data services. " +
            "This service is being developed using a technology stack comprised of .NET Core, ASP.NET Core, Dapper, and Swagger. " +
            "Contact the developer for feedback, data service questions, or the development of new data services within this " +
            "web page." +
            "\n\n" +
            "USAGE - Input your HDB and Log-In information in the boxes on the top-right corner of this page before testing any of " +
            "the available APIs. These will be your typical HDB log-in credentials. For example, I will put in lchdb2, my username, " +
            "and my password in order to connect to the LC HDB as myself. In code, you would have to attach custom headers to your HTTP " +
            "Request with the keys (1)api_hdb, (2)api_user, and (3)api_pass for the (1)HDB you are trying to access and your (2)user name and (3)password " +
            "credentials." +
            "\n\n" +
            "This Application Programming Interface (API) is preliminary or provisional and is subject to revision. " +
            "It is currently in development and as such, frequent updates, downtimes, and loss of functionality are to " +
            "be expected. The API has not received final approval by Reclamation. No warranty, expressed or implied, is " +
            "made as to the functionality of the API nor shall the fact of release constitute any such warranty. " +
            "The API is provided on the condition that neither Reclamation nor the U.S. Government shall be held liable " +
            "for any damages resulting from the authorized or unauthorized use of the API.",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Developer",
            Email = "jrocha@usbr.gov"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Group endpoints into legacy-like categories
    c.TagActionsBy(api =>
    {
        var controller = api.ActionDescriptor.RouteValues["controller"]?.ToLowerInvariant();
        return controller switch
        {
            "sites" or "datatypes" or "modelruns" or "sitedatatypes" => new[] { "HDB Tables" },
            "hdb" or "connect" => new[] { "HDB Connections" },
            "series" or "cgi" => new[] { "HDB TimeSeries Data" },
            "test" => new[] { "Testing Sandbox" },
            _ => new[] { controller ?? "default" }
        };
    });

    // Keep operations in a consistent order - POST before DELETE in series
    c.OrderActionsBy((apiDesc) =>
    {
        var controller = apiDesc.ActionDescriptor.RouteValues["controller"]?.ToLowerInvariant();
        var action = apiDesc.ActionDescriptor.RouteValues["action"]?.ToLowerInvariant();
        var method = apiDesc.HttpMethod;

        // Custom ordering for series endpoints
        if (controller == "series")
        {
            var methodOrder = method switch
            {
                "GET" => 1,
                "POST" => 2,
                "DELETE" => 3,
                _ => 4
            };
            return $"{controller}_{methodOrder}_{apiDesc.RelativePath}";
        }

        return $"{controller}_{apiDesc.RelativePath}";
    });


    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Ensure enums are represented as strings in the Swagger UI (dropdowns show readable values)
    c.SchemaFilter<HdbApi.Swagger.StringEnumSchemaFilter>();

    // Ensure top-level Swagger tag ordering matches the legacy API
    c.DocumentFilter<HdbApi.SwaggerExtensions.TagOrderDocumentFilter>();

    // Add custom header parameters
    c.AddSecurityDefinition("HDB Authentication", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "api_hdb",
        In = ParameterLocation.Header,
        Description = "HDB instance name (e.g., lchdb2)"
    });

    c.AddSecurityDefinition("User Authentication", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "api_user",
        In = ParameterLocation.Header,
        Description = "HDB username"
    });

    c.AddSecurityDefinition("Password Authentication", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "api_pass",
        In = ParameterLocation.Header,
        Description = "HDB password"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "HDB Authentication"
                }
            },
            new string[] {}
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "User Authentication"
                }
            },
            new string[] {}
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Password Authentication"
                }
            },
            new string[] {}
        }
    });
});

// Register database service
builder.Services.AddScoped<HdbApi.Services.IDatabaseService, HdbApi.Services.DatabaseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
{    // Redirect legacy swagger path to the current UI path
    app.MapGet("/swagger/ui/index", context =>
    {
        context.Response.Redirect("/swagger/ui");
        return Task.CompletedTask;
    });
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HDB Data Services API BETA");
        c.RoutePrefix = "swagger/ui/index"; // Match old API path
        c.DocumentTitle = "HDB Data Services API";
        c.DefaultModelsExpandDepth(-1); // Hide models section
        c.DefaultModelRendering(ModelRendering.Example);
        c.DisplayRequestDuration();

        // Enable response content type selection
        c.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Delete, SubmitMethod.Patch);

        // Inject custom CSS and JS
        c.InjectStylesheet("/swagger-extensions/screen.css");
        c.InjectStylesheet("/swagger-extensions/typography.css");
        c.InjectJavascript("/swagger-extensions/discoveryUrlSelector.js");
        c.InjectJavascript("/swagger-extensions/basicAuth.js");
        c.InjectJavascript("/swagger-extensions/parameterTable.js");

        // Custom index.html
        c.IndexStream = () => new FileStream(Path.Combine(app.Environment.ContentRootPath, "SwaggerExtensions", "index.html"), FileMode.Open, FileAccess.Read);
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Serve static files for Swagger extensions
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "SwaggerExtensions")),
    RequestPath = "/swagger-extensions"
});

app.MapControllers();

app.Run();