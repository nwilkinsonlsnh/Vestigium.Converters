# Vestigium.Converters — Developer’s Guide v1.0

Document ID: VEST-CONV-SRS-001 implementation notes.  
Platform: .NET 10 LTS, WPF, CommunityToolkit.Mvvm-compatible, `Microsoft.Extensions.DependencyInjection`.  
Catalog: CONV-01…CONV-42 (v1 locked set plus the v1.1 common / useful additions).

## 1. Host integration

```csharp
services.AddVestigiumConverters();
VestigiumConverterHost.ServiceProvider = services.BuildServiceProvider();
```

Do this during `Application.OnStartup` (or the generic host build) **before** the first window. Optional `IConfiguration` and `IThemeBrushes` must be registered **before** `AddVestigiumConverters` if domain brushes should read suite preferences or theme tokens.

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

`ConverterParameter`, `ConverterCulture`, `Mode`, and `StringFormat` stay on the `Binding`. Suite views must not declare a private catalog of static converter resources.

## 2. Infrastructure

| Type | Role |
|---|---|
| `IConverterProvider` | Singleton locator |
| `DiConverterExtension` / `ResolveExtension` | Markup extension |
| `BaseDiConverter` | Sealed Convert wrappers, UnsetValue on throw |
| `BaseDiMultiConverter` | `IMultiValueConverter` sibling (CONV-31, CONV-32) |
| `FallbackConverter` | Always UnsetValue |
| `BrushCache` | Frozen `SolidColorBrush` by hex |
| `ConversionHelpers` | Flag parse, equality, numeric coerce |

`BaseDiConverter.GetService<T>()` resolves optional host services lazily. Hits are cached on the singleton instance; misses are not, so assigning `VestigiumConverterHost.ServiceProvider` after the first Convert still works. Catalog converters stay parameterless so the designer can `Activator.CreateInstance` them.

`IConverterProvider` only Activator-constructs types that implement `IValueConverter` or `IMultiValueConverter`. Anything else returns `FallbackConverter`.

`BaseDiConverter.ResolveSemanticBrush(key)` asks `IThemeBrushes.TryGet` then falls back to:

| Semantic | Fallback |
|---|---|
| Success | `#22C55E` |
| Warning | `#EAB308` |
| Danger | `#EF4444` |
| AccentWarning | `#F97316` |
| Neutral | `#94A3B8` |

### ConverterParameter flags

Tokens are split on `|`. Known flags are stripped from the remainder:

| Flag | Meaning |
|---|---|
| `Inverse` / `Invert` / `true` / leading `!` | Flip the boolean or visibility result |
| `Ordinal` | Case-sensitive string compare |
| `Fraction` / `Ratio` | Treat 0–1 as a ratio (percent / packet loss) |
| `Long` | Long English words (CONV-10) |
| `Remainder` | `100 − value` (CONV-14) |
| `Path` | Path mode for CONV-20 (empty in v1) |

Numeric pairs use comma: `50,150` or `Fraction|1,5`. Parsing is invariant first, then the binding culture. Non-numeric flag tokens are ignored by `TryParsePair`.

## 3. Catalog

### Visibility / boolean (CONV-01…07, 21–22, 25–26, 31–32, 40–42)

- Nullable bool is treated as `false` unless the converter requires a real bool and then returns `UnsetValue`.
- CONV-03 uses `Hidden` so ProbeHost scan grids do not reflow.
- CONV-21 / CONV-22 compare value to `ConverterParameter` (`AreEqual`: numeric epsilon, else ignore-case string).
- CONV-40 compares strings. Default ignore-case; `Ordinal|MX` is case-sensitive.
- CONV-41 maps `Visible` ↔ `Collapsed`. `Hidden` becomes `Visible`. Also accepts a bool.
- CONV-42 matches any comma-separated name after `NormalizeKey` (`A,AAAA,MX` or `Inverse|Timeout,TimedOut`).
- CONV-25 parameter is `TrueSemantic,FalseSemantic` (default `Success,Neutral`).
- CONV-26 parameter is `on,off` opacity (default `1,0.35`).
- CONV-31 / CONV-32 are `IMultiValueConverter` AND / OR over `MultiBinding`.

```xml
<StackPanel Visibility="{Binding Status,
    Converter={v:Resolve {x:Type v:EnumMatchToVisibilityConverter}},
    ConverterParameter=Ready,Complete}">
```

```xml
<Border.Visibility>
  <MultiBinding Converter="{v:Resolve {x:Type v:BooleanAndToVisibilityConverter}}">
    <Binding Path="HasSelection"/>
    <Binding Path="IsConnected"/>
  </MultiBinding>
</Border.Visibility>
```

### Formatting (CONV-08…11, 28–29, 36–37)

- CONV-08 reads `[Description]` then `[Display(Name)]`.
- CONV-09 is 1024-based; optional integer parameter is decimal places (default 2).
- CONV-10 compact units are English `d/h/m/s` (OPEN-04). `ConverterParameter=Long` uses English words.
- CONV-11 uses the binding culture; `null` becomes `""`.
- CONV-28 default treats 0–100. `Fraction|1` prints `0.042` as `4.2%`.
- CONV-29 accepts milliseconds or `TimeSpan`. Optional integer parameter is decimals.
- CONV-36 truncates with `…`. Parameter is max length (default 24).
- CONV-37 formats `DateTime` / `DateTimeOffset` / parseable string as UTC. Unspecified `DateTime` is treated as UTC. Default format `yyyy-MM-dd HH:mm:ss`; any other parameter is a .NET format string.

### Numeric (CONV-12…14, 23–24, 39)

- Parameters parse invariant first, then the binding culture.
- CONV-14 clamps 0–100 and emits `GridLength` star. `Remainder` emits `(100 − value)*`.
- CONV-23 is exclusive less-than. CONV-24 is inclusive `lo,hi` (order-insensitive) with optional Inverse.
- CONV-39 maps 0–1 progress to 0–100. Values already `> 1` are treated as percent and clamped. `ConvertBack` returns `0–1` except when the target type is `int`.

### Collections (CONV-15…16, 30)

- Strings are **not** enumerated as character collections for emptiness / count (badge treats a numeric string as a count).
- CONV-30 is the boolean sibling of CONV-15.

### Domain (CONV-17…20, 27, 33–35, 38)

Domain converters accept **enum or string or number** and must not reference PingIQ / DnsIQ / TraceIQ / HttpIQ / ProbeHost assemblies.

Ping logical names (CONV-17) after `NormalizeKey`:

| Status | Brush intent | Fallback |
|---|---|---|
| Success / Ok / Reply | Success | `#22C55E` |
| Timeout / TimedOut | Warning | `#EAB308` |
| DestinationUnreachable / Unreachable | Danger | `#EF4444` |
| TtlExpired / TimeExceeded | AccentWarning | `#F97316` |
| other | Neutral | `#94A3B8` |

Latency bands (CONV-18):

| Band | Default | Brush |
|---|---|---|
| Good | `< 50 ms` | Success |
| Warn | `50–150 inclusive` | Warning |
| Critical | `> 150 ms` | Danger |

Override order: `ConverterParameter` `"50,150"` wins over `IConfiguration` keys `Vestigium:Latency:GoodMilliseconds` and `Vestigium:Latency:WarnMilliseconds`.

Packet loss bands (CONV-38):

| Band | Default | Brush |
|---|---|---|
| Good | `< 1%` | Success |
| Warn | `1–5 inclusive` | Warning |
| Critical | `> 5%` | Danger |

`ConverterParameter` `"1,5"` or `"Fraction|1,5"` wins over `Vestigium:PacketLoss:GoodPercent` / `WarnPercent`. `Fraction` multiplies values `≤ 1` by 100.

HTTP status class (CONV-27): 1xx Neutral, 2xx Success, 3xx AccentWarning, 4xx Warning, 5xx Danger.

HTTP phrase (CONV-19): `"200 OK"`; unknown codes `"599 Unknown"`.

DNS RCODE (CONV-33): `0` / `NOERROR` → `No Error`, `3` / `NXDOMAIN` → `Non-Existent Domain`, plus FORMERR, SERVFAIL, NOTIMP, REFUSED, YXDOMAIN, NOTAUTH, timeout.

Trace hop (CONV-34): Replied/Success → Success; Timeout/`*` → Warning; Filtered → AccentWarning; Unreachable → Danger.

Port state (CONV-35): Open → Success; OpenFiltered → AccentWarning; Filtered/Stealth → Warning; Closed/Reset → Danger.

CONV-20 default mode is Segoe Fluent glyph. `ConverterParameter=Path` returns `""` in v1 (OPEN-03).

Optional `IThemeBrushes.TryGet(semanticKey)` replaces fallbacks when registered (OPEN-02).

## 4. Adding a converter later

1. Inherit `BaseDiConverter` (or `BaseDiMultiConverter`).
2. Register it in `ServiceCollectionExtensions.AddVestigiumConverters`.
3. Add an `XmlnsDefinition` if the namespace is new (already covered for Boolean / Formatting / Numeric / Collections / Domain).
4. Add mapping + fail-safe tests.
5. Update this guide and the README catalog table.

Unlisted converters are a versioned catalog change, not a silent extra class. Convert never throws on the dispatcher.

## 5. Tests

Run on Windows:

```text
dotnet test src/Vestigium.Converters.Tests
```

Coverage includes mapping tables, `ConvertBack` policy, UnsetValue fail-safe, singleton registration, CONV-18 / CONV-38 configuration and parameter overrides, collection/badge edge cases, and the v1.1 catalog (`CatalogV11Tests`).
