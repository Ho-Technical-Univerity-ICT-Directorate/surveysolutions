﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Infrastructure
{
    /// <summary>
    /// This class only needed for local development. It's not used in runtime.
    /// This class required for `Add-Migration` to work from Package Manager Console
    /// </summary>
    public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
    {
        public TenantDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseNpgsql(
                "Server=127.0.0.1;Port=5432;User Id=postgres;Password=P@$$w0rd;Database=ExportService;");

            var tenantInfo = new TenantInfo
            {
                Id = TenantId.None,
                Name = null
            };
            var connectionSettings = Options.Create(new DbConnectionSettings
            {
                DefaultConnection = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=P@$$w0rd;Database=ExportService;"
            });
            var tenantContext = new TenantContext(null)
            {
                Tenant = tenantInfo
            };
            return new TenantDbContext(tenantContext, connectionSettings, optionsBuilder.Options);
        }
    }
}