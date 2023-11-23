using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.InMemory;


namespace CRUDTests
{
    public class CustomWebApplicationFactory: WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.UseEnvironment("Test");

            builder.ConfigureServices(services => {
                //megkeresi a dbcontexet, ami a Program-ban van:
                var descripter = services.SingleOrDefault(t => t.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                //remove original db:
                if (descripter != null)  services.Remove(descripter);

                //inmemory db letrehozas. MIndig uj ures db jon letre amikor az appot futtatjuk
                //ebben nem lesz useSQLserver, ami van a Programban
                //itt barmi lehet a nev: DatabaseForTesting
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("DatabaseForTesting");
                });

            });
        } 
    }
}
