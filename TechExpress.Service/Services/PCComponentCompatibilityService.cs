using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Constants;

namespace TechExpress.Service.Services
{

    public class PCComponentCompatibilityService
    {
        private readonly UnitOfWork _unitOfWork;

        public PCComponentCompatibilityService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string?> GetSpecValueByProductAndCodeAsync(Guid productId, string specCode)
        {
            var specDef = await _unitOfWork.SpecDefinitionRepository.FindByCodeAsync(specCode);
            if (specDef == null) return null;

            var psv = await _unitOfWork.ProductSpecValueRepository.FindByProductIdAndSpecDefinitionIdAsync(productId, specDef.Id);
            if (psv == null) return null;

            return GetSpecValueAsString(psv, specDef.AcceptValueType);
        }

        private static string? GetSpecValueAsString(ProductSpecValue psv, SpecAcceptValueType acceptValueType)
        {
            return acceptValueType switch
            {
                SpecAcceptValueType.Text => psv.TextValue?.Trim(),
                SpecAcceptValueType.Number => psv.NumberValue?.ToString(),
                SpecAcceptValueType.Decimal => psv.DecimalValue?.ToString(),
                SpecAcceptValueType.Bool => psv.BoolValue?.ToString(),
                _ => null
            };
        }

        private static bool ValuesMatch(string? a, string? b)
        {
            if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b)) return true;
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool ValueContains(string? container, string? part)
        {
            if (string.IsNullOrWhiteSpace(container) || string.IsNullOrWhiteSpace(part)) return false;
            var parts = container.Split(',', '/', ';')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s));
            return parts.Any(p => string.Equals(p, part.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static bool TryParseDecimal(string? value, out decimal result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(value)) return false;
            return decimal.TryParse(value.Trim().Replace(",", "."), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseInt(string? value, out int result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(value)) return false;
            return int.TryParse(value.Trim(), out result);
        }

        /// <summary>
        /// Kiểm tra tương thích CPU và Mainboard: cùng giá trị socket (CPU: cpu_socket, Mainboard: mb_socket), vd LGA1700.
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckCpuMainboardCompatibilityAsync(Guid cpuProductId, Guid mainboardProductId)
        {
            var cpuSocket = await GetSpecValueByProductAndCodeAsync(cpuProductId, SpecCodeConstant.CpuSocket);
            var mbSocket = await GetSpecValueByProductAndCodeAsync(mainboardProductId, SpecCodeConstant.MbSocket);

            if (string.IsNullOrWhiteSpace(cpuSocket))
                return (false, "CPU không có thông số socket (cpu_socket).");
            if (string.IsNullOrWhiteSpace(mbSocket))
                return (false, "Mainboard không có thông số socket (mb_socket).");

            if (!ValuesMatch(cpuSocket, mbSocket))
                return (false, $"Socket không khớp: CPU ({cpuSocket}) và Mainboard ({mbSocket}) phải cùng loại (vd. LGA1700).");

            return (true, null);
        }

        /// <summary>
        /// Kiểm tra tương thích Mainboard và RAM: Mainboard mb_memory_type phải trùng với RAM ram_type (value).
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckMainboardRamCompatibilityAsync(Guid mainboardProductId, Guid ramProductId)
        {
            var mbMemoryType = await GetSpecValueByProductAndCodeAsync(mainboardProductId, SpecCodeConstant.MbMemoryType);
            var ramType = await GetSpecValueByProductAndCodeAsync(ramProductId, SpecCodeConstant.RamType);

            if (string.IsNullOrWhiteSpace(mbMemoryType))
                return (false, "Mainboard không có thông số loại RAM (mb_memory_type).");
            if (string.IsNullOrWhiteSpace(ramType))
                return (false, "RAM không có thông số loại (ram_type).");

            if (!ValuesMatch(mbMemoryType, ramType))
                return (false, $"Loại RAM không khớp: Mainboard hỗ trợ ({mbMemoryType}), RAM là ({ramType}).");

            return (true, null);
        }

        /// <summary>
        /// Mainboard và RAM: số lượng khe RAM khách chọn <= mb_memory_slots và tổng dung lượng <= mb_max_memory.
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckMainboardRamSlotsAndCapacityAsync(
            Guid mainboardProductId,
            IReadOnlyList<(Guid RamProductId, int Quantity)> ramComponents)
        {
            if (ramComponents == null || ramComponents.Count == 0)
                return (true, null);

            var mbSlotsStr = await GetSpecValueByProductAndCodeAsync(mainboardProductId, SpecCodeConstant.MbMemorySlots);
            var mbMaxMemoryStr = await GetSpecValueByProductAndCodeAsync(mainboardProductId, SpecCodeConstant.MbMaxMemory);

            if (!TryParseDecimal(mbSlotsStr, out var mbSlotsValue))
                return (false, "Mainboard không có thông số số khe RAM (mb_memory_slots).");
            if (!TryParseDecimal(mbMaxMemoryStr, out var mbMaxMemory))
                return (false, "Mainboard không có thông số dung lượng RAM tối đa (mb_max_memory).");

            var totalRamSticks = ramComponents.Sum(r => r.Quantity);
            if (totalRamSticks > mbSlotsValue)
                return (false, $"Số thanh RAM vượt quá khe Mainboard: Chọn {totalRamSticks} thanh, Mainboard hỗ trợ tối đa {mbSlotsValue} khe.");

            decimal totalCapacity = 0;
            foreach (var (ramProductId, quantity) in ramComponents)
            {
                var ramCapacityStr = await GetSpecValueByProductAndCodeAsync(ramProductId, SpecCodeConstant.RamCapacity);
                if (!TryParseDecimal(ramCapacityStr, out var ramCapacity))
                    return (false, "RAM không có thông số dung lượng (ram_capacity).");
                totalCapacity += ramCapacity * quantity;
            }

            if (totalCapacity > mbMaxMemory)
                return (false, $"Tổng dung lượng RAM vượt quá Mainboard: Chọn {totalCapacity}GB, Mainboard hỗ trợ tối đa {mbMaxMemory}GB.");

            return (true, null);
        }

        /// <summary>
        /// Case và Mainboard: case_form_factor phải chứa mb_form_factor.
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckCaseMainboardCompatibilityAsync(Guid caseProductId, Guid mainboardProductId)
        {
            var caseFormFactor = await GetSpecValueByProductAndCodeAsync(caseProductId, SpecCodeConstant.CaseFormFactor);
            var mbFormFactor = await GetSpecValueByProductAndCodeAsync(mainboardProductId, SpecCodeConstant.MbFormFactor);

            if (string.IsNullOrWhiteSpace(caseFormFactor))
                return (false, "Case (vỏ) không có thông số form factor (case_form_factor).");
            if (string.IsNullOrWhiteSpace(mbFormFactor))
                return (false, "Mainboard không có thông số form factor (mb_form_factor).");

            if (!ValueContains(caseFormFactor, mbFormFactor))
                return (false, $"Case không hỗ trợ form factor của Mainboard: Case ({caseFormFactor}), Mainboard ({mbFormFactor}).");

            return (true, null);
        }

        /// <summary>
        /// Case và VGA: case_max_gpu_length > gpu_length.
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckCaseGpuCompatibilityAsync(Guid caseProductId, Guid gpuProductId)
        {
            var caseMaxGpuLength = await GetSpecValueByProductAndCodeAsync(caseProductId, SpecCodeConstant.CaseMaxGpuLength);
            var gpuLength = await GetSpecValueByProductAndCodeAsync(gpuProductId, SpecCodeConstant.GpuLength);

            if (!TryParseDecimal(caseMaxGpuLength, out var caseLen))
                return (false, "Case (vỏ) không có thông số chiều dài VGA tối đa (case_max_gpu_length).");
            if (!TryParseDecimal(gpuLength, out var gpuLen))
                return (false, "VGA không có thông số chiều dài (gpu_length).");

            if (caseLen <= gpuLen)
                return (false, $"Case không đủ dài cho VGA: Case hỗ trợ tối đa {caseLen}mm, VGA dài {gpuLen}mm.");

            return (true, null);
        }

        /// <summary>
        /// Case và Tản nhiệt: case_max_cooler_height > cooler_height.
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckCaseCoolerCompatibilityAsync(Guid caseProductId, Guid coolerProductId)
        {
            var caseMaxCoolerHeight = await GetSpecValueByProductAndCodeAsync(caseProductId, SpecCodeConstant.CaseMaxCoolerHeight);
            var coolerHeight = await GetSpecValueByProductAndCodeAsync(coolerProductId, SpecCodeConstant.CoolerHeight);

            if (!TryParseDecimal(caseMaxCoolerHeight, out var caseHeight))
                return (false, "Case (vỏ) không có thông số chiều cao tản nhiệt tối đa (case_max_cooler_height).");
            if (!TryParseDecimal(coolerHeight, out var coolerH))
                return (false, "Tản nhiệt không có thông số chiều cao (cooler_height).");

            if (caseHeight < coolerH)
                return (false, $"Case không đủ cao cho tản nhiệt: Case hỗ trợ tối đa {caseHeight}mm, Tản nhiệt cao {coolerH}mm.");

            return (true, null);
        }

        /// <summary>
        /// CPU và Tản nhiệt: cooler_socket_support phải chứa cpu_socket.
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckCpuCoolerCompatibilityAsync(Guid cpuProductId, Guid coolerProductId)
        {
            var cpuSocket = await GetSpecValueByProductAndCodeAsync(cpuProductId, SpecCodeConstant.CpuSocket);
            var coolerSocketSupport = await GetSpecValueByProductAndCodeAsync(coolerProductId, SpecCodeConstant.CoolerSocketSupport);

            if (string.IsNullOrWhiteSpace(cpuSocket))
                return (false, "CPU không có thông số socket (cpu_socket).");
            if (string.IsNullOrWhiteSpace(coolerSocketSupport))
                return (false, "Tản nhiệt không có thông số socket hỗ trợ (cooler_socket_support).");

            if (!ValueContains(coolerSocketSupport, cpuSocket))
                return (false, $"Tản nhiệt không hỗ trợ socket của CPU: CPU ({cpuSocket}), Tản nhiệt hỗ trợ ({coolerSocketSupport}).");

            return (true, null);
        }

        /// <summary>
        /// CPU và Mainboard: loại RAM CPU hỗ trợ phải trùng loại RAM Mainboard sử dụng.
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckCpuMainboardMemoryTypeCompatibilityAsync(Guid cpuProductId, Guid mainboardProductId)
        {
            var cpuMemoryType = await GetSpecValueByProductAndCodeAsync(cpuProductId, SpecCodeConstant.CpuMemoryType);
            var mbMemoryType = await GetSpecValueByProductAndCodeAsync(mainboardProductId, SpecCodeConstant.MbMemoryType);

            // Nếu thiếu spec thì bỏ qua check này để không phá dữ liệu cũ
            if (string.IsNullOrWhiteSpace(cpuMemoryType) || string.IsNullOrWhiteSpace(mbMemoryType))
                return (true, null);

            if (!ValuesMatch(cpuMemoryType, mbMemoryType))
                return (false, $"Loại RAM CPU hỗ trợ ({cpuMemoryType}) không khớp với loại RAM của Mainboard ({mbMemoryType}).");

            return (true, null);
        }

        /// <summary>
        /// CPU và Tản nhiệt: công suất tản nhiệt (cooler_tdp_rating) phải >= TDP CPU (cpu_tdp).
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckCpuCoolerTdpCompatibilityAsync(Guid cpuProductId, Guid coolerProductId)
        {
            var cpuTdpStr = await GetSpecValueByProductAndCodeAsync(cpuProductId, SpecCodeConstant.CpuTdp);
            var coolerTdpStr = await GetSpecValueByProductAndCodeAsync(coolerProductId, SpecCodeConstant.CoolerTdpRating);

            if (!TryParseDecimal(cpuTdpStr, out var cpuTdp) || !TryParseDecimal(coolerTdpStr, out var coolerTdp))
                return (true, null);

            if (coolerTdp < cpuTdp)
                return (false, $"Công suất tản nhiệt không đủ: CPU TDP {cpuTdp}W, tản nhiệt hỗ trợ tối đa {coolerTdp}W.");

            return (true, null);
        }

        /// <summary>
        /// PSU và Case: case_psu_form_factor phải chứa psu_form_factor.
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckPsuCaseCompatibilityAsync(Guid psuProductId, Guid caseProductId)
        {
            var psuFormFactor = await GetSpecValueByProductAndCodeAsync(psuProductId, SpecCodeConstant.PsuFormFactor);
            var casePsuFormFactor = await GetSpecValueByProductAndCodeAsync(caseProductId, SpecCodeConstant.CasePsuFormFactor);

            // Nếu thiếu spec thì bỏ qua check này để không phá dữ liệu cũ
            if (string.IsNullOrWhiteSpace(psuFormFactor) || string.IsNullOrWhiteSpace(casePsuFormFactor))
                return (true, null);

            if (!ValueContains(casePsuFormFactor, psuFormFactor))
                return (false, $"Case không hỗ trợ chuẩn nguồn: PSU ({psuFormFactor}), Case hỗ trợ ({casePsuFormFactor}).");

            return (true, null);
        }

        /// <summary>
        /// Case và Storage (2.5"/3.5"): tổng số ổ 2.5" và 3.5" không vượt quá số bay tương ứng trên case.
        /// Ổ M.2 được bỏ qua vì đã được check bằng khe M.2 của mainboard.
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckCaseStorageBayCompatibilityAsync(
            Guid caseProductId,
            IReadOnlyList<(Guid StorageProductId, int Quantity)> storageComponents)
        {
            if (storageComponents == null || storageComponents.Count == 0)
                return (true, null);

            var caseDriveBays25Str = await GetSpecValueByProductAndCodeAsync(caseProductId, SpecCodeConstant.CaseDriveBays25);
            var caseDriveBays35Str = await GetSpecValueByProductAndCodeAsync(caseProductId, SpecCodeConstant.CaseDriveBays35);

            // Nếu thiếu thông số bay, bỏ qua check để không phá dữ liệu cũ
            if (!TryParseInt(caseDriveBays25Str, out var caseDriveBays25) &&
                !TryParseInt(caseDriveBays35Str, out var caseDriveBays35))
            {
                return (true, null);
            }

            int required25 = 0;
            int required35 = 0;

            foreach (var (storageProductId, quantity) in storageComponents)
            {
                var storInterface = await GetSpecValueByProductAndCodeAsync(storageProductId, SpecCodeConstant.StorInterface);
                if (!string.IsNullOrWhiteSpace(storInterface) &&
                    storInterface.Contains("M.2", StringComparison.OrdinalIgnoreCase))
                {
                    // Ổ M.2 dùng khe M.2 trên mainboard, không chiếm bay 2.5"/3.5"
                    continue;
                }

                var storType = await GetSpecValueByProductAndCodeAsync(storageProductId, SpecCodeConstant.StorType);
                if (string.IsNullOrWhiteSpace(storType))
                    continue;

                var type = storType.Trim();

                if (type.Contains("2.5", StringComparison.OrdinalIgnoreCase))
                {
                    required25 += quantity;
                }
                else if (type.Contains("3.5", StringComparison.OrdinalIgnoreCase))
                {
                    required35 += quantity;
                }
            }

            if (TryParseInt(caseDriveBays25Str, out caseDriveBays25) && required25 > caseDriveBays25)
            {
                return (false,
                    $"Số lượng ổ 2.5\" vượt quá số bay của Case: chọn {required25} ổ, Case hỗ trợ tối đa {caseDriveBays25} bay 2.5\".");
            }

            if (TryParseInt(caseDriveBays35Str, out caseDriveBays35) && required35 > caseDriveBays35)
            {
                return (false,
                    $"Số lượng ổ 3.5\" vượt quá số bay của Case: chọn {required35} ổ, Case hỗ trợ tối đa {caseDriveBays35} bay 3.5\".");
            }

            return (true, null);
        }

        /// <summary>
        /// Mainboard và Storage: số ổ M.2 (NVMe) không vượt quá mb_m2_slots.
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckMainboardM2StorageCompatibilityAsync(
            Guid mainboardProductId,
            IReadOnlyList<(Guid StorageProductId, int Quantity)> storageComponents)
        {
            if (storageComponents == null || storageComponents.Count == 0)
                return (true, null);

            var mbM2SlotsStr = await GetSpecValueByProductAndCodeAsync(mainboardProductId, SpecCodeConstant.MbM2Slots);
            if (!TryParseDecimal(mbM2SlotsStr, out var mbM2Slots) || mbM2Slots <= 0)
                return (true, null);

            decimal usedM2Slots = 0;
            foreach (var (storageProductId, quantity) in storageComponents)
            {
                var storInterface = await GetSpecValueByProductAndCodeAsync(storageProductId, SpecCodeConstant.StorInterface);
                if (string.IsNullOrWhiteSpace(storInterface))
                    continue;

                if (storInterface.Contains("M.2", StringComparison.OrdinalIgnoreCase))
                {
                    usedM2Slots += quantity;
                }
            }

            if (usedM2Slots > mbM2Slots)
                return (false, $"Số lượng ổ M.2 ({usedM2Slots}) vượt quá số khe M.2 trên Mainboard ({mbM2Slots}).");

            return (true, null);
        }

        /// <summary>
        /// PSU và cả hệ thống: psu_wattage > tổng gpu_tdp + tổng cpu_tdp + 100.
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckPsuSystemCompatibilityAsync(Guid psuProductId, IReadOnlyList<Guid> cpuIds, IReadOnlyList<Guid> gpuIds)
        {
            var psuWattageStr = await GetSpecValueByProductAndCodeAsync(psuProductId, SpecCodeConstant.PsuWattage);
            if (!TryParseDecimal(psuWattageStr, out var psuWattage))
                return (false, "Nguồn không có thông số công suất (psu_wattage).");

            decimal totalCpuTdp = 0;
            foreach (var cpuId in cpuIds)
            {
                var tdp = await GetSpecValueByProductAndCodeAsync(cpuId, SpecCodeConstant.CpuTdp);
                if (TryParseDecimal(tdp, out var v)) totalCpuTdp += v;
            }

            decimal totalGpuTdp = 0;
            foreach (var gpuId in gpuIds)
            {
                var tdp = await GetSpecValueByProductAndCodeAsync(gpuId, SpecCodeConstant.GpuTdp);
                if (TryParseDecimal(tdp, out var v)) totalGpuTdp += v;
            }

            var required = totalCpuTdp + totalGpuTdp + 100;
            if (psuWattage < required)
                return (false, $"Nguồn không đủ công suất: PSU {psuWattage}W, cần tối thiểu {required}W (CPU {totalCpuTdp}W + GPU {totalGpuTdp}W + 100W dự phòng).");

            return (true, null);
        }

        /// <summary>
        /// Hệ thống đồ họa: cấu hình phải có ít nhất một GPU rời hoặc CPU có iGPU.
        /// </summary>
        public async Task<(bool IsCompatible, string? Message)> CheckGraphicsAvailabilityAsync(
            IReadOnlyList<Guid> cpuIds,
            IReadOnlyList<Guid> gpuIds)
        {
            // Nếu đã có GPU rời thì chắc chắn có hiển thị
            if (gpuIds != null && gpuIds.Count > 0)
                return (true, null);

            if (cpuIds == null || cpuIds.Count == 0)
                return (true, null);

            var anyCpuWithSpec = false;
            var anyCpuWithIntegratedGpu = false;

            foreach (var cpuId in cpuIds)
            {
                var iGpuStr = await GetSpecValueByProductAndCodeAsync(cpuId, SpecCodeConstant.CpuIntegratedGpu);
                if (string.IsNullOrWhiteSpace(iGpuStr))
                    continue;

                anyCpuWithSpec = true;

                var normalized = iGpuStr.Trim().ToLowerInvariant();

                if (normalized is "true" or "1" ||
                    normalized.Contains("yes") ||
                    normalized.Contains("có"))
                {
                    anyCpuWithIntegratedGpu = true;
                    break;
                }

            }

            if (!anyCpuWithSpec)
                return (true, null);

            if (!anyCpuWithIntegratedGpu)
            {
                return (false,
                    "Cấu hình không có card đồ họa rời và CPU cũng không tích hợp iGPU. Máy sẽ không xuất được hình ảnh.");
            }

            return (true, null);
        }

        /// <summary>
        /// Kết quả validate tương thích: có tương thích hay không và danh sách lỗi (nếu có).
        /// </summary>
        public record ValidationResult(bool IsCompatible, IReadOnlyList<string> Errors);

        /// <summary>
    
        /// Ma trận tương thích 
        /// </summary>
        public async Task<ValidationResult> ValidatePcComponentsAsync(IReadOnlyList<(Guid ComponentProductId, int Quantity)> components)
        {
            if (components == null || components.Count == 0)
                return new ValidationResult(true, []);

            var componentProductIds = components.Select(c => c.ComponentProductId).Distinct().ToList();
            var products = await _unitOfWork.ProductRepository.FindByIdsIncludeCategoryAsync(componentProductIds);
            var errors = new List<string>();

            var cpuIds = products.Where(p => p.Category?.Name == CategoryNameConstant.CPU).Select(p => p.Id).ToList();
            var mainboardIds = products.Where(p => p.Category?.Name == CategoryNameConstant.Motherboard).Select(p => p.Id).ToList();
            var ramIds = products.Where(p => p.Category?.Name == CategoryNameConstant.RAM).Select(p => p.Id).ToHashSet();
            var ramComponents = components.Where(c => ramIds.Contains(c.ComponentProductId)).ToList();
            var caseIds = products.Where(p => p.Category?.Name == CategoryNameConstant.Case).Select(p => p.Id).ToList();
            var gpuIds = products.Where(p => p.Category?.Name == CategoryNameConstant.GPU).Select(p => p.Id).ToList();
            var coolerIds = products.Where(p => p.Category?.Name == CategoryNameConstant.CpuCooler).Select(p => p.Id).ToList();
            var psuIds = products.Where(p => p.Category?.Name == CategoryNameConstant.PSU).Select(p => p.Id).ToList();
            var storageIds = products.Where(p => p.Category?.Name == CategoryNameConstant.Storage).Select(p => p.Id).ToHashSet();
            var storageComponents = components.Where(c => storageIds.Contains(c.ComponentProductId)).ToList();

            foreach (var cpuId in cpuIds)
            {
                foreach (var mbId in mainboardIds)
                {
                    var (ok, msg) = await CheckCpuMainboardCompatibilityAsync(cpuId, mbId);
                    if (!ok && !string.IsNullOrEmpty(msg)) errors.Add(msg);

                    var (memOk, memMsg) = await CheckCpuMainboardMemoryTypeCompatibilityAsync(cpuId, mbId);
                    if (!memOk && !string.IsNullOrEmpty(memMsg)) errors.Add(memMsg);
                }
                foreach (var coolerId in coolerIds)
                {
                    var (ok, msg) = await CheckCpuCoolerCompatibilityAsync(cpuId, coolerId);
                    if (!ok && !string.IsNullOrEmpty(msg)) errors.Add(msg);

                    var (tdpOk, tdpMsg) = await CheckCpuCoolerTdpCompatibilityAsync(cpuId, coolerId);
                    if (!tdpOk && !string.IsNullOrEmpty(tdpMsg)) errors.Add(tdpMsg);
                }
            }

            foreach (var mbId in mainboardIds)
            {
                foreach (var ramId in ramIds)
                {
                    var (ok, msg) = await CheckMainboardRamCompatibilityAsync(mbId, ramId);
                    if (!ok && !string.IsNullOrEmpty(msg)) errors.Add(msg);
                }
                var (slotsOk, slotsMsg) = await CheckMainboardRamSlotsAndCapacityAsync(mbId, ramComponents);
                if (!slotsOk && !string.IsNullOrEmpty(slotsMsg)) errors.Add(slotsMsg);

                var (m2Ok, m2Msg) = await CheckMainboardM2StorageCompatibilityAsync(mbId, storageComponents);
                if (!m2Ok && !string.IsNullOrEmpty(m2Msg)) errors.Add(m2Msg);

                foreach (var caseId in caseIds)
                {
                    var (ok, msg) = await CheckCaseMainboardCompatibilityAsync(caseId, mbId);
                    if (!ok && !string.IsNullOrEmpty(msg)) errors.Add(msg);
                }
            }

            foreach (var caseId in caseIds)
            {
                foreach (var gpuId in gpuIds)
                {
                    var (ok, msg) = await CheckCaseGpuCompatibilityAsync(caseId, gpuId);
                    if (!ok && !string.IsNullOrEmpty(msg)) errors.Add(msg);
                }
                foreach (var coolerId in coolerIds)
                {
                    var (ok, msg) = await CheckCaseCoolerCompatibilityAsync(caseId, coolerId);
                    if (!ok && !string.IsNullOrEmpty(msg)) errors.Add(msg);
                }
                foreach (var psuId in psuIds)
                {
                    var (ok, msg) = await CheckPsuCaseCompatibilityAsync(psuId, caseId);
                    if (!ok && !string.IsNullOrEmpty(msg)) errors.Add(msg);
                }
            }

            foreach (var caseId in caseIds)
            {
                var (ok, msg) = await CheckCaseStorageBayCompatibilityAsync(caseId, storageComponents);
                if (!ok && !string.IsNullOrEmpty(msg)) errors.Add(msg);
            }

            var (graphicsOk, graphicsMsg) = await CheckGraphicsAvailabilityAsync(cpuIds, gpuIds);
            if (!graphicsOk && !string.IsNullOrEmpty(graphicsMsg)) errors.Add(graphicsMsg);

            foreach (var psuId in psuIds)
            {
                var (ok, msg) = await CheckPsuSystemCompatibilityAsync(psuId, cpuIds, gpuIds);
                if (!ok && !string.IsNullOrEmpty(msg)) errors.Add(msg);
            }

            return new ValidationResult(errors.Count == 0, errors);
        }
    }
}
