using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application
{
    public static class BusinessStartup
    {

        public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
        {
            // here we register our application services , which contain the business logic of our application
            // DI (Dependancy Injections)

            services.AddScoped<ICustomerService, CustomerService>();

            return services;
        }
    }
}
