using Apis.Services;
using LPS.GrpcServices;

namespace LPS.Apis
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Register gRPC
            services.AddGrpc();
            // Register MVC Controllers
            services.AddControllersWithViews();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.DictionaryKeyPolicy = null;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Register gRPC service
                endpoints.MapGrpcService<NodeGRPCService>();
                endpoints.MapGrpcService<MetricsGrpcService>();
                endpoints.MapGrpcService<EntityDiscoveryGrpcService>();
                endpoints.MapGrpcService<MonitorGRPCService>();

                // Register MVC routes
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
