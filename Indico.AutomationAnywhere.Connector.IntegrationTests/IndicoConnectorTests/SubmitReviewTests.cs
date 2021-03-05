using System;
using System.Threading.Tasks;
using FluentAssertions;
using Indico.Exception;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Indico.AutomationAnywhere.Connector.IntegrationTests.IndicoConnectorTests
{
    public class SubmitReviewTests : IntegrationTestsBase
    {
        [Test]
        public async Task SubmitReview_ShouldReturnResult()
        {
            // Arrange
            var submissionResult = await _dataHelper.Submissions().GetNewResult();
            var submissionId = submissionResult.Value<int>("submission_id");
            var changes = submissionResult.SelectToken("results.document.results").ToString();

            // Act
            var submitReviewResult = _connector.SubmitReview(submissionId, changes, false);
            var deserializedSubmitReviewResult = JObject.Parse(submitReviewResult);

            // Assert
            deserializedSubmitReviewResult.Value<string>("submission_status").Should().Be("pending_review");
            deserializedSubmitReviewResult.Value<bool>("success").Should().Be(true);
        }

        [Test]
        public async Task SubmitReview_ShouldThrowIfNoChangesAndNotRejected()
        {
            // Arrange
            var submissionResult = await _dataHelper.Submissions().GetNewResult();
            var submissionId = submissionResult.Value<int>("submission_id");

            // Act
            Action act = () => _connector.SubmitReview(submissionId, null, false);

            // Assert
            act.Should().Throw<RuntimeException>().WithMessage("Must provide Changes or Reject=true");
        }
    }
}
