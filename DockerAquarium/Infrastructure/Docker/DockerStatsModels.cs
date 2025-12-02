using System;
using System.Collections.Generic;

namespace DockerAquarium.Infrastructure.Docker;

/// <summary>
/// Docker stats response structure for deserialization
/// </summary>
#pragma warning disable CS8618
internal class StatsResponse
{
    public CpuStats CpuStats { get; set; }
    public CpuStats PreCpuStats { get; set; }
    public MemoryStats MemoryStats { get; set; }
    public Dictionary<string, NetworkStats> Networks { get; set; }
}

internal class CpuStats
{
    public CpuUsage CpuUsage { get; set; }
    public ulong SystemCpuUsage { get; set; }
}

internal class CpuUsage
{
    public ulong TotalUsage { get; set; }
    public List<ulong> PercpuUsage { get; set; }
}

internal class MemoryStats
{
    public ulong Usage { get; set; }
    public ulong Limit { get; set; }
}

internal class NetworkStats
{
    public ulong RxBytes { get; set; }
    public ulong TxBytes { get; set; }
}
#pragma warning restore CS8618

