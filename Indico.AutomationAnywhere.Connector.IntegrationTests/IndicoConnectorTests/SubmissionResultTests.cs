using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Indico.AutomationAnywhere.Connector.IntegrationTests.IndicoConnectorTests
{
    public class SubmissionResultTests : IntegrationTestsBase
    {
        [Test]
        public async Task SubmissionResult_ShouldReturnJobResult()
        {
            // Arrange
            var submissionId = (await _dataHelper.Submissions().GetAnyAsync()).Id;

            // Act
            var submissionResult = _connector.SubmissionResult(submissionId, null, 200, 5000);
            var deserialized = JObject.Parse(submissionResult);

            // Assert
            submissionResult.Should().NotBeNull();
            deserialized.ContainsKey("submission_id").Should().BeTrue();
            deserialized.ContainsKey("results").Should().BeTrue();
        }
    }
}
