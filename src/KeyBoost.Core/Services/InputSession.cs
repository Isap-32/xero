namespace KeyBoost.Core.Services;
public sealed record InputObservation(int Key, bool Down, DateTimeOffset Timestamp, double? HoldMs, double? IntervalMs, int TapCount, int Concurrent);
public sealed class InputSession
{
    private readonly Dictionary<int, double> held = new();
    private readonly Dictionary<int, double> previous = new();
    private readonly Dictionary<int, int> taps = new();
    public IReadOnlyCollection<int> Pressed => held.Keys;
    public int Peak { get; private set; }

    public InputObservation? Observe(int key, bool down, double monotonicMs, DateTimeOffset timestamp)
    {
        double? hold = null, interval = null;
        if (down)
        {
            if (held.ContainsKey(key))
                return null;
            if (previous.TryGetValue(key, out var last))
                interval = monotonicMs - last;
            held[key] = monotonicMs;
            previous[key] = monotonicMs;
            taps[key] = taps.GetValueOrDefault(key) + 1;
            Peak = Math.Max(Peak, held.Count);
        }
        else
        {
            if (!held.Remove(key, out var start))
                return null;
            hold = monotonicMs - start;
        }

        return new(key, down, timestamp, hold, interval, taps.GetValueOrDefault(key), held.Count);
    }

    public void ReleaseAll() => held.Clear();
    public void Clear()
    {
        held.Clear();
        previous.Clear();
        taps.Clear();
        Peak = 0;
    }
}
