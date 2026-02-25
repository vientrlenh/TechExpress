using System;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Commands;
using TechExpress.Service.Constants;

namespace TechExpress.Service.Services;

public class ComputerCompatibilityService
{
    private readonly UnitOfWork _unitOfWork;

    public ComputerCompatibilityService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Product>> GetComponentProductsFromRequestedIds(List<Guid> ids)
    {
        var components = await _unitOfWork.ProductRepository.FindByIdsIncludeCategoryAsync(ids);

        var foundIds = components.Where(p => p.Status != ProductStatus.Unavailable).Select(p => p.Id).ToHashSet();
        var missingIds = ids.Where(id => !foundIds.Contains(id)).ToList();

        if (missingIds.Count > 0)
        {
            var missingProductIds = string.Join(", ", missingIds);
            throw new BadRequestException($"Sản phẩm {missingProductIds} không tồn tại hoặc đã ngưng bán.");
        }
        return components;
    }

    public async Task<List<string>> CheckComputerCompatibility(List<AddComputerComponentCommand> componentCommands, List<Product> components)
    {
        var warnings = new List<string>();

        // Lấy map sản phẩm yêu cầu được group bởi tên của Category
        var categoryMap = BuildComponentCategoryDict(componentCommands, components);

        // Batch fetching các spec của tất cả sản phẩm trong db và group lại dựa trên id của sản phẩm
        var componentProducts = categoryMap.SelectMany(c => c.Value).Select(c => c.Product).ToList();
        var componentSpecDicts = await GetSpecDictsOnComponents(componentProducts);

        // Lấy spec của mainboard trong map
        var mbSpec = ValidateAndGetMotherboardSpec(categoryMap, componentSpecDicts);
        var isDualSocketMb = IsDualSocketMotherboard(mbSpec);
        
        // Lấy spec của CPU trong map
        var cpuSpec = ValidateAndGetCpuSpec(categoryMap, componentSpecDicts, isDualSocketMb);
        var cpuRequestedQuantity = categoryMap[CategoryNameConstant.CPU].Sum(c => c.Command.Quantity);
        int cpuTdp = GetCpuTdp(cpuSpec, cpuRequestedQuantity);

        // Kiểm tra tương thích giữa CPU và mainboard
        CheckCompatibilityBetweenCpuAndMotherboard(cpuSpec, mbSpec);
        

        // Lấy spec của RAM trong map
        var ramSpec = ValidateAndGetRamSpec(categoryMap, componentSpecDicts);
        var ramRequestedQuantity = categoryMap[CategoryNameConstant.RAM].Sum(r => r.Command.Quantity);

        // Kiểm tra tương thích giữa mainboard và RAM
        CheckCompatibilityBetweenMotherboardAndRam(mbSpec, ramSpec, ramRequestedQuantity);

        // Kiểm tra tương thích giữa mainboard + CPU và RAM nếu hỗ trợ ECC
        var isCpuAndMotherboardSupportEcc = IsCpuAndMotherboardSupportEcc(cpuSpec, mbSpec);
        CheckRamIsEccWithSupportedCpuAndMotherboard(ramSpec, isCpuAndMotherboardSupportEcc);
        CheckRamRegisteredMemoryWithSupportedCpuAndMotherboard(ramSpec, isCpuAndMotherboardSupportEcc);
        
        // Kiểm tra và lấy spec của Storage
        ValidateStorageFromRequest(categoryMap);
        (var totalM2Count, var totalSataCount) = GetM2CountAndSataCountFromStorageRequest(categoryMap, componentSpecDicts);
        CheckStorageCountsWithMotherboard(totalM2Count, totalSataCount, mbSpec);

        // Kiểm tra và lấy spec của PSU
        var psuSpec = ValidateAndGetPsuSpec(categoryMap, componentSpecDicts);

        // Kiểm tra và lấy spec của Tản nhiệt CPU
        var cpuCoolerSpec = ValidateAndGetCpuCoolerSpec(categoryMap, componentSpecDicts, isDualSocketMb, cpuRequestedQuantity);

        // Kiểm tra tương thích giữa CPU và Tản nhiệt CPU
        CheckCompatibilityBetweenCpuAndCpuCooler(cpuSpec, cpuCoolerSpec);

        // Kiểm tra và lấy spec của Case (nếu có)
        var caseSpec = ValidateAndGetCaseSpec(categoryMap, componentSpecDicts);
        if (caseSpec != null)
        {
            CheckCasePsuFormFactorWithPsuFormFactor(caseSpec, psuSpec);
            CheckCaseMaxCoolerHeightSupportCpuCoolerHeight(caseSpec, cpuCoolerSpec);
            CheckCaseFormFactorSupportMainboardFormFactor(caseSpec, mbSpec);

            // Kiểm tra số khay ổ cứng của case có đủ không
            (var total25Count, var total35Count) = GetDriveBayCountsFromStorageRequest(categoryMap, componentSpecDicts);
            CheckCaseDriveBaysWithStorage(caseSpec, total25Count, total35Count);
        }

        // Lấy spec của GPU trong map (nếu có)
        int totalGpuTdp = 0;
        int gpuQuantity = 0;
        if (categoryMap.TryGetValue(CategoryNameConstant.GPU, out var gpuDatas))
        {
            gpuQuantity = gpuDatas.Sum(g => g.Command.Quantity);
            CheckGpuQuantityWithMotherboardPClex16Slots(gpuQuantity, mbSpec);
            foreach (var gpuData in gpuDatas)
            {
                var gpuSpec = componentSpecDicts[gpuData.Product.Id];
                var gpuTdp = GetGpuTdp(gpuSpec, gpuData);
                totalGpuTdp += gpuTdp;

                if (caseSpec != null)
                {
                    CheckCaseMaxGpuLengthWithGpuLength(caseSpec, gpuSpec);
                }
                CheckMainboardGpuPcieVersionCompatibility(mbSpec, gpuSpec, warnings);
            }
        }
        // Kiểm tra nếu CPU trong cấu hình không có GPU tích hợp
        CheckCpuContainsIGpu(cpuSpec, gpuDatas);

        // Kiểm tra tổng số làn PCIe sử dụng có vượt quá giới hạn của mainboard không
        CheckPcieLaneBudget(mbSpec, gpuQuantity, totalM2Count);

        // Kiểm tra lượng điện cung cấp của PSU có tương thích không
        CheckPsuPowerSupplyIsEnough(psuSpec, totalGpuTdp, cpuTdp);

        return warnings;
    }

    private static Dictionary<string, List<ComponentData>> BuildComponentCategoryDict(List<AddComputerComponentCommand> commands, List<Product> products)
    {
        var componentData = commands.Select(c => new ComponentData(
            c,
            products.First(p => p.Id == c.ComponentId)
        ));
        return componentData.GroupBy(x => x.Product.Category.Name).ToDictionary(g => g.Key, g => g.ToList());
    }

    private async Task<Dictionary<Guid, Dictionary<string, string>>> GetSpecDictsOnComponents(List<Product> componentProducts)
    {
        var categoryIds = componentProducts.Select(p => p.CategoryId).ToList();
        var allSpecDefs = await _unitOfWork.SpecDefinitionRepository.FindDictByCategoryIdsAndIsNotDeletedAsync(categoryIds);

        var productIds = componentProducts.Select(p => p.Id).ToList();
        var allSpecValues = await _unitOfWork.ProductSpecValueRepository.FindByProductIdsAsync(productIds);
        var specValueByProduct = allSpecValues.ToLookup(s => s.ProductId);

        Dictionary<Guid, Dictionary<string, string>> specDicts = [];
        foreach (var componentProduct in componentProducts)
        {
            Dictionary<string, string> specDict = [];
            var categorySpecDict = allSpecDefs[componentProduct.CategoryId];
            var productSpecValues = specValueByProduct[componentProduct.Id];

            foreach (var specValue in productSpecValues)
            {
                if (categorySpecDict.TryGetValue(specValue.SpecDefinitionId, out var spec))
                {
                    GetValueForSpecDictOnComponent(spec, specValue, specDict);
                }
            }
            specDicts[componentProduct.Id] = specDict;
        }
        return specDicts;
    }

    private static void GetValueForSpecDictOnComponent(SpecDefinition spec, ProductSpecValue specValue, Dictionary<string, string> componentSpecDict) 
    {
        switch (spec.AcceptValueType)
        {
            case SpecAcceptValueType.Text:
                if (!string.IsNullOrWhiteSpace(specValue.TextValue)) {
                    componentSpecDict.Add(spec.Code, specValue.TextValue);
                }
                break;
            case SpecAcceptValueType.Number:
                if (specValue.NumberValue.HasValue) {
                    componentSpecDict.Add(spec.Code, specValue.NumberValue.Value.ToString());
                }
                break;
            case SpecAcceptValueType.Decimal:
                if (specValue.DecimalValue.HasValue) {
                    componentSpecDict.Add(spec.Code, specValue.DecimalValue.Value.ToString());
                }
                break;
            case SpecAcceptValueType.Bool:
                if (specValue.BoolValue.HasValue) {
                    componentSpecDict.Add(spec.Code, specValue.BoolValue.Value.ToString());
                }
                break;
        }
    }

    private static Dictionary<string, string> ValidateAndGetMotherboardSpec(Dictionary<string, List<ComponentData>> categoryMap, Dictionary<Guid, Dictionary<string, string>> componentSpecDicts)
    {
        if (!categoryMap.TryGetValue(CategoryNameConstant.Motherboard, out var mbData))
        {
            throw new BadRequestException("Cấu hình đang thiếu mainboard");
        }
        var mbRequestedQuantity = mbData.Sum(m => m.Command.Quantity);
        if (mbRequestedQuantity != 1)
        {
            throw new BadRequestException($"Số lượng mainboard phải bằng 1.");
        }
        if (!componentSpecDicts.TryGetValue(categoryMap[CategoryNameConstant.Motherboard].First().Product.Id, out var mbSpec))
        {
            throw new NotFoundException("Không tìm thấy thông số của mainboard.");
        }
        return mbSpec;
    }

    private static bool IsDualSocketMotherboard(Dictionary<string, string> mbSpec)
    {
        if (mbSpec.TryGetValue(SpecCodeConstant.MbDualSocket, out var mbDualSocketValue))
        {
            if (!bool.TryParse(mbDualSocketValue, out var mbDualSocket))
            {
                throw new BadRequestException("Giá trị hiện tại của dual socket của mainboard không hợp lệ để kiểm tra tương thích.");
            }
            if (mbDualSocket) return true;
        }
        return false;
    }

    private static Dictionary<string, string> ValidateAndGetCpuSpec(Dictionary<string, List<ComponentData>> categoryMap, Dictionary<Guid, Dictionary<string, string>> componentSpecDicts, bool isDualSocketMb) {
        if (!categoryMap.TryGetValue(CategoryNameConstant.CPU, out var cpuDatas))
        {
            throw new BadRequestException("Cấu hình đang thiếu CPU.");
        }
        if (isDualSocketMb)
        {
            var totalCpu = cpuDatas.Sum(c => c.Command.Quantity);
            if (totalCpu > 2)
            {
                throw new BadRequestException("Số lượng CPU tối đa cho phép đối với dual socket mainboard là 2.");
            }
            if (cpuDatas.Select(c => c.Command.ComponentId).Distinct().Count() > 1)
            {
                throw new BadRequestException("CPU phải cùng loại");
            }
        }
        else
        {
            var totalCpu = cpuDatas.Sum(c => c.Command.Quantity);
            if (totalCpu != 1)
            {
                throw new BadRequestException("Số lượng CPU phải bằng 1.");
            }
        }
        if (!componentSpecDicts.TryGetValue(categoryMap[CategoryNameConstant.CPU].First().Product.Id, out var cpuSpec))
        {
            throw new NotFoundException("Không tìm thấy thông số của CPU.");
        }
        return cpuSpec;
    }

    private static Dictionary<string, string> ValidateAndGetRamSpec(Dictionary<string, List<ComponentData>> categoryMap, Dictionary<Guid, Dictionary<string, string>> componentSpecDicts)
    {
        if (!categoryMap.TryGetValue(CategoryNameConstant.RAM, out var ramData))
        {
            throw new BadRequestException("Cấu hình đang thiếu RAM");
        }
        var ramRequestedQuantity = ramData.Sum(r => r.Command.Quantity);
        if (ramRequestedQuantity < 1)
        {
            throw new BadRequestException("Số lượng RAM phải lớn hơn 0.");
        }
        if (ramData.Select(r => r.Product.Id).Distinct().Count() > 1)
        {
            throw new BadRequestException("RAM phải cùng loại.");
        }
        if (!componentSpecDicts.TryGetValue(categoryMap[CategoryNameConstant.RAM].First().Product.Id, out var ramSpec))
        {
            throw new NotFoundException("Không tìm thấy thông số của RAM");
        }
        return ramSpec;
    }

    private static Dictionary<string, string> ValidateAndGetPsuSpec(Dictionary<string, List<ComponentData>> categoryMap, Dictionary<Guid, Dictionary<string, string>> componentSpecDicts)
    {
        if (!categoryMap.TryGetValue(CategoryNameConstant.PSU, out var psuData))
        {
            throw new BadRequestException("Cấu hình đang thiếu PSU.");
        }
        var psuRequestedQuantity = psuData.Sum(p => p.Command.Quantity);
        if (psuRequestedQuantity != 1)
        {
            throw new BadRequestException("Số lượng PSU phải bằng 1.");
        }
        if (!componentSpecDicts.TryGetValue(psuData.First().Product.Id, out var psuSpec))
        {
            throw new NotFoundException("Không tìm thấy thông số của PSU.");
        }
        return psuSpec;
    }

    private static Dictionary<string, string> ValidateAndGetCpuCoolerSpec(Dictionary<string, List<ComponentData>> categoryMap, Dictionary<Guid, Dictionary<string, string>> componentSpecDicts, bool isDualSocketMb, int cpuRequestedQuantity)
    {
        if (!categoryMap.TryGetValue(CategoryNameConstant.CpuCooler, out var coolerData))
        {
            throw new BadRequestException("Cấu hình đang thiếu tản nhiệt CPU.");
        }
        var coolerQuantity = coolerData.Sum(c => c.Command.Quantity);
        var requiredCoolers = 1;
        if (isDualSocketMb && cpuRequestedQuantity == 2)
        {
            requiredCoolers = 2;
        }
        if (coolerQuantity != requiredCoolers)
        {
            throw new BadRequestException($"Số lượng tản nhiệt CPU phải bằng {requiredCoolers}.");
        }
        if (!componentSpecDicts.TryGetValue(coolerData.First().Product.Id, out var coolerSpec))
        {
            throw new NotFoundException("Không tìm thấy thông số của tản nhiệt CPU.");
        }
        return coolerSpec;
    }

    private static Dictionary<string, string>? ValidateAndGetCaseSpec(Dictionary<string, List<ComponentData>> categoryMap, Dictionary<Guid, Dictionary<string, string>> componentSpecDicts)
    {
        if (!categoryMap.TryGetValue(CategoryNameConstant.Case, out var caseData))
        {
            return null;
        }
        var caseRequestedQuantity = caseData.Sum(c => c.Command.Quantity);
        if (caseRequestedQuantity != 1)
        {
            throw new BadRequestException("Số lượng case phải bằng 1.");
        }
        if (!componentSpecDicts.TryGetValue(caseData.First().Product.Id, out var caseSpec))
        {
            throw new NotFoundException("Không tìm thấy thông số của Case.");
        }
        return caseSpec;
    }

    private static void ValidateStorageFromRequest(Dictionary<string, List<ComponentData>> categoryMap)
    {
        if (!categoryMap.TryGetValue(CategoryNameConstant.Storage, out var storageData))
        {
            throw new BadRequestException("Cấu hình đang thiếu ổ cứng.");
        }
        var storageRequestedQuantity = storageData.Sum(s => s.Command.Quantity);
        if (storageRequestedQuantity < 1)
        {
            throw new BadRequestException("Số lượng ổ cứng phải lớn hơn 0.");
        }
    }

    private static void CheckGpuQuantityWithMotherboardPClex16Slots(int gpuQuantity, Dictionary<string, string> mbSpec)
    {
        if (!mbSpec.TryGetValue(SpecCodeConstant.MbPcieX16Slots, out var mbPciex16SlotValue))
        {
            throw new NotFoundException("Không tìm thấy thông số của số khe cắm Pcie cho mainboard.");
        }
        if (!int.TryParse(mbPciex16SlotValue, out var mbPciex16Slots))
        {
            throw new NotFoundException("Giá trị của số slot Pcie của mainboard hiện tại không phù hợp để kiểm tra tương thích.");
        }
        if (gpuQuantity > mbPciex16Slots)
        {
            throw new BadRequestException("Số lượng GPU vượt quá số khe cắm Pcie mà mainboard hỗ trợ");
        }
    }

    private static void CheckCompatibilityBetweenCpuAndMotherboard(Dictionary<string, string> cpuSpec, Dictionary<string, string> mbSpec)
    {
        CheckCpuSocketWithMbSocket(cpuSpec, mbSpec);
        CheckCpuMemoryTypeWithMbMemoryType(cpuSpec, mbSpec);
    }

    private static void CheckCpuSocketWithMbSocket(Dictionary<string, string> cpuSpec, Dictionary<string, string> mbSpec)
    {
        if (!cpuSpec.TryGetValue(SpecCodeConstant.CpuSocket, out var cpuSocketValue))
        {
            throw new NotFoundException("Không tìm thấy thông số socket của CPU.");
        }
        if (!mbSpec.TryGetValue(SpecCodeConstant.MbSocket, out var mbSocketValue))
        {
            throw new NotFoundException("Không tìm thấy thông số socket của mainboard.");
        }
        if (cpuSocketValue != mbSocketValue)
        {
            throw new BadRequestException("Socket của CPU không tương thích với mainboard.");
        }
    }

    private static void CheckCpuMemoryTypeWithMbMemoryType(Dictionary<string, string> cpuSpec, Dictionary<string, string> mbSpec)
    {
        if (!cpuSpec.TryGetValue(SpecCodeConstant.CpuMemoryType, out var cpuMemTypeValue))
        {
            throw new NotFoundException("Không tìm thấy thông số loại bộ nhớ hỗ trợ của CPU.");
        }
        if (!mbSpec.TryGetValue(SpecCodeConstant.MbMemoryType, out var mbMemTypeValue))
        {
            throw new NotFoundException("Không tìm thấy thông số loại bộ nhớ hỗ trợ của mainboard.");
        }
        var cpuMemTypes = cpuMemTypeValue.Split(",").Select(s => s.Trim());
        if (!cpuMemTypes.Contains(mbMemTypeValue))
        {
            throw new BadRequestException("Loại bộ nhớ của CPU không tương thích với loại bộ nhớ của mainboard.");
        }
    }

    private static int GetCpuTdp(Dictionary<string, string> cpuSpec, int cpuRequestedQuantity)
    {
        if (!cpuSpec.TryGetValue(SpecCodeConstant.CpuTdp, out var cpuTdpValue))
        {
            throw new NotFoundException("Không tìm thấy thông số Tdp của CPU.");
        }
        if (!int.TryParse(cpuTdpValue, out var cpuTdp))
        {
            throw new NotFoundException("Thông số Tdp của CPU không hợp lệ để kiểm tra tương thích.");
        }
        return cpuTdp * cpuRequestedQuantity;
    }

    private static int GetGpuTdp(Dictionary<string, string> gpuSpec, ComponentData gpuData)
    {
        if (!gpuSpec.TryGetValue(SpecCodeConstant.GpuTdp, out var gpuTdpValue))
        {
            throw new NotFoundException($"Không tìm thấy thông số Tdp của GPU {gpuData.Product.Name}");
        }
        if (!int.TryParse(gpuTdpValue, out var gpuTdp))
        {
            throw new NotFoundException($"Giá trị thông số của GPU {gpuData.Product.Name} không hợp lệ để kiểm tra tương thích.");
        }
        return gpuTdp * gpuData.Command.Quantity;
    }

    private static void CheckCompatibilityBetweenMotherboardAndRam(Dictionary<string, string> mbSpec, Dictionary<string, string> ramSpec, int ramRequestedQuantity)
    {
        CheckRamTypeWithMotherboardMemoryType(mbSpec, ramSpec);
        CheckRamSticksWithMotherboardMemorySlots(mbSpec, ramSpec, ramRequestedQuantity);
        CheckRamTotalCapacityWithMbMaxMemory(mbSpec, ramSpec, ramRequestedQuantity);
    }

    private static void CheckRamTypeWithMotherboardMemoryType(Dictionary<string, string> mbSpec, Dictionary<string, string> ramSpec)
    {
        if (!mbSpec.TryGetValue(SpecCodeConstant.MbMemoryType, out var mbMemTypeValue))
        {
            throw new NotFoundException("Không tìm thấy thông số loại bộ nhớ hỗ trợ của mainboard.");
        }
        if (!ramSpec.TryGetValue(SpecCodeConstant.RamType, out var ramTypeValue))
        {
            throw new NotFoundException("Không tìm thấy thông số loại RAM.");
        }
        if (mbMemTypeValue != ramTypeValue)
        {
            throw new BadRequestException("Loại RAM không tương thích với mainboard.");
        }
    }

    private static void CheckRamSticksWithMotherboardMemorySlots(Dictionary<string, string> mbSpec, Dictionary<string, string> ramSpec, int ramRequestedQuantity)
    {
        if (!mbSpec.TryGetValue(SpecCodeConstant.MbMemorySlots, out var mbMemorySlotValue))
        {
            throw new NotFoundException("Không tìm thấy thông số số slot của bộ nhớ trong mainboard.");
        }
        if (!ramSpec.TryGetValue(SpecCodeConstant.RamSticks, out var ramStickValue))
        {
            throw new NotFoundException("Không tìm thấy thông số số cây RAM trong sản phẩm RAM.");
        }
        if (!int.TryParse(mbMemorySlotValue, out var mbMemorySlot))
        {
            throw new NotFoundException("Thông số số slot bộ nhớ của mainboard hiện tại không phù hợp để kiểm tra tương thích.");
        }
        if (!int.TryParse(ramStickValue, out var ramStick))
        {
            throw new NotFoundException("Thông số số RAM của sản phẩm RAM hiện tại không phù hợp để kiểm tra tương thích.");
        }
        var totalStick = ramStick * ramRequestedQuantity;
        if (totalStick > mbMemorySlot)
        {
            throw new BadRequestException("Số lượng thanh RAM yêu cầu vượt quá số khe cắm cho phép của mainboard.");
        }
    }

    private static void CheckRamTotalCapacityWithMbMaxMemory(Dictionary<string, string> mbSpec, Dictionary<string, string> ramSpec, int ramRequestedQuantity)
    {
        if (!ramSpec.TryGetValue(SpecCodeConstant.RamSticks, out var ramStickValue))
        {
            throw new NotFoundException("Không tìm thấy thông số số cây RAM trong sản phẩm RAM.");
        }
        if (!ramSpec.TryGetValue(SpecCodeConstant.RamCapacity, out var ramCapacityValue))
        {
            throw new NotFoundException("Không tỉm thấy thông số dung lượng RAM trong sản phẩm RAM");
        }
        if (!mbSpec.TryGetValue(SpecCodeConstant.MbMaxMemory, out var mbMaxMemoryValue))
        {
            throw new NotFoundException("Không tìm thấy thông số bộ nhớ tối đa của mainboard.");
        }
        if (!int.TryParse(ramStickValue, out var ramStick))
        {
            throw new NotFoundException("Thông số số RAM của sản phẩm RAM hiện tại không phù hợp để kiểm tra tương thích.");
        }
        if (!int.TryParse(ramCapacityValue, out var ramCapacity))
        {
            throw new NotFoundException("Thông số dung lượng RAM của sản phẩm RAM hiện tại không phù hợp để kiểm tra tương thích.");
        }
        if (!int.TryParse(mbMaxMemoryValue, out var mbMaxMemory))
        {
            throw new NotFoundException("Thông số bộ nhớ tối đa của mainboard không phù hợp để kiểm tra tương thích.");
        }
        var ramTotalCapacity = ramStick * ramRequestedQuantity * ramCapacity;
        if (ramTotalCapacity > mbMaxMemory)
        {
            throw new BadRequestException("Dung lượng RAM yêu cầu vượt quá bộ nhớ tối đa của mainboard.");
        }

    }

    private static (int, int) GetM2CountAndSataCountFromStorageRequest(Dictionary<string, List<ComponentData>> categoryMap, Dictionary<Guid, Dictionary<string, string>> componentSpecDicts)
    {
        var storageDatas = categoryMap[CategoryNameConstant.Storage];
        int totalM2Count = 0;
        int totalSataCount = 0;
        foreach (var storageData in storageDatas)
        {
            var storageSpec = componentSpecDicts[storageData.Product.Id];

            if (storageSpec.TryGetValue(SpecCodeConstant.StorFormFactor, out var formFactorValue) &&
                formFactorValue.Equals("M.2", StringComparison.OrdinalIgnoreCase))
            {
                totalM2Count += storageData.Command.Quantity;
                continue;
            }

            if (!storageSpec.TryGetValue(SpecCodeConstant.StorType, out var storTypeValue))
            {
                throw new NotFoundException("Không tìm thấy thông số loại ổ cứng trong sản phẩm Ổ cứng");
            }
            if (storTypeValue.Equals("nvme", StringComparison.OrdinalIgnoreCase))
            {
                totalM2Count += storageData.Command.Quantity;
            }
            else
            {
                totalSataCount += storageData.Command.Quantity;
            }
        }
        return (totalM2Count, totalSataCount);
    }


    private static void CheckStorageCountsWithMotherboard(int totalM2Count, int totalSataCount, Dictionary<string, string> mbSpec)
    {
        if (!mbSpec.TryGetValue(SpecCodeConstant.MbM2Slots, out var mbM2SlotValue))
        {
            throw new NotFoundException("Không tìm thấy thông số số m2 slot của mainboard.");
        }
        if (!mbSpec.TryGetValue(SpecCodeConstant.MbSataPorts, out var mbSataPortValue))
        {
            throw new NotFoundException("Không tìm thấy thông số số cổng sata của mainboard.");
        }
        if (!int.TryParse(mbM2SlotValue, out var mbM2Slots))
        {
            throw new NotFoundException("Thông số số m2 slot của mainboard không hợp lệ để kiểm tra tương thích.");
        }
        if (!int.TryParse(mbSataPortValue, out var mbSataPorts))
        {
            throw new NotFoundException("Thông số số cổng sata của mainboard không hợp lệ để kiểm tra tương thích.");
        }
        if (totalM2Count > mbM2Slots)
        {
            throw new BadRequestException("Số lượng ổ cứng M.2 yêu cầu vượt quá số khe M.2 cho phép của mainboard.");
        }
        if (totalSataCount > mbSataPorts)
        {
            throw new BadRequestException("Số lượng ổ cứng SATA và HDD vượt quá số cổng SATA cho phép của mainboard.");
        }

    }

    private static void CheckCasePsuFormFactorWithPsuFormFactor(Dictionary<string, string> caseSpec, Dictionary<string, string> psuSpec)
    {
        if (!caseSpec.TryGetValue(SpecCodeConstant.CasePsuFormFactor, out var casePsuFormFactorValue))
        {
            throw new NotFoundException("Không tìm thấy thông số PSU form factor của Case.");
        }
        if (!psuSpec.TryGetValue(SpecCodeConstant.PsuFormFactor, out var psuFormFactorValue))
        {
            throw new NotFoundException("Không tìm thấy thông số form factor của PSU");
        }
        var casePsuFormFactors = casePsuFormFactorValue.Split(",").Select(s => s.Trim());
        if (!casePsuFormFactors.Contains(psuFormFactorValue))
        {
            throw new BadRequestException("Thông số form factor giữa case và PSU không trùng khớp.");
        }
    }

    private static void CheckPsuPowerSupplyIsEnough(Dictionary<string, string> psuSpec, int totalGpuTdp, int cpuTdp)
    {
        if (!psuSpec.TryGetValue(SpecCodeConstant.PsuWattage, out var psuWattageValue))
        {
            throw new NotFoundException("Không tìm thấy thông số watt của PSU.");
        }
        if (!int.TryParse(psuWattageValue, out var psuWattage))
        {
            throw new NotFoundException("Thông số hiện tại của PSU watt không hợp lệ để kiểm tra tương thích");
        }
        var totalTdp = totalGpuTdp + cpuTdp;
        var recommendedPsuWattage = totalTdp + (totalTdp * 0.2);
        if (psuWattage < recommendedPsuWattage)
        {
            throw new BadRequestException("Số watt cung cấp của PSU không đủ đối với cấu hình hiện tại");
        }
    }

    private static void CheckCompatibilityBetweenCpuAndCpuCooler(Dictionary<string, string> cpuSpec, Dictionary<string, string> cpuCoolerSpec)
    {
        CheckCpuCoolerSocketSupportCpuSocket(cpuSpec, cpuCoolerSpec);
        CheckCoolerTdpRatingSupportCpuTdp(cpuSpec, cpuCoolerSpec);
    } 

    private static void CheckCpuCoolerSocketSupportCpuSocket(Dictionary<string, string> cpuSpec, Dictionary<string, string> cpuCoolerSpec)
    {
        if (!cpuSpec.TryGetValue(SpecCodeConstant.CpuSocket, out var cpuSocketValue))
        {
            throw new NotFoundException("Không tìm thấy thông số socket của CPU");
        }
        if (!cpuCoolerSpec.TryGetValue(SpecCodeConstant.CoolerSocketSupport, out var coolerSocketSupportValue))
        {
            throw new NotFoundException("Không tìm thấy thông số socket hỗ trợ của Tản nhiệt CPU.");
        }
        var coolerSocketSupports = coolerSocketSupportValue.Split(",").Select(c => c.Trim());
        if (!coolerSocketSupports.Contains(cpuSocketValue))
        {
            throw new BadRequestException("Tản nhiệt CPU không đáp ứng được socket của CPU.");
        }
    }


    private static void CheckCoolerTdpRatingSupportCpuTdp(Dictionary<string, string> cpuSpec, Dictionary<string, string> cpuCoolerSpec)
    {
        if (!cpuSpec.TryGetValue(SpecCodeConstant.CpuTdp, out var cpuTdpValue))
        {
            throw new NotFoundException("Không tìm thấy thông số Tdp của CPU.");
        }
        if (!cpuCoolerSpec.TryGetValue(SpecCodeConstant.CoolerTdpRating, out var coolerTdpRatingValue))
        {
            throw new NotFoundException("Không tìm thấy thông số Tdp rating của Tản nhiệt CPU.");
        }
        if (!int.TryParse(cpuTdpValue, out var cpuTdp))
        {
            throw new NotFoundException("Thông số Tdp của CPU hiện tại không hợp lệ để kiểm tra tương thích.");
        }
        if (!int.TryParse(coolerTdpRatingValue, out var coolerTdpRating))
        {
            throw new NotFoundException("Thông số Tdp Rating của Tản nhiệt CPU hiện tại không hợp lệ để kiểm tra tương thích.");
        }
        if (cpuTdp > coolerTdpRating)
        {
            throw new BadRequestException("Tản nhiệt CPU không đủ tiêu chuẩn cho CPU hiện tại.");
        }
    }

    private static void CheckCaseMaxCoolerHeightSupportCpuCoolerHeight(Dictionary<string, string> caseSpec, Dictionary<string, string> cpuCoolerSpec)
    {
        if (!caseSpec.TryGetValue(SpecCodeConstant.CaseMaxCoolerHeight, out var caseMaxCoolerHeightValue))
        {
            throw new NotFoundException("Không tìm thấy thông số chiều cao tối đa tản nhiệt CPU của Case.");
        }
        if (!cpuCoolerSpec.TryGetValue(SpecCodeConstant.CoolerHeight, out var coolerHeightValue))
        {
            throw new NotFoundException("Không tìm thấy thông số chiều cao của Tản nhiệt CPU");
        }
        if (!int.TryParse(caseMaxCoolerHeightValue, out var caseMaxCoolerHeight))
        {
            throw new NotFoundException("Thông số chiều cao tối đa Tản nhiệt CPU của Case hiện tại không hợp lệ để kiểm tra tương thích.");
        }
        if (!int.TryParse(coolerHeightValue, out var coolerHeight))
        {
            throw new NotFoundException("Thông số chiều cao của Tản nhiệt CPU hiện tại không hợp lệ để kiểm tra tương thích.");
        }
        if (coolerHeight > caseMaxCoolerHeight)
        {
            throw new BadRequestException("Tản nhiệt CPU hiện tại có chiều cao quá cao so với chiều cao tối đa của Case.");
        }
    }

    private static void CheckCaseFormFactorSupportMainboardFormFactor(Dictionary<string, string> caseSpec, Dictionary<string, string> mbSpec)
    {
        if (!caseSpec.TryGetValue(SpecCodeConstant.CaseFormFactor, out var caseFormFactorValue))
        {
            throw new NotFoundException("Không tìm thấy thông số form factor của Case.");
        }
        if (!mbSpec.TryGetValue(SpecCodeConstant.MbFormFactor, out var mbFormFactorValue))
        {
            throw new NotFoundException("Không tìm thấy thông số form factor của Mainboard.");
        }
        var caseFormFactors = caseFormFactorValue.Split(",").Select(s => s.Trim());
        if (!caseFormFactors.Contains(mbFormFactorValue))
        {
            throw new BadRequestException("Form factor của Mainboard không tương thích với Case.");
        }
    }

    private static void CheckCaseMaxGpuLengthWithGpuLength(Dictionary<string, string> caseSpec, Dictionary<string, string> gpuSpec)
    {
        if (!caseSpec.TryGetValue(SpecCodeConstant.CaseMaxGpuLength, out var caseMaxGpuLengthValue))
        {
            throw new NotFoundException("Không tìm thấy thông số chiều dài tối đa của GPU mà case hỗ trợ");
        }
        if (!gpuSpec.TryGetValue(SpecCodeConstant.GpuLength, out var gpuLengthValue))
        {
            throw new NotFoundException("Không tìm thấy thông số chiều dài của GPU.");
        }
        if (!int.TryParse(caseMaxGpuLengthValue, out var caseMaxGpuLength))
        {
            throw new NotFoundException("Thông số chiều dài của GPU của Case hiện tại không hợp lệ để kiểm tra tương thích.");
        }
        if (!int.TryParse(gpuLengthValue, out var gpuLength))
        {
            throw new NotFoundException("Thông số chiều dài của GPU hiện tại không hợp lệ để kiểm tra tương thích.");
        }
        if (caseMaxGpuLength < gpuLength)
        {
            throw new BadRequestException("Chiều dài của GPU hiện tại đang vượt quá kích thước so với Case.");
        }
    }
    

    private static int ExtractPcieVersion(string value)
    {
        var idx = value.IndexOf("PCIe", StringComparison.OrdinalIgnoreCase);
        var s = (idx >= 0 ? value[(idx + 4)..] : value).TrimStart(' ', '\t', '.', '_');
        var numStr = new string([..s.TakeWhile(char.IsDigit)]);
        return int.TryParse(numStr, out var v) ? v : 0;
    }


    private static void CheckMainboardGpuPcieVersionCompatibility(Dictionary<string, string> mbSpec, Dictionary<string, string> gpuSpec, List<string> warnings)
    {
        if (!mbSpec.TryGetValue(SpecCodeConstant.MbPcieVersion, out var mbPcieValue) ||
            !gpuSpec.TryGetValue(SpecCodeConstant.GpuPcieSlot, out var gpuPcieSlotValue))
        {
            return;
        }

        var mbVer = ExtractPcieVersion(mbPcieValue);
        var gpuVer = ExtractPcieVersion(gpuPcieSlotValue);

        if (mbVer == 0 || gpuVer == 0)
            return;

        if (mbVer < gpuVer)
        {
            warnings.Add(
                $"Mainboard có chuẩn PCIe {mbVer} thấp hơn chuẩn PCIe {gpuVer} của GPU. GPU vẫn hoạt động nhưng băng thông sẽ bị giới hạn theo chuẩn PCIe {mbVer}.");
        }
    }

    private static void CheckCpuContainsIGpu(Dictionary<string, string> cpuSpec, List<ComponentData>? gpuDatas)
    {
        if (!cpuSpec.TryGetValue(SpecCodeConstant.CpuIntegratedGpu, out var cpuIntegratedGpuValue))
        {
            throw new NotFoundException("Không tìm thấy thông số GPU tích hợp của CPU.");
        }
        if (!bool.TryParse(cpuIntegratedGpuValue, out var cpuIntegratedGpu))
        {
            throw new NotFoundException("Thông số GPU tích hợp hiện tại của CPU không hợp lệ để kiểm tra tương thích.");
        }
        if (!cpuIntegratedGpu && (gpuDatas == null || gpuDatas.Count == 0))
        {
            throw new BadRequestException("GPU là bắt buộc trong cấu hình vì CPU được chọn không có GPU tích hợp");
        }
    }

    private static void CheckRamIsEccWithSupportedCpuAndMotherboard(Dictionary<string, string> ramSpec, bool isCpuAndMotherboardSupportEcc)
    {
        if (!ramSpec.TryGetValue(SpecCodeConstant.RamEcc, out var ramEccValue))
        {
            return;
        }
        if (!bool.TryParse(ramEccValue, out var ramEcc))
        {
            throw new NotFoundException("Giá trị của Ram Ecc hiện tại không hợp lệ để kiểm tra tương thích");
        }
        if (ramEcc)
        {
            if (!isCpuAndMotherboardSupportEcc)
            {
                throw new BadRequestException("Cấu hình hiện tại chứa CPU hoặc Mainboard không hỗ trợ RAM ECC.");
            }
        }
        
    }

    private static void CheckRamRegisteredMemoryWithSupportedCpuAndMotherboard(Dictionary<string, string> ramSpec, bool isCpuAndMotherboardSupportEcc)
    {
        if (!ramSpec.TryGetValue(SpecCodeConstant.RamRegistered, out var ramRegisteredValue))
        {
            return;
        }
        if (ramRegisteredValue.Equals("rdimm", StringComparison.OrdinalIgnoreCase) || ramRegisteredValue.Equals("lrdimm", StringComparison.OrdinalIgnoreCase))
        {
            if (!isCpuAndMotherboardSupportEcc)
            {
                throw new BadRequestException("RAM Registered (RDIMM/LRDIMM) yêu cầu CPU và Mainboard hỗ trợ ECC. Cấu hình hiện tại không đáp ứng yêu cầu này.");
            }
        }
    }

    private static bool IsCpuAndMotherboardSupportEcc(Dictionary<string, string> cpuSpec, Dictionary<string, string> mbSpec)
    {
        if (!cpuSpec.TryGetValue(SpecCodeConstant.CpuEccSupport, out var cpuEccSupportValue) || !mbSpec.TryGetValue(SpecCodeConstant.MbEccSupport, out var mbEccSupportValue))
        {
            return false;
        }
        if (!bool.TryParse(cpuEccSupportValue, out var cpuEccSupport))
        {
            throw new NotFoundException("Giá trị thông số hỗ trợ Ecc của CPU hiện tại không hợp lệ để kiểm tra tương thích.");
        }
        if (!bool.TryParse(mbEccSupportValue, out var mbEccSupport))
        {
            throw new NotFoundException("Giá trị thông số hỗ trợ Ecc của Mainboard hiện tại không hợp lệ để kiểm tra tương thích.");
        }
        if (!cpuEccSupport || !mbEccSupport) return false;
        return true;
    }

    private static (int, int) GetDriveBayCountsFromStorageRequest(Dictionary<string, List<ComponentData>> categoryMap, Dictionary<Guid, Dictionary<string, string>> componentSpecDicts)
    {
        var storageDatas = categoryMap[CategoryNameConstant.Storage];
        int total25Count = 0;
        int total35Count = 0;
        foreach (var storageData in storageDatas)
        {
            var storageSpec = componentSpecDicts[storageData.Product.Id];
            if (!storageSpec.TryGetValue(SpecCodeConstant.StorFormFactor, out var formFactorValue))
            {
                continue;
            }
            if (formFactorValue.Equals("2.5\"", StringComparison.OrdinalIgnoreCase))
            {
                total25Count += storageData.Command.Quantity;
            }
            else if (formFactorValue.Equals("3.5\"", StringComparison.OrdinalIgnoreCase))
            {
                total35Count += storageData.Command.Quantity;
            }
        }
        return (total25Count, total35Count);
    }

    private static void CheckCaseDriveBaysWithStorage(Dictionary<string, string> caseSpec, int total25Count, int total35Count)
    {
        if (total25Count > 0 && caseSpec.TryGetValue(SpecCodeConstant.CaseDriveBays25, out var caseBays25Value))
        {
            if (int.TryParse(caseBays25Value, out var caseBays25) && total25Count > caseBays25)
            {
                throw new BadRequestException($"Số lượng ổ cứng 2.5\" ({total25Count}) vượt quá số khay 2.5\" của case ({caseBays25}).");
            }
        }
        if (total35Count > 0 && caseSpec.TryGetValue(SpecCodeConstant.CaseDriveBays35, out var caseBays35Value))
        {
            if (int.TryParse(caseBays35Value, out var caseBays35) && total35Count > caseBays35)
            {
                throw new BadRequestException($"Số lượng ổ cứng 3.5\" ({total35Count}) vượt quá số khay 3.5\" của case ({caseBays35}).");
            }
        }
    }

    private static void CheckPcieLaneBudget(Dictionary<string, string> mbSpec, int gpuQuantity, int totalM2Count)
    {
        if (!mbSpec.TryGetValue(SpecCodeConstant.MbTotalPcieLanes, out var mbTotalPcieLanesValue))
        {
            return;
        }
        if (!int.TryParse(mbTotalPcieLanesValue, out var mbTotalPcieLanes))
        {
            throw new NotFoundException("Giá trị tổng số làn PCIe của mainboard không hợp lệ để kiểm tra tương thích.");
        }
        var totalUsedLanes = (gpuQuantity * 16) + (totalM2Count * 4);
        if (totalUsedLanes > mbTotalPcieLanes)
        {
            throw new BadRequestException($"Tổng số làn PCIe sử dụng ({totalUsedLanes}) vượt quá số làn PCIe của mainboard ({mbTotalPcieLanes}).");
        }
    }
}

public record ComponentData(
    AddComputerComponentCommand Command,
    Product Product
);
