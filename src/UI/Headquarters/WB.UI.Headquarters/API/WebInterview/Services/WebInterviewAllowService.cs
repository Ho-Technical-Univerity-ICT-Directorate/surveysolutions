using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    class WebInterviewAllowService : IWebInterviewAllowService
    {
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private static readonly List<InterviewStatus> AllowedInterviewStatuses = new List<InterviewStatus>
        {
            InterviewStatus.InterviewerAssigned,
            InterviewStatus.Restarted,
            InterviewStatus.RejectedBySupervisor
        };

        private static readonly List<InterviewStatus> AnonymousUserAllowedStatuses = new List<InterviewStatus>
        {
            InterviewStatus.InterviewerAssigned
        };

        public WebInterviewAllowService(
            ITransactionManagerProvider transactionManagerProvider,
            IPlainTransactionManagerProvider plainTransactionManagerProvider,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage,
            IWebInterviewConfigProvider webInterviewConfigProvider, 
            IAuthorizedUser authorizedUser)
        {
            this.transactionManagerProvider = transactionManagerProvider;
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.authorizedUser = authorizedUser;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
        }

        public void CheckWebInterviewAccessPermissions(string interviewId)
        {
            Guid interviewGuid = Guid.Parse(interviewId);
            var interview = transactionManagerProvider.GetTransactionManager()
                .ExecuteInQueryTransaction(
                    () => interviewSummaryStorage.GetById(interviewGuid));

            if (interview == null)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound, Enumerator.Native.Resources.WebInterview.Error_NotFound);

            if (!AllowedInterviewStatuses.Contains(interview.Status))
                throw new InterviewAccessException(InterviewAccessExceptionReason.NoActionsNeeded, Enumerator.Native.Resources.WebInterview.Error_NoActionsNeeded);

            if (this.authorizedUser.IsInterviewer)
            {
                if (interview.ResponsibleId == this.authorizedUser.Id)
                    return;
                else
                {
                    throw new InterviewAccessException(InterviewAccessExceptionReason.Forbidden,
                        Enumerator.Native.Resources.WebInterview.Error_Forbidden);
                }
            }

            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion);

            WebInterviewConfig webInterviewConfig = plainTransactionManagerProvider
                .GetPlainTransactionManager()
                    .ExecuteInPlainTransaction(
                        () => webInterviewConfigProvider.Get( questionnaireIdentity));

            //interview is not public available and logged in user is not current interview responsible
            if (!webInterviewConfig.Started && interview.Status == InterviewStatus.InterviewerAssigned && this.authorizedUser.IsAuthenticated)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.UserNotAuthorised,
                    Enumerator.Native.Resources.WebInterview.Error_UserNotAuthorised);
            }

            if (!webInterviewConfig.Started || !AnonymousUserAllowedStatuses.Contains(interview.Status))
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }
        }
    }
}