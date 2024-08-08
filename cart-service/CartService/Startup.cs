using CartService.Data;
using CartService.Services;

namespace CartService;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddScoped<ICartService, Services.CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IRepository, Repository>();
        services.AddScoped<IUserService, UserService>();
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors(options =>
        {
            options.AllowAnyHeader();
            options.AllowAnyMethod();
            options.AllowAnyOrigin();
        });

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/",
                async context =>
                {
                    await context.Response.WriteAsync("Welcome to CART SERVICE!");
                });
        });
    }
}