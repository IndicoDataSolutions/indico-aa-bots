using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Indico.AutomationAnywhere.Connector.IntegrationTests.IndicoConnectorTests
{
    public class WorkflowSubmissionTests : IntegrationTestsBase
    {
        [Test]
        public async Task WorkflowSubmission_ShouldReturnId_WhenFilepathSubmitted()
        {
            //Arrange
            var sources = new [] { _dataHelper.Files().GetSampleFilePath() };
            int workflowId = (await _dataHelper.Workflows().GetAnyWorkflow()).Id;

            //Act
            var result = _connector.WorkflowSubmission(sources, null, workflowId);

            //Assert
            var submissionId = result.Single();
            submissionId.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task WorkflowSubmission_ShouldReturnId_WhenUriSubmitted()
        {
            //Arrange
            var sources = new[] { _dataHelper.Files().GetSampleUri() };
            int workflowId = (await _dataHelper.Workflows().GetAnyWorkflow()).Id;

            //Act
            var result = _connector.WorkflowSubmission(null, sources, workflowId);

            //Assert
            var submissionId = result.Single();
            submissionId.Should().BeGreaterThan(0);
        }
    }
}
