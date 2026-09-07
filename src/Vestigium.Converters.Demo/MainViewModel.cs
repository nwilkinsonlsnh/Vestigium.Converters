using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vestigium.Converters.Demo;

public enum ProbePhase
{
    [Description("Idle — waiting for the next scan")]
    Idle,

    [Description("Probe in flight")]
    Scanning,

    [Description("Last run completed")]
    Complete,

    [Description("Operator cancelled the run")]
    Cancelled
}

public sealed partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        RestoreHops();
    }

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool hasSelection = true;
    [ObservableProperty] private bool isConnected = true;
    [ObservableProperty] private object? selectedHop;
    [ObservableProperty] private string note = "Ready";
    [ObservableProperty] private double latencyMs = 42;
    [ObservableProperty] private double packetLoss = 0.4;
    [ObservableProperty] private long payloadBytes = 1_048_576;
    [ObservableProperty] private int httpStatus = 200;
    [ObservableProperty] private string dnsType = "MX";
    [ObservableProperty] private string pingStatus = "Success";
    [ObservableProperty] private TimeSpan uptime = new(2, 15, 30);
    [ObservableProperty] private int resultCount = 12;
    [ObservableProperty] private string targetHost = "edge-west-01.probe.vestigium.example.net";
    [ObservableProperty] private DateTimeOffset capturedAt = new(2026, 9, 6, 23, 50, 0, TimeSpan.Zero);
    [ObservableProperty] private double progress = 0.42;
    [ObservableProperty] private double splitPercent = 62;
    [ObservableProperty] private double multiplier = 2;
    [ObservableProperty] private ProbePhase phase = ProbePhase.Scanning;
    [ObservableProperty] private string traceHopStatus = "Replied";
    [ObservableProperty] private string portState = "Open";
    [ObservableProperty] private string dnsRcode = "NXDOMAIN";
    [ObservableProperty] private string equalityNeedle = "Success";
    [ObservableProperty] private string matchToken = "MX";

    public ObservableCollection<string> Hops { get; } = [];

    public IReadOnlyList<string> PingStatuses { get; } =
        ["Success", "Timeout", "DestinationUnreachable", "TtlExpired", "Unknown"];

    public IReadOnlyList<string> DnsTypes { get; } =
        ["A", "AAAA", "MX", "TXT", "CNAME", "NS", "PTR", "SRV"];

    public IReadOnlyList<ProbePhase> ProbePhases { get; } = Enum.GetValues<ProbePhase>();

    public IReadOnlyList<string> TraceHopStatuses { get; } =
        ["Replied", "Success", "Timeout", "*", "Filtered", "Unreachable"];

    public IReadOnlyList<string> PortStates { get; } =
        ["Open", "OpenFiltered", "Filtered", "Stealth", "Closed", "Reset"];

    public IReadOnlyList<string> DnsRcodes { get; } =
        ["NOERROR", "FORMERR", "SERVFAIL", "NXDOMAIN", "NOTIMP", "REFUSED", "NOTAUTH", "Timeout"];

    [RelayCommand]
    private void ToggleBusy() => IsBusy = !IsBusy;

    [RelayCommand]
    private void ClearHops()
    {
        Hops.Clear();
        SelectedHop = null;
        HasSelection = false;
        Note = string.Empty;
        ResultCount = 0;
        OnPropertyChanged(nameof(Hops));
    }

    [RelayCommand]
    private void RestoreHops()
    {
        if (Hops.Count == 0)
        {
            Hops.Add("edge-1.vestigium.local");
            Hops.Add("core-2.vestigium.local");
            Hops.Add("anycast-9.vestigium.local");
        }

        SelectedHop = Hops[0];
        HasSelection = true;
        Note = "Route restored";
        ResultCount = Hops.Count;
        OnPropertyChanged(nameof(Hops));
    }

    [RelayCommand]
    private void StampNow() => CapturedAt = DateTimeOffset.UtcNow;
}
