# Vestigium.Converters — Requirements Specification

| | |
|---|---|
| **Document ID** | VEST-CONV-SRS-001 |
| **Product** | Vestigium.Converters |
| **Version** | 1.1 (finalized) |
| **Status** | Final — source of truth for the shipped catalog |
| **Date** | 7 September 2026 |
| **Owner** | Wilkinson Business / Vestigium engineering |
| **Platform** | Microsoft Visual Studio · C# · WPF · .NET 10 LTS |
| **Architecture** | MVVM (CommunityToolkit.Mvvm-compatible) · `Microsoft.Extensions.DependencyInjection` |
| **Audience** | Library authors, suite app authors (PingIQ, DnsIQ, TraceIQ, HttpIQ, ProbeHost), QA |
| **Companion** | [`DevelopersGuide_v1.0.md`](DevelopersGuide_v1.0.md) (implementation notes) |

This document is the single source of truth for Vestigium.Converters. It restates the locked v1 catalog (CONV-01…CONV-20), the v1.1 common / useful additions (CONV-21…CONV-42), and the contracts, acceptance criteria, and host-integration rules that the library implements.

The original draft SRS (6 September 2026) described twenty converters. This finalized edition records what shipped.

---

## 1. Purpose and product overview

### 1.1 Purpose

Vestigium.Converters is a dedicated WPF class library that supplies a centralized, dependency-injection-driven catalog of `IValueConverter` and `IMultiValueConverter` implementations for the Vestigium network diagnostic suite: PingIQ, DnsIQ, TraceIQ, HttpIQ, and ProbeHost.

Suite views must not instantiate converters as static XAML resources in every view or in `App.xaml`. The library exposes one DI-resolved provider and a matching markup extension so views request converters by type. That reduces XAML boilerplate, keeps converter instances singleton-scoped for high-frequency bindings (continuous ping UI), and lets converters consume optional host services such as configuration and theme brushes.

### 1.2 Goals

- One library, one registration path, one XAML resolution syntax for every catalog converter.
- Zero unhandled exceptions on the WPF dispatcher from converter code.
- Designer-safe resolution so Visual Studio XAML preview does not fail when the host `IServiceProvider` is absent.
- State-free converters registered as Singleton to avoid per-binding allocations during rapid UI updates.
- Domain converters may read user preferences through injected services; generic converters must not require host services to function.
- Domain converters accept enum **or** string **or** number and must not reference PingIQ / DnsIQ / TraceIQ / HttpIQ / ProbeHost assemblies.

### 1.3 Non-goals (v1 / v1.1)

- A visual converter designer or runtime converter marketplace.
- Replacing WPF styles, `DataTrigger`s, or `IValueConverter` usage outside Vestigium apps.
- Implementing network diagnostics themselves (ping, DNS, traceroute, HTTP). Those remain in the respective IQ products.
- Shipping a custom control library. This assembly is converters plus resolution infrastructure only.
- A public NuGet branding / marketing site in this version.

### 1.4 In scope / out of scope

| In scope | Out of scope |
|---|---|
| 42 named converters in §6 plus the infrastructure types in §5 | Unlisted converters, unless added by change control |
| `IConverterProvider`, `{v:Resolve}` / `{v:DiConverter}`, `BaseDiConverter`, `BaseDiMultiConverter` | App-level ViewModels, views, or navigation in suite products |
| Host DI extension method and design-time fallbacks | Localization resource authoring beyond culture-aware formatting |
| Fail-safe `Convert` / `ConvertBack` contracts | Theme token authoring (hosts may register `IThemeBrushes`) |
| In-repo tabbed gallery demo using `{v:Resolve}` only | Product chrome that references suite assemblies |

---

## 2. References and definitions

### 2.1 Platform constraints

| ID | Requirement |
|---|---|
| **ARCH-01** | Target framework: `net10.0-windows` (.NET 10 LTS). |
| **ARCH-02** | UI framework: WPF (`UseWPF = true`). SDK-style class library. |
| **ARCH-03** | Language: C# with nullable reference types enabled. |
| **ARCH-04** | MVVM integration: CommunityToolkit.Mvvm compatible. Converters must not take a dependency on a specific ViewModel base beyond normal binding. |
| **ARCH-05** | DI container: `Microsoft.Extensions.DependencyInjection`. No other container API in the public surface. |
| **ARCH-06** | IDE: Microsoft Visual Studio. XAML designer compatibility is mandatory (see §7). |
| **ARCH-07** | Assembly / root namespace: `Vestigium.Converters`. |
| **ARCH-08** | XML namespace: `http://schemas.vestigium.dev/converters` with `XmlnsPrefix` `v` and `XmlnsDefinition` for Boolean, Formatting, Numeric, Collections, Domain, and Xaml types. |

### 2.2 Terms

| Term | Meaning |
|---|---|
| Converter hub / provider | The single `IConverterProvider` registered in the host `IServiceCollection` that locates converter instances. |
| Resolve extension | `DiConverterExtension` / `ResolveExtension` — the markup extension used in XAML bindings (`{v:Resolve}`). |
| State-free converter | A converter whose result depends only on `value`, `targetType`, `parameter`, `culture`, and optional injected services that are themselves thread-safe singletons. |
| Host application | PingIQ, DnsIQ, TraceIQ, HttpIQ, ProbeHost, or a future suite app that references this library. |
| UnsetValue | `DependencyProperty.UnsetValue` — the required fail-safe return when conversion cannot proceed. |
| Suite | The Vestigium network diagnostic products listed above. |
| Semantic brush | A named theme token (`Success`, `Warning`, `Danger`, `AccentWarning`, `Neutral`) resolved via `IThemeBrushes` or a frozen hex fallback. |

---

## 3. Architectural requirements

### 3.1 Centralized injection model

- **ARCH-10** The library shall implement a single provider that resolves requested converters from the host application's `IServiceProvider`.
- **ARCH-11** Views shall request converters by CLR type via `{v:Resolve}`. Views shall not declare per-converter `<local:FooConverter/>` resources for library converters.
- **ARCH-12** `App.xaml` may register the XML namespace. It shall not list the catalog as static resources.
- **ARCH-13** A single extension method (`AddVestigiumConverters`) shall register the provider and every catalog converter.

### 3.2 Service lifetimes

- **ARCH-20** All state-free converters shall be registered as Singleton. Mandatory for high-frequency UI such as continuous pings in PingIQ.
- **ARCH-21** `IConverterProvider` shall be Singleton.
- **ARCH-22** If a future converter must hold per-view mutable state, it shall be documented as Transient. No such converter exists in the v1 / v1.1 catalog.
- **ARCH-23** Brushes returned by domain / boolean brush converters shall be frozen (or retrieved from a theme cache) so Singleton converters do not publish unfrozen `Freezable`s across threads.

### 3.3 Layering

The library has two layers:

1. **Infrastructure** — `IConverterProvider`, `DiConverterExtension` / `ResolveExtension`, `BaseDiConverter`, `BaseDiMultiConverter`, `FallbackConverter`, `BrushCache`, `ConversionHelpers`, host registration extensions, design-time construction.
2. **Catalog** — CONV-01…CONV-42, each inheriting `BaseDiConverter` or `BaseDiMultiConverter` and remaining independently testable.

Catalog converters may depend on abstractions (`IConfiguration`, `IThemeBrushes`) but must not depend on a concrete suite application assembly.

### 3.4 Required project layout

| Path | Contents |
|---|---|
| `src/Vestigium.Converters/` | `net10.0-windows` class library, UseWPF, nullable enable |
| `DependencyInjection/` | `AddVestigiumConverters()` and provider registration |
| `Xaml/` | Markup extensions |
| `Infrastructure/` | Base types, helpers, brush cache, host accessor |
| `Converters/Boolean/` | Visibility and boolean catalog types |
| `Converters/Formatting/` | String / duration / percent / timestamp formatters |
| `Converters/Numeric/` | Comparisons, multiplier, grid length, progress |
| `Converters/Collections/` | Empty / count / has-items |
| `Converters/Domain/` | Ping, latency, HTTP, DNS, trace, port, packet loss |
| `src/Vestigium.Converters.Tests/` | Mapping, fail-safe, DI lifetime, configuration tests |
| `src/Vestigium.Converters.Demo/` | Tabbed `{v:Resolve}` gallery (not a product) |

---

## 4. Host integration contract

### 4.1 Registration

- **INT-01** Public extension method: `IServiceCollection.AddVestigiumConverters()`.
- **INT-02** That method registers `IConverterProvider`, `FallbackConverter`, and every catalog converter as Singleton (`TryAddSingleton`).
- **INT-03** Hosts call `AddVestigiumConverters()` next to other suite service registrations.
- **INT-04** At WPF startup the host assigns the built `IServiceProvider` to `VestigiumConverterHost.ServiceProvider` **before** the first window.
- **INT-05** If the accessor is null at runtime (not design-time), `ProvideValue` shall not throw. It returns a no-op converter that always yields `UnsetValue`.
- **INT-06** Optional `IConfiguration` and/or `IThemeBrushes` must be registered **before** `AddVestigiumConverters` if latency / packet-loss converters should read suite preferences or theme tokens.
- **INT-07** `BaseDiConverter.GetService<T>()` resolves optional services lazily. Hits are cached on the singleton instance; **misses are not cached**, so a late-assigned `ServiceProvider` still works.

Canonical host hookup:

```csharp
var services = new ServiceCollection();
services.AddVestigiumConverters();
VestigiumConverterHost.ServiceProvider = services.BuildServiceProvider();
```

### 4.2 XAML usage

```xml
xmlns:v="http://schemas.vestigium.dev/converters"

<Ellipse Fill="{Binding PingStatus,
                Converter={v:Resolve {x:Type v:PingStatusToColorBrushConverter}}}"/>
```

Named property form is also valid:

```xml
{Binding Path=IsBusy,
         Converter={v:Resolve ConverterType={x:Type v:InverseBooleanToVisibilityConverter}}}
```

- **INT-10** The extension shall accept a `ConverterType` (`Type`) identifying the catalog class.
- **INT-11** A positional Type constructor is required in addition to the named property.
- **INT-12** The extension resolves the instance from `IConverterProvider` and returns that instance as the Binding's `Converter`. It does not wrap the binding itself.
- **INT-13** `ConverterParameter`, `ConverterCulture`, `Mode`, and `StringFormat` remain standard Binding properties and are passed through unchanged.
- **INT-14** Comma-containing `ConverterParameter` values must be quoted inside a markup extension (`ConverterParameter='50,150'`). An unquoted comma is parsed as another markup property.

### 4.3 CommunityToolkit.Mvvm

- **INT-20** Converters shall operate against ordinary binding sources including `ObservableObject` properties and `ObservableCollection<T>`. No converter may require `[ObservableProperty]` generated members by name.
- **INT-21** The library assembly does not require a CommunityToolkit.Mvvm package reference. The demo may use it.

### 4.4 ConverterParameter flags

Tokens are split on `|`. Known flags are stripped from the remainder before numeric / string parsing.

| Flag | Meaning |
|---|---|
| `Inverse` / `Invert` / `true` / leading `!` | Flip the boolean or visibility result |
| `Ordinal` | Case-sensitive string compare (CONV-40) |
| `Fraction` / `Ratio` | Treat 0–1 as a ratio (CONV-28, CONV-38) |
| `Name` | Omit the numeric prefix (CONV-33) |
| `Long` | English words instead of compact units (CONV-10) |
| `Remainder` | Emit `(100 − value)*` (CONV-14) |
| `Path` | Path mode for CONV-20 (empty string in v1.1) |

Numeric pairs use a comma: `50,150` or `Fraction|1,5`. Parsing is invariant first, then the binding culture. Non-numeric flag tokens are ignored by pair parsing.

---

## 5. Core infrastructure components

### 5.1 IConverterProvider

- **INF-01** Single interface registered in the DI container that acts as factory / locator for all converters.
- **INF-02** Exposes `GetConverter(Type)` returning `IValueConverter` or `IMultiValueConverter` as `object`, and a generic `GetConverter<T>()`.
- **INF-03** Unknown types shall not throw on the UI path. The provider returns `FallbackConverter`, which always yields `UnsetValue`.
- **INF-04** The default implementation resolves from `IServiceProvider`. Designer / null-host construction uses `Activator.CreateInstance` on parameterless catalog types that implement `IValueConverter` or `IMultiValueConverter`. Anything else returns `FallbackConverter`.

### 5.2 DiConverterExtension / ResolveExtension

- **INF-10** Markup extension enabling `{v:Resolve {x:Type v:PingStatusToColorBrushConverter}}`.
- **INF-11** `ProvideValue` detects design mode via `DesignerProperties.GetIsInDesignMode` (or a documented equivalent) and then returns a design-time instance instead of touching a missing `IServiceProvider`.
- **INF-12** The extension is stateless besides the requested Type. It is safe to use on many bindings in one view.

### 5.3 BaseDiConverter / BaseDiMultiConverter

- **INF-20** Every v1 / v1.1 `IValueConverter` inherits `BaseDiConverter`. CONV-31 and CONV-32 inherit `BaseDiMultiConverter`.
- **INF-21** The base class implements `Convert` / `ConvertBack` as sealed wrappers that: (a) catch all exceptions and return `UnsetValue`; (b) treat `UnsetValue` and `Binding.DoNothing` inputs as pass-through of `UnsetValue`; (c) dispatch to `ConvertCore` / `ConvertBackCore`.
- **INF-22** `ConvertBackCore` defaults to `Binding.DoNothing` so one-way converters need not implement it.
- **INF-23** Safe when `IServiceProvider` is null. Optional services are resolved lazily.
- **INF-24** `BaseDiMultiConverter` is the `IMultiValueConverter` sibling. An `UnsetValue` in the multi-value array returns `UnsetValue`.
- **INF-25** `ConvertCore` implementations shall be reentrant and shall not mutate instance fields after construction, except lazy service fields using thread-safe initialization.
- **INF-26** `ResolveSemanticBrush(key)` asks `IThemeBrushes.TryGet` then falls back to the frozen hex table in §6.9.
- **INF-27** Catalog types stay public and parameterless so the designer can construct them.

---

## 6. Functional converter catalog

Requirement IDs CONV-01 through CONV-42 are stable and shall be used in tests, PRs, and commit messages. Unlisted converters are a versioned catalog change, not a silent extra class.

### 6.1 Summary

#### v1 core (locked)

| ID | Converter | Group | Primary consumer |
|---|---|---|---|
| CONV-01 | `BooleanToVisibilityConverter` | Visibility | All suite apps |
| CONV-02 | `InverseBooleanToVisibilityConverter` | Visibility | PingIQ, HttpIQ, ProbeHost overlays |
| CONV-03 | `BooleanToVisibilityHiddenConverter` | Visibility | ProbeHost scan grids |
| CONV-04 | `InverseBooleanConverter` | Boolean | All suite apps |
| CONV-05 | `NullToVisibilityConverter` | Visibility | TraceIQ hop details, DnsIQ record details |
| CONV-06 | `NullToBooleanConverter` | Boolean | Command enablement |
| CONV-07 | `EmptyStringToVisibilityConverter` | Visibility | Validation banners, optional notes |
| CONV-08 | `EnumToDescriptionConverter` | Formatting | All suite apps |
| CONV-09 | `BytesToReadableSizeConverter` | Formatting | HttpIQ |
| CONV-10 | `TimeSpanToReadableConverter` | Formatting | PingIQ |
| CONV-11 | `StringToUpperConverter` | Formatting | Headers and badges |
| CONV-12 | `IsGreaterThanToBooleanConverter` | Numeric | Threshold badges |
| CONV-13 | `MathMultiplierConverter` | Numeric | Charts, layout scaling |
| CONV-14 | `PercentageToGridLengthConverter` | Numeric | ProbeHost, HttpIQ charts |
| CONV-15 | `CollectionEmptyToVisibilityConverter` | Collections | TraceIQ, DnsIQ result lists |
| CONV-16 | `CountToBadgeStringConverter` | Collections | Notification badges |
| CONV-17 | `PingStatusToColorBrushConverter` | Domain | PingIQ |
| CONV-18 | `LatencyToSeverityColorConverter` | Domain | PingIQ, TraceIQ hop RTT |
| CONV-19 | `HttpStatusCodeToDescriptionConverter` | Domain | HttpIQ |
| CONV-20 | `DnsRecordTypeToIconConverter` | Domain | DnsIQ |

#### v1.1 common + useful

| ID | Converter | Group | Primary consumer |
|---|---|---|---|
| CONV-21 | `EqualityToBooleanConverter` | Boolean | Status / tab equality |
| CONV-22 | `EqualityToVisibilityConverter` | Visibility | Conditional chrome |
| CONV-23 | `IsLessThanToBooleanConverter` | Numeric | Threshold badges |
| CONV-24 | `IsBetweenToBooleanConverter` | Numeric | Inclusive band checks |
| CONV-25 | `BooleanToBrushConverter` | Boolean | Connected / armed indicators |
| CONV-26 | `BooleanToOpacityConverter` | Boolean | Dim idle telemetry |
| CONV-27 | `HttpStatusToSeverityBrushConverter` | Domain | HttpIQ class coloring |
| CONV-28 | `PercentToStringConverter` | Formatting | Loss / progress captions |
| CONV-29 | `MillisecondsToStringConverter` | Formatting | RTT captions |
| CONV-30 | `CollectionHasItemsToBooleanConverter` | Collections | Command enablement |
| CONV-31 | `BooleanAndToVisibilityConverter` | MultiBinding | Combined gate chrome |
| CONV-32 | `BooleanOrToVisibilityConverter` | MultiBinding | Degraded / busy banners |
| CONV-33 | `DnsResponseCodeToDescriptionConverter` | Domain | DnsIQ RCODE |
| CONV-34 | `TraceHopStatusToBrushConverter` | Domain | TraceIQ hop row |
| CONV-35 | `PortStateToBrushConverter` | Domain | ProbeHost port scan |
| CONV-36 | `TruncateStringConverter` | Formatting | Long host names in grids |
| CONV-37 | `UtcTimestampToStringConverter` | Formatting | Capture clocks |
| CONV-38 | `PacketLossToSeverityBrushConverter` | Domain | PingIQ loss band |
| CONV-39 | `ProgressToPercentConverter` | Numeric | Scan / probe progress |
| CONV-40 | `StringEqualsToVisibilityConverter` | Visibility | Record-type rows |
| CONV-41 | `InvertVisibilityConverter` | Visibility | Complementary panels |
| CONV-42 | `EnumMatchToVisibilityConverter` | Visibility | Multi-state chrome |

### 6.2 Cross-cutting converter rules

- **CONV-G01** Unless a converter's row says otherwise, `ConvertBack` returns `Binding.DoNothing`.
- **CONV-G02** Invalid casts, malformed `ConverterParameter` values, and uninterpretable nulls return `DependencyProperty.UnsetValue` — never throw.
- **CONV-G03** `targetType` may be used as a hint. A mismatch shall not throw.
- **CONV-G04** Culture from the binding is used for numeric and text formatting. When culture is null, `CultureInfo.CurrentCulture` is used.
- **CONV-G05** `ConverterParameter` parsing accepts invariant culture first, then the binding culture.
- **CONV-G06** Public class names shall match the catalog names exactly.
- **CONV-G07** Nullable bool is treated as `false` on CONV-01…04 unless the converter requires a real bool and then returns `UnsetValue`.
- **CONV-G08** Strings are not enumerated as character collections for emptiness or count. CONV-16 may parse a numeric string as a count.

### 6.3 Visibility and boolean

| ID | Mapping |
|---|---|
| CONV-01 | `true` → Visible; `false` / null → Collapsed. ConvertBack: Visible → true; Collapsed / Hidden → false. |
| CONV-02 | Inverse of CONV-01. |
| CONV-03 | `true` → Visible; `false` / null → Hidden (layout preserved). |
| CONV-04 | `true` ↔ `false`. ConvertBack supported. |
| CONV-05 | not-null → Visible; null → Collapsed. Parameter `Inverse` flips. |
| CONV-06 | not-null → true; null → false. Parameter `Inverse` flips. |
| CONV-07 | null / empty / whitespace → Collapsed; else Visible. Parameter `Inverse` flips. |
| CONV-21 | Value equals parameter (`AreEqual`: numeric epsilon, else ignore-case string). Parameter `Inverse` flips. |
| CONV-22 | Same as CONV-21, as `Visibility`. |
| CONV-25 | Parameter `TrueSemantic,FalseSemantic` (default `Success,Neutral`). |
| CONV-26 | Parameter `on,off` opacity (default `1,0.35`). |
| CONV-30 | Collection has items → true. Parameter `Inverse` flips. |
| CONV-40 | String-only equals. Default ignore-case; `Ordinal|MX` is case-sensitive. |
| CONV-41 | `Visible` ↔ `Collapsed`. `Hidden` → `Visible`. Also accepts a bool (`true` → Collapsed). ConvertBack supported. |
| CONV-42 | Matches any comma-separated name after `NormalizeKey` (`A,AAAA,MX` or `Inverse|Timeout,TimedOut`). |

### 6.4 Formatting

| ID | Mapping |
|---|---|
| CONV-08 | `[Description]` then `[Display(Name)]`, else member name. Non-enum → UnsetValue. |
| CONV-09 | 1024-based `B / KB / MB / GB / TB`. Parameter = decimal places (default 2). Example: `1048576` → `1.00 MB`. Negative / non-numeric → UnsetValue. |
| CONV-10 | Compact English `d/h/m/s` (OPEN-04). Omit leading zero units. Zero → `0s`. Parameter `Long` uses English words. Accepts `TimeSpan` or ticks. |
| CONV-11 | Culture uppercase. `null` → `""`. |
| CONV-28 | Default treats 0–100. `4.2` → `4.2%`. `Fraction` treats 0–1 (`0.042` → `4.2%`). |
| CONV-29 | Milliseconds or `TimeSpan` → `42 ms`. Optional integer parameter is decimals. |
| CONV-36 | Truncate with ellipsis. Parameter is max length (default 24). |
| CONV-37 | `DateTime` / `DateTimeOffset` / parseable string as UTC. Unspecified `DateTime` Kind is treated as UTC. Default format `yyyy-MM-dd HH:mm:ss`; any other parameter is a .NET format string. |

### 6.5 Numeric and layout

| ID | Mapping |
|---|---|
| CONV-12 | `value > parameter` → true. Missing / unparseable → UnsetValue. |
| CONV-23 | Exclusive `value < parameter`. |
| CONV-24 | Inclusive `lo,hi` (order-insensitive). Parameter `Inverse` flips. |
| CONV-13 | `value × parameter`. ConvertBack divides when factor ≠ 0. |
| CONV-14 | Clamp 0–100 → `GridLength` star. `Remainder` emits `(100 − value)*`. ConvertBack reads star `GridLength.Value`. |
| CONV-39 | Maps 0–1 progress to 0–100. Values already `> 1` are treated as percent and clamped. ConvertBack returns 0–1 except when the target type is `int`. |

### 6.6 Collections

| ID | Mapping |
|---|---|
| CONV-15 | null or Count 0 → Collapsed; else Visible. Parameter `Inverse` flips. Prefer `ICollection.Count`; other `IEnumerable`s are enumerated once. |
| CONV-16 | Count as badge string. Parameter is max (default 99) → `99+`. `0` stays `0`. Negative treated as 0. |
| CONV-30 | Boolean sibling of CONV-15. |

### 6.7 MultiBinding

| ID | Mapping |
|---|---|
| CONV-31 | Every value true → Visible; else Collapsed. Parameter `Inverse` flips. |
| CONV-32 | Any value true → Visible. Parameter `Inverse` flips. |

Non-bool values coerce: non-zero numbers and non-empty strings are true. An `UnsetValue` in the value array returns `UnsetValue`.

### 6.8 Domain converters

Domain converters accept **enum or string or number** and must not reference suite product assemblies. Unknown inputs use Neutral / documented fallback — they do not throw.

#### CONV-17 Ping status → brush

After `NormalizeKey` (separators stripped, ignore-case):

| Logical status | Brush intent | Fallback |
|---|---|---|
| Success / Ok / Reply | Success | `#22C55E` |
| Timeout / TimedOut | Warning | `#EAB308` |
| DestinationUnreachable / Unreachable | Danger | `#EF4444` |
| TtlExpired / TimeExceeded / Ttl | AccentWarning | `#F97316` |
| other | Neutral | `#94A3B8` |

#### CONV-18 Latency bands

| Band | Default | Brush |
|---|---|---|
| Good | `< 50 ms` | Success |
| Warn | `50–150` inclusive | Warning |
| Critical | `> 150 ms` | Danger |

Override order: `ConverterParameter` `"50,150"` wins over `IConfiguration` keys `Vestigium:Latency:GoodMilliseconds` and `Vestigium:Latency:WarnMilliseconds` (**DOM-01**, **DOM-02**). Missing config uses the defaults. Accepts numeric milliseconds or `TimeSpan`.

#### CONV-19 HTTP phrase

`200` → `200 OK`; `503` → `503 Service Unavailable`. Unknown codes → `{code} Unknown`. Input may be `int`, numeric string, or `HttpStatusCode`.

Required phrases include at least: 200 OK, 201 Created, 204 No Content, 301 Moved Permanently, 302 Found, 304 Not Modified, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 408 Request Timeout, 429 Too Many Requests, 500 Internal Server Error, 502 Bad Gateway, 503 Service Unavailable, 504 Gateway Timeout.

#### CONV-20 DNS type glyph

Default mode is a Segoe Fluent Icons glyph. `ConverterParameter=Path` returns `""` in v1.1 (OPEN-03). Unknown type → generic record glyph. Null → empty string.

| Type | Glyph intent |
|---|---|
| A / AAAA | Host / address mark |
| MX | Mail / envelope mark |
| TXT | Document / text mark |
| CNAME / NS / SOA | Link or nameserver mark |
| PTR | Reverse / reply mark |
| SRV / CAA / other | Generic record mark |

Exact codepoints live in a source table and unit tests.

#### CONV-27 HTTP status class brush

| Class | Brush |
|---|---|
| 1xx | Neutral |
| 2xx | Success |
| 3xx | AccentWarning |
| 4xx | Warning |
| 5xx | Danger |
| other | Neutral |

#### CONV-33 DNS RCODE

Accepts numeric code or name (`3`, `NXDOMAIN`). Default form includes the code and phrase (implementation may emit `3 NXDOMAIN` or a long phrase). Parameter `Name` omits the numeric prefix.

Recognized names include NOERROR, FORMERR, SERVFAIL, NXDOMAIN, NOTIMP, REFUSED, YXDOMAIN, YXRRSET, NXRRSET, NOTAUTH, NOTZONE, and timeout.

#### CONV-34 Trace hop status → brush

| Status | Brush |
|---|---|
| Replied / Success | Success |
| Timeout / Star / `*` | Warning |
| Filtered | AccentWarning |
| Unreachable / Error | Danger |
| other | Neutral |

#### CONV-35 Port state → brush

| State | Brush |
|---|---|
| Open | Success |
| OpenFiltered | AccentWarning |
| Filtered / Stealth | Warning |
| Closed / Reset | Danger |
| TimedOut | AccentWarning |
| other | Neutral |

#### CONV-38 Packet loss bands

Input is 0–100 percent unless `Fraction` is set (values `≤ 1` multiplied by 100).

| Band | Default | Brush |
|---|---|---|
| Good | `< 1%` | Success |
| Warn | `1–5` inclusive | Warning |
| Critical | `> 5%` | Danger |

Override order: `ConverterParameter` `"1,5"` or `"Fraction|1,5"` wins over `Vestigium:PacketLoss:GoodPercent` / `WarnPercent` (**DOM-03**).

### 6.9 Semantic brush fallbacks

When `IThemeBrushes.TryGet(key)` returns a brush, that wins. Otherwise `BrushCache` returns a frozen `SolidColorBrush`.

| Key | Fallback |
|---|---|
| Success | `#22C55E` |
| Warning | `#EAB308` |
| Danger | `#EF4444` |
| AccentWarning | `#F97316` |
| Neutral | `#94A3B8` |

---

## 7. Performance, safety, and design-time

### 7.1 Fail-safe execution

- **NFR-01** Converters sit on the UI rendering path and must never throw unhandled exceptions that crash the WPF dispatcher.
- **NFR-02** Invalid casts, malformed parameters, and null bindings must be caught and must return `UnsetValue`.
- **NFR-03** `BaseDiConverter` / `BaseDiMultiConverter` is the enforcement point. Catalog authors still write defensive `ConvertCore` methods.
- **NFR-04** Logging of swallowed exceptions is allowed at Debug / Trace only, and must itself be guarded.

### 7.2 Performance

- **NFR-10** Singleton lifetime for all catalog converters (ARCH-20).
- **NFR-11** Boolean / visibility / comparison success paths shall allocate no managed objects beyond the boxed return WPF already requires.
- **NFR-12** String formatters may allocate the result string. They shall not allocate per-call `StringBuilder`s when interpolation suffices.
- **NFR-13** Brush converters shall return cached frozen brushes, not a new `SolidColorBrush` per call.
- **NFR-14** Collection emptiness checks shall not enumerate large sequences when `Count` is available.

### 7.3 Design-time support

- **DT-01** Detect the Visual Studio XAML Designer via `DesignerProperties.GetIsInDesignMode` (or a documented equivalent for WPF on .NET 10).
- **DT-02** When design mode is true, or the host provider is null, construct catalog types with `Activator.CreateInstance` so the previewer does not throw.
- **DT-03** Design-time instances implement the same interfaces. Plausible outputs are preferred over `UnsetValue` for common types so layout is visible.
- **DT-04** Design-time path shall not require App startup, Host build, or configuration files.

---

## 8. Quality attributes and testing

### 8.1 Unit tests

- **QA-01** Each CONV-xx shall have tests for the documented mapping table, ConvertBack policy, and at least one fail-safe case (wrong type, bad parameter, null).
- **QA-02** `BaseDiConverter` shall have tests proving that a throwing `ConvertCore` still returns `UnsetValue`.
- **QA-03** `AddVestigiumConverters` shall be tested against `ServiceCollection`: every catalog type resolves, and all are Singleton.
- **QA-04** Threshold override vs configuration for CONV-18 and CONV-38 shall be tested with a fake `IConfiguration`.
- **QA-05** v1.1 additions are covered by `CatalogV11Tests` in addition to group test classes.

Run on Windows:

```text
dotnet test src/Vestigium.Converters.Tests
```

### 8.2 Acceptance criteria (library done)

- A host can call `AddVestigiumConverters()`, set `VestigiumConverterHost.ServiceProvider`, and use `{v:Resolve}` on all forty-two types.
- XAML designer opens a sample view that references representative converters without error.
- A soak binding updating ping status and latency at ≥ 10 Hz for 60 seconds does not grow converter instance count (singleton) and does not throw on the dispatcher.
- No converter public method documents "throws" for conversion failure.
- CI on `windows-latest` restores, builds, and tests green.
- The in-repo demo is a tabbed gallery that resolves every catalog family through `{v:Resolve}` and does not declare a static converter resource dictionary.

### 8.3 Accessibility and theming

- **QA-10** Color converters exist for status indication and must not be the only channel. Views should still expose text status.
- **QA-11** Default fallback colors shall remain distinguishable in both light and dark sample palettes.

### 8.4 Demo gallery

`Vestigium.Converters.Demo` is a gallery host, not a product window.

- **DEMO-01** Resolve every catalog converter through `{v:Resolve}`.
- **DEMO-02** Do not declare a static converter resource dictionary.
- **DEMO-03** Do not reference PingIQ / DnsIQ / TraceIQ / HttpIQ / ProbeHost assemblies.
- **DEMO-04** Domain values are strings or a demo-only enum (for CONV-08 `[Description]`).
- **DEMO-05** Tabs group the catalog: Showcase, Visibility, Boolean, Formatting, Numeric, Collections, Domain, MultiBinding.

---

## 9. Dependencies

| Package / API | Role | Required? |
|---|---|---|
| Microsoft.WindowsDesktop.App (WPF) | `IValueConverter`, `MarkupExtension`, `Visibility`, `GridLength`, `DesignerProperties` | Yes |
| Microsoft.Extensions.DependencyInjection.Abstractions | `IServiceCollection` / `IServiceProvider` | Yes |
| Microsoft.Extensions.DependencyInjection | Host registration helper target | Yes |
| Microsoft.Extensions.Configuration.Abstractions | CONV-18 / CONV-38 options | Optional — guard if absent |
| Microsoft.Extensions.Logging.Abstractions | Diagnostic logging | Optional — guard if absent |
| CommunityToolkit.Mvvm | Demo MVVM only | No (library) |
| `System.ComponentModel` / DataAnnotations | `[Description]` / `[Display]` for CONV-08 | Yes for CONV-08 |

---

## 10. Traceability

Use these IDs in tests, PRs, and review checklists.

| Source topic | Requirement IDs |
|---|---|
| .NET 10 / WPF / MVVM / DI | ARCH-01…ARCH-08, ARCH-10…ARCH-23 |
| `IConverterProvider` | INF-01…INF-04 |
| Markup extension | INF-10…INF-12, INT-10…INT-14 |
| Base converters | INF-20…INF-27 |
| Catalog | CONV-01…CONV-42, CONV-G01…CONV-G08 |
| Domain tables and config | DOM-01, DOM-02, DOM-03 |
| Fail-safe and performance | NFR-01…NFR-14 |
| Designer | DT-01…DT-04 |
| Host hookup | INT-01…INT-07, INT-20…INT-21 |
| Tests / acceptance | QA-01…QA-05, QA-10…QA-11 |
| Demo gallery | DEMO-01…DEMO-05 |

---

## 11. Open points

Defaults in this document apply until the suite owner records a further decision. Several original draft items are now closed.

| ID | Question | Status | Default / decision |
|---|---|---|---|
| OPEN-01 | Where does the Ping status enum live? | **Closed** | Accept enum or string; no hard reference on PingIQ. |
| OPEN-02 | Theme service type name | **Closed** | Optional `IThemeBrushes`; else frozen hex fallbacks. |
| OPEN-03 | CONV-20 SVG path pack | Open | Glyph mode ships; `Path` returns `""` for unimplemented types. |
| OPEN-04 | Localization of TimeSpan units | Open | English unit suffixes in v1.1; culture used for numbers only. |
| OPEN-05 | XML namespace URI | **Closed** | `http://schemas.vestigium.dev/converters` plus `XmlnsDefinition`. |

---

## 12. Adding a converter later

1. Inherit `BaseDiConverter` or `BaseDiMultiConverter`.
2. Keep a public parameterless constructor.
3. Register it in `AddVestigiumConverters`.
4. Add an `XmlnsDefinition` if the namespace is new.
5. Add mapping + fail-safe tests.
6. Update this specification, the developer’s guide, and the README catalog table.

Convert never throws on the dispatcher.

---

## 13. Document control

| Version | Date | Notes |
|---|---|---|
| 1.0 | 2026-09-06 | Draft working SRS. Twenty-converter catalog plus implementable contracts for .NET 10 WPF + MVVM + DI. |
| 1.1 | 2026-09-07 | **Finalized.** Records the shipped library: CONV-01…CONV-42, DI host hook, semantic brushes, MultiBinding converters, packet-loss / progress / truncate / UTC catalog, tabbed demo gallery, and closed open points OPEN-01 / OPEN-02 / OPEN-05. |

Changes to converter names, mapping tables, or infrastructure types require an update to this document before merge. Adding an unlisted converter is a versioned catalog change, not a silent extra class.
