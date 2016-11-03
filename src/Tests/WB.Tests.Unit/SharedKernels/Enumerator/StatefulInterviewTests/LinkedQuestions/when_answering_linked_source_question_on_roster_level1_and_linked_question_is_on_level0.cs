using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.LinkedQuestions
{
    internal class when_answering_linked_source_question_on_roster_level1_and_linked_question_is_on_level0 : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
           {
                Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId),
                Create.Entity.Roster(rosterId: roster1Id, rosterSizeQuestionId: rosterSizeQuestionId, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: sourceOfLinkedQuestionId),
                }),
                Create.Entity.SingleQuestion(id: linkedSingleQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId),
                Create.Entity.MultyOptionsQuestion(id: linkedMultiQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId)
           });
            var plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, 0);

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: plainQuestionnaire);
            interview.AnswerNumericIntegerQuestion(interviewerId, rosterSizeQuestionId, RosterVector.Empty, DateTime.UtcNow, 2);
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(0), DateTime.UtcNow, "answer 0");
        };

        Because of = () =>
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, "answer 1");

        It should_linked_single_question_has_2_option = () =>
        {
            var answersToBeOptions = interview
                .FindAnswersOfReferencedQuestionForLinkedQuestion(sourceOfLinkedQuestionId, Create.Entity.Identity(linkedSingleQuestionId, RosterVector.Empty))
                .ToList();

            answersToBeOptions.Count.ShouldEqual(2);
            answersToBeOptions.OfType<TextAnswer>().Select(x => x.Answer).ShouldContainOnly("answer 0", "answer 1");
        };

        It should_linked_multi_question_has_2_options = () => {
            var answersToBeOptions = interview
                .FindAnswersOfReferencedQuestionForLinkedQuestion(sourceOfLinkedQuestionId, Create.Entity.Identity(linkedMultiQuestionId, RosterVector.Empty))
                .ToList();

            answersToBeOptions.Count.ShouldEqual(2);
            answersToBeOptions.OfType<TextAnswer>().Select(x => x.Answer).ShouldContainOnly("answer 0", "answer 1");
        };

        static StatefulInterview interview;

        static readonly Guid roster1Id = Guid.Parse("11111111111111111111111111111111");

        static readonly Guid rosterSizeQuestionId = Guid.Parse("44444444444444444444444444444444");

        static readonly Guid linkedSingleQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid linkedMultiQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        static readonly Guid sourceOfLinkedQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static readonly Guid interviewerId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
    }
}