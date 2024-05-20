namespace ChainLib.Node;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

public class Startup
{
    public Startup(IConfiguration configuration) => this.Configuration = configuration;

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    [Obsolete]
    public void ConfigureServices(IServiceCollection services)
    {
        _ = services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        _ = services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    [Obsolete]
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            _ = app.UseDeveloperExceptionPage();
        }
        else
        {
            _ = app.UseExceptionHandler("/Home/Error");
            _ = app.UseHsts();
        }

        _ = app.UseHttpsRedirection();
        _ = app.UseStaticFiles();
        _ = app.UseCookiePolicy();

        _ = app.UseMvc(routes => routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}"));
    }
}
