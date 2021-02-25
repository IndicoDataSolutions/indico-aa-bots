using Indico.AutomationAnywhere.Connector.IntegrationTests.Utils;
using IndicoV2.IntegrationTests.Utils.DataHelpers;
using NUnit.Framework;
using Unity;

namespace Indico.AutomationAnywhere.Connector.IntegrationTests.IndicoConnectorTests
{
    public abstract class IntegrationTestsBase
    {
        protected IndicoConnector _connector;
        protected TestDataHelper _dataHelper;
        protected IUnityContainer _container;

        [SetUp]
        public void SetUp()
        {
            var indicoConnectorBuilder = new IndicoConnectorTestBuilder();
            _connector = indicoConnectorBuilder.Build();
            _container = indicoConnectorBuilder.BuildContainer();
            _dataHelper = new TestDataHelper(_container);
        }
    }
}
