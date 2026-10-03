/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Treachery.Server;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();
        services.AddRazorPages();
        services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaximumReceiveMessageSize = 4194304;
                options.AddFilter<ErrorLoggingHubFilter>();
            }).AddJsonProtocol(jsonOptions => GameEventJsonTypeInfoResolver.Configure(jsonOptions.PayloadSerializerOptions));
        services.AddDbContext<TreacheryContext>();
        services.AddScoped<ErrorLogService>();
        services.AddScoped<ErrorLoggingHubFilter>();
        services.AddHostedService<ErrorLogCleanupService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseMiddleware<ErrorLoggingMiddleware>();

        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapHub<GameHub>("/gameHub");
            endpoints.MapFallbackToFile("index.html");
        });
        
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<TreacheryContext>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
        try
        {
            logger.LogInformation("Starting game database migration...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var compressedGames = GameStorageMigration.Migrate(context, logger);
            logger.LogInformation("Game database migration completed in {Elapsed}.", stopwatch.Elapsed);
            if (compressedGames > 0)
                logger.LogInformation("Compressed legacy JSON for {GameCount} games.", compressedGames);
            var deletedErrorLogs = context.ErrorLogs
                .Where(entry => entry.OccurredAt < DateTime.UtcNow.AddDays(-30))
                .ExecuteDelete();
            logger.LogInformation("Deleted {DeletedCount} error-log entries older than 30 days.", deletedErrorLogs);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Game database migration failed; the server will not start.");
            throw;
        }
    }
}