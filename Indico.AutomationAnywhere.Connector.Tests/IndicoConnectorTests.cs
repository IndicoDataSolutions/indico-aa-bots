using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using IndicoV2.Submissions;
using Moq;
using NUnit.Framework;

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

    }
}