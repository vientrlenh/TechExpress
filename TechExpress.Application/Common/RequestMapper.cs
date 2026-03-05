using TechExpress.Application.Dtos.Requests;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Service.Commands;

namespace TechExpress.Application.Common
{
    public class RequestMapper
    {

        public static List<CreateProductSpecValueCommand> MapToCreateProductSpecValueCommandsFromRequests(List<CreateProductSpecValueRequest> requests)
        {
            List<CreateProductSpecValueCommand> commands = [];
            HashSet<Guid> specIds = [];
            foreach (var request in requests)
            {
                if (specIds.Contains(request.SpecDefinitionId))
                {
                    throw new BadRequestException($"Thông số trùng lặp khi gửi yêu cầu {request.SpecDefinitionId}");
                }
                var command = new CreateProductSpecValueCommand 
                { 
                    SpecDefinitionId = request.SpecDefinitionId,
                    Value = request.Value.Trim(),
                };
                commands.Add(command);
                specIds.Add(request.SpecDefinitionId);
            }
            return commands;
        }

        public static List<AddComputerComponentCommand> MapToAddComputerComponentCommandListFromRequest(List<ProductPCComponentRequest> requests)
        {
            List<AddComputerComponentCommand> commands = [];
            HashSet<Guid> componentIds = [];
            foreach (var request in requests)
            {
                if (componentIds.Contains(request.ComponentProductId))
                {
                    throw new BadRequestException($"Sản phẩm trùng lặp khi gửi yêu cầu {request.ComponentProductId}");
                }
                commands.Add(new AddComputerComponentCommand
                {
                    ComponentId = request.ComponentProductId,
                    Quantity = request.Quantity
                });
                componentIds.Add(request.ComponentProductId);
            }
            return commands;
        }

        public static List<CreatePromotionFreeProductCommand> MapToCreatePromotionFreeProductCommandListFromRequest(List<CreatePromotionFreeProductRequest> requests)
        {
            List<CreatePromotionFreeProductCommand> commands = [];
            HashSet<Guid> productIds = [];
            foreach (var request in requests)
            {
                if (productIds.Contains(request.ProductId))
                {
                    throw new BadRequestException($"Sản phẩm quà tặng trùng lặp khi gửi yêu cầu {request.ProductId}");
                }
                commands.Add(new CreatePromotionFreeProductCommand
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                });
                productIds.Add(request.ProductId);
            }
            return commands;
        }

        public static List<CreatePromotionRequiredProductCommand> MapToCreatePromotionRequiredProductCommandListFromRequest(List<CreatePromotionRequiredProductRequest> requests)
        {
            List<CreatePromotionRequiredProductCommand> commands = [];
            HashSet<Guid> productIds = [];
            foreach (var request in requests)
            {
                if (productIds.Contains(request.ProductId))
                {
                    throw new BadRequestException($"Sản phẩm cần cho việc áp dụng khuyến mãi trùng lặp: {request.ProductId}");
                }
                commands.Add(new CreatePromotionRequiredProductCommand
                {
                    ProductId = request.ProductId,
                    MinQuantity = request.MinQuantity,
                    MaxQuantity = request.MaxQuantity,
                });
                productIds.Add(request.ProductId);
            }
            return commands;
        }
    }
}
