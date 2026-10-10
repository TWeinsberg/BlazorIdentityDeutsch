# Blazor Identity Deutsch

Erweiterung der **ASP.NET Core Blazor Web App Standardvorlage für .NET 10** (Visual Studio 2026, **Individual Accounts**) um weitgehend deutsch übersetzte Identity-Seiten und die optionale Vorbereitung der Rollenverwaltung.

Dies ist **keine eigenständige Blazor-Anwendung** und keine vollständige grafische Rollenadministration. Die Dateien werden in ein neu angelegtes Projekt übernommen. Die Übersetzungen sind anpassbar; dynamische Identity-Fehlermeldungen und DataAnnotations können weiterhin zusätzliche Lokalisierung benötigen.

## Voraussetzungen

- .NET 10 SDK und eine Blazor Web App mit **Individual Accounts**
- Die mit der Standardvorlage erzeugten Identity-Komponenten
- EF Core/Identity-Datenbankkonfiguration des Zielprojekts

## Installation

1. **`Components/Account` ersetzen:** Das vorhandene Verzeichnis im neuen Projekt durch `Components/Account` aus diesem Repository ersetzen. Vorher Änderungen sichern.
2. **`Resources` hinzufügen:** Den Ordner `Resources` ins Projektstammverzeichnis kopieren.
3. **Namespace anpassen:** Die enthaltenen Dateien verwenden den Beispiel-Namespace `BlazorApp1`. Diesen an den Namespace des Zielprojekts anpassen (insbesondere `IdentityTexts.cs` und `@using`-Anweisungen). `IdentityTexts.cs` liegt unter `Components/Account`, deklariert aber den Root-Namespace `BlazorApp1`; das ist für die Ressourcenauflösung relevant.
4. **`Program.cs` ergänzen:** Siehe `Program.Beispiel.cs.txt` als Referenz. **Nicht** die gesamte `Program.cs` blind überschreiben: Datenbank, Namespaces und sonstige Dienste müssen zur Zielanwendung passen.

### Lokalisierung registrieren

In `Program.cs` vor `builder.Build()`:

```csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
```

Nach `builder.Build()`, vor dem Mapping der Komponenten:

```csharp
using Microsoft.AspNetCore.Localization;

var cultures = new[] { "de-DE", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("de-DE")
    .AddSupportedCultures(cultures)
    .AddSupportedUICultures(cultures);

// Optional: Browser-/Cookie-Sprache ignorieren und Deutsch fest vorgeben.
localizationOptions.RequestCultureProviders.Clear();
app.UseRequestLocalization(localizationOptions);
```

Die RESX-Dateien enthalten englische Ressourcenschlüssel und deutsche Übersetzungen. Die automatische Auswahl nach Browser- oder Cookie-Sprache wird durch `Clear()` abgeschaltet. Für einen späteren Sprachwechsel diesen Teil entsprechend anpassen.

### Optional: Rollenmanagement

In der **bestehenden** Identity-Registrierung `.AddRoles<IdentityRole>()` ergänzen:

```csharp
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
```

Dazu `using Microsoft.AspNetCore.Identity;` ergänzen. Der `ApplicationDbContext` muss Rollen unterstützen, beispielsweise durch `IdentityDbContext<ApplicationUser>` statt `IdentityUserContext<ApplicationUser>`. Je nach bestehendem Datenbankschema Migration erstellen und anwenden:

```bash
dotnet ef migrations add AddIdentityRoles
dotnet ef database update
```

Die Migration ist nur dann nötig, wenn die benötigten Rollentabellen noch fehlen. Die Rolle `Administrator` wird **nicht automatisch** erstellt oder einem Benutzer zugewiesen.

Eine geschützte Razor-Seite kann so aussehen:

```razor
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize(Roles = "Administrator")]
```

## Inhalt

- `Components/Account/` – angepasste Identity-Razor-Komponenten und Hilfsdateien
- `Resources/IdentityTexts.resx` – englische Basisressourcen
- `Resources/IdentityTexts.de.resx` – deutsche Übersetzungen
- `Program.Beispiel.cs.txt` – Beispiel der projektspezifischen Registrierung, **nicht direkt kompilieren oder kopieren**

## Hinweise

- Nach dem Ersetzen der Komponenten auf Namensräume, neue Identity-API-Änderungen und den verwendeten Rendermodus achten.
- Rollenverwaltung bedeutet hier zunächst Identity-Unterstützung, nicht eine fertige Verwaltungsoberfläche.
- Bei `IStringLocalizer` ist die Groß-/Kleinschreibung der Schlüssel relevant.
- Die Vorlage wurde als Dateisammlung vorbereitet; ein vollständiger Build gegen ein frisch erzeugtes .NET-10-Projekt ist hier nicht durchgeführt worden.

## Lizenz

Siehe [LICENSE](LICENSE). Die Vorlage enthält Anpassungen an durch Microsoft generierten Identity-Komponenten; gegebenenfalls geltende Hinweise und Lizenzen der ursprünglichen Microsoft-Bestandteile bleiben unberührt.
