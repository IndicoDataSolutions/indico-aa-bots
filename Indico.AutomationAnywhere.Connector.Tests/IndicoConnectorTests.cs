using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using IndicoV2.Submissions;
using IndicoV2.Submissions.Models;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SubmissionFilterV2 = IndicoV2.Submissions.Models.SubmissionFilter;

namespace Indico.AutomationAnywhere.Connector.Tests
{
    public class IndicoConnectorTests
    {
        private IIndicoConnector _connector;
        private Mock<ISubmissionsClient> _submissionsClientMock;

        [SetUp]
        public void Setup()
        {
            _submissionsClientMock = new Mock<ISubmissionsClient>();
            _connector = new IndicoConnector(_submissionsClientMock.Object);
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
    }
}