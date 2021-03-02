using Indico.AutomationAnywhere.Connector.IntegrationTests.Utils.DataHelpers.Files;
using Indico.AutomationAnywhere.Connector.IntegrationTests.Utils.DataHelpers.Submissions;
using IndicoV2.IntegrationTests.Utils.DataHelpers.Workflows;
using Unity;

namespace IndicoV2.IntegrationTests.Utils.DataHelpers
{
    public class TestDataHelper
    {
        private readonly IUnityContainer _container;

        public TestDataHelper(IUnityContainer container) => _container = container;

        public WorkflowHelper Workflows() => _container.Resolve<WorkflowHelper>();

        public SubmissionHelper Submissions() => _container.Resolve<SubmissionHelper>();

        public FileHelper Files() => _container.Resolve<FileHelper>();

    }
}
