using System.Diagnostics;
using System.Runtime; // GCSettings
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Background watchdog that trims memory when approaching a soft limit
/// and terminates the process if the hard limit is exceeded.
/// </summary>
public sealed class MemoryPressureGuard : BackgroundService
{
    private const long SoftLimitMb = 300;   // start trimming here
    private const long HardLimitMb = 500;  // never allow above this
    private static readonly TimeSpan CheckEvery = TimeSpan.FromSeconds(30);

    private readonly ILogger<MemoryPressureGuard> log;
    private readonly IMemoryCache cache;
    private readonly IHostApplicationLifetime lifetime;

    public MemoryPressureGuard(
        ILogger<MemoryPressureGuard> log,
        IMemoryCache cache,
        IHostApplicationLifetime lifetime)
    {
        this.log = log;
        this.cache = cache;
        this.lifetime = lifetime;
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

                if (usedMb >= SoftLimitMb)
                {
                    this.log.LogWarning("Memory soft limit reached: ~{UsedMb} MB. Compacting caches and forcing GC.", usedMb);

                    if (this.cache is MemoryCache mem)
                    {
                        mem.Compact(0.50); // Trim half under pressure
                    }

                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, blocking: false, compacting: true);
                }

                if (usedMb >= HardLimitMb)
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
