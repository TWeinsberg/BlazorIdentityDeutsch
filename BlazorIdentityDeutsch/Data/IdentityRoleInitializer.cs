using Microsoft.AspNetCore.Identity;

namespace BlazorIdentityDeutsch.Data {
    // Step2.1: Rollenmanagement konfigurieren
    public static class IdentityRoleInitializer {
        public static async Task InitializeAsync(
            IServiceProvider services) {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var configuration = scope.ServiceProvider
                .GetRequiredService<IConfiguration>();

            // 1. Rollen anlegen
            foreach (var role in ApplicationRoles.All) {
                if (await roleManager.RoleExistsAsync(role))
                    continue;

                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        string.Join("; ",
                            result.Errors.Select(e => e.Description)));
            }

            // 2. Bereits ein Administrator vorhanden?
            if (await userManager.GetUsersInRoleAsync(
                ApplicationRoles.Admin) is { Count: > 0 }) {
                return;
            }

            /* 3. Bootstrap-Administrator nur bei Konfiguration
             * Temporären Benutzer mit Admin-Rechten einrichten:
             * Ins Projekt-Verzeichnis wechseln, (Datei *.csproj muss vorhanden sein)
             * 
             * dotnet user-secrets set "BootstrapAdmin:Email" "admin@example.com"
             * dotnet user-secrets set "BootstrapAdmin:Password" "EinSicheresPasswort123!"
             * 
             * Und wieder entfernen:
             * dotnet user-secrets remove "BootstrapAdmin:Email"
             * dotnet user-secrets remove "BootstrapAdmin:Password"
            */
            var email = configuration["BootstrapAdmin:Email"];
            var password = configuration["BootstrapAdmin:Password"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password)) {
                return;
            }

            var admin = await userManager.FindByEmailAsync(email);

            if (admin is null) {
                admin = new ApplicationUser {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(
                    admin, password);

                if (!createResult.Succeeded)
                    throw new InvalidOperationException(
                        string.Join("; ",
                            createResult.Errors.Select(e => e.Description)));
            }

            var roleResult = await userManager.AddToRoleAsync(
                admin, ApplicationRoles.Admin);

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    string.Join("; ",
                        roleResult.Errors.Select(e => e.Description)));
        }
    }
}
