using Microsoft.Extensions.Caching.Memory;

namespace BffService;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private readonly Dictionary<string, string> _routes = new();

    private void LoadRoutes()
    {
        var productService = Environment.GetEnvironmentVariable("PRODUCT_SERVICE");
        if (!string.IsNullOrWhiteSpace(productService))
            _routes.Add("product-svc", productService);
        var cartService = Environment.GetEnvironmentVariable("CART_SERVICE");
        if (!string.IsNullOrWhiteSpace(cartService))
            _routes.Add("cart-svc", cartService);
        var importService = Environment.GetEnvironmentVariable("IMPORT_SERVICE");
        if (!string.IsNullOrWhiteSpace(importService))
            _routes.Add("import-svc", importService);
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        LoadRoutes();
        services.AddHttpClient();
        services.AddMemoryCache();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.Map("/{service}/{*request}", async (
                string service,
                string request,
                HttpClient client,
                IMemoryCache memoryCache,
                HttpContext context
            ) =>
            {
                var queryString = context.Request.QueryString.Value ?? string.Empty;
                var method = context.Request.Method;
                var body = context.Request.Body;
                var headers = context.Request.Headers;
                
                if (!_routes.TryGetValue(service, out var route))
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync($"Service {service} not found");
                    return;
                }
                
                var targetUrl = $"{route}/{request}{queryString}";

                var isCacheable = method == "GET" && service == "product-svc" && request == "products";

                if (isCacheable && memoryCache.TryGetValue(targetUrl, out dynamic? cachedResponse) && cachedResponse is not null)
                {
                    Console.WriteLine("HIT");
                    foreach (var header in cachedResponse.Headers)
                    {
                        if (header.Key == "Transfer-Encoding")
                            continue;
                    
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }
                    await context.Response.WriteAsync((string) cachedResponse.Body);
                    return;
                }
                
                var targetRequest = new HttpRequestMessage(new HttpMethod(method), targetUrl)
                {
                    Content = new StreamContent(body)
                };
                
                foreach (var header in headers
                             .Where(x => x.Key == "Content-Type" || x.Key == "Content-Length"))
                {
                    targetRequest.Content.Headers.Add(header.Key, header.Value.ToArray());
                }
                
                foreach (var header in headers
                             .Where(x => x.Key != "Host" && x.Key != "Content-Type" && x.Key != "Content-Length"))
                {
                    targetRequest.Headers.Add(header.Key, header.Value.ToArray());
                }
                
                var response = await client.SendAsync(targetRequest);
                
                context.Response.StatusCode = (int)response.StatusCode;
                
                foreach (var header in response.Headers)
                {
                    if (header.Key == "Transfer-Encoding")
                        continue;
                    
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                var responseBody = await response.Content.ReadAsStringAsync();

                await context.Response.WriteAsync(responseBody);

                if (isCacheable)
                {
                    memoryCache.Set(targetUrl,
                        new { Body = responseBody, Headers = context.Response.Headers.ToList() },
                        TimeSpan.FromMinutes(2)
                    );
                }
            });
        });
    }
}