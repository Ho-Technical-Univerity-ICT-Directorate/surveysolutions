using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using WB.Core.BoundedContexts.Designer;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Infrastructure.Native.Files;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Designer.Api.Designer;
using WB.UI.Designer.App_Start;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.ConfigurationManager;
using WB.UI.Designer.CommandDeserialization;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Kernel;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Settings;
using WB.UI.Shared.Web.Versions;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof (AutofacWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof (AutofacWebCommon), "Stop")]

namespace WB.UI.Designer.App_Start
{
    public static class AutofacWebCommon
    {
        public static void Start()
        {
            CreateKernel();
        }

        public static void Stop()
        {
        }

        private static void CreateKernel()
        {
            var settingsProvider = new SettingsProvider();

            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            MvcApplication.Initialize(); // pinging global.asax to perform it's part of static initialization

            var dynamicCompilerSettings = settingsProvider.GetSection<DynamicCompilerSettingsGroup>("dynamicCompilerSettingsGroup");

            var pdfSettings = settingsProvider.GetSection<PdfConfigSection>("pdf");

            var deskSettings = settingsProvider.GetSection<DeskConfigSection>("desk");

            var membershipSection = settingsProvider.GetSection<MembershipSection>("system.web/membership");
            var membershipSettings = membershipSection?.Providers[membershipSection.DefaultProvider].Parameters;
            var ormSettings = new UnitOfWorkConnectionSettings
            {
                ConnectionString = settingsProvider.ConnectionStrings["Postgres"].ConnectionString,
                PlainMappingAssemblies = new List<Assembly>
                {
                    typeof(DesignerBoundedContextModule).Assembly,
                    typeof(ProductVersionModule).Assembly,
                },
                PlainStorageSchemaName = "plainstore",
                ReadSideMappingAssemblies = new List<Assembly>(),
                PlainStoreUpgradeSettings = new DbUpgradeSettings(typeof(Migrations.PlainStore.M001_Init).Assembly, typeof(Migrations.PlainStore.M001_Init).Namespace)
            };


            var kernel = new AutofacWebKernel();
            kernel.Load(
                new EventFreeInfrastructureModule(),
                new InfrastructureModule(),
                new NcqrsModule(),
                new WebConfigurationModule(membershipSettings),
                new CaptchaModule(settingsProvider.AppSettings.Get("CaptchaService")),
                new NLogLoggingModule(),
                new OrmModule(ormSettings),
                new DesignerCommandDeserializationModule(),
                new DesignerBoundedContextModule(dynamicCompilerSettings),
                new QuestionnaireVerificationModule(),
                new MembershipModule(),
                new FileInfrastructureModule(),
                new ProductVersionModule(typeof(MvcApplication).Assembly),
                new DesignerRegistry(pdfSettings, deskSettings, settingsProvider.AppSettings.GetInt("QuestionnaireChangeHistoryLimit", 500)),
                new DesignerWebModule(),
                new AutofacWebCommonModule()
                );

            var config = GlobalConfiguration.Configuration;

            kernel.ContainerBuilder.RegisterControllers(Assembly.GetExecutingAssembly());
            kernel.ContainerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            kernel.ContainerBuilder.RegisterWebApiFilterProvider(config);
            kernel.ContainerBuilder.RegisterWebApiModelBinderProvider();

            kernel.ContainerBuilder.RegisterFilterProvider();
            kernel.ContainerBuilder.RegisterModelBinderProvider();

            //temp logging
            //kernel.ContainerBuilder.RegisterModule<LogRequestModule>();

            // init
            var initTask = kernel.InitAsync();

            if (CoreSettings.IsDevelopmentEnvironment)
                initTask.Wait();
            else
                initTask.Wait(TimeSpan.FromSeconds(10));

            var serviceLocatorFactory = new AutofacServiceLocatorFactory();
            ServiceLocator.SetLocatorProvider(() => serviceLocatorFactory.GetServiceLocator());

            // DependencyResolver
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(kernel.Container);

            DependencyResolver.SetResolver(new AutofacDependencyResolver(kernel.Container));

            InScopeExecutor.Init(new UnitOfWorkInScopeExecutor(kernel.Container));
        }
    }
}