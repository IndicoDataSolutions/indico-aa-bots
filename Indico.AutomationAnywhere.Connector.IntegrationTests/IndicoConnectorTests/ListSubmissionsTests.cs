using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Indico.AutomationAnywhere.Connector.IntegrationTests.IndicoConnectorTests
{
    public class ListSubmissionsTests : IntegrationTestsBase
    {
        [Test]
        public async Task ListSubmissions_ShouldReturnProperSubmission_WhenIdProvided()
        {
            //Arrange
            var submissionId = (await _dataHelper.Submissions().GetAny()).Id;
            var definition = new[] { new { Id = 0 } };

            //Act
            var result = _connector.ListSubmissions(new int[] { submissionId }, null, null, null, null);
            var deserializedResult = JsonConvert.DeserializeAnonymousType(result, definition);

            //Assert
            var submission = deserializedResult.Single();
            submission.Id.Should().Equals(submissionId);
        }

        [Test]
        public async Task ListSubmissions_ShouldReturnProperSubmission_WhenWorkflowIdProvided()
        {
            //Arrange
            var workflowId = (await _dataHelper.Workflows().GetAny()).Id;
            var definition = new[] { new { WorkflowId = 0 } };

            //Act
            var result = _connector.ListSubmissions(null, new int[] { workflowId }, null, null, null, 1);
            var deserializedResult = JsonConvert.DeserializeAnonymousType(result, definition);

            //Assert
            var submission = deserializedResult.Single();
            submission.WorkflowId.Should().Equals(workflowId);
        }
    }
}
