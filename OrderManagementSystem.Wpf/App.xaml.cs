using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Application;
using System.IO;
using System.Windows;
using Applications = System.Windows.Application; // Solution Here
using OrderManagementSystem.Infrastructure;
using OrderManagementSystem.Infrastructure.DBContext;
using OrderManagementSystem.Wpf.ViewModels.Customers;

namespace OrderManagementSystem.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Applications
    {
        public static IServiceProvider? ServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; } = default!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var services = new ServiceCollection();

            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            var mainWWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // register the IConfiguration instance (non-nullable)
            services.AddSingleton<IConfiguration>(Configuration);

            // call DataAccess (Infrastructure)
            // this line register DbContext , IDbconnection , Repositories
            services.AddDataLayer(Configuration);

            // call application Layer (Application)
            // this line register Services
            services.AddBusinessLayer();

            // ViewModels Registers (DI)
            services.AddTransient<CustomersViewModel>();
            services.AddTransient<AddCustomerViewModel>();

            // UI Registers (DI)
            services.AddSingleton<MainWindow>();
        }
    }

}
