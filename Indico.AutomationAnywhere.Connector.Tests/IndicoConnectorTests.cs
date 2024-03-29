using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using IndicoV2.Extensions.Jobs;
using IndicoV2.Reviews;
using IndicoV2.Extensions.SubmissionResult;
using IndicoV2.Submissions;
using IndicoV2.Submissions.Models;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SubmissionFilterV2 = IndicoV2.Submissions.Models.SubmissionFilter;

namespace Indico.AutomationAnywhere.Connector.Tests
{
    public class IndicoConnectorTests
    {
        private IIndicoConnector _connector;
        private Mock<ISubmissionsClient> _submissionsClientMock;
        private Mock<ISubmissionResultAwaiter> _submissionResultAwaiterMock;
        private Mock<IReviewsClient> _reviewsClientMock;
        private Mock<IJobAwaiter> _jobAwaiterMock;

        [SetUp]
        public void Setup()
        {
            _submissionsClientMock = new Mock<ISubmissionsClient>();
            _submissionResultAwaiterMock = new Mock<ISubmissionResultAwaiter>();
            _reviewsClientMock = new Mock<IReviewsClient>();
            _jobAwaiterMock = new Mock<IJobAwaiter>();
            _connector = new IndicoConnector(_submissionsClientMock.Object, _submissionResultAwaiterMock.Object, _reviewsClientMock.Object, _jobAwaiterMock.Object);
        }

        [Test]
        public void WorkflowSubmission_ShouldReturnIds_WhenPostingFilePaths()
        {
            //Arrange
            var sources = new List<string> { "test", "test 2" };
            var submissionIdsToReturn = new List<int> { 1, 2 };
            int workflowId = 3;

            _submissionsClientMock
                .Setup(s => s.CreateAsync(workflowId, sources, default))
                .ReturnsAsync(submissionIdsToReturn);

            //Act
            var result = _connector.WorkflowSubmission(sources.ToArray(), null, workflowId);

            //Assert
            _submissionsClientMock.Verify(s => s.CreateAsync(workflowId, sources, default), Times.Once);

            result.Should().HaveCount(submissionIdsToReturn.Count);
            result.Should().BeEquivalentTo(submissionIdsToReturn);
        }

        [Test]
        public void WorkflowSubmission_ShouldReturnIds_WhenPostingUris()
        {
            //Arrange
            var sources = new List<string> { @"https://test.test", @"https://test2.test" };
            var uris = sources.Select(s => new Uri(s)).ToArray();

            var submissionIdsToReturn = new List<int> { 1, 2 };
            int workflowId = 3;

            _submissionsClientMock
                .Setup(s => s.CreateAsync(workflowId, uris, default))
                .ReturnsAsync(submissionIdsToReturn);

            //Act
            var result = _connector.WorkflowSubmission(null, sources.ToArray(), workflowId);

            //Assert
            _submissionsClientMock.Verify(s => s.CreateAsync(workflowId, uris, default), Times.Once);

            result.Should().HaveCount(submissionIdsToReturn.Count);
            result.Should().BeEquivalentTo(submissionIdsToReturn);
        }

        [TestCase(null, null)]
        [TestCase(new string[] { }, new string[] { })]
        [TestCase(new string[] { }, null)]
        [TestCase(null, new string[] { })]
        public void WorkflowFileSubmission_ShouldThrowArgumentNull_WhenInvalidData(string[] filePaths, string[] uris)
        {
            //Arrange
            int workflowId = 3;

            //Act
            Action act = () => _connector.WorkflowSubmission(filePaths, uris, workflowId);

            //Assert
            act.Should().Throw<ArgumentException>();
        }

        [TestCase("test", "PROCESSING", "True")]
        [TestCase("test2", "FAILED", "true")]
        [TestCase("test3", "COMPLETE", "False")]
        [TestCase("test4", "PENDING_ADMIN_REVIEW", "false")]
        [TestCase("test4", "PENDING_REVIEW", "TRUE")]
        [TestCase("test5", "PENDING_AUTO_REVIEW", "FALSE")]
        public void ListSubmissions_ShouldBuildProperFilterObject(string inputFileName, string status, string retrieved)
        {
            //Arrange
            var statusParsed = Enum.Parse<SubmissionStatus>(status);
            var retrievedParsed = bool.Parse(retrieved);
            var limit = 1000;

            _submissionsClientMock.Setup(s =>
                s.ListAsync(null, null, It.IsAny<SubmissionFilterV2>(), limit, default))
                    .ReturnsAsync(new List<ISubmission>());

            //Act
            _connector.ListSubmissions(null, null, inputFileName, status, retrieved, limit);

            //Assert
            _submissionsClientMock.Verify(s => s.ListAsync(null, null, It.Is<SubmissionFilterV2>
                (sf =>
                    sf.InputFilename == inputFileName &&
                    sf.Status == statusParsed &&
                    sf.Retrieved == retrievedParsed),
            limit, default), Times.Once);
        }

        [Test]
        public void ListSubmissions_ShouldThrowArgumentException_WhenWrongStatusValueProvided()
        {
            //Act
            Action act = () => _connector.ListSubmissions(null, null, null, "WRONG_VALUE", null);

            //Assert
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void ListSubmissions_ShouldThrowArgumentException_WhenWrongRetrievedValueProvided()
        {
            //Act
            Action act = () => _connector.ListSubmissions(null, null, null, null, "WRONG_VALUE");

            //Assert
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void SubmissionResult_ShouldGetSubmission()
        {
            //Arrange
            var definition = new { submissionId = 0 };

            const int submissionId = 1;
            const int checkInterval = 1000;
            
            _submissionResultAwaiterMock.Setup(cli => cli.WaitReady(
                    submissionId,
                    It.Is<TimeSpan>(ts => (int)ts.TotalMilliseconds == checkInterval),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(JObject.Parse($"{{\"submissionId\": {submissionId} }}"));

            //Act
            var submissionResult = _connector.SubmissionResult(submissionId, null);
            var deserializedResult = JsonConvert.DeserializeAnonymousType(submissionResult, definition);

            //Assert
            deserializedResult.submissionId.Should().Be(submissionId);
        }

        [Theory]
        public void SubmissionResult_ShouldProperlyParseSubmissionStatus(SubmissionStatus status)
        {
            //Arrange
            var serializedStatus = status.ToString();

            _submissionResultAwaiterMock.Setup(cli => cli.WaitReady(
                    It.IsAny<int>(),
                    status,
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(JObject.Parse($"{{\"submissionId\": {0} }}"));

            //Act
            var submissionResult = _connector.SubmissionResult(0, serializedStatus);

            //Assert
            _submissionResultAwaiterMock.Verify(s => s.WaitReady(
                It.IsAny<int>(), 
                It.Is<SubmissionStatus>(ss => ss == status), 
                It.IsAny<TimeSpan>(), 
                It.IsAny<CancellationToken>()), 
                    Times.Once);
        }

        [Test]
        public void SubmissionResult_ShouldThrowArgumentException_WhenAwaitStatusWrong()
        {
            //Act
            Action act = () => _connector.SubmissionResult(default, "INVALID_VALUE");

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("Wrong checkStatus value. Please pass one of valid values for Submission Status.");
        }

        [Theory]
        public void SubmitReview_ShouldCallReviewsClient(bool rejected, bool? forceComplete)
        {
            // Arrange
            var submissionId = 1;
            var changes = JObject.FromObject(new { name = "test", id = 12, boolCol = true });
            var changesJson = changes.ToString();
            const int idValue = 2;

            _jobAwaiterMock.Setup(cli =>
                    cli.WaitReadyAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JToken.FromObject(new { id = idValue }));

            // Act
            var result = forceComplete.HasValue
                ? _connector.SubmitReview(submissionId, changesJson, rejected, forceComplete.Value)
                : _connector.SubmitReview(submissionId, changesJson, rejected);

            var deserializedResult = JObject.Parse(result);

            // Assert
            _reviewsClientMock.Verify(cli => cli.SubmitReviewAsync(
                submissionId,
                It.Is<JObject>(actualChanges => JToken.DeepEquals(actualChanges, changes)),
                rejected,
                forceComplete,
                It.IsAny<CancellationToken>()),
                Times.Once);
            result.Should().NotBeNull();
            deserializedResult.Value<int>("id").Should().Be(idValue);
        }
    }
}