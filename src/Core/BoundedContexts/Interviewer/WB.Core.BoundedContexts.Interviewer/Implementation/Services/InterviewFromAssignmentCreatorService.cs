using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewFromAssignmentCreatorService : IInterviewFromAssignmentCreatorService
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewerPrincipal interviewerPrincipal;
        private readonly IMvxMessenger messenger;
        private readonly ICommandService commandService;
        private readonly IInterviewAnswerSerializer answerSerializer;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;
        private readonly ILastCreatedInterviewStorage lastCreatedInterviewStorage;

        public InterviewFromAssignmentCreatorService(IMvxMessenger messenger,
            ICommandService commandService,
            IInterviewerPrincipal interviewerPrincipal,
            IViewModelNavigationService viewModelNavigationService,
            IInterviewAnswerSerializer answerSerializer,
            IAssignmentDocumentsStorage assignmentsRepository,
            IInterviewUniqueKeyGenerator keyGenerator,
            ILastCreatedInterviewStorage lastCreatedInterviewStorage)
        {
            this.messenger = messenger;
            this.commandService = commandService;
            this.interviewerPrincipal = interviewerPrincipal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.answerSerializer = answerSerializer;
            this.assignmentsRepository = assignmentsRepository;
            this.keyGenerator = keyGenerator;
            this.lastCreatedInterviewStorage = lastCreatedInterviewStorage;
        }

        public async Task CreateInterviewAsync(int assignmentId)
        {
            try
            {
                this.messenger.Publish(new StartingLongOperationMessage(this));
                var interviewId = Guid.NewGuid();
                var interviewerIdentity = this.interviewerPrincipal.CurrentUserIdentity;
                var assignment = this.assignmentsRepository.GetById(assignmentId);
                this.assignmentsRepository.FetchPreloadedData(assignment);
                var questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);

                List<InterviewAnswer> answers = this.GetAnswers(assignment.Answers);

                ICommand createInterviewCommand = new CreateInterview(interviewId,
                    interviewerIdentity.UserId,
                    new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version),
                    answers,
                    DateTime.UtcNow,
                    interviewerIdentity.SupervisorId,
                    interviewerIdentity.UserId,
                    keyGenerator.Get(),
                    assignment.Id
                );

                await this.commandService.ExecuteAsync(createInterviewCommand);
                assignment.CreatedInterviewsCount = (assignment.CreatedInterviewsCount ?? 0) + 1;
                assignmentsRepository.Store(assignment);
                var formatGuid = interviewId.FormatGuid();
                this.lastCreatedInterviewStorage.Store(formatGuid);
                this.viewModelNavigationService.NavigateToPrefilledQuestions(formatGuid);
            }
            catch (InterviewException e)
            {
                // This code is going to be removed after KP-9461. And according to research in KP-9513 we should reduce amount of dependencies in constructor

                var userInteractionService = Mvx.Resolve<IUserInteractionService>();
                Mvx.Resolve<ILoggerProvider>().GetFor<AssignmentDashboardItemViewModel>().Error(e.Message, e);
                await userInteractionService.AlertAsync(string.Format(InterviewerUIResources.FailedToCreateInterview, e.Message), UIResources.Error);
            }
            finally
            {
                this.messenger.Publish(new StopingLongOperationMessage(this));
            }
        }

        private List<InterviewAnswer> GetAnswers(List<AssignmentDocument.AssignmentAnswer> identifyingAnswers)
        {
            var elements = identifyingAnswers
                .Select(ia => new InterviewAnswer
                {
                    Identity = ia.Identity,
                    Answer = this.answerSerializer.Deserialize<AbstractAnswer>(ia.SerializedAnswer)
                })
                .Where(x => x.Answer != null)
                .ToList();

            return elements;
        }
    }
}