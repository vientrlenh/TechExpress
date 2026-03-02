using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using TechExpress.Repository;
using TechExpress.Repository.Models;
using TechExpress.Service.Contexts;
using TechExpress.Service.Hubs;
using TechExpress.Service.Services;
using TechExpress.Service.Utils;

namespace TechExpress.Service
{
    public class ServiceProviders
    {
        public AuthService AuthService { get; }
        public UserService UserService { get; }
        public SpecDefinitionService SpecDefinitionService { get; }
        public BrandService BrandService { get; }
        public ProductService ProductService { get; }
        public CategoryService CategoryService { get; }
        public CartService CartService { get; }
        public PaymentService PaymentService { get; }
        public InstallmentService InstallmentService { get; }
        public OrderService OrderService { get; }

        public UserContext UserContext { get; }

        public ServiceProviders(UnitOfWork unitOfWork, PayOsClient payOsClient,RedisUtils redisUtils, SmtpEmailSender emailSender, JwtUtils jwtUtils, UserContext userContext, OtpUtils otpUtils, IConnectionMultiplexer redis, IHubContext<CartHub> cartHubContext)
        {
            AuthService = new AuthService(unitOfWork, jwtUtils, userContext, otpUtils, emailSender);
            UserService = new UserService(unitOfWork, userContext, redis);
            ProductService = new ProductService(unitOfWork);
            CategoryService = new CategoryService(unitOfWork);
            SpecDefinitionService = new SpecDefinitionService(unitOfWork);
            BrandService = new BrandService(unitOfWork);
            CartService = new CartService(unitOfWork);
            PaymentService = new PaymentService(unitOfWork, redisUtils, payOsClient);
            InstallmentService = new InstallmentService(unitOfWork);
            OrderService = new OrderService(unitOfWork, userContext);
            UserContext = userContext;
        }
    }
}
