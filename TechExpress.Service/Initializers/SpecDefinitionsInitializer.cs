using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;

namespace TechExpress.Service.Initializers;

public static class SpecDefinitionsInitializer
{
    public static async Task Init(ApplicationDbContext context)
    {
        if (await context.SpecDefinitions.AnyAsync())
        {
            return;
        }

        var categories = await context.Categories.ToListAsync();
        var cpuId = categories.First(c => c.Name == "CPU").Id;
        var mbId = categories.First(c => c.Name == "Bo mạch chủ").Id;
        var ramId = categories.First(c => c.Name == "RAM").Id;
        var gpuId = categories.First(c => c.Name == "Card đồ họa").Id;
        var psuId = categories.First(c => c.Name == "Nguồn máy tính").Id;
        var storageId = categories.First(c => c.Name == "Ổ cứng").Id;
        var caseId = categories.First(c => c.Name == "Vỏ máy tính").Id;
        var coolerId = categories.First(c => c.Name == "Tản nhiệt CPU").Id;
        var laptopId = categories.First(c => c.Name == "Máy tính xách tay").Id;
        var keyboardId = categories.First(c => c.Name == "Bàn phím").Id;
        var mouseId = categories.First(c => c.Name == "Chuột").Id;
        var headsetId = categories.First(c => c.Name == "Tai nghe").Id;
        var monitorId = categories.First(c => c.Name == "Màn hình").Id;
        var webcamId = categories.First(c => c.Name == "Webcam").Id;
        var speakerId = categories.First(c => c.Name == "Loa").Id;
        var mousepadId = categories.First(c => c.Name == "Lót chuột").Id;
        var networkId = categories.First(c => c.Name == "Thiết bị mạng").Id;
        var extStorageId = categories.First(c => c.Name == "Ổ cứng di động").Id;
        var upsId = categories.First(c => c.Name == "Bộ lưu điện").Id;
        var caseFanId = categories.First(c => c.Name == "Quạt case").Id;
        var cableId = categories.First(c => c.Name == "Cáp & Đầu chuyển").Id;
        var chairId = categories.First(c => c.Name == "Ghế gaming").Id;

        var specs = new List<SpecDefinition>
        {
            // ============= CPU =============
            new()
            {
                Id = Guid.NewGuid(), Code = "cpu_socket", Name = "Socket",
                CategoryId = cpuId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại socket CPU (VD: LGA1700, AM5)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "cpu_cores", Name = "Số nhân",
                CategoryId = cpuId, Unit = "cores", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số nhân vật lý", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "cpu_threads", Name = "Số luồng",
                CategoryId = cpuId, Unit = "threads", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số luồng xử lý", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "cpu_base_clock", Name = "Xung nhịp cơ bản",
                CategoryId = cpuId, Unit = "GHz", AcceptValueType = SpecAcceptValueType.Decimal,
                Description = "Tần số xung nhịp cơ bản", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "cpu_boost_clock", Name = "Xung nhịp tối đa",
                CategoryId = cpuId, Unit = "GHz", AcceptValueType = SpecAcceptValueType.Decimal,
                Description = "Tần số xung nhịp turbo tối đa", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "cpu_tdp", Name = "TDP",
                CategoryId = cpuId, Unit = "W", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Công suất nhiệt thiết kế", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "cpu_memory_type", Name = "Loại RAM hỗ trợ",
                CategoryId = cpuId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại bộ nhớ hỗ trợ (VD: DDR5, DDR4)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "cpu_integrated_gpu", Name = "GPU tích hợp",
                CategoryId = cpuId, Unit = "", AcceptValueType = SpecAcceptValueType.Bool,
                Description = "Có GPU tích hợp hay không", IsRequired = true,
            },

            // ============= MOTHERBOARD =============
            new()
            {
                Id = Guid.NewGuid(), Code = "mb_socket", Name = "Socket CPU",
                CategoryId = mbId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại socket CPU (phải khớp với cpu_socket)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mb_chipset", Name = "Chipset",
                CategoryId = mbId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Chipset bo mạch chủ (VD: Z790, B650)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mb_form_factor", Name = "Form Factor",
                CategoryId = mbId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Kích thước bo mạch (ATX, mATX, ITX)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mb_memory_type", Name = "Loại RAM",
                CategoryId = mbId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại bộ nhớ hỗ trợ (DDR4, DDR5)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mb_memory_slots", Name = "Số khe RAM",
                CategoryId = mbId, Unit = "slots", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số khe cắm DIMM", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mb_max_memory", Name = "RAM tối đa",
                CategoryId = mbId, Unit = "GB", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Dung lượng RAM tối đa hỗ trợ", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mb_m2_slots", Name = "Số khe M.2",
                CategoryId = mbId, Unit = "slots", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số khe cắm M.2 NVMe", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mb_pcie_version", Name = "Phiên bản PCIe",
                CategoryId = mbId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Phiên bản PCIe (VD: PCIe 5.0, PCIe 4.0)", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mb_pcie_x16_slots", Name = "Số khe PCIe x16",
                CategoryId = mbId, Unit = "slots", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số khe cắm PCIe x16 cho card đồ họa", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mb_sata_ports", Name = "Số cổng SATA",
                CategoryId = mbId, Unit = "ports", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số cổng SATA cho ổ cứng HDD/SSD 2.5\"", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mb_max_ram_speed", Name = "Tốc độ RAM tối đa",
                CategoryId = mbId, Unit = "MHz", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tốc độ RAM tối đa hỗ trợ", IsRequired = false,
            },

            // ============= RAM =============
            new()
            {
                Id = Guid.NewGuid(), Code = "ram_type", Name = "Loại RAM",
                CategoryId = ramId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại bộ nhớ (DDR4, DDR5)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "ram_speed", Name = "Tốc độ",
                CategoryId = ramId, Unit = "MHz", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tốc độ bus (VD: 3200, 5600)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "ram_capacity", Name = "Dung lượng mỗi thanh",
                CategoryId = ramId, Unit = "GB", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Dung lượng mỗi thanh RAM (VD: 8, 16, 32)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "ram_sticks", Name = "Số thanh",
                CategoryId = ramId, Unit = "pcs", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số thanh RAM trong bộ (VD: 2 cho dual-channel)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "ram_latency", Name = "CAS Latency",
                CategoryId = ramId, Unit = "CL", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Độ trễ CAS (VD: CL16, CL36)", IsRequired = false,
            },

            // ============= GPU =============
            new()
            {
                Id = Guid.NewGuid(), Code = "gpu_vram", Name = "VRAM",
                CategoryId = gpuId, Unit = "GB", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Dung lượng bộ nhớ video", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "gpu_tdp", Name = "TDP",
                CategoryId = gpuId, Unit = "W", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Công suất tiêu thụ", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "gpu_length", Name = "Chiều dài card",
                CategoryId = gpuId, Unit = "mm", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Chiều dài vật lý của card (kiểm tra với vỏ case)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "gpu_pcie_slot", Name = "Khe PCIe",
                CategoryId = gpuId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại khe cắm PCIe (VD: PCIe x16)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "gpu_power_connector", Name = "Đầu cắm nguồn",
                CategoryId = gpuId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại đầu cắm nguồn phụ (VD: 1x 8-pin, 1x 16-pin)", IsRequired = true,
            },

            // ============= PSU =============
            new()
            {
                Id = Guid.NewGuid(), Code = "psu_wattage", Name = "Công suất",
                CategoryId = psuId, Unit = "W", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tổng công suất nguồn", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "psu_efficiency", Name = "Hiệu suất",
                CategoryId = psuId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Chứng nhận hiệu suất (VD: 80+ Gold, 80+ Platinum)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "psu_modular", Name = "Modular",
                CategoryId = psuId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Kiểu modular (Full, Semi, Non-modular)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "psu_form_factor", Name = "Form Factor",
                CategoryId = psuId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Kích thước nguồn (ATX, SFX)", IsRequired = true,
            },

            // ============= STORAGE =============
            new()
            {
                Id = Guid.NewGuid(), Code = "stor_type", Name = "Loại ổ cứng",
                CategoryId = storageId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại lưu trữ (NVMe, SATA SSD, HDD)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "stor_capacity", Name = "Dung lượng",
                CategoryId = storageId, Unit = "GB", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Dung lượng lưu trữ (VD: 500, 1000, 2000)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "stor_interface", Name = "Giao tiếp",
                CategoryId = storageId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Kiểu kết nối (M.2, 2.5\", 3.5\")", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "stor_form_factor", Name = "Form Factor",
                CategoryId = storageId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Kích thước vật lý (M.2, 2.5\", 3.5\")", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "stor_read_speed", Name = "Tốc độ đọc",
                CategoryId = storageId, Unit = "MB/s", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tốc độ đọc tuần tự", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "stor_write_speed", Name = "Tốc độ ghi",
                CategoryId = storageId, Unit = "MB/s", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tốc độ ghi tuần tự", IsRequired = false,
            },

            // ============= CASE =============
            new()
            {
                Id = Guid.NewGuid(), Code = "case_form_factor", Name = "Form Factor hỗ trợ",
                CategoryId = caseId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Các kích thước bo mạch hỗ trợ (VD: ATX,mATX,ITX)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "case_max_gpu_length", Name = "Chiều dài GPU tối đa",
                CategoryId = caseId, Unit = "mm", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Chiều dài card đồ họa tối đa cho phép", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "case_max_cooler_height", Name = "Chiều cao tản nhiệt tối đa",
                CategoryId = caseId, Unit = "mm", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Chiều cao tản nhiệt CPU tối đa cho phép", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "case_psu_form_factor", Name = "Form Factor nguồn",
                CategoryId = caseId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Kích thước nguồn hỗ trợ (ATX, SFX)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "case_drive_bays_25", Name = "Khay 2.5\"",
                CategoryId = caseId, Unit = "slots", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số khay ổ cứng 2.5\" cho SATA SSD", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "case_drive_bays_35", Name = "Khay 3.5\"",
                CategoryId = caseId, Unit = "slots", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số khay ổ cứng 3.5\" cho HDD", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "case_max_fan_size", Name = "Kích thước quạt tối đa",
                CategoryId = caseId, Unit = "mm", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Kích thước quạt tối đa hỗ trợ (VD: 120, 140)", IsRequired = false,
            },

            // ============= CPU COOLER =============
            new()
            {
                Id = Guid.NewGuid(), Code = "cooler_type", Name = "Loại tản nhiệt",
                CategoryId = coolerId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Kiểu tản nhiệt (Air, AIO Liquid)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "cooler_socket_support", Name = "Socket hỗ trợ",
                CategoryId = coolerId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Các socket hỗ trợ (VD: LGA1700,AM5)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "cooler_tdp_rating", Name = "TDP hỗ trợ",
                CategoryId = coolerId, Unit = "W", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Công suất nhiệt tối đa hỗ trợ", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "cooler_height", Name = "Chiều cao",
                CategoryId = coolerId, Unit = "mm", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Chiều cao tản nhiệt (kiểm tra với vỏ case)", IsRequired = true,
            },

            // ============= LAPTOP =============
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_cpu", Name = "CPU",
                CategoryId = laptopId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Tên CPU (VD: Intel Core i7-13700H, AMD Ryzen 7 7840HS)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_cpu_cores", Name = "Số nhân CPU",
                CategoryId = laptopId, Unit = "cores", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số nhân vật lý của CPU", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_cpu_threads", Name = "Số luồng CPU",
                CategoryId = laptopId, Unit = "threads", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số luồng xử lý của CPU", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_ram_capacity", Name = "Dung lượng RAM",
                CategoryId = laptopId, Unit = "GB", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tổng dung lượng RAM", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_ram_type", Name = "Loại RAM",
                CategoryId = laptopId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại bộ nhớ (DDR4, DDR5, LPDDR5)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_ram_speed", Name = "Tốc độ RAM",
                CategoryId = laptopId, Unit = "MHz", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tốc độ bus RAM", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_gpu", Name = "Card đồ họa",
                CategoryId = laptopId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Tên GPU (VD: RTX 4060, Radeon RX 7600M, Integrated)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_gpu_vram", Name = "VRAM",
                CategoryId = laptopId, Unit = "GB", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Dung lượng bộ nhớ video (0 nếu dùng GPU tích hợp)", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_storage", Name = "Ổ cứng",
                CategoryId = laptopId, Unit = "GB", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Dung lượng ổ cứng", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_storage_type", Name = "Loại ổ cứng",
                CategoryId = laptopId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại lưu trữ (NVMe SSD, SATA SSD, HDD)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_screen_size", Name = "Kích thước màn hình",
                CategoryId = laptopId, Unit = "inch", AcceptValueType = SpecAcceptValueType.Decimal,
                Description = "Kích thước màn hình (VD: 14, 15.6, 16)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_screen_resolution", Name = "Độ phân giải",
                CategoryId = laptopId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Độ phân giải màn hình (VD: 1920x1080, 2560x1440)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_refresh_rate", Name = "Tần số quét",
                CategoryId = laptopId, Unit = "Hz", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tần số quét màn hình (VD: 60, 120, 144)", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_battery", Name = "Dung lượng pin",
                CategoryId = laptopId, Unit = "Wh", AcceptValueType = SpecAcceptValueType.Decimal,
                Description = "Dung lượng pin", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_weight", Name = "Trọng lượng",
                CategoryId = laptopId, Unit = "kg", AcceptValueType = SpecAcceptValueType.Decimal,
                Description = "Trọng lượng máy", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "lap_os", Name = "Hệ điều hành",
                CategoryId = laptopId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Hệ điều hành đi kèm (VD: Windows 11, No OS)", IsRequired = false,
            },

            // ============= BÀN PHÍM =============
            new()
            {
                Id = Guid.NewGuid(), Code = "kb_type", Name = "Loại bàn phím",
                CategoryId = keyboardId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Cơ học, membrane, quang học", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "kb_switch", Name = "Loại switch",
                CategoryId = keyboardId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại switch (VD: Cherry MX Red, Gateron Brown)", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "kb_layout", Name = "Layout",
                CategoryId = keyboardId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Layout bàn phím (Full-size, TKL, 75%, 65%, 60%)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "kb_connection", Name = "Kết nối",
                CategoryId = keyboardId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Có dây, không dây, Bluetooth, USB 2.4GHz", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "kb_rgb", Name = "LED RGB",
                CategoryId = keyboardId, Unit = "", AcceptValueType = SpecAcceptValueType.Bool,
                Description = "Có đèn LED RGB hay không", IsRequired = false,
            },

            // ============= CHUỘT =============
            new()
            {
                Id = Guid.NewGuid(), Code = "mouse_sensor", Name = "Cảm biến",
                CategoryId = mouseId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại cảm biến (VD: PixArt PAW3395, HERO 25K)", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mouse_dpi", Name = "DPI tối đa",
                CategoryId = mouseId, Unit = "DPI", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Độ nhạy tối đa", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mouse_connection", Name = "Kết nối",
                CategoryId = mouseId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Có dây, không dây, Bluetooth, USB 2.4GHz", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mouse_weight", Name = "Trọng lượng",
                CategoryId = mouseId, Unit = "g", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Trọng lượng chuột", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mouse_buttons", Name = "Số nút",
                CategoryId = mouseId, Unit = "nút", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số nút bấm", IsRequired = false,
            },

            // ============= TAI NGHE =============
            new()
            {
                Id = Guid.NewGuid(), Code = "hs_type", Name = "Kiểu tai nghe",
                CategoryId = headsetId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Over-ear, On-ear, In-ear", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "hs_driver", Name = "Driver",
                CategoryId = headsetId, Unit = "mm", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Kích thước driver", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "hs_connection", Name = "Kết nối",
                CategoryId = headsetId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "3.5mm, USB, Bluetooth, USB 2.4GHz", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "hs_microphone", Name = "Có micro",
                CategoryId = headsetId, Unit = "", AcceptValueType = SpecAcceptValueType.Bool,
                Description = "Có microphone tích hợp hay không", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "hs_noise_cancelling", Name = "Chống ồn",
                CategoryId = headsetId, Unit = "", AcceptValueType = SpecAcceptValueType.Bool,
                Description = "Có chống ồn chủ động (ANC) hay không", IsRequired = false,
            },

            // ============= MÀN HÌNH =============
            new()
            {
                Id = Guid.NewGuid(), Code = "mon_size", Name = "Kích thước",
                CategoryId = monitorId, Unit = "inch", AcceptValueType = SpecAcceptValueType.Decimal,
                Description = "Kích thước màn hình", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mon_resolution", Name = "Độ phân giải",
                CategoryId = monitorId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Độ phân giải (VD: 1920x1080, 2560x1440, 3840x2160)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mon_panel", Name = "Loại tấm nền",
                CategoryId = monitorId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "IPS, VA, TN, OLED", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mon_refresh_rate", Name = "Tần số quét",
                CategoryId = monitorId, Unit = "Hz", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tần số quét (VD: 60, 144, 165, 240)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mon_response_time", Name = "Thời gian phản hồi",
                CategoryId = monitorId, Unit = "ms", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Thời gian phản hồi (GtG)", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mon_ports", Name = "Cổng kết nối",
                CategoryId = monitorId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Các cổng (VD: HDMI, DisplayPort, USB-C)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mon_hdr", Name = "HDR",
                CategoryId = monitorId, Unit = "", AcceptValueType = SpecAcceptValueType.Bool,
                Description = "Hỗ trợ HDR hay không", IsRequired = false,
            },

            // ============= WEBCAM =============
            new()
            {
                Id = Guid.NewGuid(), Code = "wc_resolution", Name = "Độ phân giải",
                CategoryId = webcamId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Độ phân giải (VD: 720p, 1080p, 4K)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "wc_fps", Name = "FPS",
                CategoryId = webcamId, Unit = "fps", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tốc độ khung hình tối đa", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "wc_microphone", Name = "Có micro",
                CategoryId = webcamId, Unit = "", AcceptValueType = SpecAcceptValueType.Bool,
                Description = "Có microphone tích hợp hay không", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "wc_autofocus", Name = "Tự động lấy nét",
                CategoryId = webcamId, Unit = "", AcceptValueType = SpecAcceptValueType.Bool,
                Description = "Có autofocus hay không", IsRequired = false,
            },

            // ============= LOA =============
            new()
            {
                Id = Guid.NewGuid(), Code = "spk_power", Name = "Công suất",
                CategoryId = speakerId, Unit = "W", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tổng công suất loa", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "spk_channels", Name = "Kênh",
                CategoryId = speakerId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Số kênh (VD: 2.0, 2.1, 5.1)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "spk_connection", Name = "Kết nối",
                CategoryId = speakerId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "3.5mm, USB, Bluetooth, RCA", IsRequired = true,
            },

            // ============= LÓT CHUỘT =============
            new()
            {
                Id = Guid.NewGuid(), Code = "mp_size", Name = "Kích thước",
                CategoryId = mousepadId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Kích thước (VD: S, M, L, XL, XXL)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mp_material", Name = "Chất liệu",
                CategoryId = mousepadId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Chất liệu bề mặt (vải, nhựa cứng, kính)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "mp_rgb", Name = "LED RGB",
                CategoryId = mousepadId, Unit = "", AcceptValueType = SpecAcceptValueType.Bool,
                Description = "Có đèn LED RGB hay không", IsRequired = false,
            },

            // ============= THIẾT BỊ MẠNG =============
            new()
            {
                Id = Guid.NewGuid(), Code = "net_type", Name = "Loại thiết bị",
                CategoryId = networkId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Router, Card mạng, USB Wifi, Switch", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "net_speed", Name = "Tốc độ tối đa",
                CategoryId = networkId, Unit = "Mbps", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tốc độ truyền tải tối đa", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "net_wifi_standard", Name = "Chuẩn Wifi",
                CategoryId = networkId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Chuẩn Wifi (VD: Wifi 6, Wifi 6E, Wifi 7)", IsRequired = false,
            },

            // ============= Ổ CỨNG DI ĐỘNG =============
            new()
            {
                Id = Guid.NewGuid(), Code = "ext_type", Name = "Loại",
                CategoryId = extStorageId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "HDD di động, SSD di động, USB Flash, Box ổ cứng", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "ext_capacity", Name = "Dung lượng",
                CategoryId = extStorageId, Unit = "GB", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Dung lượng lưu trữ", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "ext_interface", Name = "Giao tiếp",
                CategoryId = extStorageId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "USB 3.0, USB-C, Thunderbolt", IsRequired = true,
            },

            // ============= BỘ LƯU ĐIỆN =============
            new()
            {
                Id = Guid.NewGuid(), Code = "ups_capacity", Name = "Công suất",
                CategoryId = upsId, Unit = "VA", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Công suất UPS (VA)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "ups_wattage", Name = "Công suất thực",
                CategoryId = upsId, Unit = "W", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Công suất thực (W)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "ups_outlets", Name = "Số ổ cắm",
                CategoryId = upsId, Unit = "ổ", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số ổ cắm đầu ra", IsRequired = false,
            },

            // ============= QUẠT CASE =============
            new()
            {
                Id = Guid.NewGuid(), Code = "fan_size", Name = "Kích thước",
                CategoryId = caseFanId, Unit = "mm", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Kích thước quạt (VD: 120, 140)", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "fan_rpm", Name = "Tốc độ tối đa",
                CategoryId = caseFanId, Unit = "RPM", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Tốc độ quay tối đa", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "fan_airflow", Name = "Lưu lượng gió",
                CategoryId = caseFanId, Unit = "CFM", AcceptValueType = SpecAcceptValueType.Decimal,
                Description = "Lưu lượng gió tối đa", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "fan_rgb", Name = "LED RGB",
                CategoryId = caseFanId, Unit = "", AcceptValueType = SpecAcceptValueType.Bool,
                Description = "Có đèn LED RGB hay không", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "fan_quantity", Name = "Số lượng trong bộ",
                CategoryId = caseFanId, Unit = "pcs", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Số quạt trong bộ sản phẩm", IsRequired = true,
            },

            // ============= CÁP & ĐẦU CHUYỂN =============
            new()
            {
                Id = Guid.NewGuid(), Code = "cable_type", Name = "Loại cáp",
                CategoryId = cableId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "HDMI, DisplayPort, USB-C, USB-A, Ổ cắm điện", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "cable_length", Name = "Chiều dài",
                CategoryId = cableId, Unit = "m", AcceptValueType = SpecAcceptValueType.Decimal,
                Description = "Chiều dài cáp", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "cable_version", Name = "Phiên bản",
                CategoryId = cableId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Phiên bản (VD: HDMI 2.1, DP 1.4, USB 3.2)", IsRequired = false,
            },

            // ============= GHẾ GAMING =============
            new()
            {
                Id = Guid.NewGuid(), Code = "chair_material", Name = "Chất liệu",
                CategoryId = chairId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Da PU, vải lưới, da thật", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "chair_max_weight", Name = "Tải trọng tối đa",
                CategoryId = chairId, Unit = "kg", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Trọng lượng người dùng tối đa", IsRequired = true,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "chair_recline", Name = "Góc ngả tối đa",
                CategoryId = chairId, Unit = "°", AcceptValueType = SpecAcceptValueType.Number,
                Description = "Góc ngả lưng tối đa (VD: 135, 155, 180)", IsRequired = false,
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "chair_armrest", Name = "Tay vịn",
                CategoryId = chairId, Unit = "", AcceptValueType = SpecAcceptValueType.Text,
                Description = "Loại tay vịn (1D, 2D, 3D, 4D)", IsRequired = false,
            },
        };

        context.SpecDefinitions.AddRange(specs);
        await context.SaveChangesAsync();
    }
}