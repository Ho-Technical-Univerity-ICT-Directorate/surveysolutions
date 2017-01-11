using System;
using System.Diagnostics;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Controllers;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public class WebInterviewController : BaseController
    {
        public WebInterviewController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger)
            : base(commandService, globalInfo, logger)
        {

        }

        [ValidateInput(false)]
        public ActionResult Index()
        {
            return this.View();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
           HandleInDebugMode(filterContext);
        }

        [Conditional("DEBUG")]
        private void HandleInDebugMode(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;
            
            filterContext.Result = new ContentResult
            {
                Content = @"
<h1>No <b>Index.cshtml<b> found in ~Views\WebInterview folder.</h1>
<p>Index.cshtml is generated by 'WB.UI.Headquarters.Interview' application build. </p>
<p>Please navigate to WB.UI.Headquarters.Interview folder and run following commands that will install all nodejs deps and run dev server</p>
<pre>
npm install 
npm run dev
</pre>
"
            };
        }
    }
}