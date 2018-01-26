using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_that_has_substitution_reference_on_TextList_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionWithSubstitutionReferenceTomultiAnswerQuestionId = Guid.Parse("10000000000000000000000000000000");
            multiAnswerQuestionId = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument(
                Create.TextListQuestion(
                    multiAnswerQuestionId,
                    variable: unsupported
                ),
                Create.SingleQuestion
                (
                    questionWithSubstitutionReferenceTomultiAnswerQuestionId,
                    variable: "var",
                    title: string.Format("hello %{0}%!", unsupported),
                    options: new List<Answer> { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
                ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0018 () =>
            verificationMessages.Single().Code.ShouldEqual("WB0018");

        [NUnit.Framework.Test] public void should_return_message_with_2_references () =>
            verificationMessages.Single().References.Count().ShouldEqual(2);

        [NUnit.Framework.Test] public void should_return_firts_message_reference_with_type_Question () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_firts_message_reference_with_id_of_questionWithSubstitutionReferenceTomultiAnswerQuestionId () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(questionWithSubstitutionReferenceTomultiAnswerQuestionId);

        [NUnit.Framework.Test] public void should_return_last_message_reference_with_type_Question () =>
            verificationMessages.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_last_message_reference_with_id_of_questionSubstitutionReferencerOfNotSupportedTypeId () =>
            verificationMessages.Single().References.Last().Id.ShouldEqual(multiAnswerQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionReferenceTomultiAnswerQuestionId;
        private static Guid multiAnswerQuestionId;
        private const string unsupported = "multiAnswerVariable";
    }
}