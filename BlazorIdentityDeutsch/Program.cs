using BlazorIdentityDeutsch.Components;
using BlazorIdentityDeutsch.Components.Account;
using BlazorIdentityDeutsch.Data;
using BlazorIdentityDeutsch.Models;
using BlazorIdentityDeutsch.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BlazorIdentityDeutsch {
    public partial class Program {
        private static async Task Main() {
            var builder = WebApplication.CreateBuilder();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Step1.1: Vorbereitung Localization Order Resources anlegen
            // Step1.2: IdentityTexts.resx + IdentityTexts.de.resx erstellen (Sparversion)
            // Step1.3: Resourcen Order einbinden
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddAuthentication(options => {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
                .AddIdentityCookies();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options => {
                options.SignIn.RequireConfirmedAccount = true;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
                .AddRoles<IdentityRole>() // Step2.3: Rollen Management aktivieren
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            // Add Mail-Services for Identity
            var smtpSettings = builder.Configuration
                .GetSection("Smtp")
                .Get<SmtpSettings>() ?? new();

            if (smtpSettings.Enabled) {
                builder.Services.AddScoped<IEmailSender<ApplicationUser>, IdentityEmailSender>();
            } else {
                builder.Services.AddScoped<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseMigrationsEndPoint();
            } else {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Step2.4: Rollenmanagement konfigurieren
            await IdentityRoleInitializer.InitializeAsync(app.Services);
            
            // Step1.4: Verwendete Sprachen definieren
            var supportedCultures = new[] {
                new CultureInfo("de-DE"),
                new CultureInfo("de"),
                new CultureInfo("en-US"),
                new CultureInfo("en")
            };

            // Step1.5: Verwendete Sprachen konfigurieren und Default-Sprache festlegen
            var localizationOptions = new RequestLocalizationOptions {
                DefaultRequestCulture = new RequestCulture("de"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            // Step1.5.1: Bei Problemen alte Einstellungen löschen
            //localizationOptions.RequestCultureProviders.Clear();

            // Step1.6: Konfigurierte Sprachen aktivieren
            app.UseRequestLocalization(localizationOptions);

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}