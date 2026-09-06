# Vestigium.Converters

DI-driven WPF `IValueConverter` catalog for the Vestigium suite (PingIQ, DnsIQ, TraceIQ, HttpIQ, ProbeHost).

**Target:** .NET 10 LTS / WPF / Visual Studio 2026  
**Architecture:** MVVM + `Microsoft.Extensions.DependencyInjection`  
**Startup project:** `Vestigium.Converters.Demo`

Verbose reference: [`_Documentation/DevelopersGuide_v1.0.md`](_Documentation/DevelopersGuide_v1.0.md)

## Why this library

Suite views do **not** declare twenty static converter resources. Hosts register the catalog once, assign `VestigiumConverterHost.ServiceProvider`, and resolve converters from XAML:

```xml
xmlns:v="http://schemas.vestigium.dev/converters"

<Ellipse Fill="{Binding PingStatus,
                Converter={v:Resolve {x:Type v:PingStatusToColorBrushConverter}}}"/>
```

State-free converters are **Singleton** so continuous ping UI does not allocate a converter per binding.

## Open in Visual Studio

1. Clone this repository.
2. Open `Vestigium.Converters.slnx` in Visual Studio 2026.
3. Restore NuGet, set **Vestigium.Converters.Demo** as the startup project.
4. Run on Windows.

## Host in two calls

```csharp
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Converters;
using Vestigium.Converters.DependencyInjection;

var services = new ServiceCollection();
services.AddVestigiumConverters();
VestigiumConverterHost.ServiceProvider = services.BuildServiceProvider();
```

Do this during `Application.OnStartup` (or your generic host build) before the first window.

Optional: register `IConfiguration` and/or `IThemeBrushes` **before** `AddVestigiumConverters` if CONV-18 / CONV-17 should read suite preferences or theme tokens.

## Catalog (20)

| ID | Converter | Group |
|---|---|---|
| CONV-01 | `BooleanToVisibilityConverter` | Visibility |
| CONV-02 | `InverseBooleanToVisibilityConverter` | Visibility |
| CONV-03 | `BooleanToVisibilityHiddenConverter` | Visibility |
| CONV-04 | `InverseBooleanConverter` | Boolean |
| CONV-05 | `NullToVisibilityConverter` | Visibility |
| CONV-06 | `NullToBooleanConverter` | Boolean |
| CONV-07 | `EmptyStringToVisibilityConverter` | Visibility |
| CONV-08 | `EnumToDescriptionConverter` | Formatting |
| CONV-09 | `BytesToReadableSizeConverter` | Formatting |
| CONV-10 | `TimeSpanToReadableConverter` | Formatting |
| CONV-11 | `StringToUpperConverter` | Formatting |
| CONV-12 | `IsGreaterThanToBooleanConverter` | Numeric |
| CONV-13 | `MathMultiplierConverter` | Numeric |
| CONV-14 | `PercentageToGridLengthConverter` | Numeric |
| CONV-15 | `CollectionEmptyToVisibilityConverter` | Collections |
| CONV-16 | `CountToBadgeStringConverter` | Collections |
| CONV-17 | `PingStatusToColorBrushConverter` | Domain |
| CONV-18 | `LatencyToSeverityColorConverter` | Domain |
| CONV-19 | `HttpStatusCodeToDescriptionConverter` | Domain |
| CONV-20 | `DnsRecordTypeToIconConverter` | Domain |

## Projects

| Project | Role |
|---|---|
| `Vestigium.Converters` | Infrastructure + catalog |
| `Vestigium.Converters.Tests` | Mapping, fail-safe, DI lifetime tests |
| `Vestigium.Converters.Demo` | Host sample using `{v:Resolve}` |

## Contracts that do not move

- Convert never throws on the dispatcher. Failures return `DependencyProperty.UnsetValue`.
- Design-time XAML preview instantiates converters without a host `IServiceProvider`.
- Domain converters accept **enum or string** and do not reference PingIQ / HttpIQ assemblies.
- Frozen cached brushes on CONV-17 / CONV-18.
