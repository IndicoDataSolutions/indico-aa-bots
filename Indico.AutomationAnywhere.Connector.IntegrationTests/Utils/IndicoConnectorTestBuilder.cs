using System;
using IndicoV2;
using IndicoV2.DataSets;
using IndicoV2.Extensions.Jobs;
using IndicoV2.Extensions.SubmissionResult;
using IndicoV2.Submissions;
using IndicoV2.Workflows;
using Unity;
using Unity.Lifetime;
using V1Client = Indico.IndicoClient;
using V2Client = IndicoV2.IndicoClient;

namespace Indico.AutomationAnywhere.Connector.IntegrationTests.Utils
{
    public class IndicoConnectorTestBuilder
    {
        private string BaseUrl => Environment.GetEnvironmentVariable("INDICO_HOST");
        private string ApiToken => Environment.GetEnvironmentVariable("INDICO_TOKEN");

        public IndicoConnector Build()
        {
            var connector = new IndicoConnector();
            connector.Init(ApiToken, BaseUrl);

            return connector;
        }

        public IUnityContainer BuildContainer()
        {
            var container = new UnityContainer();
            container.RegisterFactory<V1Client>(c => new V1Client(new IndicoConfig(
                ApiToken,
                host: new Uri(BaseUrl).Host)));
            container.RegisterFactory<V2Client>(c => new V2Client(ApiToken, new Uri(BaseUrl)), new SingletonLifetimeManager());
            container.RegisterType<IIndicoClient, V2Client>();

            container.RegisterFactory<IDataSetClient>(c => c.Resolve<V2Client>().DataSets());
            container.RegisterFactory<IWorkflowsClient>(c => c.Resolve<V2Client>().Workflows());
            container.RegisterFactory<ISubmissionsClient>(c => c.Resolve<V2Client>().Submissions());
            container.RegisterFactory<ISubmissionResultAwaiter>(c => c.Resolve<V2Client>().GetSubmissionResultAwaiter());
            container.RegisterFactory<IJobAwaiter>(c => c.Resolve<V2Client>().JobAwaiter());

            return container;
        }
    }
}
