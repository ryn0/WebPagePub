using System.Diagnostics;
using System.Runtime; // GCSettings
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Background watchdog that trims memory when approaching a soft limit
/// and terminates the process if the hard limit is exceeded.
/// </summary>
public sealed class MemoryPressureGuard : BackgroundService
{
    // Normal steady state for these sites is ~250-350 MB of process memory, so the old
    // 300/500 MB limits trimmed caches every 30s and restarted every site 5-10x a day
    // (each restart = ~10s of 502s, seen by Googlebot). Soft limit now looks at the managed
    // heap (what cache trimming can actually free); hard limit is a runaway-leak guard only.
    // Both can be overridden in appsettings: MemoryGuard:SoftHeapLimitMb / MemoryGuard:HardLimitMb.
    private const long DefaultSoftHeapLimitMb = 350;
    private const long DefaultHardLimitMb = 1200;
    private static readonly TimeSpan CheckEvery = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan SoftLogEvery = TimeSpan.FromMinutes(30);

    private readonly ILogger<MemoryPressureGuard> log;
    private readonly IMemoryCache cache;
    private readonly IHostApplicationLifetime lifetime;
    private readonly long softHeapLimitMb;
    private readonly long hardLimitMb;
    private DateTime lastSoftLogUtc = DateTime.MinValue;

    public MemoryPressureGuard(
        ILogger<MemoryPressureGuard> log,
        IMemoryCache cache,
        IHostApplicationLifetime lifetime,
        IConfiguration config)
    {
        this.log = log;
        this.cache = cache;
        this.lifetime = lifetime;
        this.softHeapLimitMb = config.GetValue<long?>("MemoryGuard:SoftHeapLimitMb") ?? DefaultSoftHeapLimitMb;
        this.hardLimitMb = config.GetValue<long?>("MemoryGuard:HardLimitMb") ?? DefaultHardLimitMb;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var proc = Process.GetCurrentProcess();
                long working = proc.WorkingSet64;
                long privateBytes = proc.PrivateMemorySize64;
                long usedMb = Math.Max(working, privateBytes) / (1024 * 1024);
                long heapMb = GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024);

                if (heapMb >= this.softHeapLimitMb)
                {
                    if (DateTime.UtcNow - this.lastSoftLogUtc >= SoftLogEvery)
                    {
                        this.lastSoftLogUtc = DateTime.UtcNow;
                        this.log.LogWarning("Memory soft limit reached: heap ~{HeapMb} MB, process ~{UsedMb} MB. Compacting caches.", heapMb, usedMb);
                    }

                    if (this.cache is MemoryCache mem)
                    {
                        mem.Compact(0.50); // Trim half under pressure
                    }

                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, blocking: false, compacting: true);
                }

                if (usedMb >= this.hardLimitMb)
                {
                    this.log.LogError("Memory HARD limit exceeded: ~{UsedMb} MB. Stopping application.", usedMb);
                    this.lifetime.StopApplication();

                    // Failsafe if graceful stop stalls
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), CancellationToken.None);
                        Environment.FailFast("Hard memory cap exceeded");
                    });

                    return;
                }
            }
            catch (Exception ex)
            {
                this.log.LogError(ex, "MemoryPressureGuard error");
            }

            try { await Task.Delay(CheckEvery, stoppingToken); } catch { /* ignore */ }
        }
    }
}
