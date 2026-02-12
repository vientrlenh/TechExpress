namespace TechExpress.Service.Constants;

public static class SpecCodeConstant
{
    // ============= CPU =============
    public const string CpuSocket = "cpu_socket";
    public const string CpuCores = "cpu_cores";
    public const string CpuThreads = "cpu_threads";
    public const string CpuBaseClock = "cpu_base_clock";
    public const string CpuBoostClock = "cpu_boost_clock";
    public const string CpuTdp = "cpu_tdp";
    public const string CpuMemoryType = "cpu_memory_type";
    public const string CpuIntegratedGpu = "cpu_integrated_gpu";
    public const string CpuEccSupport = "cpu_ecc_support";
    public const string CpuPcieLanes = "cpu_pcie_lanes";

    // ============= MOTHERBOARD =============
    public const string MbSocket = "mb_socket";
    public const string MbChipset = "mb_chipset";
    public const string MbFormFactor = "mb_form_factor";
    public const string MbMemoryType = "mb_memory_type";
    public const string MbMemorySlots = "mb_memory_slots";
    public const string MbMaxMemory = "mb_max_memory";
    public const string MbM2Slots = "mb_m2_slots";
    public const string MbPcieVersion = "mb_pcie_version";
    public const string MbPcieX16Slots = "mb_pcie_x16_slots";
    public const string MbSataPorts = "mb_sata_ports";
    public const string MbMaxRamSpeed = "mb_max_ram_speed";
    public const string MbDualSocket = "mb_dual_socket";
    public const string MbEccSupport = "mb_ecc_support";
    public const string MbIpmiSupport = "mb_ipmi_support";
    public const string MbTotalPcieLanes = "mb_total_pcie_lanes";

    // ============= RAM =============
    public const string RamType = "ram_type";
    public const string RamSpeed = "ram_speed";
    public const string RamCapacity = "ram_capacity";
    public const string RamSticks = "ram_sticks";
    public const string RamLatency = "ram_latency";
    public const string RamEcc = "ram_ecc";
    public const string RamRegistered = "ram_registered";

    // ============= GPU =============
    public const string GpuVram = "gpu_vram";
    public const string GpuTdp = "gpu_tdp";
    public const string GpuLength = "gpu_length";
    public const string GpuPcieSlot = "gpu_pcie_slot";
    public const string GpuPowerConnector = "gpu_power_connector";
    public const string GpuEccVram = "gpu_ecc_vram";
    public const string GpuProfessional = "gpu_professional";
    public const string GpuCudaCores = "gpu_cuda_cores";
    public const string GpuTensorCores = "gpu_tensor_cores";

    // ============= PSU =============
    public const string PsuWattage = "psu_wattage";
    public const string PsuEfficiency = "psu_efficiency";
    public const string PsuModular = "psu_modular";
    public const string PsuFormFactor = "psu_form_factor";
    public const string PsuRedundant = "psu_redundant";

    // ============= STORAGE =============
    public const string StorType = "stor_type";
    public const string StorCapacity = "stor_capacity";
    public const string StorInterface = "stor_interface";
    public const string StorFormFactor = "stor_form_factor";
    public const string StorReadSpeed = "stor_read_speed";
    public const string StorWriteSpeed = "stor_write_speed";
    public const string StorTbw = "stor_tbw";
    public const string StorDwpd = "stor_dwpd";
    public const string StorPowerLossProtection = "stor_power_loss_protection";

    // ============= CASE =============
    public const string CaseFormFactor = "case_form_factor";
    public const string CaseMaxGpuLength = "case_max_gpu_length";
    public const string CaseMaxCoolerHeight = "case_max_cooler_height";
    public const string CasePsuFormFactor = "case_psu_form_factor";
    public const string CaseDriveBays25 = "case_drive_bays_25";
    public const string CaseDriveBays35 = "case_drive_bays_35";
    public const string CaseMaxFanSize = "case_max_fan_size";

    // ============= CPU COOLER =============
    public const string CoolerType = "cooler_type";
    public const string CoolerSocketSupport = "cooler_socket_support";
    public const string CoolerTdpRating = "cooler_tdp_rating";
    public const string CoolerHeight = "cooler_height";

    // ============= LAPTOP =============
    public const string LapCpu = "lap_cpu";
    public const string LapCpuCores = "lap_cpu_cores";
    public const string LapCpuThreads = "lap_cpu_threads";
    public const string LapRamCapacity = "lap_ram_capacity";
    public const string LapRamType = "lap_ram_type";
    public const string LapRamSpeed = "lap_ram_speed";
    public const string LapGpu = "lap_gpu";
    public const string LapGpuVram = "lap_gpu_vram";
    public const string LapStorage = "lap_storage";
    public const string LapStorageType = "lap_storage_type";
    public const string LapScreenSize = "lap_screen_size";
    public const string LapScreenResolution = "lap_screen_resolution";
    public const string LapRefreshRate = "lap_refresh_rate";
    public const string LapBattery = "lap_battery";
    public const string LapWeight = "lap_weight";
    public const string LapOs = "lap_os";

    // ============= KEYBOARD =============
    public const string KbType = "kb_type";
    public const string KbSwitch = "kb_switch";
    public const string KbLayout = "kb_layout";
    public const string KbConnection = "kb_connection";
    public const string KbRgb = "kb_rgb";

    // ============= MOUSE =============
    public const string MouseSensor = "mouse_sensor";
    public const string MouseDpi = "mouse_dpi";
    public const string MouseConnection = "mouse_connection";
    public const string MouseWeight = "mouse_weight";
    public const string MouseButtons = "mouse_buttons";

    // ============= HEADSET =============
    public const string HsType = "hs_type";
    public const string HsDriver = "hs_driver";
    public const string HsConnection = "hs_connection";
    public const string HsMicrophone = "hs_microphone";
    public const string HsNoiseCancelling = "hs_noise_cancelling";

    // ============= MONITOR =============
    public const string MonSize = "mon_size";
    public const string MonResolution = "mon_resolution";
    public const string MonPanel = "mon_panel";
    public const string MonRefreshRate = "mon_refresh_rate";
    public const string MonResponseTime = "mon_response_time";
    public const string MonPorts = "mon_ports";
    public const string MonHdr = "mon_hdr";

    // ============= WEBCAM =============
    public const string WcResolution = "wc_resolution";
    public const string WcFps = "wc_fps";
    public const string WcMicrophone = "wc_microphone";
    public const string WcAutofocus = "wc_autofocus";

    // ============= SPEAKER =============
    public const string SpkPower = "spk_power";
    public const string SpkChannels = "spk_channels";
    public const string SpkConnection = "spk_connection";

    // ============= MOUSEPAD =============
    public const string MpSize = "mp_size";
    public const string MpMaterial = "mp_material";
    public const string MpRgb = "mp_rgb";

    // ============= NETWORK =============
    public const string NetType = "net_type";
    public const string NetSpeed = "net_speed";
    public const string NetWifiStandard = "net_wifi_standard";

    // ============= EXTERNAL STORAGE =============
    public const string ExtType = "ext_type";
    public const string ExtCapacity = "ext_capacity";
    public const string ExtInterface = "ext_interface";

    // ============= UPS =============
    public const string UpsCapacity = "ups_capacity";
    public const string UpsWattage = "ups_wattage";
    public const string UpsOutlets = "ups_outlets";

    // ============= CASE FAN =============
    public const string FanSize = "fan_size";
    public const string FanRpm = "fan_rpm";
    public const string FanAirflow = "fan_airflow";
    public const string FanRgb = "fan_rgb";
    public const string FanQuantity = "fan_quantity";

    // ============= CABLE =============
    public const string CableType = "cable_type";
    public const string CableLength = "cable_length";
    public const string CableVersion = "cable_version";

    // ============= GAMING CHAIR =============
    public const string ChairMaterial = "chair_material";
    public const string ChairMaxWeight = "chair_max_weight";
    public const string ChairRecline = "chair_recline";
    public const string ChairArmrest = "chair_armrest";
}
