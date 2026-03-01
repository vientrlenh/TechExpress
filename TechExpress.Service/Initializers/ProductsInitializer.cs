using System;
using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;

namespace TechExpress.Service.Initializers;

public static class ProductsInitializer
{
    public static async Task Init(ApplicationDbContext context)
    {
        if (await context.Products.AnyAsync())
        {
            return;
        }

        var allSpecValues = new List<ProductSpecValue>();
        var allProductImages = new List<ProductImage>();

        // ============= GPU PRODUCTS =============
        await InitGpuProducts(context, allSpecValues, allProductImages);

        // ============= CPU PRODUCTS =============
        await InitCpuProducts(context, allSpecValues, allProductImages);

        // ============= MOTHERBOARD PRODUCTS =============
        await InitMotherboardProducts(context, allSpecValues, allProductImages);

        // ============= RAM PRODUCTS =============
        await InitRamProducts(context, allSpecValues, allProductImages);

        // ============= STORAGE PRODUCTS =============
        await InitStorageProducts(context, allSpecValues, allProductImages);

        // ============= PSU PRODUCTS =============
        await InitPsuProducts(context, allSpecValues, allProductImages);

        // ============= CASE PRODUCTS =============
        await InitCaseProducts(context, allSpecValues, allProductImages);

        // ============= CPU COOLER PRODUCTS =============
        await InitCoolerProducts(context, allSpecValues, allProductImages);

        // ============= KEYBOARD PRODUCTS =============
        await InitKeyboardProducts(context, allSpecValues, allProductImages);

        // ============= MOUSE PRODUCTS =============
        await InitMouseProducts(context, allSpecValues, allProductImages);

        // ============= HEADSET PRODUCTS =============
        await InitHeadsetProducts(context, allSpecValues, allProductImages);

        context.ProductSpecValues.AddRange(allSpecValues);
        context.ProductImages.AddRange(allProductImages);

        await context.SaveChangesAsync();
    }

    private static async Task InitGpuProducts(ApplicationDbContext context, List<ProductSpecValue> specValues, List<ProductImage> productImages)
    {
        var gpuCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Card đồ họa")
            ?? throw new NotFoundException("Không tìm thấy danh mục Card đồ họa");

        var gpuSpecs = await context.SpecDefinitions
            .Where(s => s.CategoryId == gpuCategory.Id)
            .ToListAsync();

        // Load all GPU brands
        var asusBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "ASUS")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu ASUS");
        var msiBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "MSI")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu MSI");
        var gigabyteBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Gigabyte")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Gigabyte");
        var zotacBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Zotac")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Zotac");
        var palitBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Palit")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Palit");
        var galaxBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Galax")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Galax");
        var inno3dBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "INNO3D")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu INNO3D");
        var colorfulBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Colorful")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Colorful");
        var pnyBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "PNY")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu PNY");
        var sapphireBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Sapphire")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Sapphire");
        var xfxBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "XFX")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu XFX");
        var powerColorBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "PowerColor")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu PowerColor");

        // ============= NVIDIA RTX 4090 =============

        // ASUS ROG STRIX RTX 4090 OC
        var asus4090Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = asus4090Id,
            Name = "ASUS ROG STRIX GeForce RTX 4090 OC Edition 24GB",
            Sku = "GPU-RTX4090-ASUS-STRIX-OC",
            CategoryId = gpuCategory.Id,
            BrandId = asusBrand.Id,
            Price = 54990000,
            Stock = 8,
            Description = "Card đồ họa ASUS ROG STRIX RTX 4090 OC Edition với 24GB GDDR6X, kiến trúc Ada Lovelace. Tản nhiệt 3.5 slot với 3 quạt Axial-tech, Aura Sync RGB, GPU Tweak III. Card đồ họa flagship cho gaming 4K và workstation.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, asus4090Id, vram: 24, tdp: 450, length: 358, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 16-pin (12VHPWR)");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = asus4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F829b3b61-2dcc-46f0-a024-83ae09df2500%2Fimages%2F1770355376547_0.webp?alt=media&token=9be42280-dbd5-483d-9f4a-df73021c1a16"
            },
            new ProductImage
            {
                ProductId = asus4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F829b3b61-2dcc-46f0-a024-83ae09df2500%2Fimages%2F1770355391981_0.webp?alt=media&token=08789856-be00-4d79-99f6-0dc7ce4fa4d3"
            },
            new ProductImage
            {
                ProductId = asus4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F829b3b61-2dcc-46f0-a024-83ae09df2500%2Fimages%2F1770355391981_1.webp?alt=media&token=06311fb0-e090-4d86-84c3-14aeb0a28685"
            },
            new ProductImage
            {
                ProductId = asus4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F829b3b61-2dcc-46f0-a024-83ae09df2500%2Fimages%2F1770355391982_2.webp?alt=media&token=4c6aa0c7-bcbf-4343-bd09-e7fce7d2531c"
            },
            new ProductImage
            {
                ProductId = asus4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F829b3b61-2dcc-46f0-a024-83ae09df2500%2Fimages%2F1770355391982_3.webp?alt=media&token=90b442e7-eaf3-4c9b-9021-d663c4062234"
            },
            new ProductImage
            {
                ProductId = asus4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F829b3b61-2dcc-46f0-a024-83ae09df2500%2Fimages%2F1770355391982_4.webp?alt=media&token=476fd043-cf2a-485a-bcaf-6ec0b480fbe4"
            },
        ]);

        // MSI SUPRIM X RTX 4090
        var msi4090Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = msi4090Id,
            Name = "MSI GeForce RTX 4090 SUPRIM X 24G",
            Sku = "GPU-RTX4090-MSI-SUPRIM-X",
            CategoryId = gpuCategory.Id,
            BrandId = msiBrand.Id,
            Price = 52990000,
            Stock = 10,
            Description = "Card đồ họa MSI RTX 4090 SUPRIM X với 24GB GDDR6X, thiết kế tản nhiệt TRI FROZR 3S với 3 quạt TORX 5.0. Mystic Light RGB, Zero Frozr technology. Hiệu năng đỉnh cao cho gaming 4K và AI.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, msi4090Id, vram: 24, tdp: 450, length: 336, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 16-pin (12VHPWR)");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = msi4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F6730de27-d421-42a4-a009-6f57534b590e%2Fimages%2F1770356355552_0.webp?alt=media&token=eefe2a01-ad61-424d-a397-4894d86a676f"
            },
            new ProductImage
            {
                ProductId = msi4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F6730de27-d421-42a4-a009-6f57534b590e%2Fimages%2F1770356371354_0.webp?alt=media&token=debe19f2-3169-424d-890e-64e752c76c02"
            },
            new ProductImage
            {
                ProductId = msi4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F6730de27-d421-42a4-a009-6f57534b590e%2Fimages%2F1770356371356_1.webp?alt=media&token=da8d2e9e-d87f-429b-a27f-ee115a0f2d5f"
            }
        ]);

        // Gigabyte AORUS Master RTX 4090
        var gigabyte4090Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gigabyte4090Id,
            Name = "Gigabyte AORUS GeForce RTX 4090 MASTER 24G",
            Sku = "GPU-RTX4090-GIGABYTE-AORUS-MASTER",
            CategoryId = gpuCategory.Id,
            BrandId = gigabyteBrand.Id,
            Price = 51990000,
            Stock = 12,
            Description = "Card đồ họa Gigabyte AORUS RTX 4090 MASTER với 24GB GDDR6X, hệ thống tản nhiệt WINDFORCE với 3 quạt. LCD Edge View hiển thị thông tin real-time, RGB Fusion 2.0. Card cao cấp cho enthusiast.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, gigabyte4090Id, vram: 24, tdp: 450, length: 358, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 16-pin (12VHPWR)");
        productImages.AddRange(
        [
            new ProductImage 
            {
                ProductId = gigabyte4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F1fa5a15f-0508-480b-a49b-fcdb2610d3f9%2Fimages%2F1770356871139_0.webp?alt=media&token=5ea3a304-1c2b-4a1e-ad39-495e99e4584c",
            },
            new ProductImage 
            {
                ProductId = gigabyte4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F1fa5a15f-0508-480b-a49b-fcdb2610d3f9%2Fimages%2F1770356901190_0.webp?alt=media&token=8e77fe80-3fdd-48d1-8f1b-a71be4ed46fd",
            },
            new ProductImage 
            {
                ProductId = gigabyte4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F1fa5a15f-0508-480b-a49b-fcdb2610d3f9%2Fimages%2F1770356901191_1.webp?alt=media&token=3b8c5c39-c67b-4204-9216-67e79e23d4a5",
            },
            new ProductImage 
            {
                ProductId = gigabyte4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F1fa5a15f-0508-480b-a49b-fcdb2610d3f9%2Fimages%2F1770356901191_2.webp?alt=media&token=78713043-87c2-49a1-9eb5-0b9784122c95",
            },
            new ProductImage
            {
                ProductId = gigabyte4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F1fa5a15f-0508-480b-a49b-fcdb2610d3f9%2Fimages%2F1770356901191_3.webp?alt=media&token=716c1240-de38-461c-b0ab-67480a54c890",
            },
            new ProductImage
            {
                ProductId = gigabyte4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F1fa5a15f-0508-480b-a49b-fcdb2610d3f9%2Fimages%2F1770356901192_4.webp?alt=media&token=ef0ad964-01ad-4285-8657-85c519bee2b5",
            },
            new ProductImage
            {
                ProductId = gigabyte4090Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F1fa5a15f-0508-480b-a49b-fcdb2610d3f9%2Fimages%2F1770356901192_5.webp?alt=media&token=3b5a14f3-2087-4060-b599-09a97d9c8c50",
            }
        ]);

        // ============= NVIDIA RTX 4080 SUPER =============

        // ASUS TUF Gaming RTX 4080 SUPER
        var asus4080sId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = asus4080sId,
            Name = "ASUS TUF Gaming GeForce RTX 4080 SUPER 16GB OC",
            Sku = "GPU-RTX4080S-ASUS-TUF-OC",
            CategoryId = gpuCategory.Id,
            BrandId = asusBrand.Id,
            Price = 32990000,
            Stock = 15,
            Description = "Card đồ họa ASUS TUF Gaming RTX 4080 SUPER OC với 16GB GDDR6X, thiết kế bền bỉ chuẩn quân sự. Tản nhiệt 3 quạt Axial-tech, dual ball fan bearings. Hiệu năng mạnh mẽ cho gaming 4K.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, asus4080sId, vram: 16, tdp: 320, length: 348, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 16-pin (12VHPWR)");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = asus4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fa2a58a6d-f0c4-4db8-9224-ab9f8f180408%2Fimages%2F1770357331942_0.webp?alt=media&token=af817e01-f7f1-4b3c-8054-e91c51314d63",
            },
            new ProductImage
            {
                ProductId = asus4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fa2a58a6d-f0c4-4db8-9224-ab9f8f180408%2Fimages%2F1770357343846_1.webp?alt=media&token=1bdd4698-d173-4f93-87e2-a38d605459ac",
            },
            new ProductImage
            {
                ProductId = asus4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fa2a58a6d-f0c4-4db8-9224-ab9f8f180408%2Fimages%2F1770357343846_2.webp?alt=media&token=f2ffb06d-8acc-40b4-a9ec-662da6442181",
            },
            new ProductImage
            {
                ProductId = asus4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fa2a58a6d-f0c4-4db8-9224-ab9f8f180408%2Fimages%2F1770357343846_3.webp?alt=media&token=59dc7f29-6e88-4aeb-bf1e-6cea8a7d4f9e",
            },
            new ProductImage
            {
                ProductId = asus4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fa2a58a6d-f0c4-4db8-9224-ab9f8f180408%2Fimages%2F1770357343847_4.webp?alt=media&token=63e08b7f-97c5-454a-b8b7-39aa72197276",
            },
            new ProductImage
            {
                ProductId = asus4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fa2a58a6d-f0c4-4db8-9224-ab9f8f180408%2Fimages%2F1770357343847_5.webp?alt=media&token=3debdb40-4b9e-4d0f-b661-dfee49ef8b02",
            },
            new ProductImage
            {
                ProductId = asus4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fa2a58a6d-f0c4-4db8-9224-ab9f8f180408%2Fimages%2F1770357365790_0.webp?alt=media&token=39f12267-7197-4ecb-b443-1d5ebd62898b",
            }
        ]);

        // MSI Gaming X Trio RTX 4080 SUPER
        var msi4080sId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = msi4080sId,
            Name = "MSI GeForce RTX 4080 SUPER 16G GAMING X TRIO",
            Sku = "GPU-RTX4080S-MSI-GAMING-X-TRIO",
            CategoryId = gpuCategory.Id,
            BrandId = msiBrand.Id,
            Price = 31990000,
            Stock = 18,
            Description = "Card đồ họa MSI RTX 4080 SUPER GAMING X TRIO với 16GB GDDR6X, tản nhiệt TRI FROZR 3 với 3 quạt TORX 5.0. Thiết kế đẹp mắt với Mystic Light RGB, Zero Frozr cho hoạt động êm ái.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, msi4080sId, vram: 16, tdp: 320, length: 337, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 16-pin (12VHPWR)");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = msi4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F8ed95b9c-7149-4723-99fc-72d76568f56a%2Fimages%2F1770357831119_0.webp?alt=media&token=06559cca-cadf-4c51-b54d-5eebc597c42a",
            },
            new ProductImage
            {
                ProductId = msi4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F8ed95b9c-7149-4723-99fc-72d76568f56a%2Fimages%2F1770357847153_2.webp?alt=media&token=49678dbc-2be8-42aa-98a3-ae6396664b76",
            },
            new ProductImage
            {
                ProductId = msi4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F8ed95b9c-7149-4723-99fc-72d76568f56a%2Fimages%2F1770357847153_3.webp?alt=media&token=bafff482-a89f-4793-8258-ede1b3e27d1e",
            },
            new ProductImage
            {
                ProductId = msi4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F8ed95b9c-7149-4723-99fc-72d76568f56a%2Fimages%2F1770357847153_4.webp?alt=media&token=e3670327-30f9-4591-94f7-7b36710c0ab4",
            },
            new ProductImage
            {
                ProductId = msi4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F8ed95b9c-7149-4723-99fc-72d76568f56a%2Fimages%2F1770357847153_5.webp?alt=media&token=117c2cdd-425a-4009-8da5-bc21cecc7fe4",
            },
            new ProductImage
            {
                ProductId = msi4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F8ed95b9c-7149-4723-99fc-72d76568f56a%2Fimages%2F1770357879915_0.webp?alt=media&token=f6b9d52d-fa69-4347-a426-25ab010f842a",
            },
            new ProductImage
            {
                ProductId = msi4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F8ed95b9c-7149-4723-99fc-72d76568f56a%2Fimages%2F1770357879916_1.webp?alt=media&token=f170acd7-a75e-40f7-a7e1-043601abcfce",
            }
        ]);

        // Zotac Trinity RTX 4080 SUPER
        var zotac4080sId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = zotac4080sId,
            Name = "ZOTAC GAMING GeForce RTX 4080 SUPER Trinity Black",
            Sku = "GPU-RTX4080S-ZOTAC-TRINITY",
            CategoryId = gpuCategory.Id,
            BrandId = zotacBrand.Id,
            Price = 29990000,
            Stock = 20,
            Description = "Card đồ họa ZOTAC RTX 4080 SUPER Trinity Black với 16GB GDDR6X, thiết kế IceStorm 3.0 với 3 quạt. SPECTRA 2.0 RGB, FireStorm utility. Giải pháp gaming cao cấp với giá cạnh tranh.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, zotac4080sId, vram: 16, tdp: 320, length: 306, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 16-pin (12VHPWR)");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = zotac4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fd141b0bc-5f35-42a6-8dfe-dddae0ec9bbd%2Fimages%2F1770361385655_0.jpg?alt=media&token=ce97d655-feef-43d9-af3b-75eebdc20469",
            },
            new ProductImage
            {
                ProductId = zotac4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fd141b0bc-5f35-42a6-8dfe-dddae0ec9bbd%2Fimages%2F1770358426643_0.jpg?alt=media&token=e909ea6f-d53a-4068-a97f-8956053580a3",
            },
            new ProductImage
            {
                ProductId = zotac4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fd141b0bc-5f35-42a6-8dfe-dddae0ec9bbd%2Fimages%2F1770358426645_1.jpg?alt=media&token=b9119cab-c258-451b-a36a-798d759b6704",
            },
            new ProductImage
            {
                ProductId = zotac4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fd141b0bc-5f35-42a6-8dfe-dddae0ec9bbd%2Fimages%2F1770358426645_2.jpg?alt=media&token=5d0b19c2-6ff6-4055-a39d-464c178f8623",
            },
            new ProductImage
            {
                ProductId = zotac4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fd141b0bc-5f35-42a6-8dfe-dddae0ec9bbd%2Fimages%2F1770358426645_3.jpg?alt=media&token=f37f550d-2280-4045-b136-20b517856cd2",
            },
            new ProductImage
            {
                ProductId = zotac4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fd141b0bc-5f35-42a6-8dfe-dddae0ec9bbd%2Fimages%2F1770358426645_4.jpg?alt=media&token=9d4a3be3-9ed4-4012-a2db-d2993e0ed316",
            },
            new ProductImage
            {
                ProductId = zotac4080sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fd141b0bc-5f35-42a6-8dfe-dddae0ec9bbd%2Fimages%2F1770358439145_0.jpg?alt=media&token=ad1b0d8a-5c85-4563-9452-bf7dcc1bacbb",
            }
        ]);

        // ============= NVIDIA RTX 4070 Ti SUPER =============

        // Gigabyte Gaming OC RTX 4070 Ti SUPER
        var gigabyte4070tiSId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gigabyte4070tiSId,
            Name = "Gigabyte GeForce RTX 4070 Ti SUPER GAMING OC 16G",
            Sku = "GPU-RTX4070TIS-GIGABYTE-GAMING-OC",
            CategoryId = gpuCategory.Id,
            BrandId = gigabyteBrand.Id,
            Price = 23990000,
            Stock = 22,
            Description = "Card đồ họa Gigabyte RTX 4070 Ti SUPER GAMING OC với 16GB GDDR6X, hệ thống tản nhiệt WINDFORCE với 3 quạt. RGB Fusion 2.0, ép xung sẵn từ nhà máy. Card tầm trung cao cấp cho gaming 1440p/4K.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, gigabyte4070tiSId, vram: 16, tdp: 285, length: 329, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 16-pin (12VHPWR)");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = gigabyte4070tiSId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F245b22de-25f9-40fc-ba83-f144ceb32cfb%2Fimages%2F1770366825481_0.webp?alt=media&token=905a1a7f-f2c9-44e2-abd9-3d14cd5005f9",
            },
            new ProductImage
            {
                ProductId = gigabyte4070tiSId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F245b22de-25f9-40fc-ba83-f144ceb32cfb%2Fimages%2F1770366835503_0.webp?alt=media&token=2389749e-693e-4d64-ae3e-723497c99245",
            },
            new ProductImage
            {
                ProductId = gigabyte4070tiSId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F245b22de-25f9-40fc-ba83-f144ceb32cfb%2Fimages%2F1770366835504_1.webp?alt=media&token=715b8efe-a720-4d26-bec6-9759ecf3e3ee",
            },
            new ProductImage
            {
                ProductId = gigabyte4070tiSId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F245b22de-25f9-40fc-ba83-f144ceb32cfb%2Fimages%2F1770366835504_2.webp?alt=media&token=48d4e30f-6b18-4c44-8453-841cf822b317",
            },
            new ProductImage
            {
                ProductId = gigabyte4070tiSId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F245b22de-25f9-40fc-ba83-f144ceb32cfb%2Fimages%2F1770366835504_3.webp?alt=media&token=e3a2dc3d-518b-4e76-9cf9-40b971b11ef4",
            },
            new ProductImage
            {
                ProductId = gigabyte4070tiSId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F245b22de-25f9-40fc-ba83-f144ceb32cfb%2Fimages%2F1770366846168_0.webp?alt=media&token=5c2b451b-2f8f-4502-b742-cfeb80a8de48",
            }
        ]);

        // Palit GameRock RTX 4070 Ti SUPER
        var palit4070tiSId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = palit4070tiSId,
            Name = "Palit GeForce RTX 4070 Ti SUPER GameRock OC 16GB",
            Sku = "GPU-RTX4070TIS-PALIT-GAMEROCK",
            CategoryId = gpuCategory.Id,
            BrandId = palitBrand.Id,
            Price = 22490000,
            Stock = 25,
            Description = "Card đồ họa Palit RTX 4070 Ti SUPER GameRock OC với 16GB GDDR6X, thiết kế tản nhiệt TurboFan 3.0 với 3 quạt. ARGB LED lighting, DrMOS power stages. Hiệu năng cao với mức giá hợp lý.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, palit4070tiSId, vram: 16, tdp: 285, length: 329, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 16-pin (12VHPWR)");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = palit4070tiSId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F5c2bcbb3-1bf6-43eb-891f-2816d901e8a3%2Fimages%2F1770367156424_0.jpg?alt=media&token=9686a97a-5b4c-46f7-8e6d-9ae5cf539a8b",
            },
            new ProductImage
            {
                ProductId = palit4070tiSId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F5c2bcbb3-1bf6-43eb-891f-2816d901e8a3%2Fimages%2F1770367172606_0.jpg?alt=media&token=8fe2874b-e412-40ef-be70-bb1a5834c74f",
            },
            new ProductImage
            {
                ProductId = palit4070tiSId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F5c2bcbb3-1bf6-43eb-891f-2816d901e8a3%2Fimages%2F1770367172607_1.jpg?alt=media&token=c0892776-fa69-42a8-819e-183c57fa0b7d",
            },
            new ProductImage
            {
                ProductId = palit4070tiSId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F5c2bcbb3-1bf6-43eb-891f-2816d901e8a3%2Fimages%2F1770367172607_2.jpg?alt=media&token=9b9ac0f8-31a2-42fa-97c5-fdc59f211629",
            }
        ]);

        // ============= NVIDIA RTX 4070 SUPER =============

        // MSI Ventus 3X RTX 4070 SUPER
        var msi4070sId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = msi4070sId,
            Name = "MSI GeForce RTX 4070 SUPER 12G VENTUS 3X OC",
            Sku = "GPU-RTX4070S-MSI-VENTUS-3X",
            CategoryId = gpuCategory.Id,
            BrandId = msiBrand.Id,
            Price = 16990000,
            Stock = 30,
            Description = "Card đồ họa MSI RTX 4070 SUPER VENTUS 3X OC với 12GB GDDR6X, tản nhiệt 3 quạt TORX 4.0. Thiết kế tinh gọn, hiệu suất ổn định. Lựa chọn tuyệt vời cho gaming 1440p.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, msi4070sId, vram: 12, tdp: 220, length: 308, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 16-pin (12VHPWR)");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = msi4070sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F01875154-8ccb-4f77-aef1-a9722bb60aef%2Fimages%2F1770367360422_0.webp?alt=media&token=86bae889-4d1d-4cd8-ad5c-0c65eb713909",
            },
            new ProductImage
            {
                ProductId = msi4070sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F01875154-8ccb-4f77-aef1-a9722bb60aef%2Fimages%2F1770367381274_0.webp?alt=media&token=db5f2149-6436-47db-9f11-332c98e0d978",
            },
            new ProductImage
            {
                ProductId = msi4070sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F01875154-8ccb-4f77-aef1-a9722bb60aef%2Fimages%2F1770367381275_1.webp?alt=media&token=243489f8-f509-4a24-8e68-f22f31d24960",
            },
            new ProductImage
            {
                ProductId = msi4070sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F01875154-8ccb-4f77-aef1-a9722bb60aef%2Fimages%2F1770367381275_2.webp?alt=media&token=60ea5975-4603-4f4d-8d33-bb174dc40611",
            },
            new ProductImage
            {
                ProductId = msi4070sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F01875154-8ccb-4f77-aef1-a9722bb60aef%2Fimages%2F1770367381276_3.webp?alt=media&token=e56166a4-38ba-4a6c-ba6c-0b2c57f52bc0",
            },
            new ProductImage
            {
                ProductId = msi4070sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F01875154-8ccb-4f77-aef1-a9722bb60aef%2Fimages%2F1770367381276_4.webp?alt=media&token=d30cccf2-deca-415c-9e54-4033bb268687",
            },
            new ProductImage
            {
                ProductId = msi4070sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F01875154-8ccb-4f77-aef1-a9722bb60aef%2Fimages%2F1770367392317_0.webp?alt=media&token=ebe2546e-797d-46c7-b988-a3a11dcb1d8a",
            }
        ]);

        // Galax RTX 4070 SUPER EX Gamer
        var galax4070sId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = galax4070sId,
            Name = "GALAX GeForce RTX 4070 SUPER EX Gamer 12GB",
            Sku = "GPU-RTX4070S-GALAX-EX-GAMER",
            CategoryId = gpuCategory.Id,
            BrandId = galaxBrand.Id,
            Price = 15990000,
            Stock = 28,
            Description = "Card đồ họa GALAX RTX 4070 SUPER EX Gamer với 12GB GDDR6X, thiết kế 2.5 slot với 3 quạt. Infinity LED Edge lighting, Xtreme Tuner Plus software. Card gaming tầm trung với hiệu năng tốt.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, galax4070sId, vram: 12, tdp: 220, length: 302, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 16-pin (12VHPWR)");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = galax4070sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3563764d-1110-43e4-b0db-d5aba85f1206%2Fimages%2F1770367738898_0.png?alt=media&token=61d303cc-e21d-47b9-a359-bc82f68bdcec",
            },
            new ProductImage
            {
                ProductId = galax4070sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3563764d-1110-43e4-b0db-d5aba85f1206%2Fimages%2F1770367749514_0.png?alt=media&token=6b100321-7f76-4cce-8bb3-1faac48f1878",
            },
            new ProductImage
            {
                ProductId = galax4070sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3563764d-1110-43e4-b0db-d5aba85f1206%2Fimages%2F1770367749515_1.png?alt=media&token=15605724-338c-4818-85cb-5e7a4404839a",
            },
            new ProductImage
            {
                ProductId = galax4070sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3563764d-1110-43e4-b0db-d5aba85f1206%2Fimages%2F1770367749515_2.png?alt=media&token=a66a3313-3c79-4658-8e21-39499c8b9857",
            },
            new ProductImage
            {
                ProductId = galax4070sId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3563764d-1110-43e4-b0db-d5aba85f1206%2Fimages%2F1770367749515_3.png?alt=media&token=972c68a4-10c5-4afd-995e-7cb6fa89e448",
            }
        ]);

        // ============= NVIDIA RTX 4070 =============

        // INNO3D RTX 4070 TWIN X2 (existing)
        var rtx4070Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = rtx4070Id,
            Name = "INNO3D GeForce RTX 4070 TWIN X2",
            Sku = "GPU-RTX4070-INNO3D-TWINX2",
            CategoryId = gpuCategory.Id,
            BrandId = inno3dBrand.Id,
            Price = 14990000,
            Stock = 25,
            Description = "Card đồ họa INNO3D GeForce RTX 4070 TWIN X2 với kiến trúc Ada Lovelace, 12GB GDDR6X, hỗ trợ Ray Tracing và DLSS 3.0. Thiết kế tản nhiệt 2 quạt hiệu quả, phù hợp cho gaming 1440p và làm việc sáng tạo.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, rtx4070Id, vram: 12, tdp: 200, length: 267, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = rtx4070Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Ff90ab568-93d8-4ed4-9eb2-938bf2a545bf%2Fimages%2F1770369799504_0.jpg?alt=media&token=22d5035b-837d-468a-99c2-8ff85a182fd1",
            },
            new ProductImage
            {
                ProductId = rtx4070Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Ff90ab568-93d8-4ed4-9eb2-938bf2a545bf%2Fimages%2F1770369807630_0.jpg?alt=media&token=e498480a-b9af-42be-b2f7-b000cbfb0f4d",
            }
        ]);

        // ASUS Dual RTX 4070
        var asus4070Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = asus4070Id,
            Name = "ASUS Dual GeForce RTX 4070 OC Edition 12GB",
            Sku = "GPU-RTX4070-ASUS-DUAL-OC",
            CategoryId = gpuCategory.Id,
            BrandId = asusBrand.Id,
            Price = 15490000,
            Stock = 32,
            Description = "Card đồ họa ASUS Dual RTX 4070 OC với 12GB GDDR6X, thiết kế 2.5 slot với 2 quạt Axial-tech. Auto-Extreme Technology, GPU Tweak III. Cân bằng tốt giữa hiệu năng và kích thước.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, asus4070Id, vram: 12, tdp: 200, length: 267, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = asus4070Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F6871af48-978b-45ba-ba03-c05a11d545eb%2Fimages%2F1770375633196_0.webp?alt=media&token=50c16279-b27e-413c-864d-9290777fea54",
            },
            new ProductImage
            {
                ProductId = asus4070Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F6871af48-978b-45ba-ba03-c05a11d545eb%2Fimages%2F1770375650881_0.webp?alt=media&token=34b54f00-064e-4e34-ba14-48d92aa128e2",
            },
            new ProductImage
            {
                ProductId = asus4070Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F6871af48-978b-45ba-ba03-c05a11d545eb%2Fimages%2F1770375650882_1.webp?alt=media&token=3af7c58b-f126-4ebb-afc2-b300220da64c",
            }
        ]);

        // Colorful iGame RTX 4070 Ultra W DUO OC
        var colorful4070Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = colorful4070Id,
            Name = "Colorful iGame GeForce RTX 4070 Ultra W DUO OC 12GB",
            Sku = "GPU-RTX4070-COLORFUL-ULTRA-W",
            CategoryId = gpuCategory.Id,
            BrandId = colorfulBrand.Id,
            Price = 14490000,
            Stock = 30,
            Description = "Card đồ họa Colorful iGame RTX 4070 Ultra W DUO OC với 12GB GDDR6X, thiết kế trắng tinh khiết với 2 quạt. iGame Center software, LED lighting effects. Card đồ họa phong cách cho build trắng.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, colorful4070Id, vram: 12, tdp: 200, length: 275, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = colorful4070Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F792cf7eb-d8e5-460c-b429-4a5a2031834b%2Fimages%2F1770375830176_0.webp?alt=media&token=dbd09acd-35d3-4b7d-833c-17f60a56cfc7",
            },
            new ProductImage
            {
                ProductId = colorful4070Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F792cf7eb-d8e5-460c-b429-4a5a2031834b%2Fimages%2F1770375846200_0.webp?alt=media&token=c389a840-a3b2-4004-bedc-8dff092cf19d",
            },
            new ProductImage
            {
                ProductId = colorful4070Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F792cf7eb-d8e5-460c-b429-4a5a2031834b%2Fimages%2F1770375846201_1.webp?alt=media&token=02323988-5702-44a7-8461-fcd5b1739a57",
            },
            new ProductImage
            {
                ProductId = colorful4070Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F792cf7eb-d8e5-460c-b429-4a5a2031834b%2Fimages%2F1770375855007_0.webp?alt=media&token=5a75708a-e848-4fd3-93fb-f75feb080504",
            }
        ]);

        // ============= NVIDIA RTX 4060 Ti =============

        // Gigabyte EAGLE OC RTX 4060 Ti
        var gigabyte4060tiId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gigabyte4060tiId,
            Name = "Gigabyte GeForce RTX 4060 Ti EAGLE OC 8G",
            Sku = "GPU-RTX4060TI-GIGABYTE-EAGLE-OC",
            CategoryId = gpuCategory.Id,
            BrandId = gigabyteBrand.Id,
            Price = 11490000,
            Stock = 35,
            Description = "Card đồ họa Gigabyte RTX 4060 Ti EAGLE OC với 8GB GDDR6, hệ thống tản nhiệt WINDFORCE với 2 quạt. Thiết kế compact, ép xung sẵn từ nhà máy. Card gaming 1080p tốt nhất phân khúc.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, gigabyte4060tiId, vram: 8, tdp: 160, length: 261, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = gigabyte4060tiId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Ffb030e8f-a3b7-4069-924c-37a8ae1f48e7%2Fimages%2F1770376876567_0.webp?alt=media&token=ef8f9db8-604d-43ad-9d7a-b021136a43da",
            },
            new ProductImage
            {
                ProductId = gigabyte4060tiId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Ffb030e8f-a3b7-4069-924c-37a8ae1f48e7%2Fimages%2F1770376886711_0.webp?alt=media&token=b37766b3-1ca5-4b13-9dcb-00b86afe5e04",
            },
            new ProductImage
            {
                ProductId = gigabyte4060tiId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Ffb030e8f-a3b7-4069-924c-37a8ae1f48e7%2Fimages%2F1770376886712_1.webp?alt=media&token=9f42e288-c2b8-4666-a3ca-93a069a3e3e2",
            },
            new ProductImage
            {
                ProductId = gigabyte4060tiId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Ffb030e8f-a3b7-4069-924c-37a8ae1f48e7%2Fimages%2F1770376886712_2.webp?alt=media&token=0f2361cf-efd0-4187-b4c1-0e7d0d378d3e",
            },
            new ProductImage
            {
                ProductId = gigabyte4060tiId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Ffb030e8f-a3b7-4069-924c-37a8ae1f48e7%2Fimages%2F1770376892953_0.webp?alt=media&token=abb356e8-6c41-4f9b-9646-edb68c0fe283",
            }
        ]);

        // PNY VERTO Dual Fan RTX 4060 Ti
        var pny4060tiId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = pny4060tiId,
            Name = "PNY GeForce RTX 4060 Ti 8GB VERTO Dual Fan",
            Sku = "GPU-RTX4060TI-PNY-VERTO",
            CategoryId = gpuCategory.Id,
            BrandId = pnyBrand.Id,
            Price = 10990000,
            Stock = 40,
            Description = "Card đồ họa PNY RTX 4060 Ti VERTO với 8GB GDDR6, thiết kế 2 quạt hiệu quả. EPIC-X RGB lighting, dual fan cooling. Giải pháp gaming tầm trung với giá tốt.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, pny4060tiId, vram: 8, tdp: 160, length: 250, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = pny4060tiId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fdeaa3fa3-59f5-4937-bce9-2a4d7ec76850%2Fimages%2F1770377077182_0.webp?alt=media&token=e975b0ee-bc4e-4b74-a734-c4f169448d57",
            },
            new ProductImage
            {
                ProductId = pny4060tiId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fdeaa3fa3-59f5-4937-bce9-2a4d7ec76850%2Fimages%2F1770377089106_0.webp?alt=media&token=e72d675b-43bf-4141-b36b-331cbdbd167e",
            },
            new ProductImage
            {
                ProductId = pny4060tiId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fdeaa3fa3-59f5-4937-bce9-2a4d7ec76850%2Fimages%2F1770377089106_1.webp?alt=media&token=d25cdd7e-44d3-472e-90a3-dd8d453bd052",
            },
            new ProductImage
            {
                ProductId = pny4060tiId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fdeaa3fa3-59f5-4937-bce9-2a4d7ec76850%2Fimages%2F1770377089107_2.webp?alt=media&token=9eb83e31-db0a-4afc-9518-e6ff65ce4197",
            },
            new ProductImage
            {
                ProductId = pny4060tiId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fdeaa3fa3-59f5-4937-bce9-2a4d7ec76850%2Fimages%2F1770377098441_0.webp?alt=media&token=48685963-c98e-42ad-bff9-916d97a92579",
            }
        ]);

        // ============= NVIDIA RTX 4060 =============

        // MSI Ventus 2X RTX 4060
        var msi4060Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = msi4060Id,
            Name = "MSI GeForce RTX 4060 VENTUS 2X BLACK 8G OC",
            Sku = "GPU-RTX4060-MSI-VENTUS-2X",
            CategoryId = gpuCategory.Id,
            BrandId = msiBrand.Id,
            Price = 8490000,
            Stock = 45,
            Description = "Card đồ họa MSI RTX 4060 VENTUS 2X BLACK OC với 8GB GDDR6, tản nhiệt 2 quạt TORX 4.0. Thiết kế nhỏ gọn phù hợp nhiều case, Zero Frozr technology. Card gaming 1080p hiệu quả.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, msi4060Id, vram: 8, tdp: 115, length: 199, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = msi4060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F515b0611-2b9c-4a3d-b9e9-723b446c7dd5%2Fimages%2F1770377478975_0.webp?alt=media&token=91b86162-899d-4c47-8c36-2f3f88be9e20",
            },
            new ProductImage
            {
                ProductId = msi4060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F515b0611-2b9c-4a3d-b9e9-723b446c7dd5%2Fimages%2F1770377486442_0.webp?alt=media&token=7a3fab64-5d0e-46e6-a6cf-b2506e715b0b",
            },
            new ProductImage
            {
                ProductId = msi4060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F515b0611-2b9c-4a3d-b9e9-723b446c7dd5%2Fimages%2F1770377486443_1.webp?alt=media&token=fb64ffd2-4001-4479-89da-e95ffdfadae3",
            },
            new ProductImage
            {
                ProductId = msi4060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F515b0611-2b9c-4a3d-b9e9-723b446c7dd5%2Fimages%2F1770377486443_2.webp?alt=media&token=72a79be8-37e6-4a42-886b-6fc5eb45e4a0",
            },
            new ProductImage
            {
                ProductId = msi4060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F515b0611-2b9c-4a3d-b9e9-723b446c7dd5%2Fimages%2F1770377492209_0.webp?alt=media&token=b8816f14-0230-487d-85df-6fe608f73e7a",
            }
        ]);

        // INNO3D RTX 4060 TWIN X2
        var inno3d4060Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = inno3d4060Id,
            Name = "INNO3D GeForce RTX 4060 TWIN X2 8GB",
            Sku = "GPU-RTX4060-INNO3D-TWINX2",
            CategoryId = gpuCategory.Id,
            BrandId = inno3dBrand.Id,
            Price = 7990000,
            Stock = 50,
            Description = "Card đồ họa INNO3D RTX 4060 TWIN X2 với 8GB GDDR6, thiết kế 2 quạt hiệu quả. Hỗ trợ DLSS 3.0, Ray Tracing. Card gaming entry-level với công nghệ mới nhất.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, inno3d4060Id, vram: 8, tdp: 115, length: 240, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange([
            new ProductImage 
            {
                ProductId = inno3d4060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fe8b8f6d5-a7b6-4439-9841-16d7abe7fa01%2Fimages%2F1770377698059_0.png?alt=media&token=87ae8900-53ac-4804-ad63-bfadb7fd852a",
            },
            new ProductImage 
            {
                ProductId = inno3d4060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fe8b8f6d5-a7b6-4439-9841-16d7abe7fa01%2Fimages%2F1770377730847_0.png?alt=media&token=a67dab92-2c8b-4db7-a428-d45b4c75b213"
            }
        ]);

        // ============= NVIDIA RTX 3060 =============

        // Palit Dual RTX 3060
        var palit3060Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = palit3060Id,
            Name = "Palit GeForce RTX 3060 Dual 12GB",
            Sku = "GPU-RTX3060-PALIT-DUAL",
            CategoryId = gpuCategory.Id,
            BrandId = palitBrand.Id,
            Price = 6490000,
            Stock = 55,
            Description = "Card đồ họa Palit RTX 3060 Dual với 12GB GDDR6, thiết kế 2 quạt TurboFan 2.0. Kiến trúc Ampere, hỗ trợ Ray Tracing và DLSS. Card gaming phổ thông với VRAM lớn.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, palit3060Id, vram: 12, tdp: 170, length: 245, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = palit3060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F2a6b5eed-3de1-4648-8ef8-8deb03f50b4c%2Fimages%2F1770377894555_0.png?alt=media&token=bb7d6834-191b-463d-ae14-22382400b454",
            },
            new ProductImage
            {
                ProductId = palit3060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F2a6b5eed-3de1-4648-8ef8-8deb03f50b4c%2Fimages%2F1770377906270_0.png?alt=media&token=c503c653-00bf-4b8e-b066-2a27fc9bef1b",
            },
            new ProductImage
            {
                ProductId = palit3060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F2a6b5eed-3de1-4648-8ef8-8deb03f50b4c%2Fimages%2F1770377906271_1.png?alt=media&token=b1b42649-7473-42cc-9b45-2cd9f240d8c0",
            },
            new ProductImage
            {
                ProductId = palit3060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F2a6b5eed-3de1-4648-8ef8-8deb03f50b4c%2Fimages%2F1770377906271_2.png?alt=media&token=aa75024d-0c71-4f47-b425-7a41f03f1d1a",
            },
            new ProductImage
            {
                ProductId = palit3060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F2a6b5eed-3de1-4648-8ef8-8deb03f50b4c%2Fimages%2F1770377913391_0.png?alt=media&token=6401c31a-7d39-4d05-ad69-bfd4fa00b232",
            }
        ]);

        // Galax RTX 3060 (1-Click OC)
        var galax3060Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = galax3060Id,
            Name = "GALAX GeForce RTX 3060 (1-Click OC) 12GB",
            Sku = "GPU-RTX3060-GALAX-1CLICK",
            CategoryId = gpuCategory.Id,
            BrandId = galaxBrand.Id,
            Price = 6290000,
            Stock = 48,
            Description = "Card đồ họa GALAX RTX 3060 (1-Click OC) với 12GB GDDR6, tính năng 1-Click OC để ép xung nhanh chóng. Thiết kế 2 quạt, Xtreme Tuner Plus software. Card gaming giá tốt.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, galax3060Id, vram: 12, tdp: 170, length: 235, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = galax3060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fc3d2398c-deae-4d97-9b7f-13a6f507b3cf%2Fimages%2F1770378300068_0.jpg?alt=media&token=866eeba0-a485-4dea-ae6c-2c84a5efc7e9",
            },
            new ProductImage
            {
                ProductId = galax3060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fc3d2398c-deae-4d97-9b7f-13a6f507b3cf%2Fimages%2F1770378308101_0.jpg?alt=media&token=c34332a7-4b1d-4078-a6af-127d744b1206",
            },
            new ProductImage
            {
                ProductId = galax3060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fc3d2398c-deae-4d97-9b7f-13a6f507b3cf%2Fimages%2F1770378308102_1.jpg?alt=media&token=d8a0df5f-34ca-4269-bd17-4699563ad584",
            },
            new ProductImage
            {
                ProductId = galax3060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fc3d2398c-deae-4d97-9b7f-13a6f507b3cf%2Fimages%2F1770378308102_2.jpg?alt=media&token=8672cec1-3782-457b-be2f-fb78c3eb04d6",
            },
            new ProductImage
            {
                ProductId = galax3060Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fc3d2398c-deae-4d97-9b7f-13a6f507b3cf%2Fimages%2F1770378355237_0.jpg?alt=media&token=ddd6778b-14e9-4697-8373-669450562e61",
            }
        ]);

        // ============= AMD RADEON RX 7900 XTX =============

        // Sapphire NITRO+ RX 7900 XTX
        var sapphire7900xtxId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = sapphire7900xtxId,
            Name = "Sapphire NITRO+ AMD Radeon RX 7900 XTX Vapor-X 24GB",
            Sku = "GPU-RX7900XTX-SAPPHIRE-NITRO-VAPORX",
            CategoryId = gpuCategory.Id,
            BrandId = sapphireBrand.Id,
            Price = 32990000,
            Stock = 12,
            Description = "Card đồ họa Sapphire NITRO+ RX 7900 XTX Vapor-X với 24GB GDDR6, hệ thống tản nhiệt Vapor-X cooling với 3 quạt. ARGB lighting, Dual BIOS. Card AMD flagship cho gaming 4K.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, sapphire7900xtxId, vram: 24, tdp: 355, length: 320, pcieSlot: "PCIe 4.0 x16", powerConnector: "2x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = sapphire7900xtxId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3f33f8ef-dec6-4367-841e-13d2b350c66c%2Fimages%2F1770378562859_0.jpg?alt=media&token=41a1f26f-5861-4889-9f92-508ad6f33ada",
            },
            new ProductImage
            {
                ProductId = sapphire7900xtxId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3f33f8ef-dec6-4367-841e-13d2b350c66c%2Fimages%2F1770378569192_0.jpg?alt=media&token=9f519985-d580-4cef-bf33-d35d303ab0e9",
            },
            new ProductImage
            {
                ProductId = sapphire7900xtxId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3f33f8ef-dec6-4367-841e-13d2b350c66c%2Fimages%2F1770378569193_1.jpg?alt=media&token=d38ef9c6-5c3e-4b5f-997a-a9b0a8033032",
            },
            new ProductImage
            {
                ProductId = sapphire7900xtxId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3f33f8ef-dec6-4367-841e-13d2b350c66c%2Fimages%2F1770378569193_2.jpg?alt=media&token=179674a1-9ef7-4df2-aa04-80f477e43ee9",
            }
        ]);

        // XFX MERC 310 RX 7900 XTX
        var xfx7900xtxId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = xfx7900xtxId,
            Name = "XFX Speedster MERC 310 AMD Radeon RX 7900 XTX 24GB",
            Sku = "GPU-RX7900XTX-XFX-MERC310",
            CategoryId = gpuCategory.Id,
            BrandId = xfxBrand.Id,
            Price = 29990000,
            Stock = 15,
            Description = "Card đồ họa XFX Speedster MERC 310 RX 7900 XTX với 24GB GDDR6, hệ thống tản nhiệt 3 quạt hiệu quả. Thiết kế chắc chắn, dual BIOS. Card AMD high-end với giá cạnh tranh.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, xfx7900xtxId, vram: 24, tdp: 355, length: 344, pcieSlot: "PCIe 4.0 x16", powerConnector: "2x 8-pin");
        productImages.AddRange([
            new ProductImage
            {
                ProductId = xfx7900xtxId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F491a53fe-f1f4-4378-9de7-ffe7fccbf58f%2Fimages%2F1770378805836_0.jpg?alt=media&token=53168d0f-2baf-4aad-a825-cc9e3cd0f504",
            },
            new ProductImage
            {
                ProductId = xfx7900xtxId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F491a53fe-f1f4-4378-9de7-ffe7fccbf58f%2Fimages%2F1770378814271_0.png?alt=media&token=8381f31a-05b8-4191-8296-516679a44e45",
            },
            new ProductImage
            {
                ProductId = xfx7900xtxId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F491a53fe-f1f4-4378-9de7-ffe7fccbf58f%2Fimages%2F1770378814272_1.jpg?alt=media&token=71bc5eff-485f-474d-a076-a72ccdd0c46f"
            }
        ]);

        // PowerColor Red Devil RX 7900 XTX
        var powercolor7900xtxId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = powercolor7900xtxId,
            Name = "PowerColor Red Devil AMD Radeon RX 7900 XTX 24GB",
            Sku = "GPU-RX7900XTX-POWERCOLOR-REDDEVIL",
            CategoryId = gpuCategory.Id,
            BrandId = powerColorBrand.Id,
            Price = 31990000,
            Stock = 10,
            Description = "Card đồ họa PowerColor Red Devil RX 7900 XTX với 24GB GDDR6, thiết kế tản nhiệt 3 quạt với Devil Zone RGB. Triple BIOS, DrMOS power stages. Card AMD cao cấp với phong cách độc đáo.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, powercolor7900xtxId, vram: 24, tdp: 355, length: 332, pcieSlot: "PCIe 4.0 x16", powerConnector: "2x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = powercolor7900xtxId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F12e5057e-8bb8-49e2-9ad0-4e47a4cbe0a9%2Fimages%2F1770378955014_0.png?alt=media&token=dfadcac7-1301-4f7d-a161-bf9e3380629a",
            },
            new ProductImage
            {
                ProductId = powercolor7900xtxId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F12e5057e-8bb8-49e2-9ad0-4e47a4cbe0a9%2Fimages%2F1770378965412_0.png?alt=media&token=c7518480-463b-40e3-ad2a-17c5d6df43de",
            },
            new ProductImage
            {
                ProductId = powercolor7900xtxId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F12e5057e-8bb8-49e2-9ad0-4e47a4cbe0a9%2Fimages%2F1770378965412_1.png?alt=media&token=d89d01d8-4c68-463e-bb26-d9516de93bf9",
            },
            new ProductImage
            {
                ProductId = powercolor7900xtxId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F12e5057e-8bb8-49e2-9ad0-4e47a4cbe0a9%2Fimages%2F1770378972685_0.png?alt=media&token=6444abb1-c2df-4b7b-afbb-72fdf004d31c",
            }
        ]);

        // ============= AMD RADEON RX 7900 XT =============

        // Sapphire PULSE RX 7900 XT
        var sapphire7900xtId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = sapphire7900xtId,
            Name = "Sapphire PULSE AMD Radeon RX 7900 XT 20GB",
            Sku = "GPU-RX7900XT-SAPPHIRE-PULSE",
            CategoryId = gpuCategory.Id,
            BrandId = sapphireBrand.Id,
            Price = 24990000,
            Stock = 18,
            Description = "Card đồ họa Sapphire PULSE RX 7900 XT với 20GB GDDR6, hệ thống tản nhiệt Dual-X với 2 quạt. Thiết kế tinh gọn hơn NITRO+, hiệu năng xuất sắc cho gaming 4K.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, sapphire7900xtId, vram: 20, tdp: 315, length: 280, pcieSlot: "PCIe 4.0 x16", powerConnector: "2x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = sapphire7900xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Ffc2b4ec3-1e7e-4cfc-9213-f2d3a37b1922%2Fimages%2F1770379136257_0.png?alt=media&token=c09c9f06-1e56-498f-81f9-ecdd49d9bf21",
            },
            new ProductImage
            {
                ProductId = sapphire7900xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Ffc2b4ec3-1e7e-4cfc-9213-f2d3a37b1922%2Fimages%2F1770379159534_0.png?alt=media&token=d4d829ef-06df-49ea-8279-aeaef0348ffa",
            },
            new ProductImage
            {
                ProductId = sapphire7900xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Ffc2b4ec3-1e7e-4cfc-9213-f2d3a37b1922%2Fimages%2F1770379159535_1.png?alt=media&token=b4f96b1a-20fb-4010-9ed7-1e486523eeb3",
            },
            new ProductImage
            {
                ProductId = sapphire7900xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Ffc2b4ec3-1e7e-4cfc-9213-f2d3a37b1922%2Fimages%2F1770379159535_2.png?alt=media&token=0cb21891-0ed4-4731-96aa-1e7d4625c896",
            }
        ]);

        // ASUS TUF Gaming RX 7900 XT
        var asus7900xtId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = asus7900xtId,
            Name = "ASUS TUF Gaming Radeon RX 7900 XT OC Edition 20GB",
            Sku = "GPU-RX7900XT-ASUS-TUF-OC",
            CategoryId = gpuCategory.Id,
            BrandId = asusBrand.Id,
            Price = 26990000,
            Stock = 14,
            Description = "Card đồ họa ASUS TUF Gaming RX 7900 XT OC với 20GB GDDR6, thiết kế bền bỉ chuẩn quân sự. Tản nhiệt 3 quạt Axial-tech, GPU Tweak III. Card AMD bền bỉ cho gaming enthusiast.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, asus7900xtId, vram: 20, tdp: 315, length: 353, pcieSlot: "PCIe 4.0 x16", powerConnector: "2x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = asus7900xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F06c7d294-9cb9-402f-8c99-d94362c2f6f1%2Fimages%2F1770379400673_0.jpg?alt=media&token=952bfcf3-9766-4b84-8b9a-79e8cd81cb83",
            },
            new ProductImage
            {
                ProductId = asus7900xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F06c7d294-9cb9-402f-8c99-d94362c2f6f1%2Fimages%2F1770379400674_1.jpg?alt=media&token=5a3c8992-762d-4576-afdb-d08d9122b8eb",
            },
            new ProductImage
            {
                ProductId = asus7900xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F06c7d294-9cb9-402f-8c99-d94362c2f6f1%2Fimages%2F1770379400675_2.png?alt=media&token=f6448c42-c7dc-4f75-966d-5ca466d213d9",
            },
            new ProductImage
            {
                ProductId = asus7900xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F06c7d294-9cb9-402f-8c99-d94362c2f6f1%2Fimages%2F1770379400675_3.png?alt=media&token=be9f4cdf-8535-4509-92c3-dcd52addce94",
            }
        ]);

        // ============= AMD RADEON RX 7800 XT =============

        // PowerColor Red Dragon RX 7800 XT
        var powercolor7800xtId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = powercolor7800xtId,
            Name = "PowerColor Red Dragon AMD Radeon RX 7800 XT 16GB",
            Sku = "GPU-RX7800XT-POWERCOLOR-REDDRAGON",
            CategoryId = gpuCategory.Id,
            BrandId = powerColorBrand.Id,
            Price = 14990000,
            Stock = 25,
            Description = "Card đồ họa PowerColor Red Dragon RX 7800 XT với 16GB GDDR6, thiết kế 3 quạt hiệu quả. Card AMD tầm trung cao cấp, cạnh tranh trực tiếp với RTX 4070.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, powercolor7800xtId, vram: 16, tdp: 263, length: 305, pcieSlot: "PCIe 4.0 x16", powerConnector: "2x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = powercolor7800xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fda469173-4ef0-4c36-9770-f3b3455fe27c%2Fimages%2F1770379703762_0.jpg?alt=media&token=1fe19870-271f-43ba-9d45-f2efdb967505",
            },
            new ProductImage
            {
                ProductId = powercolor7800xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fda469173-4ef0-4c36-9770-f3b3455fe27c%2Fimages%2F1770379703763_1.png?alt=media&token=91663a3c-8c53-4e7f-aa10-5d1cc5ff30d8",
            },
            new ProductImage
            {
                ProductId = powercolor7800xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fda469173-4ef0-4c36-9770-f3b3455fe27c%2Fimages%2F1770379703763_2.png?alt=media&token=57868b67-d4d8-452e-89e5-5f29af1e2ca2",
            },
            new ProductImage
            {
                ProductId = powercolor7800xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fda469173-4ef0-4c36-9770-f3b3455fe27c%2Fimages%2F1770379703763_3.png?alt=media&token=66a07502-c5f0-4aa4-9025-680af920a1c8",
            }
        ]);

        // XFX QICK 319 RX 7800 XT
        var xfx7800xtId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = xfx7800xtId,
            Name = "XFX Speedster QICK 319 AMD Radeon RX 7800 XT 16GB",
            Sku = "GPU-RX7800XT-XFX-QICK319",
            CategoryId = gpuCategory.Id,
            BrandId = xfxBrand.Id,
            Price = 13990000,
            Stock = 28,
            Description = "Card đồ họa XFX Speedster QICK 319 RX 7800 XT với 16GB GDDR6, hệ thống tản nhiệt 3 quạt. Thiết kế chắc chắn, hiệu năng tuyệt vời cho gaming 1440p.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, xfx7800xtId, vram: 16, tdp: 263, length: 322, pcieSlot: "PCIe 4.0 x16", powerConnector: "2x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = xfx7800xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F4ec9b0d0-95fe-43cb-9f61-c44323b5e453%2Fimages%2F1770379837220_0.jpg?alt=media&token=d942a6d6-3c1f-4961-b6c1-ced2ff37ceea",
            },
            new ProductImage
            {
                ProductId = xfx7800xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F4ec9b0d0-95fe-43cb-9f61-c44323b5e453%2Fimages%2F1770379837221_1.jpg?alt=media&token=aae90839-de6c-4a4e-8b6c-dd70ea93418f",
            },
            new ProductImage
            {
                ProductId = xfx7800xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F4ec9b0d0-95fe-43cb-9f61-c44323b5e453%2Fimages%2F1770379837221_2.jpg?alt=media&token=6244c218-51c9-49f6-a791-ec504b3caa28",
            },
            new ProductImage
            {
                ProductId = xfx7800xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F4ec9b0d0-95fe-43cb-9f61-c44323b5e453%2Fimages%2F1770379837221_3.jpg?alt=media&token=612f2d35-1000-4f7b-9e74-bac8aeab5baa",
            }
        ]);

        // Sapphire NITRO+ RX 7800 XT
        var sapphire7800xtId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = sapphire7800xtId,
            Name = "Sapphire NITRO+ AMD Radeon RX 7800 XT 16GB",
            Sku = "GPU-RX7800XT-SAPPHIRE-NITRO",
            CategoryId = gpuCategory.Id,
            BrandId = sapphireBrand.Id,
            Price = 15490000,
            Stock = 22,
            Description = "Card đồ họa Sapphire NITRO+ RX 7800 XT với 16GB GDDR6, hệ thống tản nhiệt Tri-X với 3 quạt. ARGB Fan, Dual BIOS. Card AMD tầm trung cao cấp với build quality tuyệt vời.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, sapphire7800xtId, vram: 16, tdp: 263, length: 310, pcieSlot: "PCIe 4.0 x16", powerConnector: "2x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = sapphire7800xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F47ebaa04-5794-48fc-8b2e-6016dfa58345%2Fimages%2F1770380002749_0.jpg?alt=media&token=362f369d-d148-4c79-9053-edb4a3633fbc",
            },
            new ProductImage
            {
                ProductId = sapphire7800xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F47ebaa04-5794-48fc-8b2e-6016dfa58345%2Fimages%2F1770380002750_1.jpg?alt=media&token=cf4d9d32-36bc-4a2c-9263-df9d7064ed96",
            },
            new ProductImage
            {
                ProductId = sapphire7800xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F47ebaa04-5794-48fc-8b2e-6016dfa58345%2Fimages%2F1770380002750_2.jpg?alt=media&token=2f7c5c85-cb38-48ef-b451-8640cf8f5173",
            },
            new ProductImage
            {
                ProductId = sapphire7800xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F47ebaa04-5794-48fc-8b2e-6016dfa58345%2Fimages%2F1770380002750_3.jpg?alt=media&token=04837839-8417-496a-8a1e-f24686a24d03",
            }
        ]);

        // ============= AMD RADEON RX 7700 XT =============

        // Gigabyte Gaming OC RX 7700 XT
        var gigabyte7700xtId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gigabyte7700xtId,
            Name = "Gigabyte Radeon RX 7700 XT GAMING OC 12G",
            Sku = "GPU-RX7700XT-GIGABYTE-GAMING-OC",
            CategoryId = gpuCategory.Id,
            BrandId = gigabyteBrand.Id,
            Price = 11990000,
            Stock = 30,
            Description = "Card đồ họa Gigabyte RX 7700 XT GAMING OC với 12GB GDDR6, hệ thống tản nhiệt WINDFORCE với 3 quạt. RGB Fusion 2.0, ép xung sẵn. Card AMD tầm trung tốt cho gaming 1440p.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, gigabyte7700xtId, vram: 12, tdp: 245, length: 302, pcieSlot: "PCIe 4.0 x16", powerConnector: "2x 8-pin");
        productImages.AddRange(
        [
            new ProductImage
            {
                ProductId = gigabyte7700xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3574c2a5-4f6a-4c0e-b733-fb94a4ebb7bf%2Fimages%2F1770380152573_0.webp?alt=media&token=b7fa3b5c-7e96-444f-a453-3f1760defcea",
            },
            new ProductImage
            {
                ProductId = gigabyte7700xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3574c2a5-4f6a-4c0e-b733-fb94a4ebb7bf%2Fimages%2F1770380152574_1.webp?alt=media&token=248d32e3-c147-40e6-bf65-aad750894243",
            },
            new ProductImage
            {
                ProductId = gigabyte7700xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3574c2a5-4f6a-4c0e-b733-fb94a4ebb7bf%2Fimages%2F1770380152574_2.webp?alt=media&token=676ea73d-e58d-4152-ad34-41faac213b46",
            },
            new ProductImage
            {
                ProductId = gigabyte7700xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3574c2a5-4f6a-4c0e-b733-fb94a4ebb7bf%2Fimages%2F1770380152574_3.webp?alt=media&token=6702ac71-d3c5-46b8-9e75-62e22e6edc4b",
            },
            new ProductImage
            {
                ProductId = gigabyte7700xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3574c2a5-4f6a-4c0e-b733-fb94a4ebb7bf%2Fimages%2F1770380152574_4.webp?alt=media&token=8dfd80a2-e9b2-493b-91c3-365130adcf63",
            },
            new ProductImage
            {
                ProductId = gigabyte7700xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F3574c2a5-4f6a-4c0e-b733-fb94a4ebb7bf%2Fimages%2F1770380152575_5.webp?alt=media&token=1468bba2-4847-480d-ac9f-abf9dab37d49",
            }
        ]);

        // MSI Gaming X RX 7700 XT
        var msi7700xtId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = msi7700xtId,
            Name = "MSI Radeon RX 7700 XT GAMING X TRIO 12G",
            Sku = "GPU-RX7700XT-MSI-GAMING-X-TRIO",
            CategoryId = gpuCategory.Id,
            BrandId = msiBrand.Id,
            Price = 12490000,
            Stock = 26,
            Description = "Card đồ họa MSI RX 7700 XT GAMING X TRIO với 12GB GDDR6, tản nhiệt TRI FROZR 3 với 3 quạt TORX 5.0. Mystic Light RGB, Zero Frozr. Card AMD gaming với thiết kế cao cấp.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, msi7700xtId, vram: 12, tdp: 245, length: 325, pcieSlot: "PCIe 4.0 x16", powerConnector: "2x 8-pin");
        productImages.Add(new ProductImage { ProductId = msi7700xtId, ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fcd2bd72a-224e-4ddc-8488-029769b01d3c%2Fimages%2F1770380448566_0.png?alt=media&token=608c1977-26d8-4eba-ab8d-2d94b9b999a6" });

        // ============= AMD RADEON RX 7600 =============

        // Sapphire PULSE RX 7600
        var sapphire7600Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = sapphire7600Id,
            Name = "Sapphire PULSE AMD Radeon RX 7600 8GB",
            Sku = "GPU-RX7600-SAPPHIRE-PULSE",
            CategoryId = gpuCategory.Id,
            BrandId = sapphireBrand.Id,
            Price = 7490000,
            Stock = 40,
            Description = "Card đồ họa Sapphire PULSE RX 7600 với 8GB GDDR6, hệ thống tản nhiệt Dual-X với 2 quạt. Thiết kế compact, hiệu năng tốt cho gaming 1080p. Card AMD entry-level thế hệ mới.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, sapphire7600Id, vram: 8, tdp: 165, length: 260, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange([
            new ProductImage
            {
                ProductId = sapphire7600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F2d057a06-7c34-4af9-a2e7-a6c2a946c2bd%2Fimages%2F1770380695870_0.webp?alt=media&token=3cd985a0-44f3-457a-8d24-c57fecb4c5e6",
            },
            new ProductImage
            {
                ProductId = sapphire7600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F2d057a06-7c34-4af9-a2e7-a6c2a946c2bd%2Fimages%2F1770380695871_1.webp?alt=media&token=88e46f69-7b99-4da7-aaca-b4bc58baf385",
            },
            new ProductImage
            {
                ProductId = sapphire7600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F2d057a06-7c34-4af9-a2e7-a6c2a946c2bd%2Fimages%2F1770380695871_2.webp?alt=media&token=57de3ba4-c5da-4161-9507-b437b126c962"
            },
            new ProductImage
            {
                ProductId = sapphire7600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F2d057a06-7c34-4af9-a2e7-a6c2a946c2bd%2Fimages%2F1770380695872_3.webp?alt=media&token=8ce0e0f5-5e62-49eb-bf5f-091cb7fb325f"
            }
        ]);

        // PowerColor Fighter RX 7600
        var powercolor7600Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = powercolor7600Id,
            Name = "PowerColor Fighter AMD Radeon RX 7600 8GB",
            Sku = "GPU-RX7600-POWERCOLOR-FIGHTER",
            CategoryId = gpuCategory.Id,
            BrandId = powerColorBrand.Id,
            Price = 6990000,
            Stock = 45,
            Description = "Card đồ họa PowerColor Fighter RX 7600 với 8GB GDDR6, thiết kế 2 quạt hiệu quả. Card AMD giá tốt cho gaming 1080p với kiến trúc RDNA 3 mới nhất.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, powercolor7600Id, vram: 8, tdp: 165, length: 256, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange([
            new ProductImage
            {
                ProductId = powercolor7600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fe8d59ae9-a1e8-4c62-bd92-3ad129cd845e%2Fimages%2F1770380887432_0.jpg?alt=media&token=92f6caa0-3f7d-4339-a1a4-9d52a494a83e",
            },
            new ProductImage
            {
                ProductId = powercolor7600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fe8d59ae9-a1e8-4c62-bd92-3ad129cd845e%2Fimages%2F1770380887433_1.jpg?alt=media&token=1fdc7008-d5f9-4f1a-b038-4a7f1db44a90",
            },
            new ProductImage
            {
                ProductId = powercolor7600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fe8d59ae9-a1e8-4c62-bd92-3ad129cd845e%2Fimages%2F1770380887433_2.jpg?alt=media&token=09380c2f-df85-41fe-bfd6-873738dba110"
            }
        ]);

        // XFX SWFT 210 RX 7600
        var xfx7600Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = xfx7600Id,
            Name = "XFX Speedster SWFT 210 AMD Radeon RX 7600 8GB",
            Sku = "GPU-RX7600-XFX-SWFT210",
            CategoryId = gpuCategory.Id,
            BrandId = xfxBrand.Id,
            Price = 6790000,
            Stock = 48,
            Description = "Card đồ họa XFX Speedster SWFT 210 RX 7600 với 8GB GDDR6, thiết kế 2 quạt nhỏ gọn. Dual BIOS, build quality tốt. Card AMD budget-friendly cho gamer.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, xfx7600Id, vram: 8, tdp: 165, length: 240, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange([
            new ProductImage
            {
                ProductId = xfx7600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F88a37c98-8169-4beb-b666-5ac86d27b8e3%2Fimages%2F1770381050261_0.jpg?alt=media&token=41a1abee-40a9-4426-92d6-2868bdafc83f",
            },
            new ProductImage
            {
                ProductId = xfx7600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F88a37c98-8169-4beb-b666-5ac86d27b8e3%2Fimages%2F1770381050262_1.jpg?alt=media&token=6a67253d-7171-491b-8666-2590b72433e1",
            },
            new ProductImage 
            {
                ProductId = xfx7600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F88a37c98-8169-4beb-b666-5ac86d27b8e3%2Fimages%2F1770381050263_2.jpg?alt=media&token=ad76cbeb-2806-4b3a-b267-fe2544b23e8c"
            }
        ]);

        // ============= AMD RADEON RX 6700 XT =============

        // Sapphire NITRO+ RX 6700 XT
        var sapphire6700xtId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = sapphire6700xtId,
            Name = "Sapphire NITRO+ AMD Radeon RX 6700 XT 12GB",
            Sku = "GPU-RX6700XT-SAPPHIRE-NITRO",
            CategoryId = gpuCategory.Id,
            BrandId = sapphireBrand.Id,
            Price = 8990000,
            Stock = 35,
            Description = "Card đồ họa Sapphire NITRO+ RX 6700 XT với 12GB GDDR6, hệ thống tản nhiệt Tri-X với 3 quạt. ARGB LED, Dual BIOS. Card AMD tầm trung thế hệ trước với VRAM lớn.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, sapphire6700xtId, vram: 12, tdp: 230, length: 310, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin + 1x 6-pin");
        productImages.AddRange([
            new ProductImage
            {
                ProductId = sapphire6700xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fbcc48a31-bd28-4582-ae67-033cbd9ae713%2Fimages%2F1770381230135_0.jpg?alt=media&token=eab931c1-523d-4ec3-8a97-310ce07f4d51",
            },
            new ProductImage
            {
                ProductId = sapphire6700xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fbcc48a31-bd28-4582-ae67-033cbd9ae713%2Fimages%2F1770381230136_1.jpg?alt=media&token=61d7f002-30cf-413e-a7f2-761f16033ca9",
            },
            new ProductImage
            {
                ProductId = sapphire6700xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fbcc48a31-bd28-4582-ae67-033cbd9ae713%2Fimages%2F1770381230136_2.jpg?alt=media&token=d78efce6-620a-462d-a508-b787b6d9e410",
            },
            new ProductImage
            {
                ProductId = sapphire6700xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fbcc48a31-bd28-4582-ae67-033cbd9ae713%2Fimages%2F1770381230137_3.jpg?alt=media&token=a773446f-3b67-4a3e-a792-171a5a9c5f9b",
            },
            new ProductImage
            {
                ProductId = sapphire6700xtId,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fbcc48a31-bd28-4582-ae67-033cbd9ae713%2Fimages%2F1770381230137_4.jpg?alt=media&token=a377261a-e4e4-4301-a364-2b580f6a002e"
            }
        ]);

        // ============= AMD RADEON RX 6600 =============

        // PowerColor Fighter RX 6600
        var powercolor6600Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = powercolor6600Id,
            Name = "PowerColor Fighter AMD Radeon RX 6600 8GB",
            Sku = "GPU-RX6600-POWERCOLOR-FIGHTER",
            CategoryId = gpuCategory.Id,
            BrandId = powerColorBrand.Id,
            Price = 5490000,
            Stock = 50,
            Description = "Card đồ họa PowerColor Fighter RX 6600 với 8GB GDDR6, thiết kế 2 quạt nhỏ gọn. Card AMD budget cho gaming 1080p, tiêu thụ điện năng thấp.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, powercolor6600Id, vram: 8, tdp: 132, length: 230, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange([
            new ProductImage
            {
                ProductId = powercolor6600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F20b7a191-43b6-48e5-b936-fd74c07db05d%2Fimages%2F1770381575698_0.jpg?alt=media&token=b53b1a07-7a6c-4ded-805f-a4639f8283b3",
            },
            new ProductImage 
            {
                ProductId = powercolor6600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F20b7a191-43b6-48e5-b936-fd74c07db05d%2Fimages%2F1770381575699_1.jpg?alt=media&token=6c015867-2692-452a-aeb2-ebe061016701",
            },
            new ProductImage
            {
                ProductId = powercolor6600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F20b7a191-43b6-48e5-b936-fd74c07db05d%2Fimages%2F1770381575699_2.jpg?alt=media&token=e264001f-9900-4300-b4b7-8beef40cb95f",
            },
            new ProductImage
            {
                ProductId = powercolor6600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F20b7a191-43b6-48e5-b936-fd74c07db05d%2Fimages%2F1770381575700_3.jpg?alt=media&token=1d004073-bce3-4a95-9335-823bc320dbb1",
            },
            new ProductImage
            {
                ProductId = powercolor6600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2F20b7a191-43b6-48e5-b936-fd74c07db05d%2Fimages%2F1770381575700_4.jpg?alt=media&token=b6617040-3b42-4c1c-85a6-d2a642316c7e"
            }
        ]);

        // XFX SWFT 210 RX 6600
        var xfx6600Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = xfx6600Id,
            Name = "XFX Speedster SWFT 210 AMD Radeon RX 6600 8GB",
            Sku = "GPU-RX6600-XFX-SWFT210",
            CategoryId = gpuCategory.Id,
            BrandId = xfxBrand.Id,
            Price = 5290000,
            Stock = 55,
            Description = "Card đồ họa XFX Speedster SWFT 210 RX 6600 với 8GB GDDR6, thiết kế 2 quạt hiệu quả. Card AMD entry-level giá tốt nhất cho gaming 1080p.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddGpuSpecs(specValues, gpuSpecs, xfx6600Id, vram: 8, tdp: 132, length: 235, pcieSlot: "PCIe 4.0 x16", powerConnector: "1x 8-pin");
        productImages.AddRange([
            new ProductImage
            {
                ProductId = xfx6600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fd08a4dcc-f3d3-43eb-bc39-764acda665d2%2Fimages%2F1770382033222_0.jpg?alt=media&token=6364b26b-d43f-4882-9460-561352ec774b",
            },
            new ProductImage
            {
                ProductId = xfx6600Id,
                ImageUrl = "https://firebasestorage.googleapis.com/v0/b/tech-express-storage-3f755.firebasestorage.app/o/products%2Fd08a4dcc-f3d3-43eb-bc39-764acda665d2%2Fimages%2F1770382033223_1.jpg?alt=media&token=6a207190-f60b-4340-8c21-5762fb35739e"
            }
        ]);
    }

    private static async Task InitCpuProducts(ApplicationDbContext context, List<ProductSpecValue> specValues, List<ProductImage> productImages)
    {
        var cpuCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "CPU")
            ?? throw new NotFoundException("Không tìm thấy danh mục CPU");

        var cpuSpecs = await context.SpecDefinitions
            .Where(s => s.CategoryId == cpuCategory.Id)
            .ToListAsync();

        var intelBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Intel")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Intel");
        var amdBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "AMD")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu AMD");

        // ============= INTEL CPUs =============

        // Intel Core i3-12100F (Low-end, no iGPU)
        var i3_12100f_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = i3_12100f_Id,
            Name = "Intel Core i3-12100F",
            Sku = "CPU-INTEL-I3-12100F",
            CategoryId = cpuCategory.Id,
            BrandId = intelBrand.Id,
            Price = 2290000,
            Stock = 50,
            Description = "Bộ vi xử lý Intel Core i3-12100F thế hệ 12 Alder Lake, 4 nhân 8 luồng, xung nhịp cơ bản 3.3GHz, turbo lên đến 4.3GHz. Lựa chọn tiết kiệm cho PC văn phòng và gaming nhẹ.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, i3_12100f_Id, socket: "LGA1700", cores: 4, threads: 8, baseClock: 3.3m, boostClock: 4.3m, tdp: 58, memoryType: "DDR4, DDR5", hasIgpu: false);
        productImages.Add(new ProductImage { ProductId = i3_12100f_Id, ImageUrl = "https://ark.intel.com/content/dam/ark/assets/images/box-shots/Intel%20Core%2012th%20Gen%20i3.png" });

        // Intel Core i3-14100 (Low-end, with iGPU)
        var i3_14100_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = i3_14100_Id,
            Name = "Intel Core i3-14100",
            Sku = "CPU-INTEL-I3-14100",
            CategoryId = cpuCategory.Id,
            BrandId = intelBrand.Id,
            Price = 3190000,
            Stock = 40,
            Description = "Bộ vi xử lý Intel Core i3-14100 thế hệ 14, 4 nhân 8 luồng, xung nhịp cơ bản 3.5GHz, turbo lên đến 4.7GHz. Tích hợp Intel UHD Graphics 730, phù hợp cho PC văn phòng và giải trí.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, i3_14100_Id, socket: "LGA1700", cores: 4, threads: 8, baseClock: 3.5m, boostClock: 4.7m, tdp: 60, memoryType: "DDR4, DDR5", hasIgpu: true);
        productImages.Add(new ProductImage { ProductId = i3_14100_Id, ImageUrl = "https://ark.intel.com/content/dam/ark/assets/images/box-shots/Intel%20Core%2014th%20Gen%20i3.png" });

        // Intel Core i5-12400F (Mid-range, no iGPU)
        var i5_12400f_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = i5_12400f_Id,
            Name = "Intel Core i5-12400F",
            Sku = "CPU-INTEL-I5-12400F",
            CategoryId = cpuCategory.Id,
            BrandId = intelBrand.Id,
            Price = 3590000,
            Stock = 60,
            Description = "Bộ vi xử lý Intel Core i5-12400F thế hệ 12, 6 nhân 12 luồng, xung nhịp cơ bản 2.5GHz, turbo lên đến 4.4GHz. CPU gaming phổ thông với hiệu năng đơn nhân xuất sắc.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, i5_12400f_Id, socket: "LGA1700", cores: 6, threads: 12, baseClock: 2.5m, boostClock: 4.4m, tdp: 65, memoryType: "DDR4, DDR5", hasIgpu: false);
        productImages.Add(new ProductImage { ProductId = i5_12400f_Id, ImageUrl = "https://ark.intel.com/content/dam/ark/assets/images/box-shots/Intel%20Core%2012th%20Gen%20i5.png" });

        // Intel Core i5-14400F (Mid-range, no iGPU)
        var i5_14400f_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = i5_14400f_Id,
            Name = "Intel Core i5-14400F",
            Sku = "CPU-INTEL-I5-14400F",
            CategoryId = cpuCategory.Id,
            BrandId = intelBrand.Id,
            Price = 4990000,
            Stock = 45,
            Description = "Bộ vi xử lý Intel Core i5-14400F thế hệ 14, 10 nhân (6P+4E) 16 luồng, xung nhịp cơ bản 2.5GHz, turbo lên đến 4.7GHz. Hiệu năng đa nhiệm tốt với kiến trúc hybrid.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, i5_14400f_Id, socket: "LGA1700", cores: 10, threads: 16, baseClock: 2.5m, boostClock: 4.7m, tdp: 65, memoryType: "DDR4, DDR5", hasIgpu: false);
        productImages.Add(new ProductImage { ProductId = i5_14400f_Id, ImageUrl = "https://ark.intel.com/content/dam/ark/assets/images/box-shots/Intel%20Core%2014th%20Gen%20i5.png" });

        // Intel Core i5-14600K (Mid-high, with iGPU, unlocked)
        var i5_14600k_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = i5_14600k_Id,
            Name = "Intel Core i5-14600K",
            Sku = "CPU-INTEL-I5-14600K",
            CategoryId = cpuCategory.Id,
            BrandId = intelBrand.Id,
            Price = 7490000,
            Stock = 35,
            Description = "Bộ vi xử lý Intel Core i5-14600K thế hệ 14 unlocked, 14 nhân (6P+8E) 20 luồng, xung nhịp cơ bản 3.5GHz, turbo lên đến 5.3GHz. CPU gaming tầm trung cao cấp, hỗ trợ ép xung.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, i5_14600k_Id, socket: "LGA1700", cores: 14, threads: 20, baseClock: 3.5m, boostClock: 5.3m, tdp: 125, memoryType: "DDR4, DDR5", hasIgpu: true);
        productImages.Add(new ProductImage { ProductId = i5_14600k_Id, ImageUrl = "https://ark.intel.com/content/dam/ark/assets/images/box-shots/Intel%20Core%2014th%20Gen%20i5%20K.png" });

        // Intel Core i7-14700K (High-end, with iGPU, unlocked)
        var i7_14700k_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = i7_14700k_Id,
            Name = "Intel Core i7-14700K",
            Sku = "CPU-INTEL-I7-14700K",
            CategoryId = cpuCategory.Id,
            BrandId = intelBrand.Id,
            Price = 10490000,
            Stock = 30,
            Description = "Bộ vi xử lý Intel Core i7-14700K thế hệ 14 unlocked, 20 nhân (8P+12E) 28 luồng, xung nhịp cơ bản 3.4GHz, turbo lên đến 5.6GHz. CPU cao cấp cho gaming và sáng tạo nội dung.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, i7_14700k_Id, socket: "LGA1700", cores: 20, threads: 28, baseClock: 3.4m, boostClock: 5.6m, tdp: 125, memoryType: "DDR4, DDR5", hasIgpu: true);
        productImages.Add(new ProductImage { ProductId = i7_14700k_Id, ImageUrl = "https://ark.intel.com/content/dam/ark/assets/images/box-shots/Intel%20Core%2014th%20Gen%20i7%20K.png" });

        // Intel Core i9-14900K (Flagship, with iGPU, unlocked)
        var i9_14900k_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = i9_14900k_Id,
            Name = "Intel Core i9-14900K",
            Sku = "CPU-INTEL-I9-14900K",
            CategoryId = cpuCategory.Id,
            BrandId = intelBrand.Id,
            Price = 14990000,
            Stock = 20,
            Description = "Bộ vi xử lý Intel Core i9-14900K thế hệ 14 flagship unlocked, 24 nhân (8P+16E) 32 luồng, xung nhịp cơ bản 3.2GHz, turbo lên đến 6.0GHz. CPU desktop mạnh nhất của Intel cho gaming và workstation.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, i9_14900k_Id, socket: "LGA1700", cores: 24, threads: 32, baseClock: 3.2m, boostClock: 6.0m, tdp: 125, memoryType: "DDR4, DDR5", hasIgpu: true);
        productImages.Add(new ProductImage { ProductId = i9_14900k_Id, ImageUrl = "https://ark.intel.com/content/dam/ark/assets/images/box-shots/Intel%20Core%2014th%20Gen%20i9%20K.png" });

        // Intel Xeon W3-2423 (Entry Workstation)
        var xeon_w3_2423_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = xeon_w3_2423_Id,
            Name = "Intel Xeon W3-2423",
            Sku = "CPU-INTEL-XEON-W3-2423",
            CategoryId = cpuCategory.Id,
            BrandId = intelBrand.Id,
            Price = 8990000,
            Stock = 15,
            Description = "Bộ vi xử lý Intel Xeon W3-2423 cho workstation, 6 nhân 12 luồng, xung nhịp cơ bản 2.1GHz, turbo lên đến 4.2GHz. Hỗ trợ bộ nhớ ECC, phù hợp cho máy trạm chuyên nghiệp.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, xeon_w3_2423_Id, socket: "LGA4677", cores: 6, threads: 12, baseClock: 2.1m, boostClock: 4.2m, tdp: 120, memoryType: "DDR5 ECC", hasIgpu: false);
        productImages.Add(new ProductImage { ProductId = xeon_w3_2423_Id, ImageUrl = "https://ark.intel.com/content/dam/ark/assets/images/box-shots/Intel%20Xeon%20W.png" });

        // Intel Xeon W5-2455X (Mid Workstation)
        var xeon_w5_2455x_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = xeon_w5_2455x_Id,
            Name = "Intel Xeon W5-2455X",
            Sku = "CPU-INTEL-XEON-W5-2455X",
            CategoryId = cpuCategory.Id,
            BrandId = intelBrand.Id,
            Price = 24990000,
            Stock = 10,
            Description = "Bộ vi xử lý Intel Xeon W5-2455X cho workstation cao cấp, 12 nhân 24 luồng, xung nhịp cơ bản 3.2GHz, turbo lên đến 4.6GHz. Hiệu năng mạnh mẽ cho render, mô phỏng và tính toán khoa học.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, xeon_w5_2455x_Id, socket: "LGA4677", cores: 12, threads: 24, baseClock: 3.2m, boostClock: 4.6m, tdp: 200, memoryType: "DDR5 ECC", hasIgpu: false);
        productImages.Add(new ProductImage { ProductId = xeon_w5_2455x_Id, ImageUrl = "https://ark.intel.com/content/dam/ark/assets/images/box-shots/Intel%20Xeon%20W.png" });

        // Intel Xeon W9-3495X (High-end Workstation)
        var xeon_w9_3495x_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = xeon_w9_3495x_Id,
            Name = "Intel Xeon W9-3495X",
            Sku = "CPU-INTEL-XEON-W9-3495X",
            CategoryId = cpuCategory.Id,
            BrandId = intelBrand.Id,
            Price = 129990000,
            Stock = 5,
            Description = "Bộ vi xử lý Intel Xeon W9-3495X flagship cho workstation, 56 nhân 112 luồng, xung nhịp cơ bản 1.9GHz, turbo lên đến 4.8GHz. CPU workstation mạnh nhất của Intel, 105MB cache, hỗ trợ 4TB RAM.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, xeon_w9_3495x_Id, socket: "LGA4677", cores: 56, threads: 112, baseClock: 1.9m, boostClock: 4.8m, tdp: 350, memoryType: "DDR5 ECC", hasIgpu: false);
        productImages.Add(new ProductImage { ProductId = xeon_w9_3495x_Id, ImageUrl = "https://ark.intel.com/content/dam/ark/assets/images/box-shots/Intel%20Xeon%20W.png" });

        // ============= AMD CPUs =============

        // AMD Ryzen 3 4100 (Low-end, no iGPU)
        var r3_4100_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = r3_4100_Id,
            Name = "AMD Ryzen 3 4100",
            Sku = "CPU-AMD-RYZEN3-4100",
            CategoryId = cpuCategory.Id,
            BrandId = amdBrand.Id,
            Price = 1890000,
            Stock = 45,
            Description = "Bộ vi xử lý AMD Ryzen 3 4100 kiến trúc Zen 2, 4 nhân 8 luồng, xung nhịp cơ bản 3.8GHz, turbo lên đến 4.0GHz. Lựa chọn tiết kiệm nhất cho PC gaming entry-level.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, r3_4100_Id, socket: "AM4", cores: 4, threads: 8, baseClock: 3.8m, boostClock: 4.0m, tdp: 65, memoryType: "DDR4", hasIgpu: false);
        productImages.Add(new ProductImage { ProductId = r3_4100_Id, ImageUrl = "https://www.amd.com/content/dam/amd/en/images/products/processors/ryzen/2505503-ryzen-3-702x702.png" });

        // AMD Ryzen 5 5600G (Low-mid, with iGPU - APU)
        var r5_5600g_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = r5_5600g_Id,
            Name = "AMD Ryzen 5 5600G",
            Sku = "CPU-AMD-RYZEN5-5600G",
            CategoryId = cpuCategory.Id,
            BrandId = amdBrand.Id,
            Price = 3290000,
            Stock = 55,
            Description = "Bộ vi xử lý AMD Ryzen 5 5600G APU kiến trúc Zen 3, 6 nhân 12 luồng, xung nhịp cơ bản 3.9GHz, turbo lên đến 4.4GHz. Tích hợp Radeon Vega 7 Graphics, có thể chơi game nhẹ mà không cần card rời.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, r5_5600g_Id, socket: "AM4", cores: 6, threads: 12, baseClock: 3.9m, boostClock: 4.4m, tdp: 65, memoryType: "DDR4", hasIgpu: true);
        productImages.Add(new ProductImage { ProductId = r5_5600g_Id, ImageUrl = "https://www.amd.com/content/dam/amd/en/images/products/processors/ryzen/2505503-ryzen-5-702x702.png" });

        // AMD Ryzen 5 5600X (Mid-range, no iGPU)
        var r5_5600x_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = r5_5600x_Id,
            Name = "AMD Ryzen 5 5600X",
            Sku = "CPU-AMD-RYZEN5-5600X",
            CategoryId = cpuCategory.Id,
            BrandId = amdBrand.Id,
            Price = 3790000,
            Stock = 50,
            Description = "Bộ vi xử lý AMD Ryzen 5 5600X kiến trúc Zen 3, 6 nhân 12 luồng, xung nhịp cơ bản 3.7GHz, turbo lên đến 4.6GHz. CPU gaming tầm trung huyền thoại với hiệu năng đơn nhân xuất sắc.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, r5_5600x_Id, socket: "AM4", cores: 6, threads: 12, baseClock: 3.7m, boostClock: 4.6m, tdp: 65, memoryType: "DDR4", hasIgpu: false);
        productImages.Add(new ProductImage { ProductId = r5_5600x_Id, ImageUrl = "https://www.amd.com/content/dam/amd/en/images/products/processors/ryzen/2505503-ryzen-5-702x702.png" });

        // AMD Ryzen 5 7600X (Mid-range, no iGPU, AM5)
        var r5_7600x_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = r5_7600x_Id,
            Name = "AMD Ryzen 5 7600X",
            Sku = "CPU-AMD-RYZEN5-7600X",
            CategoryId = cpuCategory.Id,
            BrandId = amdBrand.Id,
            Price = 5990000,
            Stock = 40,
            Description = "Bộ vi xử lý AMD Ryzen 5 7600X kiến trúc Zen 4, 6 nhân 12 luồng, xung nhịp cơ bản 4.7GHz, turbo lên đến 5.3GHz. Nền tảng AM5 mới với DDR5 và PCIe 5.0.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, r5_7600x_Id, socket: "AM5", cores: 6, threads: 12, baseClock: 4.7m, boostClock: 5.3m, tdp: 105, memoryType: "DDR5", hasIgpu: true);
        productImages.Add(new ProductImage { ProductId = r5_7600x_Id, ImageUrl = "https://www.amd.com/content/dam/amd/en/images/products/processors/ryzen/2505503-amd-ryzen-702x702.png" });

        // AMD Ryzen 5 7600 (Mid-range, lower TDP)
        var r5_7600_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = r5_7600_Id,
            Name = "AMD Ryzen 5 7600",
            Sku = "CPU-AMD-RYZEN5-7600",
            CategoryId = cpuCategory.Id,
            BrandId = amdBrand.Id,
            Price = 5290000,
            Stock = 45,
            Description = "Bộ vi xử lý AMD Ryzen 5 7600 kiến trúc Zen 4, 6 nhân 12 luồng, xung nhịp cơ bản 3.8GHz, turbo lên đến 5.1GHz. Phiên bản tiết kiệm điện hơn 7600X, đi kèm tản nhiệt Wraith Stealth.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, r5_7600_Id, socket: "AM5", cores: 6, threads: 12, baseClock: 3.8m, boostClock: 5.1m, tdp: 65, memoryType: "DDR5", hasIgpu: true);
        productImages.Add(new ProductImage { ProductId = r5_7600_Id, ImageUrl = "https://www.amd.com/content/dam/amd/en/images/products/processors/ryzen/2505503-amd-ryzen-702x702.png" });

        // AMD Ryzen 7 7800X3D (High-end Gaming King)
        var r7_7800x3d_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = r7_7800x3d_Id,
            Name = "AMD Ryzen 7 7800X3D",
            Sku = "CPU-AMD-RYZEN7-7800X3D",
            CategoryId = cpuCategory.Id,
            BrandId = amdBrand.Id,
            Price = 10990000,
            Stock = 25,
            Description = "Bộ vi xử lý AMD Ryzen 7 7800X3D với công nghệ 3D V-Cache, 8 nhân 16 luồng, 104MB tổng cache. CPU gaming mạnh nhất thế giới, vượt trội trong mọi tựa game.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, r7_7800x3d_Id, socket: "AM5", cores: 8, threads: 16, baseClock: 4.2m, boostClock: 5.0m, tdp: 120, memoryType: "DDR5", hasIgpu: true);
        productImages.Add(new ProductImage { ProductId = r7_7800x3d_Id, ImageUrl = "https://www.amd.com/content/dam/amd/en/images/products/processors/ryzen/2505503-ryzen-7-702x702.png" });

        // AMD Ryzen 9 7900X (High-end)
        var r9_7900x_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = r9_7900x_Id,
            Name = "AMD Ryzen 9 7900X",
            Sku = "CPU-AMD-RYZEN9-7900X",
            CategoryId = cpuCategory.Id,
            BrandId = amdBrand.Id,
            Price = 11990000,
            Stock = 20,
            Description = "Bộ vi xử lý AMD Ryzen 9 7900X kiến trúc Zen 4, 12 nhân 24 luồng, xung nhịp cơ bản 4.7GHz, turbo lên đến 5.6GHz. Hiệu năng cao cho gaming và sáng tạo nội dung.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, r9_7900x_Id, socket: "AM5", cores: 12, threads: 24, baseClock: 4.7m, boostClock: 5.6m, tdp: 170, memoryType: "DDR5", hasIgpu: true);
        productImages.Add(new ProductImage { ProductId = r9_7900x_Id, ImageUrl = "https://www.amd.com/content/dam/amd/en/images/products/processors/ryzen/2505503-ryzen-9-702x702.png" });

        // AMD Ryzen 9 7950X (Flagship)
        var r9_7950x_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = r9_7950x_Id,
            Name = "AMD Ryzen 9 7950X",
            Sku = "CPU-AMD-RYZEN9-7950X",
            CategoryId = cpuCategory.Id,
            BrandId = amdBrand.Id,
            Price = 14990000,
            Stock = 15,
            Description = "Bộ vi xử lý AMD Ryzen 9 7950X flagship kiến trúc Zen 4, 16 nhân 32 luồng, xung nhịp cơ bản 4.5GHz, turbo lên đến 5.7GHz. CPU AM5 mạnh nhất cho enthusiast và content creator.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, r9_7950x_Id, socket: "AM5", cores: 16, threads: 32, baseClock: 4.5m, boostClock: 5.7m, tdp: 170, memoryType: "DDR5", hasIgpu: true);
        productImages.Add(new ProductImage { ProductId = r9_7950x_Id, ImageUrl = "https://www.amd.com/content/dam/amd/en/images/products/processors/ryzen/2505503-ryzen-9-702x702.png" });

        // AMD Ryzen Threadripper 7960X (HEDT Workstation)
        var tr_7960x_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = tr_7960x_Id,
            Name = "AMD Ryzen Threadripper 7960X",
            Sku = "CPU-AMD-THREADRIPPER-7960X",
            CategoryId = cpuCategory.Id,
            BrandId = amdBrand.Id,
            Price = 34990000,
            Stock = 10,
            Description = "Bộ vi xử lý AMD Ryzen Threadripper 7960X cho HEDT, 24 nhân 48 luồng, xung nhịp cơ bản 4.2GHz, turbo lên đến 5.3GHz. Nền tảng sTR5 với 152MB cache, hỗ trợ quad-channel DDR5.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, tr_7960x_Id, socket: "sTR5", cores: 24, threads: 48, baseClock: 4.2m, boostClock: 5.3m, tdp: 350, memoryType: "DDR5 ECC", hasIgpu: false);
        productImages.Add(new ProductImage { ProductId = tr_7960x_Id, ImageUrl = "https://www.amd.com/content/dam/amd/en/images/products/processors/ryzen/2505503-threadripper-702x702.png" });

        // AMD Ryzen Threadripper PRO 7995WX (Ultimate Workstation)
        var tr_pro_7995wx_Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = tr_pro_7995wx_Id,
            Name = "AMD Ryzen Threadripper PRO 7995WX",
            Sku = "CPU-AMD-THREADRIPPER-PRO-7995WX",
            CategoryId = cpuCategory.Id,
            BrandId = amdBrand.Id,
            Price = 249990000,
            Stock = 3,
            Description = "Bộ vi xử lý AMD Ryzen Threadripper PRO 7995WX ultimate workstation, 96 nhân 192 luồng, xung nhịp cơ bản 2.5GHz, turbo lên đến 5.1GHz. CPU desktop mạnh nhất thế giới với 384MB cache, hỗ trợ 2TB RAM 8-channel.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCpuSpecs(specValues, cpuSpecs, tr_pro_7995wx_Id, socket: "sWRX9", cores: 96, threads: 192, baseClock: 2.5m, boostClock: 5.1m, tdp: 350, memoryType: "DDR5 ECC", hasIgpu: false);
        productImages.Add(new ProductImage { ProductId = tr_pro_7995wx_Id, ImageUrl = "https://www.amd.com/content/dam/amd/en/images/products/processors/ryzen/2505503-threadripper-pro-702x702.png" });
    }

    private static void AddGpuSpecs(List<ProductSpecValue> specValues, List<SpecDefinition> specs, Guid productId, int vram, int tdp, int length, string pcieSlot, string powerConnector)
    {
        var vramSpec = specs.FirstOrDefault(s => s.Code == "gpu_vram");
        if (vramSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = vramSpec.Id, NumberValue = vram });

        var tdpSpec = specs.FirstOrDefault(s => s.Code == "gpu_tdp");
        if (tdpSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = tdpSpec.Id, NumberValue = tdp });

        var lengthSpec = specs.FirstOrDefault(s => s.Code == "gpu_length");
        if (lengthSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = lengthSpec.Id, NumberValue = length });

        var pcieSpec = specs.FirstOrDefault(s => s.Code == "gpu_pcie_slot");
        if (pcieSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = pcieSpec.Id, TextValue = pcieSlot });

        var powerSpec = specs.FirstOrDefault(s => s.Code == "gpu_power_connector");
        if (powerSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = powerSpec.Id, TextValue = powerConnector });
    }

    private static void AddCpuSpecs(List<ProductSpecValue> specValues, List<SpecDefinition> specs, Guid productId, string socket, int cores, int threads, decimal baseClock, decimal boostClock, int tdp, string memoryType, bool hasIgpu)
    {
        var socketSpec = specs.FirstOrDefault(s => s.Code == "cpu_socket");
        if (socketSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = socketSpec.Id, TextValue = socket });

        var coresSpec = specs.FirstOrDefault(s => s.Code == "cpu_cores");
        if (coresSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = coresSpec.Id, NumberValue = cores });

        var threadsSpec = specs.FirstOrDefault(s => s.Code == "cpu_threads");
        if (threadsSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = threadsSpec.Id, NumberValue = threads });

        var baseClockSpec = specs.FirstOrDefault(s => s.Code == "cpu_base_clock");
        if (baseClockSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = baseClockSpec.Id, DecimalValue = baseClock });

        var boostClockSpec = specs.FirstOrDefault(s => s.Code == "cpu_boost_clock");
        if (boostClockSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = boostClockSpec.Id, DecimalValue = boostClock });

        var tdpSpec = specs.FirstOrDefault(s => s.Code == "cpu_tdp");
        if (tdpSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = tdpSpec.Id, NumberValue = tdp });

        var memoryTypeSpec = specs.FirstOrDefault(s => s.Code == "cpu_memory_type");
        if (memoryTypeSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = memoryTypeSpec.Id, TextValue = memoryType });

        var igpuSpec = specs.FirstOrDefault(s => s.Code == "cpu_integrated_gpu");
        if (igpuSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = igpuSpec.Id, BoolValue = hasIgpu });
    }

    private static async Task InitMotherboardProducts(ApplicationDbContext context, List<ProductSpecValue> specValues, List<ProductImage> productImages)
    {
        var mbCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Bo mạch chủ")
            ?? throw new NotFoundException("Không tìm thấy danh mục Bo mạch chủ");

        var mbSpecs = await context.SpecDefinitions
            .Where(s => s.CategoryId == mbCategory.Id)
            .ToListAsync();

        // Load motherboard brands
        var asusBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "ASUS")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu ASUS");
        var msiBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "MSI")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu MSI");
        var gigabyteBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Gigabyte")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Gigabyte");
        var asrockBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "ASRock")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu ASRock");
        var biostarBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Biostar")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Biostar");

        // ============= INTEL LGA1700 Z790 (HIGH-END) =============

        // ASUS ROG MAXIMUS Z790 HERO
        var asusZ790HeroId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = asusZ790HeroId,
            Name = "ASUS ROG MAXIMUS Z790 HERO",
            Sku = "MB-ASUS-Z790-MAXIMUS-HERO",
            CategoryId = mbCategory.Id,
            BrandId = asusBrand.Id,
            Price = 16990000,
            Stock = 10,
            Description = "Bo mạch chủ ASUS ROG MAXIMUS Z790 HERO cao cấp cho Intel thế hệ 12/13/14. Hỗ trợ DDR5, PCIe 5.0 cho cả GPU và SSD, WiFi 6E, 2.5G LAN. VRM 20+1 phase, Aura Sync RGB. Lựa chọn hàng đầu cho enthusiast.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, asusZ790HeroId, socket: "LGA1700", chipset: "Z790", formFactor: "ATX", memoryType: "DDR5", memorySlots: 4, maxMemory: 192, m2Slots: 5, pcieVersion: "PCIe 5.0", pcieX16Slots: 2, sataPorts: 6, maxRamSpeed: 7800);
        productImages.Add(new ProductImage { ProductId = asusZ790HeroId, ImageUrl = "https://dlcdnwebimgs.asus.com/gain/z790-maximus-hero/w800" });

        // MSI MEG Z790 ACE
        var msiZ790AceId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = msiZ790AceId,
            Name = "MSI MEG Z790 ACE",
            Sku = "MB-MSI-Z790-MEG-ACE",
            CategoryId = mbCategory.Id,
            BrandId = msiBrand.Id,
            Price = 15990000,
            Stock = 12,
            Description = "Bo mạch chủ MSI MEG Z790 ACE flagship cho Intel LGA1700. Thiết kế VRM 24+1+2 phase, hỗ trợ DDR5-7800+, PCIe 5.0 x16, 5x M.2 slots. WiFi 6E, 10G LAN + 2.5G LAN. Bo mạch chủ cao cấp cho ép xung.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, msiZ790AceId, socket: "LGA1700", chipset: "Z790", formFactor: "ATX", memoryType: "DDR5", memorySlots: 4, maxMemory: 192, m2Slots: 5, pcieVersion: "PCIe 5.0", pcieX16Slots: 2, sataPorts: 6, maxRamSpeed: 7600);
        productImages.Add(new ProductImage { ProductId = msiZ790AceId, ImageUrl = "https://asset.msi.com/resize/image/global/product/product_z790ace.png" });

        // Gigabyte Z790 AORUS MASTER
        var gigabyteZ790MasterId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gigabyteZ790MasterId,
            Name = "Gigabyte Z790 AORUS MASTER",
            Sku = "MB-GIGABYTE-Z790-AORUS-MASTER",
            CategoryId = mbCategory.Id,
            BrandId = gigabyteBrand.Id,
            Price = 14990000,
            Stock = 15,
            Description = "Bo mạch chủ Gigabyte Z790 AORUS MASTER cao cấp. VRM 20+1+2 phase, hỗ trợ DDR5-8000+, PCIe 5.0. Fins-Array III heatsink, WiFi 6E, 10G LAN. RGB Fusion 2.0. Thiết kế sang trọng cho enthusiast build.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, gigabyteZ790MasterId, socket: "LGA1700", chipset: "Z790", formFactor: "ATX", memoryType: "DDR5", memorySlots: 4, maxMemory: 192, m2Slots: 5, pcieVersion: "PCIe 5.0", pcieX16Slots: 2, sataPorts: 6, maxRamSpeed: 7600);
        productImages.Add(new ProductImage { ProductId = gigabyteZ790MasterId, ImageUrl = "https://www.gigabyte.com/FileUpload/Global/KeyFeature/z790-aorus-master.png" });

        // ============= INTEL LGA1700 Z790 (MID-HIGH) =============

        // ASRock Z790 Steel Legend WiFi
        var asrockZ790SteelId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = asrockZ790SteelId,
            Name = "ASRock Z790 Steel Legend WiFi",
            Sku = "MB-ASROCK-Z790-STEEL-LEGEND-WIFI",
            CategoryId = mbCategory.Id,
            BrandId = asrockBrand.Id,
            Price = 8990000,
            Stock = 20,
            Description = "Bo mạch chủ ASRock Z790 Steel Legend WiFi với thiết kế camo độc đáo. VRM 16+1+1 phase, hỗ trợ DDR5-6800+, PCIe 5.0. WiFi 6E, 2.5G LAN. Polychrome Sync RGB. Giá tốt cho Z790.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, asrockZ790SteelId, socket: "LGA1700", chipset: "Z790", formFactor: "ATX", memoryType: "DDR5", memorySlots: 4, maxMemory: 128, m2Slots: 4, pcieVersion: "PCIe 5.0", pcieX16Slots: 2, sataPorts: 8, maxRamSpeed: 7200);
        productImages.Add(new ProductImage { ProductId = asrockZ790SteelId, ImageUrl = "https://www.asrock.com/mb/photo/Z790%20Steel%20Legend%20WiFi.png" });

        // ============= INTEL LGA1700 B760 (MID-RANGE) =============

        // ASUS ROG STRIX B760-F GAMING WIFI
        var asusB760StrixId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = asusB760StrixId,
            Name = "ASUS ROG STRIX B760-F GAMING WIFI",
            Sku = "MB-ASUS-B760-STRIX-F-GAMING-WIFI",
            CategoryId = mbCategory.Id,
            BrandId = asusBrand.Id,
            Price = 6990000,
            Stock = 25,
            Description = "Bo mạch chủ ASUS ROG STRIX B760-F GAMING WIFI cho Intel thế hệ 12/13/14. VRM 12+1 phase, hỗ trợ DDR5-7800+, PCIe 5.0 cho SSD. WiFi 6E, 2.5G LAN. Aura Sync RGB. Gaming motherboard tầm trung cao cấp.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, asusB760StrixId, socket: "LGA1700", chipset: "B760", formFactor: "ATX", memoryType: "DDR5", memorySlots: 4, maxMemory: 192, m2Slots: 4, pcieVersion: "PCIe 5.0", pcieX16Slots: 1, sataPorts: 4, maxRamSpeed: 7200);
        productImages.Add(new ProductImage { ProductId = asusB760StrixId, ImageUrl = "https://dlcdnwebimgs.asus.com/gain/b760-strix-f-gaming-wifi/w800" });

        // MSI MAG B760M MORTAR WIFI
        var msiB760MortarId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = msiB760MortarId,
            Name = "MSI MAG B760M MORTAR WIFI",
            Sku = "MB-MSI-B760M-MORTAR-WIFI",
            CategoryId = mbCategory.Id,
            BrandId = msiBrand.Id,
            Price = 4990000,
            Stock = 30,
            Description = "Bo mạch chủ MSI MAG B760M MORTAR WIFI dạng Micro-ATX. VRM 12+1+1 phase, hỗ trợ DDR5-7000+. WiFi 6E, 2.5G LAN. Thiết kế nhỏ gọn nhưng đầy đủ tính năng, phù hợp cho build compact.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, msiB760MortarId, socket: "LGA1700", chipset: "B760", formFactor: "mATX", memoryType: "DDR5", memorySlots: 4, maxMemory: 128, m2Slots: 2, pcieVersion: "PCIe 4.0", pcieX16Slots: 1, sataPorts: 4, maxRamSpeed: 6800);
        productImages.Add(new ProductImage { ProductId = msiB760MortarId, ImageUrl = "https://asset.msi.com/resize/image/global/product/product_b760m-mortar-wifi.png" });

        // ============= INTEL LGA1700 B760 (BUDGET) =============

        // Gigabyte B760M DS3H DDR4
        var gigabyteB760MDs3hId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gigabyteB760MDs3hId,
            Name = "Gigabyte B760M DS3H DDR4",
            Sku = "MB-GIGABYTE-B760M-DS3H-DDR4",
            CategoryId = mbCategory.Id,
            BrandId = gigabyteBrand.Id,
            Price = 2690000,
            Stock = 45,
            Description = "Bo mạch chủ Gigabyte B760M DS3H DDR4 giá rẻ cho Intel thế hệ 12/13/14. Hỗ trợ DDR4-5333, VRM 6+2+1 phase. 2x M.2 slots, PCIe 4.0. Lựa chọn tiết kiệm cho build văn phòng và gaming nhẹ.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, gigabyteB760MDs3hId, socket: "LGA1700", chipset: "B760", formFactor: "mATX", memoryType: "DDR4", memorySlots: 2, maxMemory: 64, m2Slots: 2, pcieVersion: "PCIe 4.0", pcieX16Slots: 1, sataPorts: 4, maxRamSpeed: 5333);
        productImages.Add(new ProductImage { ProductId = gigabyteB760MDs3hId, ImageUrl = "https://www.gigabyte.com/FileUpload/Global/KeyFeature/b760m-ds3h-ddr4.png" });

        // Biostar B760MX-E PRO
        var biostarB760MxId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = biostarB760MxId,
            Name = "Biostar B760MX-E PRO",
            Sku = "MB-BIOSTAR-B760MX-E-PRO",
            CategoryId = mbCategory.Id,
            BrandId = biostarBrand.Id,
            Price = 2290000,
            Stock = 35,
            Description = "Bo mạch chủ Biostar B760MX-E PRO giá cực rẻ cho Intel LGA1700. Hỗ trợ DDR4, PCIe 4.0, 1x M.2 slot. VRM cơ bản. Lựa chọn tiết kiệm nhất cho build văn phòng.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, biostarB760MxId, socket: "LGA1700", chipset: "B760", formFactor: "mATX", memoryType: "DDR4", memorySlots: 2, maxMemory: 64, m2Slots: 1, pcieVersion: "PCIe 4.0", pcieX16Slots: 1, sataPorts: 4, maxRamSpeed: 4800);
        productImages.Add(new ProductImage { ProductId = biostarB760MxId, ImageUrl = "https://www.biostar.com.tw/upload/Mainboard/b760mx-e-pro.png" });

        // ============= AMD AM5 X670E (HIGH-END) =============

        // ASUS ROG CROSSHAIR X670E HERO
        var asusX670EHeroId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = asusX670EHeroId,
            Name = "ASUS ROG CROSSHAIR X670E HERO",
            Sku = "MB-ASUS-X670E-CROSSHAIR-HERO",
            CategoryId = mbCategory.Id,
            BrandId = asusBrand.Id,
            Price = 17990000,
            Stock = 8,
            Description = "Bo mạch chủ ASUS ROG CROSSHAIR X670E HERO flagship cho AMD Ryzen 7000. VRM 18+2 phase, hỗ trợ DDR5-6400+, PCIe 5.0 cho cả GPU và SSD. WiFi 6E, 2.5G LAN. Aura Sync RGB. Bo mạch chủ AM5 cao cấp nhất.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, asusX670EHeroId, socket: "AM5", chipset: "X670E", formFactor: "ATX", memoryType: "DDR5", memorySlots: 4, maxMemory: 128, m2Slots: 5, pcieVersion: "PCIe 5.0", pcieX16Slots: 2, sataPorts: 6, maxRamSpeed: 6400);
        productImages.Add(new ProductImage { ProductId = asusX670EHeroId, ImageUrl = "https://dlcdnwebimgs.asus.com/gain/x670e-crosshair-hero/w800" });

        // MSI MEG X670E ACE
        var msiX670EAceId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = msiX670EAceId,
            Name = "MSI MEG X670E ACE",
            Sku = "MB-MSI-X670E-MEG-ACE",
            CategoryId = mbCategory.Id,
            BrandId = msiBrand.Id,
            Price = 16990000,
            Stock = 10,
            Description = "Bo mạch chủ MSI MEG X670E ACE cao cấp cho AMD AM5. VRM 22+2+1 phase, hỗ trợ DDR5-6600+, PCIe 5.0. 4x M.2 slots, WiFi 6E, 10G LAN + 2.5G LAN. Thiết kế sang trọng với Mystic Light RGB.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, msiX670EAceId, socket: "AM5", chipset: "X670E", formFactor: "ATX", memoryType: "DDR5", memorySlots: 4, maxMemory: 128, m2Slots: 4, pcieVersion: "PCIe 5.0", pcieX16Slots: 2, sataPorts: 6, maxRamSpeed: 6600);
        productImages.Add(new ProductImage { ProductId = msiX670EAceId, ImageUrl = "https://asset.msi.com/resize/image/global/product/product_x670e-ace.png" });

        // Gigabyte X670E AORUS MASTER
        var gigabyteX670EMasterId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gigabyteX670EMasterId,
            Name = "Gigabyte X670E AORUS MASTER",
            Sku = "MB-GIGABYTE-X670E-AORUS-MASTER",
            CategoryId = mbCategory.Id,
            BrandId = gigabyteBrand.Id,
            Price = 15490000,
            Stock = 12,
            Description = "Bo mạch chủ Gigabyte X670E AORUS MASTER cho AMD Ryzen 7000. VRM 16+2+2 phase, hỗ trợ DDR5-6600+, PCIe 5.0. Fins-Array III heatsink, WiFi 6E, 10G LAN. RGB Fusion 2.0. Bo mạch chủ cao cấp cho enthusiast AMD.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, gigabyteX670EMasterId, socket: "AM5", chipset: "X670E", formFactor: "ATX", memoryType: "DDR5", memorySlots: 4, maxMemory: 128, m2Slots: 4, pcieVersion: "PCIe 5.0", pcieX16Slots: 2, sataPorts: 6, maxRamSpeed: 6600);
        productImages.Add(new ProductImage { ProductId = gigabyteX670EMasterId, ImageUrl = "https://www.gigabyte.com/FileUpload/Global/KeyFeature/x670e-aorus-master.png" });

        // ============= AMD AM5 B650 (MID-RANGE) =============

        // ASUS TUF GAMING B650-PLUS WIFI
        var asusB650TufId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = asusB650TufId,
            Name = "ASUS TUF GAMING B650-PLUS WIFI",
            Sku = "MB-ASUS-B650-TUF-GAMING-PLUS-WIFI",
            CategoryId = mbCategory.Id,
            BrandId = asusBrand.Id,
            Price = 5990000,
            Stock = 28,
            Description = "Bo mạch chủ ASUS TUF GAMING B650-PLUS WIFI cho AMD AM5. VRM 12+2 phase, hỗ trợ DDR5-6400+, PCIe 4.0. WiFi 6, 2.5G LAN. Thiết kế bền bỉ chuẩn quân sự TUF. Giá tốt cho nền tảng AM5.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, asusB650TufId, socket: "AM5", chipset: "B650", formFactor: "ATX", memoryType: "DDR5", memorySlots: 4, maxMemory: 128, m2Slots: 3, pcieVersion: "PCIe 4.0", pcieX16Slots: 1, sataPorts: 4, maxRamSpeed: 6400);
        productImages.Add(new ProductImage { ProductId = asusB650TufId, ImageUrl = "https://dlcdnwebimgs.asus.com/gain/b650-tuf-gaming-plus-wifi/w800" });

        // MSI MAG B650 TOMAHAWK WIFI
        var msiB650TomahawkId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = msiB650TomahawkId,
            Name = "MSI MAG B650 TOMAHAWK WIFI",
            Sku = "MB-MSI-B650-MAG-TOMAHAWK-WIFI",
            CategoryId = mbCategory.Id,
            BrandId = msiBrand.Id,
            Price = 5490000,
            Stock = 32,
            Description = "Bo mạch chủ MSI MAG B650 TOMAHAWK WIFI cho AMD AM5. VRM 12+2+1 phase, hỗ trợ DDR5-6400+, PCIe 4.0. WiFi 6E, 2.5G LAN. Extended heatsink design. Bo mạch chủ gaming tầm trung đáng mua nhất cho AM5.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, msiB650TomahawkId, socket: "AM5", chipset: "B650", formFactor: "ATX", memoryType: "DDR5", memorySlots: 4, maxMemory: 128, m2Slots: 2, pcieVersion: "PCIe 4.0", pcieX16Slots: 1, sataPorts: 4, maxRamSpeed: 6400);
        productImages.Add(new ProductImage { ProductId = msiB650TomahawkId, ImageUrl = "https://asset.msi.com/resize/image/global/product/product_b650-tomahawk-wifi.png" });

        // ASRock B650M PG Riptide WiFi
        var asrockB650MRiptideId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = asrockB650MRiptideId,
            Name = "ASRock B650M PG Riptide WiFi",
            Sku = "MB-ASROCK-B650M-PG-RIPTIDE-WIFI",
            CategoryId = mbCategory.Id,
            BrandId = asrockBrand.Id,
            Price = 4290000,
            Stock = 35,
            Description = "Bo mạch chủ ASRock B650M PG Riptide WiFi dạng Micro-ATX cho AMD AM5. VRM 8+2+1 phase, hỗ trợ DDR5-6200+, PCIe 4.0. WiFi 6E, 2.5G LAN. Giá cạnh tranh cho nền tảng AM5.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, asrockB650MRiptideId, socket: "AM5", chipset: "B650", formFactor: "mATX", memoryType: "DDR5", memorySlots: 4, maxMemory: 128, m2Slots: 2, pcieVersion: "PCIe 4.0", pcieX16Slots: 1, sataPorts: 4, maxRamSpeed: 6200);
        productImages.Add(new ProductImage { ProductId = asrockB650MRiptideId, ImageUrl = "https://www.asrock.com/mb/photo/B650M%20PG%20Riptide%20WiFi.png" });

        // ============= AMD AM5 B650 (BUDGET) =============

        // Gigabyte B650M DS3H
        var gigabyteB650MDs3hId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gigabyteB650MDs3hId,
            Name = "Gigabyte B650M DS3H",
            Sku = "MB-GIGABYTE-B650M-DS3H",
            CategoryId = mbCategory.Id,
            BrandId = gigabyteBrand.Id,
            Price = 3290000,
            Stock = 40,
            Description = "Bo mạch chủ Gigabyte B650M DS3H giá rẻ nhất cho AMD AM5. Hỗ trợ DDR5-6000+, VRM 6+2+1 phase. 2x M.2 slots, PCIe 4.0. Lựa chọn tiết kiệm nhất để trải nghiệm Ryzen 7000.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddMotherboardSpecs(specValues, mbSpecs, gigabyteB650MDs3hId, socket: "AM5", chipset: "B650", formFactor: "mATX", memoryType: "DDR5", memorySlots: 2, maxMemory: 96, m2Slots: 2, pcieVersion: "PCIe 4.0", pcieX16Slots: 1, sataPorts: 4, maxRamSpeed: 6000);
        productImages.Add(new ProductImage { ProductId = gigabyteB650MDs3hId, ImageUrl = "https://www.gigabyte.com/FileUpload/Global/KeyFeature/b650m-ds3h.png" });
    }

    private static void AddMotherboardSpecs(List<ProductSpecValue> specValues, List<SpecDefinition> specs, Guid productId, string socket, string chipset, string formFactor, string memoryType, int memorySlots, int maxMemory, int m2Slots, string pcieVersion, int pcieX16Slots, int sataPorts, int maxRamSpeed)
    {
        var socketSpec = specs.FirstOrDefault(s => s.Code == "mb_socket");
        if (socketSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = socketSpec.Id, TextValue = socket });

        var chipsetSpec = specs.FirstOrDefault(s => s.Code == "mb_chipset");
        if (chipsetSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = chipsetSpec.Id, TextValue = chipset });

        var formFactorSpec = specs.FirstOrDefault(s => s.Code == "mb_form_factor");
        if (formFactorSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = formFactorSpec.Id, TextValue = formFactor });

        var memoryTypeSpec = specs.FirstOrDefault(s => s.Code == "mb_memory_type");
        if (memoryTypeSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = memoryTypeSpec.Id, TextValue = memoryType });

        var memorySlotsSpec = specs.FirstOrDefault(s => s.Code == "mb_memory_slots");
        if (memorySlotsSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = memorySlotsSpec.Id, NumberValue = memorySlots });

        var maxMemorySpec = specs.FirstOrDefault(s => s.Code == "mb_max_memory");
        if (maxMemorySpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = maxMemorySpec.Id, NumberValue = maxMemory });

        var m2SlotsSpec = specs.FirstOrDefault(s => s.Code == "mb_m2_slots");
        if (m2SlotsSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = m2SlotsSpec.Id, NumberValue = m2Slots });

        var pcieVersionSpec = specs.FirstOrDefault(s => s.Code == "mb_pcie_version");
        if (pcieVersionSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = pcieVersionSpec.Id, TextValue = pcieVersion });

        var pcieX16SlotsSpec = specs.FirstOrDefault(s => s.Code == "mb_pcie_x16_slots");
        if (pcieX16SlotsSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = pcieX16SlotsSpec.Id, NumberValue = pcieX16Slots });

        var sataPortsSpec = specs.FirstOrDefault(s => s.Code == "mb_sata_ports");
        if (sataPortsSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = sataPortsSpec.Id, NumberValue = sataPorts });

        var maxRamSpeedSpec = specs.FirstOrDefault(s => s.Code == "mb_max_ram_speed");
        if (maxRamSpeedSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = maxRamSpeedSpec.Id, NumberValue = maxRamSpeed });
    }

    private static async Task InitRamProducts(ApplicationDbContext context, List<ProductSpecValue> specValues, List<ProductImage> productImages)
    {
        var ramCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "RAM")
            ?? throw new NotFoundException("Không tìm thấy danh mục RAM");

        var ramSpecs = await context.SpecDefinitions
            .Where(s => s.CategoryId == ramCategory.Id)
            .ToListAsync();

        // Load RAM brands
        var corsairBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Corsair")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Corsair");
        var gskillBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "G.Skill")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu G.Skill");
        var kingstonBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Kingston")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Kingston");
        var crucialBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Crucial")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Crucial");
        var teamgroupBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "TeamGroup")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu TeamGroup");
        var adataBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "ADATA")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu ADATA");
        var patriotBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Patriot")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Patriot");

        // ============= DDR5 HIGH-END GAMING/ENTHUSIAST =============

        // Corsair Dominator Platinum RGB DDR5-6400
        var corsairDominatorId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = corsairDominatorId,
            Name = "Corsair Dominator Platinum RGB DDR5-6400 32GB (2x16GB)",
            Sku = "RAM-CORSAIR-DOMINATOR-DDR5-6400-32GB",
            CategoryId = ramCategory.Id,
            BrandId = corsairBrand.Id,
            Price = 5990000,
            Stock = 25,
            Description = "RAM Corsair Dominator Platinum RGB DDR5-6400 32GB kit (2x16GB) CL32. Thiết kế Patented DHX cooling, 12 đèn LED CAPELLIX. Tối ưu cho Intel XMP 3.0 và AMD EXPO. RAM cao cấp nhất cho gaming và enthusiast.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, corsairDominatorId, type: "DDR5", speed: 6400, capacityPerStick: 16, sticks: 2, latency: 32);
        productImages.Add(new ProductImage { ProductId = corsairDominatorId, ImageUrl = "https://www.corsair.com/medias/sys_master/images/images/hd3/h25/16920665325598/-CMT32GX5M2X6400C32-Gallery-?"});

        // G.Skill Trident Z5 RGB DDR5-6000
        var gskillTridentZ5Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gskillTridentZ5Id,
            Name = "G.Skill Trident Z5 RGB DDR5-6000 32GB (2x16GB)",
            Sku = "RAM-GSKILL-TRIDENTZ5-DDR5-6000-32GB",
            CategoryId = ramCategory.Id,
            BrandId = gskillBrand.Id,
            Price = 4990000,
            Stock = 30,
            Description = "RAM G.Skill Trident Z5 RGB DDR5-6000 32GB kit (2x16GB) CL30. Thiết kế heatsink nhôm cao cấp với LED RGB. Hỗ trợ Intel XMP 3.0. RAM flagship của G.Skill cho DDR5 platform.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, gskillTridentZ5Id, type: "DDR5", speed: 6000, capacityPerStick: 16, sticks: 2, latency: 30);
        productImages.Add(new ProductImage { ProductId = gskillTridentZ5Id, ImageUrl = "https://www.gskill.com/img/pr_img/F5-6000J3038F16GX2-TZ5RK_01.png" });

        // Kingston Fury Renegade DDR5-6400
        var kingstonRenegadeId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = kingstonRenegadeId,
            Name = "Kingston Fury Renegade DDR5-6400 32GB (2x16GB)",
            Sku = "RAM-KINGSTON-FURY-RENEGADE-DDR5-6400-32GB",
            CategoryId = ramCategory.Id,
            BrandId = kingstonBrand.Id,
            Price = 5490000,
            Stock = 28,
            Description = "RAM Kingston Fury Renegade DDR5-6400 32GB kit (2x16GB) CL32. Thiết kế tản nhiệt aggressive với LED RGB. Hỗ trợ Intel XMP 3.0 và AMD EXPO. Hiệu năng extreme cho overclocker.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, kingstonRenegadeId, type: "DDR5", speed: 6400, capacityPerStick: 16, sticks: 2, latency: 32);
        productImages.Add(new ProductImage { ProductId = kingstonRenegadeId, ImageUrl = "https://media.kingston.com/kingston/product/ktc-product-memory-fury-renegade-ddr5-rgb-2-lg.jpg" });

        // TeamGroup T-Force Delta RGB DDR5-6000
        var teamDeltaId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = teamDeltaId,
            Name = "TeamGroup T-Force Delta RGB DDR5-6000 32GB (2x16GB)",
            Sku = "RAM-TEAMGROUP-DELTA-DDR5-6000-32GB",
            CategoryId = ramCategory.Id,
            BrandId = teamgroupBrand.Id,
            Price = 4290000,
            Stock = 35,
            Description = "RAM TeamGroup T-Force Delta RGB DDR5-6000 32GB kit (2x16GB) CL30. LED RGB 120 độ sáng full-zone. Hỗ trợ Intel XMP 3.0 và AMD EXPO. Giá cạnh tranh cho DDR5 high-end.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, teamDeltaId, type: "DDR5", speed: 6000, capacityPerStick: 16, sticks: 2, latency: 30);
        productImages.Add(new ProductImage { ProductId = teamDeltaId, ImageUrl = "https://www.teamgroupinc.com/en/upload/product_catalog/product/ctk/ckcp/ff48e2dcc.png" });

        // ADATA XPG Lancer RGB DDR5-6000
        var adataLancerRgbId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = adataLancerRgbId,
            Name = "ADATA XPG Lancer RGB DDR5-6000 32GB (2x16GB)",
            Sku = "RAM-ADATA-XPG-LANCER-RGB-DDR5-6000-32GB",
            CategoryId = ramCategory.Id,
            BrandId = adataBrand.Id,
            Price = 3990000,
            Stock = 40,
            Description = "RAM ADATA XPG Lancer RGB DDR5-6000 32GB kit (2x16GB) CL30. Thiết kế RGB blade hiện đại, tản nhiệt hiệu quả. Hỗ trợ Intel XMP 3.0 và AMD EXPO. Lựa chọn RGB DDR5 giá tốt.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, adataLancerRgbId, type: "DDR5", speed: 6000, capacityPerStick: 16, sticks: 2, latency: 30);
        productImages.Add(new ProductImage { ProductId = adataLancerRgbId, ImageUrl = "https://www.xpg.com/upload/images/products/xpg-lancer-rgb-ddr5/gallery-1.png" });

        // ============= DDR5 WORKSTATION/HIGH CAPACITY =============

        // Corsair Vengeance DDR5-5600 64GB (2x32GB)
        var corsairVengeance64Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = corsairVengeance64Id,
            Name = "Corsair Vengeance DDR5-5600 64GB (2x32GB)",
            Sku = "RAM-CORSAIR-VENGEANCE-DDR5-5600-64GB",
            CategoryId = ramCategory.Id,
            BrandId = corsairBrand.Id,
            Price = 6490000,
            Stock = 20,
            Description = "RAM Corsair Vengeance DDR5-5600 64GB kit (2x32GB) CL40. Thiết kế low-profile phù hợp mọi hệ thống. Dung lượng lớn cho workstation, content creation, video editing. Intel XMP 3.0.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, corsairVengeance64Id, type: "DDR5", speed: 5600, capacityPerStick: 32, sticks: 2, latency: 40);
        productImages.Add(new ProductImage { ProductId = corsairVengeance64Id, ImageUrl = "https://www.corsair.com/medias/sys_master/images/images/hda/h14/16694755188766/-CMK64GX5M2B5600C40-Gallery-?"});

        // G.Skill Trident Z5 Neo DDR5-6000 64GB (2x32GB)
        var gskillNeo64Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gskillNeo64Id,
            Name = "G.Skill Trident Z5 Neo DDR5-6000 64GB (2x32GB)",
            Sku = "RAM-GSKILL-TRIDENTZ5-NEO-DDR5-6000-64GB",
            CategoryId = ramCategory.Id,
            BrandId = gskillBrand.Id,
            Price = 7990000,
            Stock = 15,
            Description = "RAM G.Skill Trident Z5 Neo DDR5-6000 64GB kit (2x32GB) CL30. Tối ưu cho AMD Ryzen 7000 với EXPO. Dung lượng lớn cho workstation và content creation chuyên nghiệp.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, gskillNeo64Id, type: "DDR5", speed: 6000, capacityPerStick: 32, sticks: 2, latency: 30);
        productImages.Add(new ProductImage { ProductId = gskillNeo64Id, ImageUrl = "https://www.gskill.com/img/pr_img/F5-6000J3038F32GX2-TZ5N_01.png" });

        // Kingston Fury Beast DDR5-5600 64GB (2x32GB)
        var kingstonBeast64Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = kingstonBeast64Id,
            Name = "Kingston Fury Beast DDR5-5600 64GB (2x32GB)",
            Sku = "RAM-KINGSTON-FURY-BEAST-DDR5-5600-64GB",
            CategoryId = ramCategory.Id,
            BrandId = kingstonBrand.Id,
            Price = 5990000,
            Stock = 22,
            Description = "RAM Kingston Fury Beast DDR5-5600 64GB kit (2x32GB) CL40. Thiết kế tản nhiệt hiệu quả, low-profile. Dung lượng lớn cho workstation, máy ảo, render video.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, kingstonBeast64Id, type: "DDR5", speed: 5600, capacityPerStick: 32, sticks: 2, latency: 40);
        productImages.Add(new ProductImage { ProductId = kingstonBeast64Id, ImageUrl = "https://media.kingston.com/kingston/product/ktc-product-memory-fury-beast-ddr5-2-lg.jpg" });

        // Crucial Pro DDR5-5600 128GB (4x32GB) - Workstation
        var crucialPro128Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = crucialPro128Id,
            Name = "Crucial Pro DDR5-5600 128GB Kit (4x32GB)",
            Sku = "RAM-CRUCIAL-PRO-DDR5-5600-128GB",
            CategoryId = ramCategory.Id,
            BrandId = crucialBrand.Id,
            Price = 12990000,
            Stock = 10,
            Description = "RAM Crucial Pro DDR5-5600 128GB kit (4x32GB) CL46. Dung lượng cực lớn cho workstation chuyên nghiệp, máy chủ, AI/ML training. Micron technology đảm bảo độ tin cậy cao.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, crucialPro128Id, type: "DDR5", speed: 5600, capacityPerStick: 32, sticks: 4, latency: 46);
        productImages.Add(new ProductImage { ProductId = crucialPro128Id, ImageUrl = "https://www.crucial.com/content/dam/crucial/dram-products/pro/images/in-use/crucial-pro-ddr5-in-use.png" });

        // TeamGroup T-Create Expert DDR5-6000 64GB (2x32GB) - Workstation
        var teamCreateId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = teamCreateId,
            Name = "TeamGroup T-Create Expert DDR5-6000 64GB (2x32GB)",
            Sku = "RAM-TEAMGROUP-TCREATE-DDR5-6000-64GB",
            CategoryId = ramCategory.Id,
            BrandId = teamgroupBrand.Id,
            Price = 6290000,
            Stock = 18,
            Description = "RAM TeamGroup T-Create Expert DDR5-6000 64GB kit (2x32GB) CL34. Dòng sản phẩm chuyên cho content creator và workstation. Thiết kế tối giản, hiệu năng ổn định cho công việc chuyên nghiệp.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, teamCreateId, type: "DDR5", speed: 6000, capacityPerStick: 32, sticks: 2, latency: 34);
        productImages.Add(new ProductImage { ProductId = teamCreateId, ImageUrl = "https://www.teamgroupinc.com/en/upload/product_catalog/product/ctk/ckcp/t-create-expert-ddr5.png" });

        // ============= DDR5 MID-RANGE =============

        // Corsair Vengeance DDR5-5600 32GB (2x16GB)
        var corsairVengeance32Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = corsairVengeance32Id,
            Name = "Corsair Vengeance DDR5-5600 32GB (2x16GB)",
            Sku = "RAM-CORSAIR-VENGEANCE-DDR5-5600-32GB",
            CategoryId = ramCategory.Id,
            BrandId = corsairBrand.Id,
            Price = 3290000,
            Stock = 45,
            Description = "RAM Corsair Vengeance DDR5-5600 32GB kit (2x16GB) CL36. Thiết kế low-profile compact, phù hợp mọi build. Intel XMP 3.0 và AMD EXPO. Lựa chọn phổ biến cho DDR5 mainstream.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, corsairVengeance32Id, type: "DDR5", speed: 5600, capacityPerStick: 16, sticks: 2, latency: 36);
        productImages.Add(new ProductImage { ProductId = corsairVengeance32Id, ImageUrl = "https://www.corsair.com/medias/sys_master/images/images/h8e/h07/16694755680286/-CMK32GX5M2B5600C36-Gallery-VENGEANCE-DDR5-BLACK-01.png_1200Wx1200H" });

        // Kingston Fury Beast DDR5-5200 32GB (2x16GB)
        var kingstonBeast32Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = kingstonBeast32Id,
            Name = "Kingston Fury Beast DDR5-5200 32GB (2x16GB)",
            Sku = "RAM-KINGSTON-FURY-BEAST-DDR5-5200-32GB",
            CategoryId = ramCategory.Id,
            BrandId = kingstonBrand.Id,
            Price = 2890000,
            Stock = 50,
            Description = "RAM Kingston Fury Beast DDR5-5200 32GB kit (2x16GB) CL40. Thiết kế heatsink hiệu quả, Intel XMP 3.0 và AMD EXPO. RAM DDR5 giá tốt cho người dùng phổ thông.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, kingstonBeast32Id, type: "DDR5", speed: 5200, capacityPerStick: 16, sticks: 2, latency: 40);
        productImages.Add(new ProductImage { ProductId = kingstonBeast32Id, ImageUrl = "https://media.kingston.com/kingston/product/ktc-product-memory-fury-beast-ddr5-lg.jpg" });

        // Crucial DDR5-4800 32GB (2x16GB)
        var crucial4800Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = crucial4800Id,
            Name = "Crucial DDR5-4800 32GB (2x16GB)",
            Sku = "RAM-CRUCIAL-DDR5-4800-32GB",
            CategoryId = ramCategory.Id,
            BrandId = crucialBrand.Id,
            Price = 2490000,
            Stock = 55,
            Description = "RAM Crucial DDR5-4800 32GB kit (2x16GB) CL40. RAM chuẩn JEDEC, tương thích rộng rãi. Micron technology đảm bảo độ ổn định. Lựa chọn giá rẻ nhất cho DDR5.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, crucial4800Id, type: "DDR5", speed: 4800, capacityPerStick: 16, sticks: 2, latency: 40);
        productImages.Add(new ProductImage { ProductId = crucial4800Id, ImageUrl = "https://www.crucial.com/content/dam/crucial/dram-products/ddr5/images/in-use/crucial-ddr5-in-use-image.png" });

        // ADATA XPG Lancer DDR5-5200 32GB (2x16GB)
        var adataLancer32Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = adataLancer32Id,
            Name = "ADATA XPG Lancer DDR5-5200 32GB (2x16GB)",
            Sku = "RAM-ADATA-XPG-LANCER-DDR5-5200-32GB",
            CategoryId = ramCategory.Id,
            BrandId = adataBrand.Id,
            Price = 2690000,
            Stock = 48,
            Description = "RAM ADATA XPG Lancer DDR5-5200 32GB kit (2x16GB) CL38. Thiết kế heatsink đơn giản, hiệu năng ổn định. Hỗ trợ Intel XMP 3.0. RAM DDR5 mid-range đáng mua.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, adataLancer32Id, type: "DDR5", speed: 5200, capacityPerStick: 16, sticks: 2, latency: 38);
        productImages.Add(new ProductImage { ProductId = adataLancer32Id, ImageUrl = "https://www.xpg.com/upload/images/products/xpg-lancer-ddr5/gallery-1.png" });

        // ============= DDR5 BUDGET =============

        // Patriot Viper Venom DDR5-5200 16GB (2x8GB)
        var patriotViperId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = patriotViperId,
            Name = "Patriot Viper Venom DDR5-5200 16GB (2x8GB)",
            Sku = "RAM-PATRIOT-VIPER-VENOM-DDR5-5200-16GB",
            CategoryId = ramCategory.Id,
            BrandId = patriotBrand.Id,
            Price = 1690000,
            Stock = 60,
            Description = "RAM Patriot Viper Venom DDR5-5200 16GB kit (2x8GB) CL40. Thiết kế heatsink đẹp mắt, giá cạnh tranh. Intel XMP 3.0. Entry-level DDR5 cho người mới lên platform.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, patriotViperId, type: "DDR5", speed: 5200, capacityPerStick: 8, sticks: 2, latency: 40);
        productImages.Add(new ProductImage { ProductId = patriotViperId, ImageUrl = "https://www.patriotmemory.com/products/viper-venom-ddr5.png" });

        // TeamGroup T-Force Vulcan DDR5-5200 16GB (2x8GB)
        var teamVulcanId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = teamVulcanId,
            Name = "TeamGroup T-Force Vulcan DDR5-5200 16GB (2x8GB)",
            Sku = "RAM-TEAMGROUP-VULCAN-DDR5-5200-16GB",
            CategoryId = ramCategory.Id,
            BrandId = teamgroupBrand.Id,
            Price = 1590000,
            Stock = 65,
            Description = "RAM TeamGroup T-Force Vulcan DDR5-5200 16GB kit (2x8GB) CL40. Thiết kế đơn giản, tản nhiệt tốt. Intel XMP 3.0 và AMD EXPO. RAM DDR5 giá rẻ nhất thị trường.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, teamVulcanId, type: "DDR5", speed: 5200, capacityPerStick: 8, sticks: 2, latency: 40);
        productImages.Add(new ProductImage { ProductId = teamVulcanId, ImageUrl = "https://www.teamgroupinc.com/en/upload/product_catalog/product/ctk/ckcp/t-force-vulcan-ddr5.png" });

        // ============= DDR4 (BACKWARD COMPATIBILITY) =============

        // G.Skill Trident Z Royal DDR4-3600 32GB (2x16GB)
        var gskillRoyalId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gskillRoyalId,
            Name = "G.Skill Trident Z Royal DDR4-3600 32GB (2x16GB)",
            Sku = "RAM-GSKILL-TRIDENTZ-ROYAL-DDR4-3600-32GB",
            CategoryId = ramCategory.Id,
            BrandId = gskillBrand.Id,
            Price = 3990000,
            Stock = 25,
            Description = "RAM G.Skill Trident Z Royal DDR4-3600 32GB kit (2x16GB) CL16. Thiết kế crystal crown luxury với RGB. DDR4 cao cấp nhất cho Intel/AMD platform cũ. Hiệu năng và thẩm mỹ.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, gskillRoyalId, type: "DDR4", speed: 3600, capacityPerStick: 16, sticks: 2, latency: 16);
        productImages.Add(new ProductImage { ProductId = gskillRoyalId, ImageUrl = "https://www.gskill.com/img/pr_img/F4-3600C16D-32GTRG_01.png" });

        // Corsair Vengeance LPX DDR4-3200 32GB (2x16GB)
        var corsairLpx32Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = corsairLpx32Id,
            Name = "Corsair Vengeance LPX DDR4-3200 32GB (2x16GB)",
            Sku = "RAM-CORSAIR-VENGEANCE-LPX-DDR4-3200-32GB",
            CategoryId = ramCategory.Id,
            BrandId = corsairBrand.Id,
            Price = 2290000,
            Stock = 55,
            Description = "RAM Corsair Vengeance LPX DDR4-3200 32GB kit (2x16GB) CL16. Thiết kế low-profile phổ biến nhất. XMP 2.0 compatible. RAM DDR4 best-seller cho mọi build.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, corsairLpx32Id, type: "DDR4", speed: 3200, capacityPerStick: 16, sticks: 2, latency: 16);
        productImages.Add(new ProductImage { ProductId = corsairLpx32Id, ImageUrl = "https://www.corsair.com/medias/sys_master/images/images/h8e/hfb/9109818802206/-CMK32GX4M2E3200C16-Gallery-?"});

        // Kingston Fury Beast DDR4-3200 16GB (2x8GB)
        var kingstonBeastDdr4Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = kingstonBeastDdr4Id,
            Name = "Kingston Fury Beast DDR4-3200 16GB (2x8GB)",
            Sku = "RAM-KINGSTON-FURY-BEAST-DDR4-3200-16GB",
            CategoryId = ramCategory.Id,
            BrandId = kingstonBrand.Id,
            Price = 1190000,
            Stock = 70,
            Description = "RAM Kingston Fury Beast DDR4-3200 16GB kit (2x8GB) CL16. Thiết kế heatsink đẹp, XMP ready. RAM DDR4 giá rẻ phổ biến cho gaming và văn phòng.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, kingstonBeastDdr4Id, type: "DDR4", speed: 3200, capacityPerStick: 8, sticks: 2, latency: 16);
        productImages.Add(new ProductImage { ProductId = kingstonBeastDdr4Id, ImageUrl = "https://media.kingston.com/kingston/product/ktc-product-memory-fury-beast-ddr4-lg.jpg" });

        // Crucial Ballistix DDR4-3600 32GB (2x16GB)
        var crucialBallistixId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = crucialBallistixId,
            Name = "Crucial Ballistix DDR4-3600 32GB (2x16GB)",
            Sku = "RAM-CRUCIAL-BALLISTIX-DDR4-3600-32GB",
            CategoryId = ramCategory.Id,
            BrandId = crucialBrand.Id,
            Price = 2590000,
            Stock = 40,
            Description = "RAM Crucial Ballistix DDR4-3600 32GB kit (2x16GB) CL16. Micron E-die nổi tiếng OC tốt. XMP 2.0 compatible. RAM DDR4 high-performance cho enthusiast.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddRamSpecs(specValues, ramSpecs, crucialBallistixId, type: "DDR4", speed: 3600, capacityPerStick: 16, sticks: 2, latency: 16);
        productImages.Add(new ProductImage { ProductId = crucialBallistixId, ImageUrl = "https://www.crucial.com/content/dam/crucial/dram-products/ballistix-line/images/in-use/crucial-ballistix-ddr4-in-use.png" });
    }

    private static void AddRamSpecs(List<ProductSpecValue> specValues, List<SpecDefinition> specs, Guid productId, string type, int speed, int capacityPerStick, int sticks, int latency)
    {
        var typeSpec = specs.FirstOrDefault(s => s.Code == "ram_type");
        if (typeSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = typeSpec.Id, TextValue = type });

        var speedSpec = specs.FirstOrDefault(s => s.Code == "ram_speed");
        if (speedSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = speedSpec.Id, NumberValue = speed });

        var capacitySpec = specs.FirstOrDefault(s => s.Code == "ram_capacity");
        if (capacitySpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = capacitySpec.Id, NumberValue = capacityPerStick });

        var sticksSpec = specs.FirstOrDefault(s => s.Code == "ram_sticks");
        if (sticksSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = sticksSpec.Id, NumberValue = sticks });

        var latencySpec = specs.FirstOrDefault(s => s.Code == "ram_latency");
        if (latencySpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = latencySpec.Id, NumberValue = latency });
    }

    private static async Task InitStorageProducts(ApplicationDbContext context, List<ProductSpecValue> specValues, List<ProductImage> productImages)
    {
        var storageCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Ổ cứng")
            ?? throw new NotFoundException("Không tìm thấy danh mục Ổ cứng");

        var storageSpecs = await context.SpecDefinitions
            .Where(s => s.CategoryId == storageCategory.Id)
            .ToListAsync();

        // Load storage brands
        var samsungBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Samsung")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Samsung");
        var wdBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Western Digital")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Western Digital");
        var crucialBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Crucial")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Crucial");
        var skHynixBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "SK Hynix")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu SK Hynix");
        var kingstonBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Kingston")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Kingston");
        var seagateBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Seagate")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Seagate");

        // ============= SAMSUNG SSDs =============

        // Samsung 990 Pro 2TB
        var samsung990Pro2TBId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = samsung990Pro2TBId,
            Name = "Samsung 990 Pro 2TB NVMe M.2 SSD",
            Sku = "SSD-SAMSUNG-990PRO-2TB",
            CategoryId = storageCategory.Id,
            BrandId = samsungBrand.Id,
            Price = 4500000,
            Stock = 50,
            Description = "Ổ cứng SSD Samsung 990 Pro 2TB NVMe M.2 PCIe Gen 4.0 x4, tốc độ đọc/ghi lên đến 7450/6900 MB/s. Thiết kế tản nhiệt hiệu quả, công nghệ V-NAND thế hệ mới. Lý tưởng cho gaming và workstation chuyên nghiệp.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddStorageSpecs(specValues, storageSpecs, samsung990Pro2TBId, type: "NVMe", capacity: 2000, interfaceType: "M.2", formFactor: "M.2", readSpeed: 7450, writeSpeed: 6900);
        productImages.Add(new ProductImage { ProductId = samsung990Pro2TBId, ImageUrl = "https://images.samsung.com/is/image/samsung/p6pim/uk/mz-v9p2t0bw/gallery/uk-990-pro-nvme-m2-ssd-mz-v9p2t0bw-thumb-534862489" });

        // Samsung 980 Pro 1TB
        var samsung980Pro1TBId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = samsung980Pro1TBId,
            Name = "Samsung 980 Pro 1TB NVMe M.2 SSD",
            Sku = "SSD-SAMSUNG-980PRO-1TB",
            CategoryId = storageCategory.Id,
            BrandId = samsungBrand.Id,
            Price = 2500000,
            Stock = 80,
            Description = "Ổ cứng SSD Samsung 980 Pro 1TB NVMe M.2 PCIe Gen 4.0 x4, tốc độ đọc/ghi lên đến 7000/5000 MB/s. Bộ điều khiển Elpis 8nm, công nghệ Intelligent TurboWrite. Tương thích PlayStation 5.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddStorageSpecs(specValues, storageSpecs, samsung980Pro1TBId, type: "NVMe", capacity: 1000, interfaceType: "M.2", formFactor: "M.2", readSpeed: 7000, writeSpeed: 5000);
        productImages.Add(new ProductImage { ProductId = samsung980Pro1TBId, ImageUrl = "https://images.samsung.com/is/image/samsung/p6pim/uk/mz-v8p1t0bw/gallery/uk-980-pro-nvme-m2-ssd-mz-v8p1t0bw-thumb-368338925" });

        // Samsung 870 EVO 1TB SATA
        var samsung870Evo1TBId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = samsung870Evo1TBId,
            Name = "Samsung 870 EVO 1TB SATA SSD",
            Sku = "SSD-SAMSUNG-870EVO-1TB",
            CategoryId = storageCategory.Id,
            BrandId = samsungBrand.Id,
            Price = 2300000,
            Stock = 100,
            Description = "Ổ cứng SSD Samsung 870 EVO 1TB SATA III 2.5 inch, tốc độ đọc/ghi lên đến 560/530 MB/s. Công nghệ V-NAND, bộ điều khiển MKX, độ bền cao với TBW 600TB. Nâng cấp hoàn hảo cho laptop và PC.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddStorageSpecs(specValues, storageSpecs, samsung870Evo1TBId, type: "SATA SSD", capacity: 1000, interfaceType: "2.5\"", formFactor: "2.5\"", readSpeed: 560, writeSpeed: 530);
        productImages.Add(new ProductImage { ProductId = samsung870Evo1TBId, ImageUrl = "https://images.samsung.com/is/image/samsung/p6pim/uk/mz-77e1t0b-eu/gallery/uk-870-evo-sata-3-2-5-ssd-mz-77e1t0b-eu-thumb-368338546" });

        // ============= WESTERN DIGITAL SSDs =============

        // WD Black SN850X 2TB
        var wdSn850x2TBId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = wdSn850x2TBId,
            Name = "WD Black SN850X 2TB NVMe M.2 SSD",
            Sku = "SSD-WD-SN850X-2TB",
            CategoryId = storageCategory.Id,
            BrandId = wdBrand.Id,
            Price = 4300000,
            Stock = 45,
            Description = "Ổ cứng SSD WD Black SN850X 2TB NVMe M.2 PCIe Gen 4.0 x4, tốc độ đọc/ghi lên đến 7300/6600 MB/s. Game Mode 2.0, tối ưu cho gaming với độ trễ cực thấp. Tương thích PlayStation 5.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddStorageSpecs(specValues, storageSpecs, wdSn850x2TBId, type: "NVMe", capacity: 2000, interfaceType: "M.2", formFactor: "M.2", readSpeed: 7300, writeSpeed: 6600);
        productImages.Add(new ProductImage { ProductId = wdSn850x2TBId, ImageUrl = "https://www.westerndigital.com/content/dam/store/en-us/assets/products/internal-storage/wd-black-sn850x-nvme-ssd/gallery/wd-black-sn850x-nvme-ssd-hero.png" });

        // WD Black SN770 1TB
        var wdSn7701TBId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = wdSn7701TBId,
            Name = "WD Black SN770 1TB NVMe M.2 SSD",
            Sku = "SSD-WD-SN770-1TB",
            CategoryId = storageCategory.Id,
            BrandId = wdBrand.Id,
            Price = 1800000,
            Stock = 120,
            Description = "Ổ cứng SSD WD Black SN770 1TB NVMe M.2 PCIe Gen 4.0 x4, tốc độ đọc/ghi lên đến 5150/4900 MB/s. Thiết kế không cần heatsink, tiết kiệm năng lượng. Lựa chọn tầm trung xuất sắc cho gaming.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddStorageSpecs(specValues, storageSpecs, wdSn7701TBId, type: "NVMe", capacity: 1000, interfaceType: "M.2", formFactor: "M.2", readSpeed: 5150, writeSpeed: 4900);
        productImages.Add(new ProductImage { ProductId = wdSn7701TBId, ImageUrl = "https://www.westerndigital.com/content/dam/store/en-us/assets/products/internal-storage/wd-black-sn770-nvme-ssd/gallery/wd-black-sn770-nvme-ssd-hero.png" });

        // ============= CRUCIAL SSDs =============

        // Crucial T700 2TB Gen5
        var crucialT7002TBId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = crucialT7002TBId,
            Name = "Crucial T700 2TB Gen5 NVMe M.2 SSD",
            Sku = "SSD-CRUCIAL-T700-2TB",
            CategoryId = storageCategory.Id,
            BrandId = crucialBrand.Id,
            Price = 6200000,
            Stock = 25,
            Description = "Ổ cứng SSD Crucial T700 2TB NVMe M.2 PCIe Gen 5.0 x4, tốc độ đọc/ghi lên đến 12400/11800 MB/s. SSD Gen5 nhanh nhất thế giới, công nghệ Micron 232-layer NAND. Dành cho enthusiast và workstation cao cấp.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddStorageSpecs(specValues, storageSpecs, crucialT7002TBId, type: "NVMe", capacity: 2000, interfaceType: "M.2", formFactor: "M.2", readSpeed: 12400, writeSpeed: 11800);
        productImages.Add(new ProductImage { ProductId = crucialT7002TBId, ImageUrl = "https://content.crucial.com/content/dam/crucial/ssd-products/t700/images/in-use/crucial-t700-ssd-image.png" });

        // Crucial P3 Plus 1TB
        var crucialP3Plus1TBId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = crucialP3Plus1TBId,
            Name = "Crucial P3 Plus 1TB NVMe M.2 SSD",
            Sku = "SSD-CRUCIAL-P3PLUS-1TB",
            CategoryId = storageCategory.Id,
            BrandId = crucialBrand.Id,
            Price = 1500000,
            Stock = 150,
            Description = "Ổ cứng SSD Crucial P3 Plus 1TB NVMe M.2 PCIe Gen 4.0 x4, tốc độ đọc/ghi lên đến 5000/4200 MB/s. Giải pháp lưu trữ tốc độ cao với giá cả phải chăng, phù hợp cho người dùng phổ thông và gaming.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddStorageSpecs(specValues, storageSpecs, crucialP3Plus1TBId, type: "NVMe", capacity: 1000, interfaceType: "M.2", formFactor: "M.2", readSpeed: 5000, writeSpeed: 4200);
        productImages.Add(new ProductImage { ProductId = crucialP3Plus1TBId, ImageUrl = "https://content.crucial.com/content/dam/crucial/ssd-products/p3-plus/images/in-use/crucial-p3plus-ssd-image.png" });

        // ============= SK HYNIX SSD =============

        // SK Hynix Platinum P41 1TB
        var skHynixP411TBId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = skHynixP411TBId,
            Name = "SK Hynix Platinum P41 1TB NVMe M.2 SSD",
            Sku = "SSD-SKHYNIX-P41-1TB",
            CategoryId = storageCategory.Id,
            BrandId = skHynixBrand.Id,
            Price = 2300000,
            Stock = 60,
            Description = "Ổ cứng SSD SK Hynix Platinum P41 1TB NVMe M.2 PCIe Gen 4.0 x4, tốc độ đọc/ghi lên đến 7000/6500 MB/s. Công nghệ 176-layer NAND của SK Hynix, bộ điều khiển ARIES. Hiệu năng đỉnh cao cho gaming và sáng tạo nội dung.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddStorageSpecs(specValues, storageSpecs, skHynixP411TBId, type: "NVMe", capacity: 1000, interfaceType: "M.2", formFactor: "M.2", readSpeed: 7000, writeSpeed: 6500);
        productImages.Add(new ProductImage { ProductId = skHynixP411TBId, ImageUrl = "https://ssd.skhynix.com/wp-content/uploads/2022/06/platinum_p41_image.png" });

        // ============= KINGSTON SSD =============

        // Kingston NV2 1TB
        var kingstonNv21TBId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = kingstonNv21TBId,
            Name = "Kingston NV2 1TB NVMe M.2 SSD",
            Sku = "SSD-KINGSTON-NV2-1TB",
            CategoryId = storageCategory.Id,
            BrandId = kingstonBrand.Id,
            Price = 1300000,
            Stock = 200,
            Description = "Ổ cứng SSD Kingston NV2 1TB NVMe M.2 PCIe Gen 4.0 x4, tốc độ đọc/ghi lên đến 3500/2100 MB/s. Thiết kế mỏng nhẹ form factor 2280, tiêu thụ điện năng thấp. Lựa chọn kinh tế cho nâng cấp hệ thống.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddStorageSpecs(specValues, storageSpecs, kingstonNv21TBId, type: "NVMe", capacity: 1000, interfaceType: "M.2", formFactor: "M.2", readSpeed: 3500, writeSpeed: 2100);
        productImages.Add(new ProductImage { ProductId = kingstonNv21TBId, ImageUrl = "https://media.kingston.com/kingston/product/ktc-product-ssd-snv2-1-background-zm-lg.jpg" });

        // ============= SEAGATE HDD =============

        // Seagate Barracuda 2TB HDD
        var seagateBarracuda2TBId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = seagateBarracuda2TBId,
            Name = "Seagate Barracuda 2TB 7200RPM HDD",
            Sku = "HDD-SEAGATE-BARRACUDA-2TB",
            CategoryId = storageCategory.Id,
            BrandId = seagateBrand.Id,
            Price = 1300000,
            Stock = 180,
            Description = "Ổ cứng HDD Seagate Barracuda 2TB 3.5 inch SATA III, tốc độ quay 7200RPM, cache 256MB. Công nghệ Multi-Tier Caching tối ưu hiệu suất. Dung lượng lớn với giá thành hợp lý cho lưu trữ dữ liệu.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddStorageSpecs(specValues, storageSpecs, seagateBarracuda2TBId, type: "HDD", capacity: 2000, interfaceType: "3.5\"", formFactor: "3.5\"", readSpeed: 190, writeSpeed: 190);
        productImages.Add(new ProductImage { ProductId = seagateBarracuda2TBId, ImageUrl = "https://www.seagate.com/content/dam/seagate/migrated-assets/www-content/product-content/barracuda-background-drives/barracuda-background-drives-lineup-background/barracuda-background-drives-lineup-background-background-image.png" });
    }

    private static void AddStorageSpecs(List<ProductSpecValue> specValues, List<SpecDefinition> specs, Guid productId, string type, int capacity, string interfaceType, string formFactor, int readSpeed, int writeSpeed)
    {
        var typeSpec = specs.FirstOrDefault(s => s.Code == "stor_type");
        if (typeSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = typeSpec.Id, TextValue = type });

        var capacitySpec = specs.FirstOrDefault(s => s.Code == "stor_capacity");
        if (capacitySpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = capacitySpec.Id, NumberValue = capacity });

        var interfaceSpec = specs.FirstOrDefault(s => s.Code == "stor_interface");
        if (interfaceSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = interfaceSpec.Id, TextValue = interfaceType });

        var formFactorSpec = specs.FirstOrDefault(s => s.Code == "stor_form_factor");
        if (formFactorSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = formFactorSpec.Id, TextValue = formFactor });

        var readSpeedSpec = specs.FirstOrDefault(s => s.Code == "stor_read_speed");
        if (readSpeedSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = readSpeedSpec.Id, NumberValue = readSpeed });

        var writeSpeedSpec = specs.FirstOrDefault(s => s.Code == "stor_write_speed");
        if (writeSpeedSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = writeSpeedSpec.Id, NumberValue = writeSpeed });
    }

    private static async Task InitPsuProducts(ApplicationDbContext context, List<ProductSpecValue> specValues, List<ProductImage> productImages)
    {
        var psuCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Nguồn máy tính")
            ?? throw new NotFoundException("Không tìm thấy danh mục Nguồn máy tính");

        var psuSpecs = await context.SpecDefinitions
            .Where(s => s.CategoryId == psuCategory.Id)
            .ToListAsync();

        // Load PSU brands
        var corsairBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Corsair")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Corsair");
        var seasonicBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Seasonic")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Seasonic");
        var beQuietBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "be quiet!")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu be quiet!");
        var msiBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "MSI")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu MSI");
        var coolerMasterBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Cooler Master")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Cooler Master");
        var nzxtBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "NZXT")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu NZXT");
        var thermaltakeBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Thermaltake")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Thermaltake");
        var superFlowerBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Super Flower")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Super Flower");

        // ============= HIGH-END PSUs (1000W+) =============

        // Corsair RM1000x 2021
        var corsairRm1000xId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = corsairRm1000xId,
            Name = "Corsair RM1000x 2021 1000W 80+ Gold",
            Sku = "PSU-CORSAIR-RM1000X",
            CategoryId = psuCategory.Id,
            BrandId = corsairBrand.Id,
            Price = 4500000,
            Stock = 30,
            Description = "Nguồn máy tính Corsair RM1000x 2021 công suất 1000W, chứng nhận 80+ Gold. Thiết kế Full Modular với quạt 135mm Zero RPM Mode hoạt động êm ái. Tụ điện 100% Nhật Bản, bảo hành 10 năm.",
            WarrantyMonth = 120,
            Status = ProductStatus.Available,
        });
        AddPsuSpecs(specValues, psuSpecs, corsairRm1000xId, wattage: 1000, efficiency: "80+ Gold", modular: "Full Modular", formFactor: "ATX");
        productImages.Add(new ProductImage { ProductId = corsairRm1000xId, ImageUrl = "https://assets.corsair.com/image/upload/c_pad,q_auto,h_1024,w_1024,f_auto/products/PSU/CP-9020201-NA/Gallery/RM1000x_01.webp" });

        // Seasonic Prime TX-1000 Titanium
        var seasonicPrimeTx1000Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = seasonicPrimeTx1000Id,
            Name = "Seasonic Prime TX-1000 1000W 80+ Titanium",
            Sku = "PSU-SEASONIC-PRIME-TX1000",
            CategoryId = psuCategory.Id,
            BrandId = seasonicBrand.Id,
            Price = 7500000,
            Stock = 15,
            Description = "Nguồn máy tính Seasonic Prime TX-1000 công suất 1000W, chứng nhận 80+ Titanium hiệu suất cao nhất. Thiết kế Full Modular với công nghệ Hybrid Silent Fan Control. Tụ điện Nhật Bản cao cấp, bảo hành 12 năm.",
            WarrantyMonth = 144,
            Status = ProductStatus.Available,
        });
        AddPsuSpecs(specValues, psuSpecs, seasonicPrimeTx1000Id, wattage: 1000, efficiency: "80+ Titanium", modular: "Full Modular", formFactor: "ATX");
        productImages.Add(new ProductImage { ProductId = seasonicPrimeTx1000Id, ImageUrl = "https://seasonic.com/pub/media/catalog/product/cache/8e5d6667a57342f0251089c50cb9df32/p/r/prime-tx-1000-side-connector.jpg" });

        // be quiet! Dark Power Pro 12 1200W
        var beQuietDpp12Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = beQuietDpp12Id,
            Name = "be quiet! Dark Power Pro 12 1200W 80+ Titanium",
            Sku = "PSU-BEQUIET-DPP12-1200W",
            CategoryId = psuCategory.Id,
            BrandId = beQuietBrand.Id,
            Price = 8900000,
            Stock = 10,
            Description = "Nguồn máy tính be quiet! Dark Power Pro 12 công suất 1200W, chứng nhận 80+ Titanium. Thiết kế Full Modular với quạt Silent Wings 135mm cực kỳ êm ái. Hỗ trợ chuẩn ATX 3.0 và PCIe 5.0, bảo hành 10 năm.",
            WarrantyMonth = 120,
            Status = ProductStatus.Available,
        });
        AddPsuSpecs(specValues, psuSpecs, beQuietDpp12Id, wattage: 1200, efficiency: "80+ Titanium", modular: "Full Modular", formFactor: "ATX");
        productImages.Add(new ProductImage { ProductId = beQuietDpp12Id, ImageUrl = "https://www.bequiet.com/admin/ImageServer.php?ID=7b9e9e42a26@be-quiet.net&omitaliases=true" });

        // ============= MID-HIGH PSUs (850W) =============

        // Corsair RM850x 2021
        var corsairRm850xId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = corsairRm850xId,
            Name = "Corsair RM850x 2021 850W 80+ Gold",
            Sku = "PSU-CORSAIR-RM850X",
            CategoryId = psuCategory.Id,
            BrandId = corsairBrand.Id,
            Price = 3200000,
            Stock = 50,
            Description = "Nguồn máy tính Corsair RM850x 2021 công suất 850W, chứng nhận 80+ Gold. Thiết kế Full Modular với quạt 135mm Magnetic Levitation hoạt động êm ái. Tụ điện 100% Nhật Bản, bảo hành 10 năm.",
            WarrantyMonth = 120,
            Status = ProductStatus.Available,
        });
        AddPsuSpecs(specValues, psuSpecs, corsairRm850xId, wattage: 850, efficiency: "80+ Gold", modular: "Full Modular", formFactor: "ATX");
        productImages.Add(new ProductImage { ProductId = corsairRm850xId, ImageUrl = "https://assets.corsair.com/image/upload/c_pad,q_auto,h_1024,w_1024,f_auto/products/PSU/CP-9020200-NA/Gallery/RM850x_01.webp" });

        // Seasonic Focus GX-850
        var seasonicFocusGx850Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = seasonicFocusGx850Id,
            Name = "Seasonic Focus GX-850 850W 80+ Gold",
            Sku = "PSU-SEASONIC-FOCUS-GX850",
            CategoryId = psuCategory.Id,
            BrandId = seasonicBrand.Id,
            Price = 2900000,
            Stock = 45,
            Description = "Nguồn máy tính Seasonic Focus GX-850 công suất 850W, chứng nhận 80+ Gold. Thiết kế Full Modular với quạt Fluid Dynamic Bearing 120mm. Tụ điện Nhật Bản, Hybrid Silent Fan Control, bảo hành 10 năm.",
            WarrantyMonth = 120,
            Status = ProductStatus.Available,
        });
        AddPsuSpecs(specValues, psuSpecs, seasonicFocusGx850Id, wattage: 850, efficiency: "80+ Gold", modular: "Full Modular", formFactor: "ATX");
        productImages.Add(new ProductImage { ProductId = seasonicFocusGx850Id, ImageUrl = "https://seasonic.com/pub/media/catalog/product/cache/8e5d6667a57342f0251089c50cb9df32/f/o/focus-gx-850-side-connector.jpg" });

        // MSI MPG A850GF
        var msiA850gfId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = msiA850gfId,
            Name = "MSI MPG A850GF 850W 80+ Gold",
            Sku = "PSU-MSI-MPG-A850GF",
            CategoryId = psuCategory.Id,
            BrandId = msiBrand.Id,
            Price = 2700000,
            Stock = 40,
            Description = "Nguồn máy tính MSI MPG A850GF công suất 850W, chứng nhận 80+ Gold. Thiết kế Full Modular với quạt 140mm Silent FDB hoạt động êm ái. Tụ điện 100% Nhật Bản, đáp ứng chuẩn 80 PLUS Gold, bảo hành 10 năm.",
            WarrantyMonth = 120,
            Status = ProductStatus.Available,
        });
        AddPsuSpecs(specValues, psuSpecs, msiA850gfId, wattage: 850, efficiency: "80+ Gold", modular: "Full Modular", formFactor: "ATX");
        productImages.Add(new ProductImage { ProductId = msiA850gfId, ImageUrl = "https://asset.msi.com/resize/image/global/product/product_1623138505a71f6a3c6c19a1c1c7e4e3e5c1e5e4.png62405b38c58fe0f07fcef2367d8a9ba1/1024.png" });

        // ============= MID-RANGE PSUs (750W) =============

        // Cooler Master MWE Gold 750 V2
        var cmMweGold750Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = cmMweGold750Id,
            Name = "Cooler Master MWE Gold 750 V2 750W 80+ Gold",
            Sku = "PSU-CM-MWE-GOLD-750-V2",
            CategoryId = psuCategory.Id,
            BrandId = coolerMasterBrand.Id,
            Price = 2100000,
            Stock = 60,
            Description = "Nguồn máy tính Cooler Master MWE Gold 750 V2 công suất 750W, chứng nhận 80+ Gold. Thiết kế Full Modular với quạt HDB 120mm êm ái. Cáp phẳng dễ đi dây, bảo vệ đầy đủ OVP/OPP/SCP/UVP/OCP/OTP.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddPsuSpecs(specValues, psuSpecs, cmMweGold750Id, wattage: 750, efficiency: "80+ Gold", modular: "Full Modular", formFactor: "ATX");
        productImages.Add(new ProductImage { ProductId = cmMweGold750Id, ImageUrl = "https://cdn.coolermaster.com/media/assets/1048/mwe-gold-750-v2-full-modular-gallery-1-zoom.png" });

        // NZXT C750 Gold
        var nzxtC750Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = nzxtC750Id,
            Name = "NZXT C750 Gold 750W 80+ Gold",
            Sku = "PSU-NZXT-C750-GOLD",
            CategoryId = psuCategory.Id,
            BrandId = nzxtBrand.Id,
            Price = 2400000,
            Stock = 35,
            Description = "Nguồn máy tính NZXT C750 Gold công suất 750W, chứng nhận 80+ Gold. Thiết kế Full Modular với quạt Fluid Dynamic Bearing 120mm. Chế độ Zero RPM Mode, tụ điện Nhật Bản, bảo hành 10 năm.",
            WarrantyMonth = 120,
            Status = ProductStatus.Available,
        });
        AddPsuSpecs(specValues, psuSpecs, nzxtC750Id, wattage: 750, efficiency: "80+ Gold", modular: "Full Modular", formFactor: "ATX");
        productImages.Add(new ProductImage { ProductId = nzxtC750Id, ImageUrl = "https://nzxt.com/assets/cms/34299/1632330799-c750-hero-gold.png" });

        // Super Flower Leadex III Gold 750W
        var sfLeadex3Gold750Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = sfLeadex3Gold750Id,
            Name = "Super Flower Leadex III Gold 750W 80+ Gold",
            Sku = "PSU-SF-LEADEX3-GOLD-750W",
            CategoryId = psuCategory.Id,
            BrandId = superFlowerBrand.Id,
            Price = 2300000,
            Stock = 40,
            Description = "Nguồn máy tính Super Flower Leadex III Gold công suất 750W, chứng nhận 80+ Gold. Thiết kế Full Modular với quạt FDB 130mm êm ái. Tụ điện Nhật Bản 105°C, chế độ Eco Mode tiết kiệm điện, bảo hành 7 năm.",
            WarrantyMonth = 84,
            Status = ProductStatus.Available,
        });
        AddPsuSpecs(specValues, psuSpecs, sfLeadex3Gold750Id, wattage: 750, efficiency: "80+ Gold", modular: "Full Modular", formFactor: "ATX");
        productImages.Add(new ProductImage { ProductId = sfLeadex3Gold750Id, ImageUrl = "https://www.super-flower.com/images/product/Leadex_III_Gold/Leadex_III_Gold_750W.png" });

        // ============= BUDGET PSUs (650W) =============

        // Thermaltake Toughpower GF1 650W
        var ttToughpowerGf1650Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = ttToughpowerGf1650Id,
            Name = "Thermaltake Toughpower GF1 650W 80+ Gold",
            Sku = "PSU-TT-TOUGHPOWER-GF1-650W",
            CategoryId = psuCategory.Id,
            BrandId = thermaltakeBrand.Id,
            Price = 1800000,
            Stock = 70,
            Description = "Nguồn máy tính Thermaltake Toughpower GF1 650W, chứng nhận 80+ Gold. Thiết kế Full Modular với quạt 140mm Hydraulic Bearing êm ái. Smart Zero Fan Mode, tụ điện Nhật Bản, bảo hành 10 năm.",
            WarrantyMonth = 120,
            Status = ProductStatus.Available,
        });
        AddPsuSpecs(specValues, psuSpecs, ttToughpowerGf1650Id, wattage: 650, efficiency: "80+ Gold", modular: "Full Modular", formFactor: "ATX");
        productImages.Add(new ProductImage { ProductId = ttToughpowerGf1650Id, ImageUrl = "https://www.thermaltake.com/media/catalog/product/cache/36a91ccfb2a23eaa18b14da3adc33e51/t/o/toughpower_gf1_650w_1.jpg" });

        // Corsair CV650 Bronze
        var corsairCv650Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = corsairCv650Id,
            Name = "Corsair CV650 650W 80+ Bronze",
            Sku = "PSU-CORSAIR-CV650",
            CategoryId = psuCategory.Id,
            BrandId = corsairBrand.Id,
            Price = 1200000,
            Stock = 100,
            Description = "Nguồn máy tính Corsair CV650 công suất 650W, chứng nhận 80+ Bronze. Thiết kế Non-Modular với quạt 120mm Thermally Controlled. Giải pháp nguồn ổn định với giá cả phải chăng, bảo hành 3 năm.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddPsuSpecs(specValues, psuSpecs, corsairCv650Id, wattage: 650, efficiency: "80+ Bronze", modular: "Non-Modular", formFactor: "ATX");
        productImages.Add(new ProductImage { ProductId = corsairCv650Id, ImageUrl = "https://assets.corsair.com/image/upload/c_pad,q_auto,h_1024,w_1024,f_auto/products/PSU/CP-9020211-NA/Gallery/CV650_01.webp" });
    }

    private static void AddPsuSpecs(List<ProductSpecValue> specValues, List<SpecDefinition> specs, Guid productId, int wattage, string efficiency, string modular, string formFactor)
    {
        var wattageSpec = specs.FirstOrDefault(s => s.Code == "psu_wattage");
        if (wattageSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = wattageSpec.Id, NumberValue = wattage });

        var efficiencySpec = specs.FirstOrDefault(s => s.Code == "psu_efficiency");
        if (efficiencySpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = efficiencySpec.Id, TextValue = efficiency });

        var modularSpec = specs.FirstOrDefault(s => s.Code == "psu_modular");
        if (modularSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = modularSpec.Id, TextValue = modular });

        var formFactorSpec = specs.FirstOrDefault(s => s.Code == "psu_form_factor");
        if (formFactorSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = formFactorSpec.Id, TextValue = formFactor });
    }

    private static async Task InitCaseProducts(ApplicationDbContext context, List<ProductSpecValue> specValues, List<ProductImage> productImages)
    {
        var caseCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Vỏ máy tính")
            ?? throw new NotFoundException("Không tìm thấy danh mục Vỏ máy tính");

        var caseSpecs = await context.SpecDefinitions
            .Where(s => s.CategoryId == caseCategory.Id)
            .ToListAsync();

        // Load Case brands
        var lianLiBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Lian Li")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Lian Li");
        var nzxtBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "NZXT")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu NZXT");
        var corsairBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Corsair")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Corsair");
        var phanteksBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Phanteks")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Phanteks");
        var fractalBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Fractal Design")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Fractal Design");
        var beQuietBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "be quiet!")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu be quiet!");
        var coolerMasterBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Cooler Master")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Cooler Master");
        var montechBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Montech")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Montech");
        var thermaltakeBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Thermaltake")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Thermaltake");
        var jonsboBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Jonsbo")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Jonsbo");

        // ============= HIGH-END CASES =============

        // Lian Li O11 Dynamic EVO
        var lianLiO11EvoId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = lianLiO11EvoId,
            Name = "Lian Li O11 Dynamic EVO",
            Sku = "CASE-LIANLI-O11-DYNAMIC-EVO",
            CategoryId = caseCategory.Id,
            BrandId = lianLiBrand.Id,
            Price = 4500000,
            Stock = 25,
            Description = "Vỏ case Lian Li O11 Dynamic EVO với thiết kế dual-chamber iconic. Hỗ trợ ATX/mATX/ITX, kính cường lực 2 mặt. Khả năng lắp đặt tản nhiệt nước custom loop tuyệt vời với không gian cho radiator 360mm ở nhiều vị trí. Thiết kế modular linh hoạt.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCaseSpecs(specValues, caseSpecs, lianLiO11EvoId, formFactor: "ATX,mATX,ITX", maxGpuLength: 422, maxCoolerHeight: 167, psuFormFactor: "ATX", driveBays25: 6, driveBays35: 3, maxFanSize: 140);
        productImages.Add(new ProductImage { ProductId = lianLiO11EvoId, ImageUrl = "https://lian-li.com/wp-content/uploads/2022/01/O11-EVO-1.jpg" });

        // NZXT H7 Flow
        var nzxtH7FlowId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = nzxtH7FlowId,
            Name = "NZXT H7 Flow",
            Sku = "CASE-NZXT-H7-FLOW",
            CategoryId = caseCategory.Id,
            BrandId = nzxtBrand.Id,
            Price = 3500000,
            Stock = 30,
            Description = "Vỏ case NZXT H7 Flow với thiết kế airflow tối ưu qua mặt lưới phía trước. Hỗ trợ ATX/mATX/ITX, kính cường lực bên hông. Cable management xuất sắc với không gian rộng phía sau. Hỗ trợ radiator 360mm phía trước và trên.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCaseSpecs(specValues, caseSpecs, nzxtH7FlowId, formFactor: "ATX,mATX,ITX", maxGpuLength: 400, maxCoolerHeight: 185, psuFormFactor: "ATX", driveBays25: 4, driveBays35: 2, maxFanSize: 140);
        productImages.Add(new ProductImage { ProductId = nzxtH7FlowId, ImageUrl = "https://nzxt.com/assets/cms/34299/1663791631-h7-flow-white-hero.png" });

        // Fractal Design North
        var fractalNorthId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = fractalNorthId,
            Name = "Fractal Design North",
            Sku = "CASE-FRACTAL-NORTH",
            CategoryId = caseCategory.Id,
            BrandId = fractalBrand.Id,
            Price = 3800000,
            Stock = 20,
            Description = "Vỏ case Fractal Design North với thiết kế Scandinavian độc đáo, mặt trước gỗ óc chó tự nhiên. Hỗ trợ ATX/mATX/ITX, airflow tuyệt vời qua lưới gỗ. Kính cường lực bên hông, cable management tốt. Thiết kế nội thất cao cấp.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCaseSpecs(specValues, caseSpecs, fractalNorthId, formFactor: "ATX,mATX,ITX", maxGpuLength: 355, maxCoolerHeight: 170, psuFormFactor: "ATX", driveBays25: 4, driveBays35: 2, maxFanSize: 140);
        productImages.Add(new ProductImage { ProductId = fractalNorthId, ImageUrl = "https://www.fractal-design.com/app/uploads/2022/09/North-Charcoal-Black-TG-Dark-Left-Front-Angled-scaled.jpg" });

        // ============= MID-RANGE CASES =============

        // Corsair 4000D Airflow
        var corsair4000dId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = corsair4000dId,
            Name = "Corsair 4000D Airflow",
            Sku = "CASE-CORSAIR-4000D-AIRFLOW",
            CategoryId = caseCategory.Id,
            BrandId = corsairBrand.Id,
            Price = 2500000,
            Stock = 45,
            Description = "Vỏ case Corsair 4000D Airflow với mặt lưới thép tối ưu luồng khí. Hỗ trợ ATX/mATX/ITX, kính cường lực bên hông. RapidRoute cable management system tiện lợi. Hỗ trợ radiator 360mm, đi kèm 2 quạt 120mm AirGuide.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCaseSpecs(specValues, caseSpecs, corsair4000dId, formFactor: "ATX,mATX,ITX", maxGpuLength: 360, maxCoolerHeight: 170, psuFormFactor: "ATX", driveBays25: 4, driveBays35: 2, maxFanSize: 140);
        productImages.Add(new ProductImage { ProductId = corsair4000dId, ImageUrl = "https://assets.corsair.com/image/upload/c_pad,q_auto,h_1024,w_1024,f_auto/products/Cases/CC-9011200-WW/Gallery/4000D_AF_BLK_01.webp" });

        // Phanteks Eclipse G360A
        var phanteksG360aId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = phanteksG360aId,
            Name = "Phanteks Eclipse G360A",
            Sku = "CASE-PHANTEKS-G360A",
            CategoryId = caseCategory.Id,
            BrandId = phanteksBrand.Id,
            Price = 2200000,
            Stock = 40,
            Description = "Vỏ case Phanteks Eclipse G360A với mặt lưới Ultra-fine Performance Mesh. Hỗ trợ ATX/mATX/ITX, kính cường lực bên hông. Đi kèm 3 quạt D-RGB 120mm phía trước. Hỗ trợ radiator 360mm, cable management tốt.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCaseSpecs(specValues, caseSpecs, phanteksG360aId, formFactor: "ATX,mATX,ITX", maxGpuLength: 400, maxCoolerHeight: 163, psuFormFactor: "ATX", driveBays25: 4, driveBays35: 2, maxFanSize: 140);
        productImages.Add(new ProductImage { ProductId = phanteksG360aId, ImageUrl = "https://phanteks.com/images/product/Eclipse-G360A/Eclipse-G360A-1.jpg" });

        // be quiet! Pure Base 500DX
        var beQuietPureBase500dxId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = beQuietPureBase500dxId,
            Name = "be quiet! Pure Base 500DX",
            Sku = "CASE-BEQUIET-PURE-BASE-500DX",
            CategoryId = caseCategory.Id,
            BrandId = beQuietBrand.Id,
            Price = 2800000,
            Stock = 35,
            Description = "Vỏ case be quiet! Pure Base 500DX với thiết kế cân bằng giữa airflow và độ êm. Hỗ trợ ATX/mATX/ITX, kính cường lực bên hông. Đi kèm 3 quạt Pure Wings 2 140mm. ARGB LED tích hợp, hỗ trợ radiator 360mm.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCaseSpecs(specValues, caseSpecs, beQuietPureBase500dxId, formFactor: "ATX,mATX,ITX", maxGpuLength: 369, maxCoolerHeight: 190, psuFormFactor: "ATX", driveBays25: 5, driveBays35: 2, maxFanSize: 140);
        productImages.Add(new ProductImage { ProductId = beQuietPureBase500dxId, ImageUrl = "https://www.bequiet.com/admin/ImageServer.php?ID=a8da9a52a26@be-quiet.net&omitaliases=true" });

        // ============= BUDGET CASES =============

        // Cooler Master MasterBox TD500 Mesh
        var cmTd500MeshId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = cmTd500MeshId,
            Name = "Cooler Master MasterBox TD500 Mesh",
            Sku = "CASE-CM-TD500-MESH",
            CategoryId = caseCategory.Id,
            BrandId = coolerMasterBrand.Id,
            Price = 2000000,
            Stock = 50,
            Description = "Vỏ case Cooler Master MasterBox TD500 Mesh với mặt lưới polygonal 3D độc đáo. Hỗ trợ ATX/mATX/ITX, kính cường lực bên hông. Đi kèm 3 quạt ARGB 120mm. Hỗ trợ radiator 360mm, thiết kế airflow tốt với giá phải chăng.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCaseSpecs(specValues, caseSpecs, cmTd500MeshId, formFactor: "ATX,mATX,ITX", maxGpuLength: 410, maxCoolerHeight: 165, psuFormFactor: "ATX", driveBays25: 4, driveBays35: 2, maxFanSize: 140);
        productImages.Add(new ProductImage { ProductId = cmTd500MeshId, ImageUrl = "https://cdn.coolermaster.com/media/assets/1035/td500-mesh-gallery-1-image.png" });

        // Montech Air 903 Max
        var montechAir903MaxId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = montechAir903MaxId,
            Name = "Montech Air 903 Max",
            Sku = "CASE-MONTECH-AIR-903-MAX",
            CategoryId = caseCategory.Id,
            BrandId = montechBrand.Id,
            Price = 1800000,
            Stock = 55,
            Description = "Vỏ case Montech Air 903 Max với thiết kế airflow mở tối đa. Hỗ trợ ATX/mATX/ITX, mặt lưới phía trước và trên. Đi kèm 3 quạt ARGB 140mm. Hỗ trợ radiator 360mm, giá cực kỳ cạnh tranh cho tính năng cao cấp.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCaseSpecs(specValues, caseSpecs, montechAir903MaxId, formFactor: "ATX,mATX,ITX", maxGpuLength: 400, maxCoolerHeight: 175, psuFormFactor: "ATX", driveBays25: 4, driveBays35: 2, maxFanSize: 140);
        productImages.Add(new ProductImage { ProductId = montechAir903MaxId, ImageUrl = "https://www.montech.co/wp-content/uploads/2023/05/AIR-903-MAX-Black-1.png" });

        // Thermaltake S100 TG
        var ttS100TgId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = ttS100TgId,
            Name = "Thermaltake S100 TG Snow Edition",
            Sku = "CASE-TT-S100-TG-SNOW",
            CategoryId = caseCategory.Id,
            BrandId = thermaltakeBrand.Id,
            Price = 1500000,
            Stock = 60,
            Description = "Vỏ case Thermaltake S100 TG Snow Edition dạng Micro-ATX màu trắng. Hỗ trợ mATX/ITX, kính cường lực bên hông. Thiết kế nhỏ gọn với airflow tốt. Hỗ trợ radiator 280mm, phù hợp cho build compact.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCaseSpecs(specValues, caseSpecs, ttS100TgId, formFactor: "mATX,ITX", maxGpuLength: 330, maxCoolerHeight: 165, psuFormFactor: "ATX", driveBays25: 3, driveBays35: 2, maxFanSize: 120);
        productImages.Add(new ProductImage { ProductId = ttS100TgId, ImageUrl = "https://www.thermaltake.com/media/catalog/product/cache/36a91ccfb2a23eaa18b14da3adc33e51/s/1/s100_tg_snow_1.jpg" });

        // ============= COMPACT/SFF CASES =============

        // Jonsbo D31 Mesh
        var jonsboD31MeshId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = jonsboD31MeshId,
            Name = "Jonsbo D31 Mesh",
            Sku = "CASE-JONSBO-D31-MESH",
            CategoryId = caseCategory.Id,
            BrandId = jonsboBrand.Id,
            Price = 1600000,
            Stock = 45,
            Description = "Vỏ case Jonsbo D31 Mesh dạng Micro-ATX với thiết kế mesh airflow. Hỗ trợ mATX/ITX, hỗ trợ đặt mainboard nằm ngang hoặc đứng. Kính cường lực bên hông, hỗ trợ radiator 240mm. Thiết kế compact đẹp mắt.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCaseSpecs(specValues, caseSpecs, jonsboD31MeshId, formFactor: "mATX,ITX", maxGpuLength: 325, maxCoolerHeight: 168, psuFormFactor: "ATX,SFX", driveBays25: 2, driveBays35: 1, maxFanSize: 140);
        productImages.Add(new ProductImage { ProductId = jonsboD31MeshId, ImageUrl = "https://www.jonsbo.com/Public/Uploads/uploadfile/images/20221014/D31%20MESH-01.jpg" });
    }

    private static void AddCaseSpecs(List<ProductSpecValue> specValues, List<SpecDefinition> specs, Guid productId, string formFactor, int maxGpuLength, int maxCoolerHeight, string psuFormFactor, int driveBays25, int driveBays35, int maxFanSize)
    {
        var formFactorSpec = specs.FirstOrDefault(s => s.Code == "case_form_factor");
        if (formFactorSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = formFactorSpec.Id, TextValue = formFactor });

        var maxGpuLengthSpec = specs.FirstOrDefault(s => s.Code == "case_max_gpu_length");
        if (maxGpuLengthSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = maxGpuLengthSpec.Id, NumberValue = maxGpuLength });

        var maxCoolerHeightSpec = specs.FirstOrDefault(s => s.Code == "case_max_cooler_height");
        if (maxCoolerHeightSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = maxCoolerHeightSpec.Id, NumberValue = maxCoolerHeight });

        var psuFormFactorSpec = specs.FirstOrDefault(s => s.Code == "case_psu_form_factor");
        if (psuFormFactorSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = psuFormFactorSpec.Id, TextValue = psuFormFactor });

        var driveBays25Spec = specs.FirstOrDefault(s => s.Code == "case_drive_bays_25");
        if (driveBays25Spec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = driveBays25Spec.Id, NumberValue = driveBays25 });

        var driveBays35Spec = specs.FirstOrDefault(s => s.Code == "case_drive_bays_35");
        if (driveBays35Spec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = driveBays35Spec.Id, NumberValue = driveBays35 });

        var maxFanSizeSpec = specs.FirstOrDefault(s => s.Code == "case_max_fan_size");
        if (maxFanSizeSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = maxFanSizeSpec.Id, NumberValue = maxFanSize });
    }

    private static async Task InitCoolerProducts(ApplicationDbContext context, List<ProductSpecValue> specValues, List<ProductImage> productImages)
    {
        var coolerCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Tản nhiệt CPU")
            ?? throw new NotFoundException("Không tìm thấy danh mục Tản nhiệt CPU");

        var coolerSpecs = await context.SpecDefinitions
            .Where(s => s.CategoryId == coolerCategory.Id)
            .ToListAsync();

        // Load cooler brands
        var noctuaBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Noctua")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Noctua");
        var beQuietBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "be quiet!")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu be quiet!");
        var thermalrightBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Thermalright")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Thermalright");
        var corsairBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Corsair")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Corsair");
        var nzxtBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "NZXT")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu NZXT");
        var deepCoolBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "DeepCool")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu DeepCool");
        var arcticBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Arctic")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Arctic");
        var scytheBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Scythe")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Scythe");
        var coolerMasterBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Cooler Master")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Cooler Master");
        var idCoolingBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "ID-Cooling")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu ID-Cooling");

        // ============= HIGH-END AIR COOLERS =============

        // Noctua NH-D15 chromax.black
        var noctuaNhD15Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = noctuaNhD15Id,
            Name = "Noctua NH-D15 chromax.black",
            Sku = "COOLER-NOCTUA-NH-D15-BLACK",
            CategoryId = coolerCategory.Id,
            BrandId = noctuaBrand.Id,
            Price = 2800000,
            Stock = 25,
            Description = "Tản nhiệt khí Noctua NH-D15 chromax.black cao cấp với dual tower và 2 quạt NF-A15 PWM. Hiệu năng tản nhiệt hàng đầu, hoạt động cực kỳ êm ái. Hỗ trợ đa socket bao gồm LGA1700 và AM5. Thiết kế full black sang trọng.",
            WarrantyMonth = 72,
            Status = ProductStatus.Available,
        });
        AddCoolerSpecs(specValues, coolerSpecs, noctuaNhD15Id, coolerType: "Air", socketSupport: "LGA1700,LGA1200,LGA115x,AM5,AM4", tdpRating: 250, height: 165);
        productImages.Add(new ProductImage { ProductId = noctuaNhD15Id, ImageUrl = "https://noctua.at/pub/media/catalog/product/cache/74c1057f7991b4edb2bc7bdbd8571f7f/n/h/nh_d15_chromax_black_1_5.jpg" });

        // be quiet! Dark Rock Pro 5
        var beQuietDarkRockPro5Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = beQuietDarkRockPro5Id,
            Name = "be quiet! Dark Rock Pro 5",
            Sku = "COOLER-BEQUIET-DARK-ROCK-PRO-5",
            CategoryId = coolerCategory.Id,
            BrandId = beQuietBrand.Id,
            Price = 2500000,
            Stock = 30,
            Description = "Tản nhiệt khí be quiet! Dark Rock Pro 5 với thiết kế dual tower và 7 ống dẫn nhiệt. Trang bị 2 quạt Silent Wings 4, hoạt động cực êm với độ ồn chỉ 24.3 dB(A). Hiệu năng làm mát xuất sắc cho CPU cao cấp.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCoolerSpecs(specValues, coolerSpecs, beQuietDarkRockPro5Id, coolerType: "Air", socketSupport: "LGA1700,LGA1200,LGA115x,AM5,AM4", tdpRating: 270, height: 168);
        productImages.Add(new ProductImage { ProductId = beQuietDarkRockPro5Id, ImageUrl = "https://www.bequiet.com/admin/ImageServer.php?ID=c5e5a5e5a5e@be-quiet.net&omitaliases=true" });

        // Thermalright Peerless Assassin 120 SE
        var thermalrightPA120SEId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = thermalrightPA120SEId,
            Name = "Thermalright Peerless Assassin 120 SE",
            Sku = "COOLER-THERMALRIGHT-PA120-SE",
            CategoryId = coolerCategory.Id,
            BrandId = thermalrightBrand.Id,
            Price = 850000,
            Stock = 50,
            Description = "Tản nhiệt khí Thermalright Peerless Assassin 120 SE với thiết kế dual tower giá rẻ. 6 ống dẫn nhiệt, 2 quạt TL-C12C PWM. Hiệu năng tuyệt vời so với giá tiền, cạnh tranh với các sản phẩm cao cấp hơn nhiều.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCoolerSpecs(specValues, coolerSpecs, thermalrightPA120SEId, coolerType: "Air", socketSupport: "LGA1700,LGA1200,LGA115x,AM5,AM4", tdpRating: 230, height: 155);
        productImages.Add(new ProductImage { ProductId = thermalrightPA120SEId, ImageUrl = "https://www.thermalright.com/wp-content/uploads/2022/01/PA120SE_01.jpg" });

        // ============= HIGH-END AIO COOLERS =============

        // Corsair iCUE H150i Elite LCD XT
        var corsairH150iId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = corsairH150iId,
            Name = "Corsair iCUE H150i Elite LCD XT",
            Sku = "COOLER-CORSAIR-H150I-ELITE-LCD-XT",
            CategoryId = coolerCategory.Id,
            BrandId = corsairBrand.Id,
            Price = 7500000,
            Stock = 15,
            Description = "Tản nhiệt nước AIO Corsair iCUE H150i Elite LCD XT 360mm với màn hình LCD 2.1 inch hiển thị thông tin và GIF. 3 quạt AF120 RGB Elite, đầu bơm thế hệ mới cải tiến. Tích hợp iCUE software điều khiển RGB và hiệu năng.",
            WarrantyMonth = 60,
            Status = ProductStatus.Available,
        });
        AddCoolerSpecs(specValues, coolerSpecs, corsairH150iId, coolerType: "AIO Liquid 360mm", socketSupport: "LGA1700,LGA1200,LGA115x,AM5,AM4", tdpRating: 350, height: 30);
        productImages.Add(new ProductImage { ProductId = corsairH150iId, ImageUrl = "https://assets.corsair.com/image/upload/c_pad,q_auto,h_1024,w_1024,f_auto/products/Cooling/CW-9060075-WW/Gallery/H150i_ELITE_LCD_XT_01.webp" });

        // NZXT Kraken Z73 RGB
        var nzxtKrakenZ73Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = nzxtKrakenZ73Id,
            Name = "NZXT Kraken Z73 RGB",
            Sku = "COOLER-NZXT-KRAKEN-Z73-RGB",
            CategoryId = coolerCategory.Id,
            BrandId = nzxtBrand.Id,
            Price = 7200000,
            Stock = 18,
            Description = "Tản nhiệt nước AIO NZXT Kraken Z73 RGB 360mm với màn hình LCD 2.36 inch tùy chỉnh. 3 quạt F120 RGB Core, đầu bơm Asetek thế hệ 7 mạnh mẽ. Điều khiển qua CAM software, thiết kế premium.",
            WarrantyMonth = 72,
            Status = ProductStatus.Available,
        });
        AddCoolerSpecs(specValues, coolerSpecs, nzxtKrakenZ73Id, coolerType: "AIO Liquid 360mm", socketSupport: "LGA1700,LGA1200,LGA115x,AM5,AM4", tdpRating: 350, height: 30);
        productImages.Add(new ProductImage { ProductId = nzxtKrakenZ73Id, ImageUrl = "https://nzxt.com/assets/cms/34299/1692822857-kraken-z73-rgb-black-hero.png" });

        // DeepCool LS720
        var deepCoolLS720Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = deepCoolLS720Id,
            Name = "DeepCool LS720",
            Sku = "COOLER-DEEPCOOL-LS720",
            CategoryId = coolerCategory.Id,
            BrandId = deepCoolBrand.Id,
            Price = 3500000,
            Stock = 25,
            Description = "Tản nhiệt nước AIO DeepCool LS720 360mm với thiết kế Infinity Mirror ARGB đẹp mắt. 3 quạt FC120 ARGB hiệu năng cao, đầu bơm thế hệ 4 công suất lớn. Giá thành hợp lý cho hiệu năng cao cấp.",
            WarrantyMonth = 36,
            Status = ProductStatus.Available,
        });
        AddCoolerSpecs(specValues, coolerSpecs, deepCoolLS720Id, coolerType: "AIO Liquid 360mm", socketSupport: "LGA1700,LGA1200,LGA115x,AM5,AM4", tdpRating: 320, height: 27);
        productImages.Add(new ProductImage { ProductId = deepCoolLS720Id, ImageUrl = "https://www.deepcool.com/media/catalog/product/cache/1/image/9df78eab33525d08d6e5fb8d27136e95/l/s/ls720_01.jpg" });

        // ============= MID-RANGE COOLERS =============

        // Arctic Liquid Freezer II 280
        var arcticLF280Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = arcticLF280Id,
            Name = "Arctic Liquid Freezer II 280",
            Sku = "COOLER-ARCTIC-LF2-280",
            CategoryId = coolerCategory.Id,
            BrandId = arcticBrand.Id,
            Price = 2800000,
            Stock = 30,
            Description = "Tản nhiệt nước AIO Arctic Liquid Freezer II 280mm với quạt VRM tích hợp trên đầu bơm. 2 quạt P14 PWM hiệu năng cao, ống dẫn bọc nylon chống rò rỉ. Hiệu năng làm mát xuất sắc với giá cực kỳ cạnh tranh.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCoolerSpecs(specValues, coolerSpecs, arcticLF280Id, coolerType: "AIO Liquid 280mm", socketSupport: "LGA1700,LGA1200,LGA115x,AM5,AM4", tdpRating: 300, height: 38);
        productImages.Add(new ProductImage { ProductId = arcticLF280Id, ImageUrl = "https://www.arctic.de/media/c0/fd/b7/1610709755/Liquid_Freezer_II_280_G01.png" });

        // Scythe Fuma 3
        var scytheFuma3Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = scytheFuma3Id,
            Name = "Scythe Fuma 3",
            Sku = "COOLER-SCYTHE-FUMA-3",
            CategoryId = coolerCategory.Id,
            BrandId = scytheBrand.Id,
            Price = 1500000,
            Stock = 35,
            Description = "Tản nhiệt khí Scythe Fuma 3 với thiết kế dual tower asymmetric không cản RAM. 6 ống dẫn nhiệt, 2 quạt Kaze Flex II PWM êm ái. Hiệu năng cao trong thiết kế nhỏ gọn, dễ lắp đặt.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCoolerSpecs(specValues, coolerSpecs, scytheFuma3Id, coolerType: "Air", socketSupport: "LGA1700,LGA1200,LGA115x,AM5,AM4", tdpRating: 220, height: 154);
        productImages.Add(new ProductImage { ProductId = scytheFuma3Id, ImageUrl = "https://www.scythe-eu.com/fileadmin/images/CPU-Cooler/Fuma-3/fuma3_main.jpg" });

        // Cooler Master Hyper 212 EVO V2
        var cmHyper212EvoV2Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = cmHyper212EvoV2Id,
            Name = "Cooler Master Hyper 212 EVO V2",
            Sku = "COOLER-CM-HYPER-212-EVO-V2",
            CategoryId = coolerCategory.Id,
            BrandId = coolerMasterBrand.Id,
            Price = 850000,
            Stock = 60,
            Description = "Tản nhiệt khí Cooler Master Hyper 212 EVO V2 - phiên bản cải tiến của huyền thoại Hyper 212. 4 ống dẫn nhiệt Direct Contact, quạt SickleFlow 120 PWM. Hệ thống gá đỡ mới dễ lắp đặt hơn.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCoolerSpecs(specValues, coolerSpecs, cmHyper212EvoV2Id, coolerType: "Air", socketSupport: "LGA1700,LGA1200,LGA115x,AM5,AM4", tdpRating: 180, height: 159);
        productImages.Add(new ProductImage { ProductId = cmHyper212EvoV2Id, ImageUrl = "https://cdn.coolermaster.com/media/assets/1035/hyper-212-evo-v2-gallery-1-image.png" });

        // ============= BUDGET COOLERS =============

        // ID-Cooling SE-214-XT
        var idCoolingSE214XTId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = idCoolingSE214XTId,
            Name = "ID-Cooling SE-214-XT ARGB",
            Sku = "COOLER-IDCOOLING-SE-214-XT-ARGB",
            CategoryId = coolerCategory.Id,
            BrandId = idCoolingBrand.Id,
            Price = 450000,
            Stock = 80,
            Description = "Tản nhiệt khí ID-Cooling SE-214-XT ARGB giá rẻ với 4 ống dẫn nhiệt Direct Touch. Quạt 120mm ARGB đẹp mắt, hỗ trợ sync với mainboard. Lựa chọn hoàn hảo cho build tầm trung và budget.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddCoolerSpecs(specValues, coolerSpecs, idCoolingSE214XTId, coolerType: "Air", socketSupport: "LGA1700,LGA1200,LGA115x,AM5,AM4", tdpRating: 180, height: 150);
        productImages.Add(new ProductImage { ProductId = idCoolingSE214XTId, ImageUrl = "https://www.idcooling.com/uploadfile/image/SE-214-XT-ARGB/01.jpg" });
    }

    private static void AddCoolerSpecs(List<ProductSpecValue> specValues, List<SpecDefinition> specs, Guid productId, string coolerType, string socketSupport, int tdpRating, int height)
    {
        var coolerTypeSpec = specs.FirstOrDefault(s => s.Code == "cooler_type");
        if (coolerTypeSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = coolerTypeSpec.Id, TextValue = coolerType });

        var socketSupportSpec = specs.FirstOrDefault(s => s.Code == "cooler_socket_support");
        if (socketSupportSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = socketSupportSpec.Id, TextValue = socketSupport });

        var tdpRatingSpec = specs.FirstOrDefault(s => s.Code == "cooler_tdp_rating");
        if (tdpRatingSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = tdpRatingSpec.Id, NumberValue = tdpRating });

        var heightSpec = specs.FirstOrDefault(s => s.Code == "cooler_height");
        if (heightSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = heightSpec.Id, NumberValue = height });
    }

    private static async Task InitKeyboardProducts(ApplicationDbContext context, List<ProductSpecValue> specValues, List<ProductImage> productImages)
    {
        var keyboardCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Bàn phím")
            ?? throw new NotFoundException("Không tìm thấy danh mục Bàn phím");

        var keyboardSpecs = await context.SpecDefinitions
            .Where(s => s.CategoryId == keyboardCategory.Id)
            .ToListAsync();

        // Load keyboard brands
        var logitechBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Logitech")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Logitech");
        var razerBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Razer")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Razer");
        var corsairBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Corsair")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Corsair");
        var steelSeriesBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "SteelSeries")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu SteelSeries");
        var duckyBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Ducky")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Ducky");
        var keychronBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Keychron")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Keychron");
        var akkoBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Akko")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Akko");
        var gloriousBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Glorious")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Glorious");
        var wootingBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Wooting")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Wooting");
        var leopoldBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Leopold")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Leopold");

        // ============= HIGH-END GAMING KEYBOARDS =============

        // Wooting 60HE+
        var wooting60HEId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = wooting60HEId,
            Name = "Wooting 60HE+",
            Sku = "KB-WOOTING-60HE-PLUS",
            CategoryId = keyboardCategory.Id,
            BrandId = wootingBrand.Id,
            Price = 4500000,
            Stock = 15,
            Description = "Bàn phím cơ Wooting 60HE+ với công nghệ Analog Hall Effect switches. Rapid Trigger với actuation point tùy chỉnh 0.1-4.0mm. Layout 60%, hot-swappable, được pro players tin dùng cho FPS games.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddKeyboardSpecs(specValues, keyboardSpecs, wooting60HEId, kbType: "Hall Effect", kbSwitch: "Lekker Switch", layout: "60%", connection: "USB-C có dây", rgb: true);
        productImages.Add(new ProductImage { ProductId = wooting60HEId, ImageUrl = "https://wooting.io/images/wooting-60he-plus.png" });

        // Razer Huntsman V3 Pro
        var razerHuntsmanV3ProId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = razerHuntsmanV3ProId,
            Name = "Razer Huntsman V3 Pro",
            Sku = "KB-RAZER-HUNTSMAN-V3-PRO",
            CategoryId = keyboardCategory.Id,
            BrandId = razerBrand.Id,
            Price = 5500000,
            Stock = 20,
            Description = "Bàn phím cơ Razer Huntsman V3 Pro với Analog Optical Switches Gen-2. Rapid Trigger và Adjustable Actuation. Layout TKL, magnetic wrist rest, Razer Chroma RGB. Thiết kế premium cho gaming đỉnh cao.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddKeyboardSpecs(specValues, keyboardSpecs, razerHuntsmanV3ProId, kbType: "Quang học", kbSwitch: "Razer Analog Optical Gen-2", layout: "TKL", connection: "USB-C có dây", rgb: true);
        productImages.Add(new ProductImage { ProductId = razerHuntsmanV3ProId, ImageUrl = "https://assets2.razerzone.com/images/pnx.assets/381e015fdc715eb63c822dea65c84b3e/razer-huntsman-v3-pro-usp-desktop.webp" });

        // Logitech G Pro X TKL Lightspeed
        var logitechGProXTKLId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = logitechGProXTKLId,
            Name = "Logitech G Pro X TKL Lightspeed",
            Sku = "KB-LOGITECH-G-PRO-X-TKL",
            CategoryId = keyboardCategory.Id,
            BrandId = logitechBrand.Id,
            Price = 4800000,
            Stock = 25,
            Description = "Bàn phím cơ Logitech G Pro X TKL Lightspeed không dây với GX switches hot-swappable. Kết nối Lightspeed 2.4GHz và Bluetooth. Layout TKL, LIGHTSYNC RGB, thiết kế dành cho esports professionals.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddKeyboardSpecs(specValues, keyboardSpecs, logitechGProXTKLId, kbType: "Cơ học", kbSwitch: "GX Brown Tactile", layout: "TKL", connection: "Lightspeed 2.4GHz, Bluetooth, USB-C", rgb: true);
        productImages.Add(new ProductImage { ProductId = logitechGProXTKLId, ImageUrl = "https://resource.logitechg.com/w_1000,c_limit,q_auto,f_auto,dpr_auto/d_transparent.gif/content/dam/gaming/en/products/pro-x-tkl/gallery/pro-x-tkl-gallery-1-black.png" });

        // ============= MID-RANGE MECHANICAL KEYBOARDS =============

        // Ducky One 3 TKL
        var duckyOne3TKLId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = duckyOne3TKLId,
            Name = "Ducky One 3 TKL",
            Sku = "KB-DUCKY-ONE-3-TKL",
            CategoryId = keyboardCategory.Id,
            BrandId = duckyBrand.Id,
            Price = 3200000,
            Stock = 30,
            Description = "Bàn phím cơ Ducky One 3 TKL với thiết kế hot-swappable và QUACK Mechanics. Cherry MX switches, PBT double-shot keycaps. RGB LED, 3 mức góc nghiêng, build quality xuất sắc.",
            WarrantyMonth = 12,
            Status = ProductStatus.Available,
        });
        AddKeyboardSpecs(specValues, keyboardSpecs, duckyOne3TKLId, kbType: "Cơ học", kbSwitch: "Cherry MX Brown", layout: "TKL", connection: "USB-C có dây", rgb: true);
        productImages.Add(new ProductImage { ProductId = duckyOne3TKLId, ImageUrl = "https://www.duckychannel.com.tw/upload/2022_04_2808/202204280804231909.png" });

        // Keychron Q1 Pro
        var keychronQ1ProId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = keychronQ1ProId,
            Name = "Keychron Q1 Pro",
            Sku = "KB-KEYCHRON-Q1-PRO",
            CategoryId = keyboardCategory.Id,
            BrandId = keychronBrand.Id,
            Price = 4200000,
            Stock = 25,
            Description = "Bàn phím cơ Keychron Q1 Pro với vỏ nhôm CNC full-metal, gasket mount design. Hỗ trợ Bluetooth 5.1 và USB-C. Layout 75%, hot-swappable, QMK/VIA programmable, south-facing RGB.",
            WarrantyMonth = 12,
            Status = ProductStatus.Available,
        });
        AddKeyboardSpecs(specValues, keyboardSpecs, keychronQ1ProId, kbType: "Cơ học", kbSwitch: "Gateron G Pro Brown", layout: "75%", connection: "Bluetooth 5.1, USB-C", rgb: true);
        productImages.Add(new ProductImage { ProductId = keychronQ1ProId, ImageUrl = "https://www.keychron.com/cdn/shop/files/Keychron-Q1-Pro-QMK-VIA-wireless-custom-mechanical-keyboard-75-layout-full-aluminum-black-frame-for-Mac-Windows-Linux-Gateron-Jupiter-brown-switches.jpg" });

        // Corsair K70 RGB Pro
        var corsairK70RGBProId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = corsairK70RGBProId,
            Name = "Corsair K70 RGB Pro",
            Sku = "KB-CORSAIR-K70-RGB-PRO",
            CategoryId = keyboardCategory.Id,
            BrandId = corsairBrand.Id,
            Price = 3800000,
            Stock = 35,
            Description = "Bàn phím cơ Corsair K70 RGB Pro với Cherry MX switches và 8000Hz polling rate. Frame nhôm aircraft-grade, PBT double-shot keycaps. Per-key RGB, iCUE software, dedicated media controls.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddKeyboardSpecs(specValues, keyboardSpecs, corsairK70RGBProId, kbType: "Cơ học", kbSwitch: "Cherry MX Red", layout: "Full-size", connection: "USB-C có dây", rgb: true);
        productImages.Add(new ProductImage { ProductId = corsairK70RGBProId, ImageUrl = "https://assets.corsair.com/image/upload/c_pad,q_auto,h_1024,w_1024,f_auto/products/Gaming-Keyboards/CH-9109410-NA/Gallery/K70_PRO_01.webp" });

        // SteelSeries Apex Pro TKL
        var steelSeriesApexProTKLId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = steelSeriesApexProTKLId,
            Name = "SteelSeries Apex Pro TKL (2023)",
            Sku = "KB-STEELSERIES-APEX-PRO-TKL-2023",
            CategoryId = keyboardCategory.Id,
            BrandId = steelSeriesBrand.Id,
            Price = 4500000,
            Stock = 20,
            Description = "Bàn phím cơ SteelSeries Apex Pro TKL 2023 với OmniPoint 2.0 adjustable switches. Rapid Trigger, actuation 0.2-3.8mm. Layout TKL, OLED smart display, aircraft-grade aluminum frame.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddKeyboardSpecs(specValues, keyboardSpecs, steelSeriesApexProTKLId, kbType: "Magnetic Hall Effect", kbSwitch: "OmniPoint 2.0", layout: "TKL", connection: "USB-C có dây", rgb: true);
        productImages.Add(new ProductImage { ProductId = steelSeriesApexProTKLId, ImageUrl = "https://media.steelseriescdn.com/thumbs/catalog/items/64856/fd0a854055f845dfb69dd8e70de90b82.png.1920x1080_q100_format-png_optimize-medium.png" });

        // ============= BUDGET MECHANICAL KEYBOARDS =============

        // Akko 3098B Plus
        var akko3098BPlusId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = akko3098BPlusId,
            Name = "Akko 3098B Plus Multi-Mode",
            Sku = "KB-AKKO-3098B-PLUS",
            CategoryId = keyboardCategory.Id,
            BrandId = akkoBrand.Id,
            Price = 1800000,
            Stock = 50,
            Description = "Bàn phím cơ Akko 3098B Plus với kết nối 3 chế độ (Bluetooth 5.0, 2.4GHz, USB-C). Layout 1800 compact, Akko CS switches hot-swappable, PBT double-shot keycaps. Giá tốt cho chất lượng cao.",
            WarrantyMonth = 12,
            Status = ProductStatus.Available,
        });
        AddKeyboardSpecs(specValues, keyboardSpecs, akko3098BPlusId, kbType: "Cơ học", kbSwitch: "Akko CS Silver", layout: "1800 Compact", connection: "Bluetooth 5.0, 2.4GHz, USB-C", rgb: true);
        productImages.Add(new ProductImage { ProductId = akko3098BPlusId, ImageUrl = "https://en.akkogear.com/wp-content/uploads/2022/06/3098B-Plus-Black-Gold-01.jpg" });

        // Glorious GMMK 2
        var gloriousGMMK2Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gloriousGMMK2Id,
            Name = "Glorious GMMK 2 65%",
            Sku = "KB-GLORIOUS-GMMK2-65",
            CategoryId = keyboardCategory.Id,
            BrandId = gloriousBrand.Id,
            Price = 2500000,
            Stock = 40,
            Description = "Bàn phím cơ Glorious GMMK 2 với layout 65% compact. Pre-lubed Glorious Fox switches, hot-swappable, gasket mounted plate. Rotary encoder, south-facing RGB, ABS doubleshot keycaps.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddKeyboardSpecs(specValues, keyboardSpecs, gloriousGMMK2Id, kbType: "Cơ học", kbSwitch: "Glorious Fox Linear", layout: "65%", connection: "USB-C có dây", rgb: true);
        productImages.Add(new ProductImage { ProductId = gloriousGMMK2Id, ImageUrl = "https://cdn.shopify.com/s/files/1/0549/2681/files/GMMK2_65_Black_001_720x.png" });

        // Leopold FC660M
        var leopoldFC660MId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = leopoldFC660MId,
            Name = "Leopold FC660M PD",
            Sku = "KB-LEOPOLD-FC660M-PD",
            CategoryId = keyboardCategory.Id,
            BrandId = leopoldBrand.Id,
            Price = 2800000,
            Stock = 25,
            Description = "Bàn phím cơ Leopold FC660M PD với build quality huyền thoại từ Hàn Quốc. Cherry MX switches, PBT double-shot keycaps. Layout 65% compact, thiết kế minimalist không RGB, sound dampening foam.",
            WarrantyMonth = 12,
            Status = ProductStatus.Available,
        });
        AddKeyboardSpecs(specValues, keyboardSpecs, leopoldFC660MId, kbType: "Cơ học", kbSwitch: "Cherry MX Silent Red", layout: "65%", connection: "USB-C có dây", rgb: false);
        productImages.Add(new ProductImage { ProductId = leopoldFC660MId, ImageUrl = "https://mechanicalkeyboards.com/shop/images/products/large_FC660MPD_BTPD_1.png" });
    }

    private static void AddKeyboardSpecs(List<ProductSpecValue> specValues, List<SpecDefinition> specs, Guid productId, string kbType, string kbSwitch, string layout, string connection, bool rgb)
    {
        var kbTypeSpec = specs.FirstOrDefault(s => s.Code == "kb_type");
        if (kbTypeSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = kbTypeSpec.Id, TextValue = kbType });

        var kbSwitchSpec = specs.FirstOrDefault(s => s.Code == "kb_switch");
        if (kbSwitchSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = kbSwitchSpec.Id, TextValue = kbSwitch });

        var layoutSpec = specs.FirstOrDefault(s => s.Code == "kb_layout");
        if (layoutSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = layoutSpec.Id, TextValue = layout });

        var connectionSpec = specs.FirstOrDefault(s => s.Code == "kb_connection");
        if (connectionSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = connectionSpec.Id, TextValue = connection });

        var rgbSpec = specs.FirstOrDefault(s => s.Code == "kb_rgb");
        if (rgbSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = rgbSpec.Id, BoolValue = rgb });
    }

    private static async Task InitMouseProducts(ApplicationDbContext context, List<ProductSpecValue> specValues, List<ProductImage> productImages)
    {
        var mouseCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Chuột")
            ?? throw new NotFoundException("Không tìm thấy danh mục Chuột");

        var mouseSpecs = await context.SpecDefinitions
            .Where(s => s.CategoryId == mouseCategory.Id)
            .ToListAsync();

        // Load mouse brands
        var logitechBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Logitech")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Logitech");
        var razerBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Razer")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Razer");
        var pulsarBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Pulsar")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Pulsar");
        var finalmouseBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Finalmouse")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Finalmouse");
        var lamzuBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Lamzu")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Lamzu");
        var zowieBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Zowie")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Zowie");
        var steelSeriesBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "SteelSeries")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu SteelSeries");
        var gloriousBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Glorious")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Glorious");
        var endgameGearBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Endgame Gear")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Endgame Gear");
        var vaxeeBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Vaxee")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Vaxee");

        // ============= HIGH-END WIRELESS GAMING MICE =============

        // Logitech G Pro X Superlight 2
        var logitechGProXSuperlight2Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = logitechGProXSuperlight2Id,
            Name = "Logitech G Pro X Superlight 2",
            Sku = "MOUSE-LOGITECH-GPX-SL2",
            CategoryId = mouseCategory.Id,
            BrandId = logitechBrand.Id,
            Price = 3500000,
            Stock = 30,
            Description = "Chuột gaming không dây Logitech G Pro X Superlight 2 với cảm biến HERO 2 32K. Trọng lượng chỉ 60g, kết nối Lightspeed 2.4GHz polling rate 8000Hz. Được pro players tin dùng cho FPS esports.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddMouseSpecs(specValues, mouseSpecs, logitechGProXSuperlight2Id, sensor: "HERO 2 32K", dpi: 32000, connection: "Lightspeed 2.4GHz", weight: 60, buttons: 5);
        productImages.Add(new ProductImage { ProductId = logitechGProXSuperlight2Id, ImageUrl = "https://resource.logitechg.com/w_1000,c_limit,q_auto,f_auto,dpr_auto/d_transparent.gif/content/dam/gaming/en/products/pro-x-superlight-2/gallery/pro-x-superlight-2-gallery-1-black.png" });

        // Razer DeathAdder V3 Pro
        var razerDeathAdderV3ProId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = razerDeathAdderV3ProId,
            Name = "Razer DeathAdder V3 Pro",
            Sku = "MOUSE-RAZER-DAV3-PRO",
            CategoryId = mouseCategory.Id,
            BrandId = razerBrand.Id,
            Price = 3200000,
            Stock = 35,
            Description = "Chuột gaming không dây Razer DeathAdder V3 Pro với thiết kế ergonomic huyền thoại. Cảm biến Focus Pro 30K, trọng lượng 63g, HyperSpeed Wireless. Optical switches Gen-3, pin 90 giờ.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddMouseSpecs(specValues, mouseSpecs, razerDeathAdderV3ProId, sensor: "Focus Pro 30K", dpi: 30000, connection: "HyperSpeed 2.4GHz", weight: 63, buttons: 5);
        productImages.Add(new ProductImage { ProductId = razerDeathAdderV3ProId, ImageUrl = "https://assets2.razerzone.com/images/pnx.assets/c2f6e1c29e2e5b4ec1c1f1f1f1f1f1f1/razer-deathadder-v3-pro-hero-desktop.webp" });

        // Pulsar X2 Wireless
        var pulsarX2WirelessId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = pulsarX2WirelessId,
            Name = "Pulsar X2 Wireless",
            Sku = "MOUSE-PULSAR-X2-WIRELESS",
            CategoryId = mouseCategory.Id,
            BrandId = pulsarBrand.Id,
            Price = 2800000,
            Stock = 40,
            Description = "Chuột gaming không dây Pulsar X2 với thiết kế symmetrical mini. Cảm biến PAW3395, trọng lượng siêu nhẹ 52g, 4K polling rate. Được thiết kế với input từ pro players, encoder Kailh.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddMouseSpecs(specValues, mouseSpecs, pulsarX2WirelessId, sensor: "PixArt PAW3395", dpi: 26000, connection: "2.4GHz Wireless", weight: 52, buttons: 5);
        productImages.Add(new ProductImage { ProductId = pulsarX2WirelessId, ImageUrl = "https://www.pulsar.gg/cdn/shop/files/X2_Black_Top.png" });

        // Finalmouse UltralightX
        var finalmouseUltralightXId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = finalmouseUltralightXId,
            Name = "Finalmouse UltralightX",
            Sku = "MOUSE-FINALMOUSE-ULX",
            CategoryId = mouseCategory.Id,
            BrandId = finalmouseBrand.Id,
            Price = 5500000,
            Stock = 15,
            Description = "Chuột gaming không dây Finalmouse UltralightX với trọng lượng cực nhẹ chỉ 29g. Cảm biến Finalsensor, thiết kế magnesium alloy. 8000Hz polling rate, limited edition cho enthusiasts.",
            WarrantyMonth = 12,
            Status = ProductStatus.Available,
        });
        AddMouseSpecs(specValues, mouseSpecs, finalmouseUltralightXId, sensor: "Finalsensor", dpi: 32000, connection: "2.4GHz Wireless", weight: 29, buttons: 5);
        productImages.Add(new ProductImage { ProductId = finalmouseUltralightXId, ImageUrl = "https://finalmouse.com/images/ultralightx.png" });

        // Lamzu Atlantis Mini Pro
        var lamzuAtlantisMiniProId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = lamzuAtlantisMiniProId,
            Name = "Lamzu Atlantis Mini Pro",
            Sku = "MOUSE-LAMZU-ATLANTIS-MINI-PRO",
            CategoryId = mouseCategory.Id,
            BrandId = lamzuBrand.Id,
            Price = 2500000,
            Stock = 45,
            Description = "Chuột gaming không dây Lamzu Atlantis Mini Pro với thiết kế nhỏ gọn cho cầm fingertip/claw. Cảm biến PAW3395, trọng lượng 49g, 4K polling rate. Build quality cao với giá hợp lý.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddMouseSpecs(specValues, mouseSpecs, lamzuAtlantisMiniProId, sensor: "PixArt PAW3395", dpi: 26000, connection: "2.4GHz Wireless", weight: 49, buttons: 6);
        productImages.Add(new ProductImage { ProductId = lamzuAtlantisMiniProId, ImageUrl = "https://lamzu.com/cdn/shop/files/AtlantisMiniPro_Black_1.png" });

        // ============= ESPORTS WIRELESS MICE =============

        // Zowie EC2-CW
        var zowieEC2CWId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = zowieEC2CWId,
            Name = "Zowie EC2-CW",
            Sku = "MOUSE-ZOWIE-EC2-CW",
            CategoryId = mouseCategory.Id,
            BrandId = zowieBrand.Id,
            Price = 2800000,
            Stock = 35,
            Description = "Chuột gaming không dây Zowie EC2-CW với thiết kế ergonomic được CS pro players yêu thích. Cảm biến 3370, trọng lượng 77g, 24-step scroll wheel. Không RGB, tập trung vào performance.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddMouseSpecs(specValues, mouseSpecs, zowieEC2CWId, sensor: "PixArt 3370", dpi: 3200, connection: "2.4GHz Wireless", weight: 77, buttons: 5);
        productImages.Add(new ProductImage { ProductId = zowieEC2CWId, ImageUrl = "https://zowie.benq.com/content/dam/game/en/product/mouse/ec2-cw/gallery/ec2-cw-top.png" });

        // SteelSeries Prime Wireless
        var steelSeriesPrimeWirelessId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = steelSeriesPrimeWirelessId,
            Name = "SteelSeries Prime Wireless",
            Sku = "MOUSE-STEELSERIES-PRIME-WIRELESS",
            CategoryId = mouseCategory.Id,
            BrandId = steelSeriesBrand.Id,
            Price = 2200000,
            Stock = 40,
            Description = "Chuột gaming không dây SteelSeries Prime Wireless với thiết kế cùng pro players. Cảm biến TrueMove Air, Prestige OM switches 100M clicks, trọng lượng 80g. Quantum 2.0 wireless.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddMouseSpecs(specValues, mouseSpecs, steelSeriesPrimeWirelessId, sensor: "TrueMove Air", dpi: 18000, connection: "Quantum 2.0 Wireless", weight: 80, buttons: 6);
        productImages.Add(new ProductImage { ProductId = steelSeriesPrimeWirelessId, ImageUrl = "https://media.steelseriescdn.com/thumbs/catalog/items/62593/b0c9d8b0dbae4c9e85c5c7a6a0b0b0b0.png.1920x1080_q100_format-png_optimize-medium.png" });

        // ============= MID-RANGE GAMING MICE =============

        // Glorious Model O 2 Wireless
        var gloriousModelO2WirelessId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = gloriousModelO2WirelessId,
            Name = "Glorious Model O 2 Wireless",
            Sku = "MOUSE-GLORIOUS-MODEL-O2-WIRELESS",
            CategoryId = mouseCategory.Id,
            BrandId = gloriousBrand.Id,
            Price = 2000000,
            Stock = 50,
            Description = "Chuột gaming không dây Glorious Model O 2 với thiết kế ambidextrous nhẹ. Cảm biến BAMF 2.0 26K, trọng lượng 68g, 4K polling rate. Glorious switches, RGB underglow, giá cạnh tranh.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddMouseSpecs(specValues, mouseSpecs, gloriousModelO2WirelessId, sensor: "BAMF 2.0 26K", dpi: 26000, connection: "2.4GHz Wireless, Bluetooth", weight: 68, buttons: 6);
        productImages.Add(new ProductImage { ProductId = gloriousModelO2WirelessId, ImageUrl = "https://cdn.shopify.com/s/files/1/0549/2681/files/Model-O-2-Wireless-Black-Top_720x.png" });

        // Endgame Gear OP1we
        var endgameGearOP1weId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = endgameGearOP1weId,
            Name = "Endgame Gear OP1we",
            Sku = "MOUSE-ENDGAMEGEAR-OP1WE",
            CategoryId = mouseCategory.Id,
            BrandId = endgameGearBrand.Id,
            Price = 2300000,
            Stock = 35,
            Description = "Chuột gaming không dây Endgame Gear OP1we với thiết kế egg-shape compact. Cảm biến PAW3395, trọng lượng 59g, Kailh 8.0 switches. 4K polling rate, build quality Đức.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddMouseSpecs(specValues, mouseSpecs, endgameGearOP1weId, sensor: "PixArt PAW3395", dpi: 26000, connection: "2.4GHz Wireless", weight: 59, buttons: 5);
        productImages.Add(new ProductImage { ProductId = endgameGearOP1weId, ImageUrl = "https://www.endgamegear.com/media/image/product/3264/lg/endgame-gear-op1we-wireless-gaming-mouse-black.png" });

        // Vaxee XE Wireless
        var vaxeeXEWirelessId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = vaxeeXEWirelessId,
            Name = "Vaxee XE Wireless",
            Sku = "MOUSE-VAXEE-XE-WIRELESS",
            CategoryId = mouseCategory.Id,
            BrandId = vaxeeBrand.Id,
            Price = 2600000,
            Stock = 30,
            Description = "Chuột gaming không dây Vaxee XE với thiết kế ergonomic medium size. Cảm biến PAW3395, trọng lượng 70g, Huano blue shell switches. Thiết kế từ Đài Loan với input từ pro players.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddMouseSpecs(specValues, mouseSpecs, vaxeeXEWirelessId, sensor: "PixArt PAW3395", dpi: 26000, connection: "2.4GHz Wireless", weight: 70, buttons: 5);
        productImages.Add(new ProductImage { ProductId = vaxeeXEWirelessId, ImageUrl = "https://vaxee.co/upload/images/202310/XE_Wireless_Black_top.png" });
    }

    private static void AddMouseSpecs(List<ProductSpecValue> specValues, List<SpecDefinition> specs, Guid productId, string sensor, int dpi, string connection, int weight, int buttons)
    {
        var sensorSpec = specs.FirstOrDefault(s => s.Code == "mouse_sensor");
        if (sensorSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = sensorSpec.Id, TextValue = sensor });

        var dpiSpec = specs.FirstOrDefault(s => s.Code == "mouse_dpi");
        if (dpiSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = dpiSpec.Id, NumberValue = dpi });

        var connectionSpec = specs.FirstOrDefault(s => s.Code == "mouse_connection");
        if (connectionSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = connectionSpec.Id, TextValue = connection });

        var weightSpec = specs.FirstOrDefault(s => s.Code == "mouse_weight");
        if (weightSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = weightSpec.Id, NumberValue = weight });

        var buttonsSpec = specs.FirstOrDefault(s => s.Code == "mouse_buttons");
        if (buttonsSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = buttonsSpec.Id, NumberValue = buttons });
    }

    private static async Task InitHeadsetProducts(ApplicationDbContext context, List<ProductSpecValue> specValues, List<ProductImage> productImages)
    {
        var headsetCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Tai nghe")
            ?? throw new NotFoundException("Không tìm thấy danh mục Tai nghe");

        var headsetSpecs = await context.SpecDefinitions
            .Where(s => s.CategoryId == headsetCategory.Id)
            .ToListAsync();

        // Load headset brands
        var logitechBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Logitech")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Logitech");
        var razerBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Razer")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Razer");
        var steelSeriesBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "SteelSeries")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu SteelSeries");
        var hyperXBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "HyperX")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu HyperX");
        var corsairBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Corsair")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Corsair");
        var sonyBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Sony")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Sony");
        var sennheiserBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Sennheiser")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Sennheiser");
        var beyerdynamicBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Beyerdynamic")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Beyerdynamic");
        var audioTechnicaBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Audio-Technica")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Audio-Technica");
        var boseBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Bose")
            ?? throw new NotFoundException("Không tìm thấy thương hiệu Bose");

        // ============= HIGH-END GAMING HEADSETS =============

        // SteelSeries Arctis Nova Pro Wireless
        var steelSeriesArctisNovaProId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = steelSeriesArctisNovaProId,
            Name = "SteelSeries Arctis Nova Pro Wireless",
            Sku = "HS-STEELSERIES-ARCTIS-NOVA-PRO-W",
            CategoryId = headsetCategory.Id,
            BrandId = steelSeriesBrand.Id,
            Price = 8500000,
            Stock = 15,
            Description = "Tai nghe gaming không dây SteelSeries Arctis Nova Pro với Active Noise Cancellation. Driver Neodymium 40mm cao cấp, hệ thống pin kép hot-swap. Multi-System Connect, DAC tích hợp, micro ClearCast Gen 2 retractable.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddHeadsetSpecs(specValues, headsetSpecs, steelSeriesArctisNovaProId, hsType: "Over-ear", driver: 40, connection: "USB 2.4GHz, Bluetooth", microphone: true, noiseCancelling: true);
        productImages.Add(new ProductImage { ProductId = steelSeriesArctisNovaProId, ImageUrl = "https://media.steelseriescdn.com/thumbs/catalog/items/61520/b3b3b3b3b3b3b3b3.png.1920x1080_q100_format-png_optimize-medium.png" });

        // Logitech G Pro X 2 Lightspeed
        var logitechGProX2Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = logitechGProX2Id,
            Name = "Logitech G Pro X 2 Lightspeed",
            Sku = "HS-LOGITECH-G-PRO-X2",
            CategoryId = headsetCategory.Id,
            BrandId = logitechBrand.Id,
            Price = 5500000,
            Stock = 20,
            Description = "Tai nghe gaming không dây Logitech G Pro X 2 Lightspeed với driver Graphene 50mm. Kết nối 3 chế độ Lightspeed, Bluetooth, USB-C. Micro detachable với Blue VO!CE technology, DTS Headphone:X 2.0.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddHeadsetSpecs(specValues, headsetSpecs, logitechGProX2Id, hsType: "Over-ear", driver: 50, connection: "Lightspeed 2.4GHz, Bluetooth, USB-C", microphone: true, noiseCancelling: false);
        productImages.Add(new ProductImage { ProductId = logitechGProX2Id, ImageUrl = "https://resource.logitechg.com/w_1000,c_limit,q_auto,f_auto,dpr_auto/d_transparent.gif/content/dam/gaming/en/products/pro-x-2-lightspeed/gallery/pro-x-2-lightspeed-gallery-1-black.png" });

        // Razer BlackShark V2 Pro (2023)
        var razerBlackSharkV2ProId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = razerBlackSharkV2ProId,
            Name = "Razer BlackShark V2 Pro (2023)",
            Sku = "HS-RAZER-BLACKSHARK-V2-PRO-2023",
            CategoryId = headsetCategory.Id,
            BrandId = razerBrand.Id,
            Price = 4500000,
            Stock = 25,
            Description = "Tai nghe gaming không dây Razer BlackShark V2 Pro 2023 với TriForce Titanium 50mm drivers. HyperSpeed Wireless, micro detachable HyperClear Super Wideband. THX Spatial Audio, trọng lượng nhẹ 320g.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddHeadsetSpecs(specValues, headsetSpecs, razerBlackSharkV2ProId, hsType: "Over-ear", driver: 50, connection: "HyperSpeed 2.4GHz, Bluetooth", microphone: true, noiseCancelling: false);
        productImages.Add(new ProductImage { ProductId = razerBlackSharkV2ProId, ImageUrl = "https://assets2.razerzone.com/images/pnx.assets/d3d3d3d3d3d3d3d3/razer-blackshark-v2-pro-2023-hero.webp" });

        // ============= MID-RANGE GAMING HEADSETS =============

        // HyperX Cloud III Wireless
        var hyperXCloud3WirelessId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = hyperXCloud3WirelessId,
            Name = "HyperX Cloud III Wireless",
            Sku = "HS-HYPERX-CLOUD-3-WIRELESS",
            CategoryId = headsetCategory.Id,
            BrandId = hyperXBrand.Id,
            Price = 3500000,
            Stock = 35,
            Description = "Tai nghe gaming không dây HyperX Cloud III với driver 53mm Angled. DTS Headphone:X Spatial Audio, micro detachable với noise-cancelling. Pin 120 giờ, build quality bền bỉ, đệm tai memory foam.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddHeadsetSpecs(specValues, headsetSpecs, hyperXCloud3WirelessId, hsType: "Over-ear", driver: 53, connection: "USB 2.4GHz", microphone: true, noiseCancelling: false);
        productImages.Add(new ProductImage { ProductId = hyperXCloud3WirelessId, ImageUrl = "https://hyperx.com/cdn/shop/files/hyperx_cloud_iii_wireless_black_1_main.jpg" });

        // Corsair Virtuoso RGB Wireless XT
        var corsairVirtuosoXTId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = corsairVirtuosoXTId,
            Name = "Corsair Virtuoso RGB Wireless XT",
            Sku = "HS-CORSAIR-VIRTUOSO-XT",
            CategoryId = headsetCategory.Id,
            BrandId = corsairBrand.Id,
            Price = 4200000,
            Stock = 20,
            Description = "Tai nghe gaming không dây Corsair Virtuoso RGB Wireless XT với driver 50mm Neodymium. Kết nối Slipstream, Bluetooth 5.0, 3.5mm. Vỏ nhôm cao cấp, micro broadcast-grade detachable, Dolby Atmos.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddHeadsetSpecs(specValues, headsetSpecs, corsairVirtuosoXTId, hsType: "Over-ear", driver: 50, connection: "Slipstream 2.4GHz, Bluetooth 5.0, 3.5mm", microphone: true, noiseCancelling: false);
        productImages.Add(new ProductImage { ProductId = corsairVirtuosoXTId, ImageUrl = "https://assets.corsair.com/image/upload/c_pad,q_auto,h_1024,w_1024,f_auto/products/Gaming-Headsets/CA-9011188-NA/Gallery/VIRTUOSO_XT_01.webp" });

        // ============= AUDIOPHILE / HI-FI HEADSETS =============

        // Sony WH-1000XM5
        var sonyWH1000XM5Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = sonyWH1000XM5Id,
            Name = "Sony WH-1000XM5",
            Sku = "HS-SONY-WH1000XM5",
            CategoryId = headsetCategory.Id,
            BrandId = sonyBrand.Id,
            Price = 7500000,
            Stock = 25,
            Description = "Tai nghe không dây Sony WH-1000XM5 với chống ồn chủ động hàng đầu thế giới. Driver 30mm tích hợp, 2 chip xử lý V1 + QN1. LDAC Hi-Res Audio, pin 30 giờ, Speak-to-Chat, Multipoint connection.",
            WarrantyMonth = 12,
            Status = ProductStatus.Available,
        });
        AddHeadsetSpecs(specValues, headsetSpecs, sonyWH1000XM5Id, hsType: "Over-ear", driver: 30, connection: "Bluetooth 5.2, 3.5mm", microphone: true, noiseCancelling: true);
        productImages.Add(new ProductImage { ProductId = sonyWH1000XM5Id, ImageUrl = "https://store.sony.com.vn/cdn/shop/files/WH-1000XM5_B_Main.png" });

        // Sennheiser HD 660S2
        var sennheiserHD660S2Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = sennheiserHD660S2Id,
            Name = "Sennheiser HD 660S2",
            Sku = "HS-SENNHEISER-HD660S2",
            CategoryId = headsetCategory.Id,
            BrandId = sennheiserBrand.Id,
            Price = 12000000,
            Stock = 10,
            Description = "Tai nghe audiophile Sennheiser HD 660S2 open-back với driver 42mm cải tiến. Trở kháng 300 Ohm, phù hợp với DAC/AMP. Âm thanh chi tiết, soundstage rộng, đệm tai velour thoáng khí. Made in Germany.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddHeadsetSpecs(specValues, headsetSpecs, sennheiserHD660S2Id, hsType: "Over-ear", driver: 42, connection: "6.35mm, 4.4mm Balanced", microphone: false, noiseCancelling: false);
        productImages.Add(new ProductImage { ProductId = sennheiserHD660S2Id, ImageUrl = "https://assets.sennheiser.com/img/p/q/500x500/HD-660S2_Main_01.png" });

        // Beyerdynamic DT 900 PRO X
        var beyerdynamicDT900ProXId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = beyerdynamicDT900ProXId,
            Name = "Beyerdynamic DT 900 PRO X",
            Sku = "HS-BEYERDYNAMIC-DT900-PRO-X",
            CategoryId = headsetCategory.Id,
            BrandId = beyerdynamicBrand.Id,
            Price = 7000000,
            Stock = 15,
            Description = "Tai nghe studio Beyerdynamic DT 900 PRO X open-back với driver STELLAR.45 45mm. Trở kháng 48 Ohm, dễ drive từ mọi nguồn. Âm thanh chi tiết, trung thực. Đệm tai velour, dây cáp tháo rời. Made in Germany.",
            WarrantyMonth = 24,
            Status = ProductStatus.Available,
        });
        AddHeadsetSpecs(specValues, headsetSpecs, beyerdynamicDT900ProXId, hsType: "Over-ear", driver: 45, connection: "3.5mm (Mini-XLR detachable)", microphone: false, noiseCancelling: false);
        productImages.Add(new ProductImage { ProductId = beyerdynamicDT900ProXId, ImageUrl = "https://europe.beyerdynamic.com/media/catalog/product/d/t/dt-900-pro-x_front.jpg" });

        // Audio-Technica ATH-M50xBT2
        var audioTechnicaM50xBT2Id = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = audioTechnicaM50xBT2Id,
            Name = "Audio-Technica ATH-M50xBT2",
            Sku = "HS-AUDIO-TECHNICA-ATH-M50XBT2",
            CategoryId = headsetCategory.Id,
            BrandId = audioTechnicaBrand.Id,
            Price = 5500000,
            Stock = 20,
            Description = "Tai nghe không dây Audio-Technica ATH-M50xBT2 phiên bản Bluetooth của huyền thoại M50x. Driver 45mm, codec LDAC/AAC/SBC. Pin 50 giờ, có micro tích hợp, thiết kế gập gọn. Âm thanh studio chất lượng.",
            WarrantyMonth = 12,
            Status = ProductStatus.Available,
        });
        AddHeadsetSpecs(specValues, headsetSpecs, audioTechnicaM50xBT2Id, hsType: "Over-ear", driver: 45, connection: "Bluetooth 5.0, 3.5mm", microphone: true, noiseCancelling: false);
        productImages.Add(new ProductImage { ProductId = audioTechnicaM50xBT2Id, ImageUrl = "https://www.audio-technica.com/en-us/media/catalog/product/a/t/ath-m50xbt2_01.png" });

        // Bose QuietComfort Ultra Headphones
        var boseQCUltraId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = boseQCUltraId,
            Name = "Bose QuietComfort Ultra Headphones",
            Sku = "HS-BOSE-QC-ULTRA",
            CategoryId = headsetCategory.Id,
            BrandId = boseBrand.Id,
            Price = 9000000,
            Stock = 15,
            Description = "Tai nghe không dây Bose QuietComfort Ultra với chống ồn chủ động đỉnh cao. Immersive Audio với Snapdragon Sound, CustomTune EQ tự động. Pin 24 giờ, đệm tai protein leather êm ái, Multipoint connection.",
            WarrantyMonth = 12,
            Status = ProductStatus.Available,
        });
        AddHeadsetSpecs(specValues, headsetSpecs, boseQCUltraId, hsType: "Over-ear", driver: 35, connection: "Bluetooth 5.3, 3.5mm", microphone: true, noiseCancelling: true);
        productImages.Add(new ProductImage { ProductId = boseQCUltraId, ImageUrl = "https://assets.bose.com/content/dam/cloudassets/Bose_DAM/Web/consumer_electronics/global/products/headphones/qc-ultra-headphones/product_silo_images/QCUltra_Black_Hero.png" });
    }

    private static void AddHeadsetSpecs(List<ProductSpecValue> specValues, List<SpecDefinition> specs, Guid productId, string hsType, int driver, string connection, bool microphone, bool noiseCancelling)
    {
        var hsTypeSpec = specs.FirstOrDefault(s => s.Code == "hs_type");
        if (hsTypeSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = hsTypeSpec.Id, TextValue = hsType });

        var driverSpec = specs.FirstOrDefault(s => s.Code == "hs_driver");
        if (driverSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = driverSpec.Id, NumberValue = driver });

        var connectionSpec = specs.FirstOrDefault(s => s.Code == "hs_connection");
        if (connectionSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = connectionSpec.Id, TextValue = connection });

        var microphoneSpec = specs.FirstOrDefault(s => s.Code == "hs_microphone");
        if (microphoneSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = microphoneSpec.Id, BoolValue = microphone });

        var noiseCancellingSpec = specs.FirstOrDefault(s => s.Code == "hs_noise_cancelling");
        if (noiseCancellingSpec != null) specValues.Add(new ProductSpecValue { ProductId = productId, SpecDefinitionId = noiseCancellingSpec.Id, BoolValue = noiseCancelling });
    }
}
