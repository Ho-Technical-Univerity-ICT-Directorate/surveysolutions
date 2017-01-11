﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Headquarters.API.WebInterview
{
    [HubName(@"interview")]
    public partial class WebInterview : Hub
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly ICommandService commandService;
        private readonly IUserViewFactory usersRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IMapper autoMapper;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        
        private string CallerInterviewId => this.Clients.Caller.interviewId;

        private IStatefulInterview GetCallerInterview() => this.statefulInterviewRepository.Get(this.CallerInterviewId);

        private IQuestionnaire GetCallerQuestionnaire()
            => this.questionnaireRepository.GetQuestionnaire(this.GetCallerInterview().QuestionnaireIdentity,
                this.GetCallerInterview().Language);

        public WebInterview(
            IStatefulInterviewRepository statefulInterviewRepository, 
            ICommandService commandService,
            IUserViewFactory usersRepository,
            ILiteEventRegistry eventRegistry,
            IMapper autoMapper,
            IQuestionnaireStorage questionnaireRepository,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory, 
            IWebInterviewConfigProvider webInterviewConfigProvider)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.commandService = commandService;
            this.usersRepository = usersRepository;
            this.eventRegistry = eventRegistry;
            this.autoMapper = autoMapper;
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
        }

        public object QuestionnaireDetails(string questionnaireId)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            return new
            {
                questionnaireBrowseItem.Title
            };
        }

        public string CreateInterview(string questionnaireId)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);

            var webInterviewConfig = this.webInterviewConfigProvider.Get(questionnaireIdentity);
            if (!webInterviewConfig.Started)
            {
                throw new InvalidOperationException(@"Web interview is not started for this questionnaire");
            }
            var responsibleId = webInterviewConfig.ResponsibleId;
            var interviewer = this.usersRepository.Load(new UserViewInputModel(publicKey: responsibleId));

            var interviewId = Guid.NewGuid();
            var createInterviewOnClientCommand = new CreateInterviewOnClientCommand(interviewId,
                interviewer.PublicKey, questionnaireIdentity, DateTime.UtcNow,
                interviewer.Supervisor.Id);

            this.commandService.Execute(createInterviewOnClientCommand);
            return interviewId.FormatGuid();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // statefull interview can be removed from cache here
            this.eventRegistry.Unsubscribe(this);
            return base.OnDisconnected(stopCalled);
        }
    }
}