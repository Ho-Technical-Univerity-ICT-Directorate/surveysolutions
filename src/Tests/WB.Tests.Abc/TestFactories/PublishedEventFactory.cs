using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Abc.TestFactories
{
    internal class PublishedEventFactory
    {
        public IPublishedEvent<InterviewApprovedByHQ> InterviewApprovedByHQ(
            Guid? interviewId = null, string userId = null, string comment = null)
            => new InterviewApprovedByHQ(
                ToGuid(userId) ?? Guid.NewGuid(), comment)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewApproved> InterviewApproved(Guid? interviewId = null, string userId = null, string comment = null)
            => new InterviewApproved(ToGuid(userId) ?? Guid.NewGuid(), comment, DateTime.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewCompleted> InterviewCompleted(
            Guid? interviewId = null, string userId = null, string comment = null, Guid? eventId = null,
            DateTime? completeTime = null)
            => new InterviewCompleted(ToGuid(userId) ?? Guid.NewGuid(), completeTime ?? DateTime.Now, comment)
                .ToPublishedEvent(eventSourceId: interviewId, eventId: eventId);

        public IPublishedEvent<InterviewCreated> InterviewCreated(
            Guid? interviewId = null, string userId = null, 
            string questionnaireId = null,
            long questionnaireVersion = 0,
            DateTime? createTime = null,
            DateTime? eventTimeStamp = null)
            => new InterviewCreated(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(questionnaireId) ?? Guid.NewGuid(), questionnaireVersion, null, creationTime: createTime)
                .ToPublishedEvent(eventSourceId: interviewId, eventTimeStamp: eventTimeStamp);

        public IPublishedEvent<InterviewDeleted> InterviewDeleted(string userId = null, string origin = null, Guid? interviewId = null)
            => new InterviewDeleted(ToGuid(userId) ?? Guid.NewGuid())
                .ToPublishedEvent(origin: origin, eventSourceId: interviewId);

        public IPublishedEvent<InterviewerAssigned> InterviewerAssigned(
            Guid? interviewId = null, string userId = null, string interviewerId = "", DateTime? assignTime = null)
            => new InterviewerAssigned(ToGuid(userId) ?? Guid.NewGuid(),
                    interviewerId == null ? (Guid?) null : (ToGuid(interviewerId) ?? Guid.NewGuid()), assignTime ?? DateTime.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewFromPreloadedDataCreated> InterviewFromPreloadedDataCreated(
            Guid? interviewId = null, string userId = null, string questionnaireId = null, long questionnaireVersion = 0)
            => new InterviewFromPreloadedDataCreated(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(questionnaireId) ?? Guid.NewGuid(), questionnaireVersion, null)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewHardDeleted> InterviewHardDeleted(string userId = null, Guid? interviewId = null)
            => Create.Event.InterviewHardDeleted(userId: ToGuid(userId))
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewOnClientCreated> InterviewOnClientCreated(
            Guid? interviewId = null, string userId = null, string questionnaireId = null, long questionnaireVersion = 0)
            => new InterviewOnClientCreated(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(questionnaireId) ?? Guid.NewGuid(), questionnaireVersion, null)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewRejectedByHQ> InterviewRejectedByHQ(Guid? interviewId = null, string userId = null, string comment = null)
            => new InterviewRejectedByHQ(ToGuid(userId) ?? Guid.NewGuid(), comment)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewRejected> InterviewRejected(Guid? interviewId = null, string userId = null, string comment = null)
            => new InterviewRejected(ToGuid(userId) ?? Guid.NewGuid(), comment, DateTime.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewRestarted> InterviewRestarted(Guid? interviewId = null, string userId = null, string comment = null)
            => new InterviewRestarted(ToGuid(userId) ?? Guid.NewGuid(), DateTime.Now, comment)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewRestored> InterviewRestored(Guid? interviewId = null, string userId = null, string origin = null)
            => new InterviewRestored(ToGuid(userId) ?? Guid.NewGuid())
                .ToPublishedEvent(origin: origin, eventSourceId: interviewId);

        public IPublishedEvent<InterviewStatusChanged> InterviewStatusChanged(
            Guid interviewId, InterviewStatus status, string comment = "hello", Guid? eventId = null, InterviewStatus? previousStatus = null)
            => Create.Event.InterviewStatusChanged(status, comment, previousStatus: previousStatus)
                .ToPublishedEvent(eventSourceId: interviewId, eventId: eventId);

        public IPublishedEvent<InterviewStatusChanged> InterviewStatusChanged(
            InterviewStatus status, string comment = null, Guid? interviewId = null)
            => Create.PublishedEvent.InterviewStatusChanged(interviewId ?? Guid.NewGuid(), status, comment: comment);

        public IPublishedEvent<SupervisorAssigned> SupervisorAssigned(Guid? interviewId = null, string userId = null,
            string supervisorId = null, DateTime? assignTime = null)
            => new SupervisorAssigned(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(supervisorId) ?? Guid.NewGuid(), assignTime)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<SynchronizationMetadataApplied> SynchronizationMetadataApplied(string userId = null,
            InterviewStatus status = InterviewStatus.Created, string questionnaireId = null,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta = null, bool createdOnClient = false)
            => new SynchronizationMetadataApplied(
                userId: ToGuid(userId) ?? Guid.NewGuid(),
                status: status,
                questionnaireId: ToGuid(questionnaireId) ?? Guid.NewGuid(),
                questionnaireVersion: 1,
                featuredQuestionsMeta: featuredQuestionsMeta,
                createdOnClient: createdOnClient,
                comments: null,
                rejectedDateTime: null,
                interviewerAssignedDateTime: null)
                .ToPublishedEvent();

        public IPublishedEvent<TextQuestionAnswered> TextQuestionAnswered(Guid? interviewId = null, string userId = null, DateTime? answerTime = null)
            => new TextQuestionAnswered(ToGuid(userId) ?? Guid.NewGuid(), Guid.NewGuid(), new decimal[0], answerTime ?? DateTime.Now, "tttt")
                .ToPublishedEvent();

        private static Guid? ToGuid(string stringGuid)
            => string.IsNullOrEmpty(stringGuid)
                ? null as Guid?
                : Guid.Parse(stringGuid);

        public IPublishedEvent<UnapprovedByHeadquarters> UnapprovedByHeadquarters(
            Guid? interviewId = null, string userId = null, string comment = null)
            => new UnapprovedByHeadquarters(ToGuid(userId) ?? Guid.NewGuid(), comment)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<RosterInstancesRemoved> RosterInstancesRemoved(Guid interviewId, params RosterInstance[] instances)
            => new RosterInstancesRemoved(instances).ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<DateTimeQuestionAnswered> DateTimeQuestionAnswered(Guid? interviewId = null, Guid? userId = null, 
            Guid? questionId = null, DateTime? answer = null)
            => new DateTimeQuestionAnswered(userId ?? Guid.NewGuid(), questionId ?? Guid.NewGuid(), new decimal[0], DateTime.UtcNow, answer ?? DateTime.UtcNow)
                .ToPublishedEvent(eventSourceId: interviewId);
    }
}