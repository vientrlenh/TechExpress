using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
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

        public ServiceProviders(UnitOfWork unitOfWork, SmtpEmailSender emailSender, JwtUtils jwtUtils, UserContext userContext, OtpUtils otpUtils, IConnectionMultiplexer redis, IHubContext<CartHub> cartHubContext)
        {
            AuthService = new AuthService(unitOfWork, jwtUtils, userContext, otpUtils, emailSender);
            UserService = new UserService(unitOfWork, userContext, redis);
            ProductService = new ProductService(unitOfWork);
            CategoryService = new CategoryService(unitOfWork);
            SpecDefinitionService = new SpecDefinitionService(unitOfWork);
            BrandService = new BrandService(unitOfWork);
            CartService = new CartService(unitOfWork);
        }
    }
}
