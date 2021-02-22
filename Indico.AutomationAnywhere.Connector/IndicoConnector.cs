using System;
using System.Linq;
using System.Threading.Tasks;
using IndicoV2;
using IndicoV2.Submissions;

namespace Indico.AutomationAnywhere.Connector
{
    public class IndicoConnector : IIndicoConnector
    {
        private ISubmissionsClient _submissionsClient;

        /// <summary>
        /// Constructor for AutomationAnywhere
        /// </summary>
        public IndicoConnector()
        {

        }

        public IndicoConnector(ISubmissionsClient submissionsClient) => _submissionsClient = submissionsClient;

        public void Init(string token, string uri)
        {
            var client = new IndicoV2.IndicoClient(token, new Uri(uri));
            _submissionsClient = client.Submissions();
        }

        public int[] WorkflowSubmission(string[] filepaths, string[] uris, int workflowId)
        {
            if (_submissionsClient == null)
            {
                throw new InvalidOperationException("No Init method was called before.");
            }

            bool filepathsProvided = filepaths != null && filepaths.Any();
            bool urisProvided = uris != null && uris.Any();

            if (!filepathsProvided && !urisProvided)
            {
                throw new ArgumentException("No uris or filepaths provided.");
            }

            if (filepathsProvided && urisProvided)
            {
                throw new ArgumentException("Provided uris and filepaths. Pass only one of the parameters.");
            }

            if (filepathsProvided)
            {
                return Task.Run(async () => await _submissionsClient.CreateAsync(workflowId, filepaths)).GetAwaiter().GetResult().ToArray();
            }

            return Task.Run(async () => await _submissionsClient.CreateAsync(workflowId, uris.Select(u => new Uri(u)))).GetAwaiter().GetResult().ToArray();
        }
    }
}
