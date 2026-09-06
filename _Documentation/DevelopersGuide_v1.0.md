# Vestigium.Converters — Developer’s Guide v1.0

Document ID: VEST-CONV-SRS-001 implementation notes.  
Platform: .NET 10 LTS, WPF, CommunityToolkit.Mvvm-compatible, `Microsoft.Extensions.DependencyInjection`.

## 1. Host integration

```csharp
services.AddVestigiumConverters();
VestigiumConverterHost.ServiceProvider = services.BuildServiceProvider();
```

XAML namespace:

```xml
xmlns:v="http://schemas.vestigium.dev/converters"
```

Canonical binding:

```xml
{Binding Path=IsBusy, Converter={v:Resolve ConverterType={x:Type v:InverseBooleanToVisibilityConverter}}}
```

Positional type constructor is also valid:

```xml
{Binding Converter={v:Resolve {x:Type v:BooleanToVisibilityConverter}}}
```

`ConverterParameter`, `ConverterCulture`, `Mode`, and `StringFormat` stay on the `Binding`.

## 2. Infrastructure

| Type | Role |
|---|---|
| `IConverterProvider` | Singleton locator |
| `DiConverterExtension` / `ResolveExtension` | Markup extension |
| `BaseDiConverter` | Sealed Convert wrappers, UnsetValue on throw |
| `BaseDiMultiConverter` | IMultiValueConverter sibling (no v1 catalog items) |
| `FallbackConverter` | Always UnsetValue |

`BaseDiConverter.GetService<T>()` resolves optional host services lazily and caches them on the singleton instance. Catalog converters stay parameterless so the designer can `Activator.CreateInstance` them.

## 3. Converter notes

### Visibility family (CONV-01…07)

- Nullable bool is treated as `false`.
- `ConverterParameter` `Inverse` / `true` / `invert` flips CONV-05, CONV-06, CONV-07, CONV-15.
- CONV-03 uses `Hidden` so ProbeHost scan grids do not reflow.

### Formatting (CONV-08…11)

- CONV-08 reads `[Description]` then `[Display(Name)]`.
- CONV-09 is 1024-based; optional integer parameter is decimal places (default 2).
- CONV-10 compact units are English `d/h/m/s` (OPEN-04). `ConverterParameter=Long` uses English words.
- CONV-11 uses the binding culture; `null` becomes `""`.

### Numeric (CONV-12…14)

- Parameters parse invariant first, then the binding culture.
- CONV-14 clamps 0–100 and emits `GridLength` star. `Remainder` emits `(100 − value)*`.

### Domain (CONV-17…20)

Ping logical names (enum `ToString()` or string, case-insensitive, separators stripped):

| Status | Brush intent | Fallback |
|---|---|---|
| Success | Success | `#22C55E` |
| Timeout | Warning | `#EAB308` |
| DestinationUnreachable | Danger | `#EF4444` |
| TtlExpired | AccentWarning | `#F97316` |
| other | Neutral | `#94A3B8` |

Latency bands (CONV-18):

| Band | Default | Brush |
|---|---|---|
| Good | `< 50 ms` | Success |
| Warn | `50–150 inclusive` | Warning |
| Critical | `> 150 ms` | Danger |

Override order: `ConverterParameter` `"50,150"` wins over `IConfiguration` keys `Vestigium:Latency:GoodMilliseconds` and `Vestigium:Latency:WarnMilliseconds`.

Optional `IThemeBrushes.TryGet(semanticKey)` replaces fallbacks when registered (OPEN-02).

CONV-20 default mode is Segoe Fluent glyph. `ConverterParameter=Path` returns `""` in v1 (OPEN-03).

## 4. Adding a converter later

1. Inherit `BaseDiConverter`.
2. Register it in `ServiceCollectionExtensions.AddVestigiumConverters`.
3. Add an `XmlnsDefinition` if the namespace is new.
4. Add mapping + fail-safe tests.
5. Update this guide and the README catalog table.

Unlisted converters are a versioned catalog change, not a silent extra class.

## 5. Tests

Run on Windows:

```text
dotnet test src/Vestigium.Converters.Tests
```

Coverage includes mapping tables, `ConvertBack` policy, UnsetValue fail-safe, singleton registration, and CONV-18 configuration override.
