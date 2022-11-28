using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ops.Engine.Scada.ViewModels;

/// <summary>
/// 应用程序诊断
/// </summary>
internal class AppDiagnosticViewModel : ObservableObject, IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _gcCounter;

    public AppDiagnosticViewModel()
    {
        _cpuCounter = new("Processor", "% Processor Time", "_Total");
        _gcCounter = new(".NET CLR Memory", "% Time in GC");

        _ = Task.Factory.StartNew(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                await Task.Delay(1000);

                CpuConsumption = (float)Math.Round(_cpuCounter.NextValue(), 1);
                GcConsumption = new Random().Next(1, 100); // _gcCounter.NextValue();
            }
        }, default, default, TaskScheduler.FromCurrentSynchronizationContext());
    }

    /// <summary>
    /// 获取迄今为止已处理的工作项数。
    /// </summary>
    public int CompletedWorkItemCount { get; set; }

    /// <summary>
    /// 获取当前已加入处理队列的工作项数。
    /// </summary>
    public int PendingWorkItemCount { get; set; }

    /// <summary>
    /// 获取当前存在的线程池线程数。
    /// </summary>
    public int ThreadCount { get; set; }


    private float _cpuConsumption;

    /// <summary>
    /// CPU 使用率
    /// </summary>
    public float CpuConsumption
    {
        get { return _cpuConsumption; }
        set { SetProperty(ref _cpuConsumption, value); }
    }

    private float _gcConsumption;

    /// <summary>
    /// GC 使用率
    /// </summary>
    public float GcConsumption
    {
        get { return _gcConsumption; }
        set { SetProperty(ref _gcConsumption, value); }
    }

    public void Dispose()
    {
        _cts.Cancel();

        _cpuCounter.Dispose();
        _gcCounter.Dispose();
    }
}
