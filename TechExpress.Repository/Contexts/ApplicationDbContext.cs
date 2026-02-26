using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
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
                    .IsUnique();

                user.HasIndex(u => u.Identity)
                    .HasDatabaseName("idx_user_identity")
                    .IsUnique();
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
                    .WithMany(o => o.Payments)
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.NoAction);

                pm.HasOne(p => p.Installment)
                    .WithMany()
                    .HasForeignKey(p => p.InstallmentId)
                    .OnDelete(DeleteBehavior.NoAction);
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
    }
}
