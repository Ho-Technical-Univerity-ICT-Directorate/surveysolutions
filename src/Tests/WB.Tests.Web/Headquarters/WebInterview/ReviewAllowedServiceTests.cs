﻿using System.Runtime.Caching;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Enumerator.Native.WebInterview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.WebInterview
{
    public class ReviewAllowedServiceTests
    {
        [Test]
        public void should_allow_access_for_team_supervisor()
        {
            var supervisorId = Id.g1;
            var interviewId = Id.gA;

            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.IsSupervisor == true && x.Id == supervisorId);
            var interview = Create.AggregateRoot.StatefulInterview(interviewId, supervisorId: supervisorId);
            var interviewRepository = SetUp.StatefulInterviewRepository(interview);
            var service = SetupService(interviewRepository, authorizedUser);

            Assert.DoesNotThrow(() => service.CheckIfAllowed(interviewId));
        }

        [Test]
        public void should_not_allow_access_for_others_team_supervisor()
        {
            var supervisorId = Id.g1;
            var interviewId = Id.gA;

            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.IsSupervisor == true && x.Id == supervisorId);
            var interview = Create.AggregateRoot.StatefulInterview(interviewId, supervisorId: Id.g2);
            var interviewRepository = SetUp.StatefulInterviewRepository(interview);
            var service = SetupService(interviewRepository, authorizedUser);

            Assert.Throws<InterviewAccessException>(() => service.CheckIfAllowed(interviewId));
        }

        [Test]
        public void should_allow_access_to_hq()
        {
            var interviewId = Id.gA;

            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.IsHeadquarter == true);
            var interview = Create.AggregateRoot.StatefulInterview(interviewId);
            var interviewRepository = SetUp.StatefulInterviewRepository(interview);
            var service = SetupService(interviewRepository, authorizedUser);

            Assert.DoesNotThrow(() => service.CheckIfAllowed(interviewId));
        }

        [Test]
        public void should_allow_access_to_admin()
        {
            var interviewId = Id.gA;

            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.IsAdministrator == true);
            var interview = Create.AggregateRoot.StatefulInterview(interviewId);
            var interviewRepository = SetUp.StatefulInterviewRepository(interview);
            var service = SetupService(interviewRepository, authorizedUser);

            Assert.DoesNotThrow(() => service.CheckIfAllowed(interviewId));
        }

        private IReviewAllowedService SetupService(IStatefulInterviewRepository interviewRepository = null, 
            IAuthorizedUser authorizedUser = null)
        {
            var reviewAllowedService = new ReviewAllowedService(
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>());
            return reviewAllowedService;
        }
    }
}