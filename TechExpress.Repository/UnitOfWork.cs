using Microsoft.EntityFrameworkCore.Storage;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Repositories;

namespace TechExpress.Repository
{
    public class UnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UserRepository UserRepository { get; }
        public CategoryRepository CategoryRepository { get; }
        public ProductRepository ProductRepository { get; }
        public SpecDefinitionRepository SpecDefinitionRepository { get; }
        public ProductImageRepository ProductImageRepository { get; }
        public ProductSpecValueRepository ProductSpecValueRepository { get; }
        public BrandRepository BrandRepository { get; }
        public CartRepository CartRepository { get; }
        public CartItemRepository CartItemRepository { get; }
        public ComputerComponentRepository ComputerComponentRepository { get; }

        public OrderRepository OrderRepository { get; }
        public PaymentRepository PaymentRepository { get; }
        public InstallmentRepository InstallmentRepository { get; }


        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            ProductRepository = new ProductRepository(context);
            UserRepository = new UserRepository(context);
            CategoryRepository = new CategoryRepository(context);
            SpecDefinitionRepository = new SpecDefinitionRepository(context);
            ProductImageRepository = new ProductImageRepository(context);
            ProductSpecValueRepository = new ProductSpecValueRepository(context);
            BrandRepository = new BrandRepository(context);
            CartRepository = new CartRepository(context);
            CartItemRepository = new CartItemRepository(context);
            ComputerComponentRepository = new ComputerComponentRepository(context);

            OrderRepository = new OrderRepository(context);
            PaymentRepository = new PaymentRepository(context);
            InstallmentRepository = new InstallmentRepository(context);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public IExecutionStrategy CreateExecutionStrategy()
        {
            return _context.Database.CreateExecutionStrategy();
        }
    }
}
