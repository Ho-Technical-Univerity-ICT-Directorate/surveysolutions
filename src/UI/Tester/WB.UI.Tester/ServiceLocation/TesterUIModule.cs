﻿using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Tester.Implementation.Services;
using WB.UI.Tester.Infrastructure.Internals.Settings;

namespace WB.UI.Tester.ServiceLocation
{
    public class TesterUIModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IViewModelNavigationService, ViewModelNavigationService>();
            registry.Bind<IMapService, MapService>();
            registry.Bind<TesterSettings>();

#if EXCLUDEEXTENSIONS
            registry.Bind<IAreaEditService, WB.UI.Shared.Enumerator.CustomServices.AreaEditor.DummyAreaEditService>();
#else
            registry.Bind<WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel>();
            registry.Bind<IAreaEditService, WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditService>();
#endif
        }
    }
}