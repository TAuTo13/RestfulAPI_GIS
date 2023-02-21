using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using WebMapAPI_ASP.Data;

namespace WebMapAPI_ASP
{
    public class Startup
    {
    readonly string CorsPolicy = "CorsPolicy";

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        //Contextを追加し，PostgreSplとの接続を設定
        services.AddDbContext<MapPointTableContext>(options =>
        {
        options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), x => x.UseNetTopologySuite());
        });

        //Cors設定の追加
        services.AddCors(options =>
        {
            options.AddPolicy(name: CorsPolicy,
                    builder => builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowAnyOrigin()
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

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        //Cors設定の適用
        app.UseCors(CorsPolicy);
        }
    }
}
