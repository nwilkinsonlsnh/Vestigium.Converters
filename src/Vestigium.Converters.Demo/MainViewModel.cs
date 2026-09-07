using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vestigium.Converters.Demo;

public sealed partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        Hops.Add("edge-1.vestigium.local");
        Hops.Add("core-2.vestigium.local");
    }

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private object? selectedHop = "edge-1.vestigium.local";
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

    public ObservableCollection<string> Hops { get; } = [];

    public IReadOnlyList<string> PingStatuses { get; } =
        ["Success", "Timeout", "DestinationUnreachable", "TtlExpired", "Unknown"];

    public IReadOnlyList<string> DnsTypes { get; } =
        ["A", "AAAA", "MX", "TXT", "CNAME", "NS", "PTR", "SRV"];

    [RelayCommand]
    private void ToggleBusy() => IsBusy = !IsBusy;

    [RelayCommand]
    private void ClearHops()
    {
        Hops.Clear();
        SelectedHop = null;
        Note = string.Empty;
    }

    [RelayCommand]
    private void RestoreHops()
    {
        if (Hops.Count == 0)
        {
            Hops.Add("edge-1.vestigium.local");
            Hops.Add("core-2.vestigium.local");
        }

        SelectedHop = Hops[0];
        Note = "Route restored";
    }
}
