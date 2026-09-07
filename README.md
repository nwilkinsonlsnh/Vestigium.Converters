# Vestigium.Converters

[![build](https://github.com/nwilkinsonlsnh/Vestigium.Converters/actions/workflows/build.yml/badge.svg)](https://github.com/nwilkinsonlsnh/Vestigium.Converters/actions/workflows/build.yml)

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

Optional: register `IConfiguration` and/or `IThemeBrushes` **before** `AddVestigiumConverters` if latency / packet-loss converters should read suite preferences or theme tokens.

## Catalog (42)

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
| CONV-21 | `EqualityToBooleanConverter` | Boolean |
| CONV-22 | `EqualityToVisibilityConverter` | Visibility |
| CONV-23 | `IsLessThanToBooleanConverter` | Numeric |
| CONV-24 | `IsBetweenToBooleanConverter` | Numeric |
| CONV-25 | `BooleanToBrushConverter` | Boolean |
| CONV-26 | `BooleanToOpacityConverter` | Boolean |
| CONV-27 | `HttpStatusToSeverityBrushConverter` | Domain |
| CONV-28 | `PercentToStringConverter` | Formatting |
| CONV-29 | `MillisecondsToStringConverter` | Formatting |
| CONV-30 | `CollectionHasItemsToBooleanConverter` | Collections |
| CONV-31 | `BooleanAndToVisibilityConverter` | MultiBinding |
| CONV-32 | `BooleanOrToVisibilityConverter` | MultiBinding |
| CONV-33 | `DnsResponseCodeToDescriptionConverter` | Domain |
| CONV-34 | `TraceHopStatusToBrushConverter` | Domain |
| CONV-35 | `PortStateToBrushConverter` | Domain |
| CONV-36 | `TruncateStringConverter` | Formatting |
| CONV-37 | `UtcTimestampToStringConverter` | Formatting |
| CONV-38 | `PacketLossToSeverityBrushConverter` | Domain |
| CONV-39 | `ProgressToPercentConverter` | Numeric |
| CONV-40 | `StringEqualsToVisibilityConverter` | Visibility |
| CONV-41 | `InvertVisibilityConverter` | Visibility |
| CONV-42 | `EnumMatchToVisibilityConverter` | Visibility |

## Projects

| Project | Role |
|---|---|---|
| `Vestigium.Converters` | Infrastructure + catalog |
| `Vestigium.Converters.Tests` | Mapping, fail-safe, DI lifetime tests |
| `Vestigium.Converters.Demo` | Host sample using `{v:Resolve}` |

## Contracts that do not move

- Convert never throws on the dispatcher. Failures return `DependencyProperty.UnsetValue`.
- Design-time XAML preview instantiates converters without a host `IServiceProvider`.
- Domain converters accept **enum or string** and do not reference PingIQ / HttpIQ assemblies.
- Frozen cached brushes on severity converters.
- Optional services (`IThemeBrushes`, `IConfiguration`) are resolved lazily; a miss does not cache as failure.
