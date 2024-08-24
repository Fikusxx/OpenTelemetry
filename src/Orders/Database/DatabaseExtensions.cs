using Microsoft.EntityFrameworkCore;

namespace Orders.Database;

public static class DatabaseExtensions
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDb")));

        return builder;
    }
    
    public static WebApplication CreateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<OrdersDbContext>();
        context.Database.EnsureCreated();

        return app;
    }
}