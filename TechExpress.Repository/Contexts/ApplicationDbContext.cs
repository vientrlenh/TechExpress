using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opt) : base(opt) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // db-schema for User model
            modelBuilder.Entity<User>(user =>
            {
                user.Property(u => u.Id)
                    .HasColumnName("id");

                user.Property(u => u.Email)
                    .HasColumnName("email")
                    .HasMaxLength(256)
                    .IsRequired();

                user.Property(u => u.PasswordHash)
                    .HasColumnName("password_hash")
                    .HasMaxLength(256)
                    .IsRequired();

                user.Property(u => u.Role)
                    .HasColumnName("role")
                    .HasConversion<string>()
                    .IsRequired();

                user.Property(u => u.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(256);

                user.Property(u => u.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(256);

                user.Property(u => u.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(20);

                user.Property(u => u.Gender)
                    .HasColumnName("gender")
                    .HasConversion<string>();

                user.Property(u => u.Address)
                    .HasColumnName("address")
                    .HasMaxLength(256);

                user.Property(u => u.Ward)
                    .HasColumnName("ward")
                    .HasMaxLength(100);

                user.Property(u => u.Province)
                    .HasColumnName("province")
                    .HasMaxLength(100);

                user.Property(u => u.PostalCode)
                    .HasColumnName("postal_code")
                    .HasMaxLength(20);

                user.Property(u => u.AvatarImage)
                    .HasColumnName("avatar_image");

                user.Property(u => u.Status)
                    .HasColumnName("status")
                    .HasConversion<string>();

                user.Property(u => u.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                user.Property(u => u.Identity)
                    .HasColumnName("identity")
                    .HasMaxLength(20);

                user.Property(u => u.Salary)
                    .HasColumnName("salary")
                    .HasPrecision(18, 2);

                user.HasKey(u => u.Id);

                user.HasIndex(u => u.Email)
                    .HasDatabaseName("idx_user_email")
                    .IsUnique();

                user.HasIndex(u => u.Phone)
                    .HasDatabaseName("idx_user_phone")
                    .IsUnique()
                    .HasFilter("[phone] IS NOT NULL");

                user.HasIndex(u => u.Identity)
                    .HasDatabaseName("idx_user_identity")
                    .IsUnique()
                    .HasFilter("[identity] IS NOT NULL");
            });
            

            // db-schema for Category model
            modelBuilder.Entity<Category>(ct =>
            {
                ct.Property(c => c.Id)
                    .HasColumnName("id");

                ct.Property(c => c.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsRequired();
                
                ct.Property(c => c.ParentCategoryId)
                    .HasColumnName("parent_category_id");

                ct.Property(c => c.Description)
                    .HasColumnName("description")
                    .HasMaxLength(4096)
                    .IsRequired();

                ct.Property(c => c.ImageUrl)
                    .HasColumnName("image_url")
                    .HasMaxLength(2048);

                ct.Property(c => c.IsDeleted)
                    .HasColumnName("is_deleted")
                    .HasDefaultValue(false)
                    .IsRequired();

                ct.Property(c => c.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                ct.Property(c => c.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                ct.HasKey(c => c.Id);

                ct.HasIndex(c => c.Name)
                    .HasDatabaseName("idx_category_name")
                    .IsUnique();

                ct.HasOne(c => c.ParentCategory)
                    .WithMany()
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for SpecDefinition model
            modelBuilder.Entity<SpecDefinition>(sd =>
            {
                sd.Property(s => s.Id)
                    .HasColumnName("id");

                sd.Property(s => s.Name)
                    .HasColumnName("name")
                    .HasMaxLength(256)
                    .IsRequired();

                sd.Property(s => s.Code)
                    .HasColumnName("code")
                    .HasMaxLength(100)
                    .IsRequired();

                sd.Property(s => s.CategoryId)
                    .HasColumnName("category_id")
                    .IsRequired();

                sd.Property(s => s.Unit)
                    .HasColumnName("unit")
                    .HasMaxLength(20)
                    .IsRequired();

                sd.Property(s => s.AcceptValueType)
                    .HasColumnName("accept_value_type")
                    .HasConversion<string>()
                    .IsRequired();

                sd.Property(s => s.Description)
                    .HasColumnName("description")
                    .HasMaxLength(4096)
                    .IsRequired();

                sd.Property(s => s.IsDeleted)
                    .HasColumnName("is_deleted")
                    .HasDefaultValue(false)
                    .IsRequired();

                sd.Property(s => s.IsRequired)
                    .HasColumnName("is_required")
                    .HasDefaultValue(true)
                    .IsRequired();

                sd.Property(s => s.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                sd.Property(s => s.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                sd.HasKey(s => s.Id);

                sd.HasIndex(s => new { s.Id, s.Name })
                    .HasDatabaseName("idx_spec_name")
                    .IsUnique();

                sd.HasIndex(s => s.Code)
                    .HasDatabaseName("idx_spec_code")
                    .IsUnique();
                
                sd.HasIndex(s => s.CategoryId)
                    .HasDatabaseName("idx_spec_category");

                sd.HasOne(s => s.Category)
                    .WithMany()
                    .HasForeignKey(s => s.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);
            });



            // db-schema for Brand model
            modelBuilder.Entity<Brand>(br =>
            {
                br.Property(b => b.Id)
                    .HasColumnName("id");

                br.Property(b => b.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsRequired();
                
                br.Property(b => b.ImageUrl)
                    .HasColumnName("image_url")
                    .HasMaxLength(2048);

                br.Property(b => b.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                br.Property(b => b.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                br.HasKey(b => b.Id);

                br.HasIndex(b => b.Name)
                    .HasDatabaseName("idx_brand_name")
                    .IsUnique();
            });



            // db-schema for BrandCategory model
            modelBuilder.Entity<BrandCategory>(bc =>
            {
                bc.Property(b => b.Id)
                    .HasColumnName("id");

                bc.Property(b => b.CategoryId)
                    .HasColumnName("category_id")
                    .IsRequired();

                bc.Property(b => b.BrandId)
                    .HasColumnName("brand_id")
                    .IsRequired();

                bc.Property(b => b.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                bc.HasKey(b => b.Id);

                bc.HasIndex(b => new { b.CategoryId, b.BrandId })
                    .HasDatabaseName("idx_category_brand")
                    .IsUnique();
                
                bc.HasOne(b => b.Category)
                    .WithMany()
                    .HasForeignKey(b => b.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);

                bc.HasOne(b => b.Brand)
                    .WithMany()
                    .HasForeignKey(b => b.BrandId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for Product model
            modelBuilder.Entity<Product>(pd =>
            {
                pd.Property(p => p.Id)
                    .HasColumnName("id");

                pd.Property(p => p.Name)
                    .HasColumnName("name")
                    .HasMaxLength(256)
                    .IsRequired();

                pd.Property(p => p.Sku)
                    .HasColumnName("sku")
                    .HasMaxLength(256)
                    .IsRequired();

                pd.Property(p => p.CategoryId)
                    .HasColumnName("category_id")
                    .IsRequired();

                
                pd.Property(p => p.BrandId)
                    .HasColumnName("brand_id");

                pd.Property(p => p.Price)
                    .HasColumnName("price")
                    .HasPrecision(18, 2)
                    .IsRequired();

                pd.Property(p => p.Stock)
                    .HasColumnName("stock")
                    .IsRequired();

                pd.Property(p => p.Description)
                    .HasColumnName("description")
                    .HasMaxLength(4096)
                    .IsRequired();

                pd.Property(p => p.WarrantyMonth)
                    .HasColumnName("warranty_month")
                    .IsRequired();
 
                pd.Property(p => p.Status)
                    .HasColumnName("status")
                    .HasConversion<string>()
                    .IsRequired();

                pd.Property(p => p.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();
                    
                pd.Property(p => p.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                pd.HasKey(p => p.Id);

                pd.HasIndex(p => p.Name)
                    .HasDatabaseName("idx_product_name")
                    .IsUnique();

                pd.HasIndex(p => p.Sku)
                    .HasDatabaseName("idx_product_sku")
                    .IsUnique();

                pd.HasIndex(p => p.CategoryId)
                    .HasDatabaseName("idx_product_category");

                pd.HasIndex(p => p.BrandId)
                    .HasDatabaseName("idx_product_brand");

                pd.HasOne(p => p.Category)
                    .WithMany()
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);

                pd.HasOne(p => p.Brand)
                    .WithMany()
                    .HasForeignKey(p => p.BrandId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for ProductSpecValue model
            modelBuilder.Entity<ProductSpecValue>(psv =>
            {
                psv.Property(p => p.Id)
                    .HasColumnName("id");

                psv.Property(p => p.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                psv.Property(p => p.SpecDefinitionId)
                    .HasColumnName("spec_definition_id")
                    .IsRequired();

                psv.Property(p => p.TextValue)
                    .HasColumnName("text_value")
                    .HasMaxLength(512);

                psv.Property(p => p.NumberValue)
                    .HasColumnName("number_value");

                psv.Property(p => p.DecimalValue)
                    .HasColumnName("decimal_value")
                    .HasPrecision(18, 2);

                psv.Property(p => p.BoolValue)
                    .HasColumnName("bool_value");
                
                psv.Property(p => p.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                psv.Property(p => p.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                psv.HasKey(p => p.Id);

                psv.HasIndex(p => p.ProductId)
                    .HasDatabaseName("idx_spec_value_product");

                psv.HasIndex(p => p.SpecDefinitionId)
                    .HasDatabaseName("idx_spec_value_definition");

                psv.HasIndex(p => new { p.ProductId, p.SpecDefinitionId })
                    .HasDatabaseName("idx_spec_value_product_definition");
                
                psv.HasOne(p => p.Product)
                    .WithMany(p => p.SpecValues)
                    .HasForeignKey(p => p.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                psv.HasOne(p => p.SpecDefinition)
                    .WithMany()
                    .HasForeignKey(p => p.SpecDefinitionId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for ProductImage model
            modelBuilder.Entity<ProductImage>(pi =>
            {
                pi.Property(p => p.Id)
                    .HasColumnName("id");

                pi.Property(p => p.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                pi.Property(p => p.ImageUrl)
                    .HasColumnName("image_url")
                    .HasMaxLength(2048)
                    .IsRequired();

                pi.Property(p => p.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                pi.HasKey(p => p.Id);

                pi.HasIndex(p => p.ProductId)
                    .HasDatabaseName("idx_image_product");
                
                pi.HasOne(p => p.Product)
                    .WithMany(p => p.Images)
                    .HasForeignKey(p => p.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // db-schema for Cart model
            modelBuilder.Entity<Cart>(ct =>
            {

                ct.Property(c => c.Id)
                    .HasColumnName("id");

                ct.Property(c => c.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                ct.Property(c => c.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                ct.Property(c => c.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                ct.HasKey(c => c.Id);

                ct.HasIndex(c => c.UserId)
                    .HasDatabaseName("idx_cart_user");

                ct.HasOne(c => c.User)
                    .WithOne()
                    .HasForeignKey<Cart>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // db-schema for CartItem model
            modelBuilder.Entity<CartItem>(ci =>
            {

                ci.Property(c => c.Id)
                    .HasColumnName("id");

                ci.Property(c => c.CartId)
                    .HasColumnName("cart_id")
                    .IsRequired();

                ci.Property(c => c.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                ci.Property(c => c.Quantity)
                    .HasColumnName("quantity")
                    .IsRequired();

                ci.Property(c => c.UnitPrice)
                    .HasColumnName("unit_price")
                    .HasPrecision(18, 2)
                    .IsRequired();

                ci.Property(c => c.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();
                
                ci.Property(c => c.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                ci.HasKey(c => c.Id);

                ci.HasIndex(c => c.CartId)
                    .HasDatabaseName("idx_cart_item_cart");
                
                ci.HasIndex(c => c.ProductId)
                    .HasDatabaseName("idx_cart_item_product");

                ci.HasOne(c => c.Cart)
                    .WithMany(c => c.Items)
                    .HasForeignKey(c => c.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                ci.HasOne(c => c.Product)
                    .WithMany()
                    .HasForeignKey(c => c.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // db-schema for ComputerComponent model
            modelBuilder.Entity<ComputerComponent>(cc =>
            {
                cc.Property(c => c.Id)
                    .HasColumnName("id");

                cc.Property(c => c.ComputerProductId)
                    .HasColumnName("computer_product_id")
                    .IsRequired();

                cc.Property(c => c.ComponentProductId)
                    .HasColumnName("component_product_id")
                    .IsRequired();

                cc.Property(c => c.Quantity)
                    .HasColumnName("quantity")
                    .IsRequired();

                cc.Property(c => c.AttachedAt)
                    .HasColumnName("attached_at")
                    .IsRequired();

                cc.Property(c => c.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                cc.HasKey(c => c.Id);

                cc.HasIndex(c => c.ComputerProductId)
                    .HasDatabaseName("idx_computer_product");
                
                cc.HasIndex(c => new { c.ComputerProductId, c.ComponentProductId })
                    .HasDatabaseName("idx_computer_component");
                
                cc.HasOne(c => c.ComputerProduct)
                    .WithMany()
                    .HasForeignKey(c => c.ComputerProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                cc.HasOne(c => c.ComponentProduct)
                    .WithMany()
                    .HasForeignKey(c => c.ComponentProductId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for Order model
            modelBuilder.Entity<Order>(od =>
            {
                od.Property(o => o.Id)
                    .HasColumnName("id");

                od.Property(o => o.UserId)
                    .HasColumnName("user_id");

                od.Property(o => o.DeliveryType)
                    .HasColumnName("delivery_type")
                    .HasConversion<string>()
                    .IsRequired();

                od.Property(o => o.SubTotal)
                    .HasColumnName("sub_total")
                    .HasPrecision(18, 2)
                    .IsRequired();

                od.Property(o => o.ShippingCost)
                    .HasColumnName("shipping_cost")
                    .HasPrecision(18, 2)
                    .IsRequired();

                od.Property(o => o.Tax)
                    .HasColumnName("tax")
                    .HasPrecision(18, 2)
                    .IsRequired();

                od.Property(o => o.TotalPrice)
                    .HasColumnName("total_price")
                    .HasPrecision(18, 2)
                    .IsRequired();

                od.Property(o => o.DiscountAmount)
                    .HasColumnName("discount_amount")
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0m)
                    .IsRequired();

                od.Property(o => o.ReceiverEmail)
                    .HasColumnName("receiver_email")
                    .HasMaxLength(256);

                od.Property(o => o.ReceiverFullName)
                    .HasColumnName("receiver_full_name")
                    .HasMaxLength(256);

                od.Property(o => o.ShippingAddress)
                    .HasColumnName("shipping_address")
                    .HasMaxLength(512);

                od.Property(o => o.TrackingPhone)
                    .HasColumnName("tracking_phone")
                    .HasMaxLength(20)
                    .IsRequired();

                od.Property(o => o.Notes)
                    .HasColumnName("notes")
                    .HasMaxLength(512);

                od.Property(o => o.PaidType)
                    .HasColumnName("paid_type")
                    .HasConversion<string>()
                    .IsRequired();

                od.Property(o => o.ReceiverIdentityCard)
                    .HasColumnName("receiver_identity_card")
                    .HasMaxLength(20);

                od.Property(o => o.InstallmentDurationMonth)
                    .HasColumnName("installment_duration_month");

                od.Property(o => o.OrderDate)
                    .HasColumnName("order_date")
                    .IsRequired();

                od.Property(o => o.DeliveredById)
                    .HasColumnName("delivered_by_id");

                od.Property(o => o.CourierService)
                    .HasColumnName("courier_service")
                    .HasMaxLength(100);

                od.Property(o => o.CourierTrackingCode)
                    .HasColumnName("courier_tracking_code")
                    .HasMaxLength(100);

                od.Property(o => o.ReceivedAt)
                    .HasColumnName("received_at");

                od.Property(o => o.Status)
                    .HasColumnName("status")
                    .HasConversion<string>()
                    .IsRequired();

                od.HasKey(o => o.Id);

                od.HasIndex(o => o.UserId)
                    .HasDatabaseName("idx_order_user");

                od.HasIndex(o => o.Status)
                    .HasDatabaseName("idx_order_status");

                od.HasIndex(o => o.TrackingPhone)
                    .HasDatabaseName("idx_order_tracking_phone");

                od.HasOne(o => o.User)
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                od.HasOne(o => o.DeliveredBy)
                    .WithMany()
                    .HasForeignKey(o => o.DeliveredById)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for OrderItem model
            modelBuilder.Entity<OrderItem>(oi =>
            {
                oi.Property(o => o.Id)
                    .HasColumnName("id");

                oi.Property(o => o.OrderId)
                    .HasColumnName("order_id")
                    .IsRequired();

                oi.Property(o => o.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                oi.Property(o => o.Quantity)
                    .HasColumnName("quantity")
                    .IsRequired();

                oi.Property(o => o.UnitPrice)
                    .HasColumnName("unit_price")
                    .HasPrecision(18, 2)
                    .IsRequired();

                oi.Property(o => o.IsFreeItem)
                    .HasColumnName("is_free_item")
                    .HasDefaultValue(false)
                    .IsRequired();

                oi.Property(o => o.WarrantyMonthSnapshot)
                    .HasColumnName("warranty_month_snapshot")
                    .IsRequired();

                oi.HasKey(o => o.Id);

                oi.HasIndex(o => o.OrderId)
                    .HasDatabaseName("idx_order_item_order");

                oi.HasIndex(o => o.ProductId)
                    .HasDatabaseName("idx_order_item_product");

                oi.HasOne(o => o.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(o => o.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                oi.HasOne(o => o.Product)
                    .WithMany()
                    .HasForeignKey(o => o.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for Installment model
            modelBuilder.Entity<Installment>(ins =>
            {
                ins.Property(i => i.Id)
                    .HasColumnName("id");

                ins.Property(i => i.OrderId)
                    .HasColumnName("order_id")
                    .IsRequired();

                ins.Property(i => i.Period)
                    .HasColumnName("period")
                    .IsRequired();

                ins.Property(i => i.Amount)
                    .HasColumnName("amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                ins.Property(i => i.Status)
                    .HasColumnName("status")
                    .HasConversion<string>()
                    .IsRequired();

                ins.Property(i => i.DueDate)
                    .HasColumnName("due_date")
                    .IsRequired();

                ins.HasKey(i => i.Id);

                ins.HasIndex(i => new { i.OrderId, i.Period })
                    .HasDatabaseName("idx_installment_order_period")
                    .IsUnique();

                ins.HasOne(i => i.Order)
                    .WithMany()
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // db-schema for Payment model
            modelBuilder.Entity<Payment>(pm =>
            {
                pm.Property(p => p.Id)
                    .HasColumnName("id");

                pm.Property(p => p.OrderId)
                    .HasColumnName("order_id")
                    .IsRequired();

                pm.Property(p => p.InstallmentId)
                    .HasColumnName("installment_id");

                pm.Property(p => p.Amount)
                    .HasColumnName("amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                pm.Property(p => p.Method)
                    .HasColumnName("method")
                    .HasConversion<string>()
                    .IsRequired();

                pm.Property(p => p.Status)
                    .HasColumnName("status")
                    .HasConversion<string>()
                    .IsRequired();

                pm.Property(p => p.PaymentDate)
                    .HasColumnName("payment_date")
                    .IsRequired();

                pm.HasKey(p => p.Id);

                pm.HasIndex(p => p.OrderId)
                    .HasDatabaseName("idx_payment_order");

                pm.HasIndex(p => p.InstallmentId)
                    .HasDatabaseName("idx_payment_installment");

                pm.HasOne(p => p.Order)
                    .WithMany()
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.NoAction);

                pm.HasOne(p => p.Installment)
                    .WithMany()
                    .HasForeignKey(p => p.InstallmentId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for CustomPC model
            modelBuilder.Entity<CustomPC>(cp =>
            {
                cp.Property(c => c.Id)
                    .HasColumnName("id");

                cp.Property(c => c.UserId)
                    .HasColumnName("user_id");

                cp.Property(c => c.SessionId)
                    .HasColumnName("session_id")
                    .HasMaxLength(64);

                cp.Property(c => c.Name)
                    .HasColumnName("name")
                    .HasMaxLength(256)
                    .IsRequired();

                cp.Property(c => c.IsStaffAccessible)
                    .HasColumnName("is_staff_accessible")
                    .HasDefaultValue(false)
                    .IsRequired();

                cp.Property(c => c.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                cp.Property(c => c.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                cp.HasKey(c => c.Id);

                cp.HasIndex(c => c.UserId)
                    .HasDatabaseName("idx_custom_pc_user");

                cp.HasIndex(c => c.SessionId)
                    .HasDatabaseName("idx_custom_pc_session");

                cp.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // db-schema for CustomPCItem model
            modelBuilder.Entity<CustomPCItem>(ci =>
            {
                ci.Property(c => c.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();

                ci.Property(c => c.CustomPCId)
                    .HasColumnName("custom_pc_id")
                    .IsRequired();

                ci.Property(c => c.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                ci.Property(c => c.Quantity)
                    .HasColumnName("quantity")
                    .IsRequired();

                ci.Property(c => c.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                ci.HasKey(c => c.Id);

                ci.HasIndex(c => c.CustomPCId)
                    .HasDatabaseName("idx_custom_pc_item_custom_pc");

                ci.HasIndex(c => c.ProductId)
                    .HasDatabaseName("idx_custom_pc_item_product");

                ci.HasIndex(c => new { c.CustomPCId, c.ProductId })
                    .HasDatabaseName("idx_custom_pc_item_unique")
                    .IsUnique();

                ci.HasOne(c => c.CustomPC)
                    .WithMany(c => c.Items)
                    .HasForeignKey(c => c.CustomPCId)
                    .OnDelete(DeleteBehavior.Cascade);

                ci.HasOne(c => c.Product)
                    .WithMany()
                    .HasForeignKey(c => c.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);

                ci.ToTable(t => {
                    t.HasCheckConstraint("ck_custom_pc_item_quantity", "[quantity] >= 1");
                });
            });


            // db-schema for Review model
            modelBuilder.Entity<Review>(rv =>
            {
                rv.Property(r => r.Id)
                    .HasColumnName("id");

                rv.Property(r => r.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                rv.Property(r => r.UserId)
                    .HasColumnName("user_id");

                rv.Property(r => r.FullName)
                    .HasColumnName("full_name")
                    .HasMaxLength(256);

                rv.Property(r => r.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(20);

                rv.Property(r => r.Comment)
                    .HasColumnName("comment")
                    .HasMaxLength(2048)
                    .IsRequired();

                rv.Property(r => r.Rating)
                    .HasColumnName("rating")
                    .IsRequired();

                rv.Property(r => r.IsDeleted)
                    .HasColumnName("is_deleted")
                    .HasDefaultValue(false)
                    .IsRequired();

                rv.Property(r => r.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                rv.Property(r => r.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                rv.HasKey(r => r.Id);

                rv.HasIndex(r => r.ProductId)
                    .HasDatabaseName("idx_review_product");

                rv.HasIndex(r => r.UserId)
                    .HasDatabaseName("idx_review_user");

                rv.HasOne(r => r.Product)
                    .WithMany()
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);

                rv.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for ReviewMedia model
            modelBuilder.Entity<ReviewMedia>(rm =>
            {
                rm.Property(r => r.Id)
                    .HasColumnName("id");

                rm.Property(r => r.ReviewId)
                    .HasColumnName("review_id")
                    .IsRequired();

                rm.Property(r => r.MediaUrl)
                    .HasColumnName("media_url")
                    .HasMaxLength(2048)
                    .IsRequired();

                rm.Property(r => r.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                rm.HasKey(r => r.Id);

                rm.HasIndex(r => r.ReviewId)
                    .HasDatabaseName("idx_review_media_review");

                rm.HasOne(r => r.Review)
                    .WithMany(r => r.Medias)
                    .HasForeignKey(r => r.ReviewId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // db-schema for Promotion model
            modelBuilder.Entity<Promotion>(pm =>
            {
                pm.Property(p => p.Id)
                    .HasColumnName("id");

                pm.Property(p => p.Name)
                    .HasColumnName("name")
                    .HasMaxLength(256)
                    .IsRequired();

                pm.Property(p => p.Code)
                    .HasColumnName("code")
                    .HasMaxLength(100);

                pm.Property(p => p.Description)
                    .HasColumnName("description")
                    .HasMaxLength(4096)
                    .IsRequired();

                pm.Property(p => p.Type)
                    .HasColumnName("type")
                    .HasConversion<string>()
                    .IsRequired();

                pm.Property(p => p.Scope)
                    .HasColumnName("scope")
                    .HasConversion<string>()
                    .IsRequired();

                pm.Property(p => p.DiscountValue)
                    .HasColumnName("discount_value")
                    .HasPrecision(18, 2);

                pm.Property(p => p.MaxDiscountValue)
                    .HasColumnName("max_discount_value")
                    .HasPrecision(18, 2);

                pm.Property(p => p.MinOrderValue)
                    .HasColumnName("min_order_value")
                    .HasPrecision(18, 2);

                pm.Property(p => p.RequiredProductLogic)
                    .HasColumnName("required_product_logic")
                    .HasConversion<string>();

                pm.Property(p => p.FreeItemPickCount)
                    .HasColumnName("free_item_pick_count");

                pm.Property(p => p.CategoryId)
                    .HasColumnName("category_id");

                pm.Property(p => p.BrandId)
                    .HasColumnName("brand_id");

                pm.Property(p => p.MinAppliedQuantity)
                    .HasColumnName("min_applied_quantity");

                pm.Property(p => p.MaxUsageCount)
                    .HasColumnName("max_usage_count");

                pm.Property(p => p.UsageCount)
                    .HasColumnName("usage_count")
                    .HasDefaultValue(0)
                    .IsRequired();

                pm.Property(p => p.MaxUsagePerUser)
                    .HasColumnName("max_usage_per_user");

                pm.Property(p => p.StartDate)
                    .HasColumnName("start_date")
                    .IsRequired();

                pm.Property(p => p.EndDate)
                    .HasColumnName("end_date")
                    .IsRequired();

                pm.Property(p => p.IsStackable)
                    .HasColumnName("is_stackable")
                    .HasDefaultValue(false)
                    .IsRequired();

                pm.Property(p => p.IsActive)
                    .HasColumnName("is_active")
                    .HasDefaultValue(false)
                    .IsRequired();

                pm.Property(p => p.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                pm.Property(p => p.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                pm.HasKey(p => p.Id);

                pm.HasIndex(p => p.Code)
                    .HasDatabaseName("idx_promotion_code")
                    .IsUnique()
                    .HasFilter("[code] IS NOT NULL");

                pm.HasIndex(p => new { p.StartDate, p.EndDate })
                    .HasDatabaseName("idx_promotion_date_range");

                pm.HasIndex(p => p.IsActive)
                    .HasDatabaseName("idx_promotion_is_active");

                pm.HasIndex(p => p.CategoryId)
                    .HasDatabaseName("idx_promotion_category");

                pm.HasIndex(p => p.BrandId)
                    .HasDatabaseName("idx_promotion_brand");

                pm.HasOne(p => p.Category)
                    .WithMany()
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);

                pm.HasOne(p => p.Brand)
                    .WithMany()
                    .HasForeignKey(p => p.BrandId)
                    .OnDelete(DeleteBehavior.NoAction);

                pm.ToTable(t =>
                {
                    t.HasCheckConstraint("ck_promotion_discount_value", "[discount_value] > 0");
                    t.HasCheckConstraint("ck_promotion_usage_count", "[usage_count] >= 0");
                    t.HasCheckConstraint("ck_promotion_date", "[end_date] > [start_date]");
                    t.HasCheckConstraint("ck_promotion_free_item_pick_count", "[free_item_pick_count] > 0");
                    t.HasCheckConstraint("ck_promotion_min_applied_quantity", "[min_applied_quantity] > 0");
                });
            });


            // db-schema for PromotionRequiredProduct model
            modelBuilder.Entity<PromotionRequiredProduct>(prp =>
            {
                prp.Property(p => p.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();

                prp.Property(p => p.PromotionId)
                    .HasColumnName("promotion_id")
                    .IsRequired();

                prp.Property(p => p.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                prp.Property(p => p.MinQuantity)
                    .HasColumnName("min_quantity")
                    .IsRequired();

                prp.Property(p => p.MaxQuantity)
                    .HasColumnName("max_quantity");

                prp.HasKey(p => p.Id);

                prp.HasIndex(p => p.PromotionId)
                    .HasDatabaseName("idx_promo_required_promotion");

                prp.HasIndex(p => new { p.PromotionId, p.ProductId })
                    .HasDatabaseName("idx_promo_required_promotion_product")
                    .IsUnique();

                prp.HasOne(p => p.Promotion)
                    .WithMany(p => p.RequiredProducts)
                    .HasForeignKey(p => p.PromotionId)
                    .OnDelete(DeleteBehavior.Cascade);

                prp.HasOne(p => p.Product)
                    .WithMany()
                    .HasForeignKey(p => p.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);

                prp.ToTable(t =>
                {
                    t.HasCheckConstraint("ck_promo_required_min_quantity", "[min_quantity] >= 1");
                    t.HasCheckConstraint("ck_promo_required_max_quantity", "[max_quantity] >= [min_quantity]");
                });
            });


            // db-schema for PromotionFreeProduct model
            modelBuilder.Entity<PromotionFreeProduct>(pfp =>
            {
                pfp.Property(p => p.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();

                pfp.Property(p => p.PromotionId)
                    .HasColumnName("promotion_id")
                    .IsRequired();

                pfp.Property(p => p.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                pfp.Property(p => p.Quantity)
                    .HasColumnName("quantity")
                    .IsRequired();

                pfp.HasKey(p => p.Id);

                pfp.HasIndex(p => p.PromotionId)
                    .HasDatabaseName("idx_promo_free_promotion");

                pfp.HasIndex(p => new { p.PromotionId, p.ProductId })
                    .HasDatabaseName("idx_promo_free_promotion_product")
                    .IsUnique();

                pfp.HasOne(p => p.Promotion)
                    .WithMany(p => p.FreeProducts)
                    .HasForeignKey(p => p.PromotionId)
                    .OnDelete(DeleteBehavior.Cascade);

                pfp.HasOne(p => p.Product)
                    .WithMany()
                    .HasForeignKey(p => p.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);

                pfp.ToTable(t =>
                {
                    t.HasCheckConstraint("ck_promo_free_quantity", "[quantity] >= 1");
                });
            });


            // db-schema for PromotionAppliedProduct model
            modelBuilder.Entity<PromotionAppliedProduct>(pap =>
            {
                pap.Property(p => p.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();

                pap.Property(p => p.PromotionId)
                    .HasColumnName("promotion_id")
                    .IsRequired();

                pap.Property(p => p.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                pap.HasKey(p => p.Id);

                pap.HasIndex(p => p.PromotionId)
                    .HasDatabaseName("idx_promo_applied_promotion");

                pap.HasIndex(p => new { p.PromotionId, p.ProductId })
                    .HasDatabaseName("idx_promo_applied_promotion_product")
                    .IsUnique();

                pap.HasOne(p => p.Promotion)
                    .WithMany(p => p.AppliedProducts)
                    .HasForeignKey(p => p.PromotionId)
                    .OnDelete(DeleteBehavior.Cascade);

                pap.HasOne(p => p.Product)
                    .WithMany()
                    .HasForeignKey(p => p.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for PromotionUsage model
            modelBuilder.Entity<PromotionUsage>(pu =>
            {
                pu.Property(p => p.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();

                pu.Property(p => p.PromotionId)
                    .HasColumnName("promotion_id")
                    .IsRequired();

                pu.Property(p => p.UserId)
                    .HasColumnName("user_id");

                pu.Property(p => p.OrderId)
                    .HasColumnName("order_id")
                    .IsRequired();

                pu.Property(p => p.FullName)
                    .HasColumnName("full_name")
                    .HasMaxLength(256);

                pu.Property(p => p.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(20);

                pu.Property(p => p.DiscountAmount)
                    .HasColumnName("discount_amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                pu.Property(p => p.UsedAt)
                    .HasColumnName("used_at")
                    .IsRequired();

                pu.HasKey(p => p.Id);

                pu.HasIndex(p => p.PromotionId)
                    .HasDatabaseName("idx_promo_usage_promotion");

                pu.HasIndex(p => p.UserId)
                    .HasDatabaseName("idx_promo_usage_user");

                pu.HasIndex(p => p.OrderId)
                    .HasDatabaseName("idx_promo_usage_order");

                pu.HasIndex(p => new { p.PromotionId, p.UserId })
                    .HasDatabaseName("idx_promo_usage_promotion_user");

                pu.HasOne(p => p.Promotion)
                    .WithMany(p => p.Usages)
                    .HasForeignKey(p => p.PromotionId)
                    .OnDelete(DeleteBehavior.NoAction);

                pu.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                pu.HasOne(p => p.Order)
                    .WithMany()
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.NoAction);

                pu.ToTable(t =>
                {
                    t.HasCheckConstraint("ck_promo_usage_discount_amount", "[discount_amount] >= 0");
                });
            });


            // db-schema for Ticket model
            modelBuilder.Entity<Ticket>(tk =>
            {
                tk.Property(t => t.Id)
                    .HasColumnName("id");

                tk.Property(t => t.UserId)
                    .HasColumnName("user_id");

                tk.Property(t => t.FullName)
                    .HasColumnName("full_name")
                    .HasMaxLength(256);

                tk.Property(t => t.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(20);

                tk.Property(t => t.Title)
                    .HasColumnName("title")
                    .HasMaxLength(256)
                    .IsRequired();

                tk.Property(t => t.Description)
                    .HasColumnName("description")
                    .HasMaxLength(4096)
                    .IsRequired();

                tk.Property(t => t.Type)
                    .HasColumnName("type")
                    .HasConversion<string>()
                    .IsRequired();

                tk.Property(t => t.Status)
                    .HasColumnName("status")
                    .HasConversion<string>()
                    .IsRequired();

                tk.Property(t => t.Priority)
                    .HasColumnName("priority")
                    .HasConversion<string>()
                    .IsRequired();

                tk.Property(t => t.CustomPCId)
                    .HasColumnName("custom_pc_id");

                tk.Property(t => t.OrderId)
                    .HasColumnName("order_id");

                tk.Property(t => t.OrderItemId)
                    .HasColumnName("order_item_id");

                tk.Property(t => t.AssignedToUserId)
                    .HasColumnName("assigned_to_user_id");

                tk.Property(t => t.ResolvedAt)
                    .HasColumnName("resolved_at");

                tk.Property(t => t.ClosedAt)
                    .HasColumnName("closed_at");

                tk.Property(t => t.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                tk.Property(t => t.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                tk.HasKey(t => t.Id);

                tk.HasIndex(t => t.UserId)
                    .HasDatabaseName("idx_ticket_user");

                tk.HasIndex(t => t.Status)
                    .HasDatabaseName("idx_ticket_status");

                tk.HasIndex(t => t.AssignedToUserId)
                    .HasDatabaseName("idx_ticket_assigned");

                tk.HasIndex(t => t.CustomPCId)
                    .HasDatabaseName("idx_ticket_custom_pc");

                tk.HasIndex(t => t.OrderId)
                    .HasDatabaseName("idx_ticket_order");

                tk.HasIndex(t => t.OrderItemId)
                    .HasDatabaseName("idx_ticket_order_item");

                tk.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                tk.HasOne(t => t.AssignedTo)
                    .WithMany()
                    .HasForeignKey(t => t.AssignedToUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                tk.HasOne(t => t.CustomPC)
                    .WithMany()
                    .HasForeignKey(t => t.CustomPCId)
                    .OnDelete(DeleteBehavior.NoAction);

                tk.HasOne(t => t.Order)
                    .WithMany()
                    .HasForeignKey(t => t.OrderId)
                    .OnDelete(DeleteBehavior.NoAction);

                tk.HasOne(t => t.OrderItem)
                    .WithMany()
                    .HasForeignKey(t => t.OrderItemId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for TicketMessage model
            modelBuilder.Entity<TicketMessage>(tm =>
            {
                tm.Property(t => t.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();

                tm.Property(t => t.TicketId)
                    .HasColumnName("ticket_id")
                    .IsRequired();

                tm.Property(t => t.UserId)
                    .HasColumnName("user_id");

                tm.Property(t => t.Content)
                    .HasColumnName("content")
                    .HasMaxLength(4096)
                    .IsRequired();

                tm.Property(t => t.IsStaffMessage)
                    .HasColumnName("is_staff_message")
                    .HasDefaultValue(false)
                    .IsRequired();

                tm.Property(t => t.SentAt)
                    .HasColumnName("sent_at")
                    .IsRequired();

                tm.HasKey(t => t.Id);

                tm.HasIndex(t => t.TicketId)
                    .HasDatabaseName("idx_ticket_message_ticket");

                tm.HasIndex(t => t.UserId)
                    .HasDatabaseName("idx_ticket_message_user");

                tm.HasOne(t => t.Ticket)
                    .WithMany(t => t.Messages)
                    .HasForeignKey(t => t.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                tm.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for TicketAttachment model
            modelBuilder.Entity<TicketAttachment>(ta =>
            {
                ta.Property(t => t.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();

                ta.Property(t => t.MessageId)
                    .HasColumnName("message_id")
                    .IsRequired();

                ta.Property(t => t.FileUrl)
                    .HasColumnName("file_url")
                    .HasMaxLength(2048)
                    .IsRequired();

                ta.Property(t => t.UploadedAt)
                    .HasColumnName("uploaded_at")
                    .IsRequired();

                ta.HasKey(t => t.Id);

                ta.HasIndex(t => t.MessageId)
                    .HasDatabaseName("idx_ticket_attachment_message");

                ta.HasOne(t => t.Message)
                    .WithMany(t => t.Attachments)
                    .HasForeignKey(t => t.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // db-schema for Notification model
            modelBuilder.Entity<Notification>(nf =>
            {
                nf.Property(n => n.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();

                nf.Property(n => n.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                nf.Property(n => n.Type)
                    .HasColumnName("type")
                    .HasConversion<string>()
                    .IsRequired();

                nf.Property(n => n.Title)
                    .HasColumnName("title")
                    .HasMaxLength(256)
                    .IsRequired();

                nf.Property(n => n.Message)
                    .HasColumnName("message")
                    .HasMaxLength(1024)
                    .IsRequired();

                nf.Property(n => n.ReferenceId)
                    .HasColumnName("reference_id");

                nf.Property(n => n.ReferenceType)
                    .HasColumnName("reference_type")
                    .HasConversion<string>();

                nf.Property(n => n.IsRead)
                    .HasColumnName("is_read")
                    .HasDefaultValue(false)
                    .IsRequired();

                nf.Property(n => n.ReadAt)
                    .HasColumnName("read_at");

                nf.Property(n => n.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                nf.HasKey(n => n.Id);

                nf.HasIndex(n => n.UserId)
                    .HasDatabaseName("idx_notification_user");

                nf.HasIndex(n => new { n.UserId, n.IsRead })
                    .HasDatabaseName("idx_notification_user_is_read");

                nf.HasIndex(n => n.Type)
                    .HasDatabaseName("idx_notification_type");

                nf.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // db-schema for ChatSession model
            modelBuilder.Entity<ChatSession>(cs =>
            {
                cs.Property(c => c.Id)
                    .HasColumnName("id");

                cs.Property(c => c.UserId)
                    .HasColumnName("user_id");

                cs.Property(c => c.FullName)
                    .HasColumnName("full_name")
                    .HasMaxLength(256)
                    .IsRequired();

                cs.Property(c => c.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(20);

                cs.Property(c => c.IsEscalated)
                    .HasColumnName("is_escalated")
                    .HasDefaultValue(false)
                    .IsRequired();

                cs.Property(c => c.IsClosed)
                    .HasColumnName("is_closed")
                    .HasDefaultValue(false)
                    .IsRequired();

                cs.Property(c => c.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                cs.Property(c => c.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                cs.HasKey(c => c.Id);

                cs.HasIndex(c => c.UserId)
                    .HasDatabaseName("idx_chat_session_user");

                cs.HasIndex(c => c.IsClosed)
                    .HasDatabaseName("idx_chat_session_is_closed");

                cs.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for ChatMessage model
            modelBuilder.Entity<ChatMessage>(cm =>
            {
                cm.Property(c => c.Id)
                    .HasColumnName("id");

                cm.Property(c => c.SessionId)
                    .HasColumnName("session_id")
                    .IsRequired();

                cm.Property(c => c.SentById)
                    .HasColumnName("sent_by_id");

                cm.Property(c => c.SentByFullName)
                    .HasColumnName("sent_by_full_name")
                    .HasMaxLength(256);

                cm.Property(c => c.Message)
                    .HasColumnName("message")
                    .HasMaxLength(4096)
                    .IsRequired();

                cm.Property(c => c.IsAiMessage)
                    .HasColumnName("is_ai_message")
                    .HasDefaultValue(false)
                    .IsRequired();

                cm.Property(c => c.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                cm.HasKey(c => c.Id);

                cm.HasIndex(c => c.SessionId)
                    .HasDatabaseName("idx_chat_message_session");

                cm.HasOne(c => c.Session)
                    .WithMany(s => s.Messages)
                    .HasForeignKey(c => c.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                cm.HasOne(c => c.SentBy)
                    .WithMany()
                    .HasForeignKey(c => c.SentById)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // db-schema for ChatMedia model
            modelBuilder.Entity<ChatMedia>(cma =>
            {
                cma.Property(c => c.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();

                cma.Property(c => c.MessageId)
                    .HasColumnName("message_id")
                    .IsRequired();

                cma.Property(c => c.MediaUrl)
                    .HasColumnName("media_url")
                    .HasMaxLength(2048)
                    .IsRequired();

                cma.Property(c => c.Type)
                    .HasColumnName("type")
                    .HasConversion<string>()
                    .IsRequired();

                cma.HasKey(c => c.Id);

                cma.HasIndex(c => c.MessageId)
                    .HasDatabaseName("idx_chat_media_message");

                cma.HasOne(c => c.Message)
                    .WithMany(m => m.Medias)
                    .HasForeignKey(c => c.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }


        public DbSet<User> Users { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<BrandCategory> BrandCategories { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductSpecValue> ProductSpecValues { get; set; }
        public DbSet<SpecDefinition> SpecDefinitions { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Installment> Installments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ComputerComponent> ComputerComponents { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewMedia> ReviewMedias { get; set; }
        public DbSet<CustomPC> CustomPCs { get; set; }
        public DbSet<CustomPCItem> CustomPCItems { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionRequiredProduct> PromotionRequiredProducts { get; set; }
        public DbSet<PromotionFreeProduct> PromotionFreeProducts { get; set; }
        public DbSet<PromotionAppliedProduct> PromotionAppliedProducts { get; set; }
        public DbSet<PromotionUsage> PromotionUsages { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketMessage> TicketMessages { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatMedia> ChatMedias { get; set; }
    }
}
