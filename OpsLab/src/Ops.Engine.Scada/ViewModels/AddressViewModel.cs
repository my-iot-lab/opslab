using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ops.Exchange.Management;
using Ops.Exchange.Model;

namespace Ops.Engine.Scada.ViewModels;

internal sealed class AddressViewModel : ObservableObject
{
    private readonly DeviceInfoManager _deviceInfoManager;

    public AddressViewModel(DeviceInfoManager deviceInfoManager)
    {
        _deviceInfoManager = deviceInfoManager;

        SelectedStationCommand = new RelayCommand(() =>
        {
            SelectStation();
        });

        Init();
    }

    void Init()
    {
        var deviceInfos = _deviceInfoManager.GetAll();
        foreach (var deviceInfo in deviceInfos)
        {
            _stations.Add(deviceInfo.Schema);
        }
    }

    #region 绑定属性

    private ObservableCollection<DeviceSchema> _stations = new();
    public ObservableCollection<DeviceSchema> Stations
    {
        get => _stations;
        set { SetProperty(ref _stations, value); }
    }

    private DeviceSchema? _selectedStationValue;
    public DeviceSchema? SelectedStationValue
    {
        get => _selectedStationValue;
        set { SetProperty(ref _selectedStationValue, value); }
    }

    private ObservableCollection<DeviceVariable> _deviceVariables = new();
    public ObservableCollection<DeviceVariable> DeviceVariables
    {
        get => _deviceVariables;
        set { SetProperty(ref _deviceVariables, value); }
    }

    #endregion

    #region Binding Command

    /// <summary>
    /// 工站下拉框更改事件
    /// </summary>
    public ICommand SelectedStationCommand { get; }

    #endregion

    #region Private

    private void SelectStation()
    {
        DeviceVariables.Clear();

        var deviceInfo = _deviceInfoManager.Get(SelectedStationValue!.Line, SelectedStationValue!.Station);
        foreach (var variable in deviceInfo!.Variables)
        {
            DeviceVariables.Add(variable);
        }
    }

    #endregion
}
