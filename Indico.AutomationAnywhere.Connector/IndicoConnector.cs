using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndicoV2;
using IndicoV2.Submissions;
using IndicoV2.Submissions.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        public string ListSubmissions(int[] submissionIds, int[] workflowIds, string inputFileName, string status, string retrieved, int limit = 1000)
        {
            SubmissionStatus? parsedStatus = null;
            bool? parsedRetrieved = null;

            if (string.IsNullOrWhiteSpace(inputFileName))
            {
                inputFileName = null;
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (!Enum.TryParse(status, out SubmissionStatus statusValue))
                {
                    throw new ArgumentException("Wrong status value provided. Please provide one of the valid submission statuses.");
                }
                else
                {
                    parsedStatus = statusValue;
                }
            }

            if (!string.IsNullOrEmpty(retrieved))
            {
                if (!bool.TryParse(retrieved, out bool retrievedValue))
                {
                    throw new ArgumentException("Wrong retreived value provided. Please provide \"True\" or \"False\" as a value.");
                }
                else
                {
                    parsedRetrieved = retrievedValue;
                }
            }

            var submissionFilter = new SubmissionFilter
            {
                InputFilename = inputFileName,
                Status = parsedStatus,
                Retrieved = parsedRetrieved
            };

            var submissions = Task.Run(async () => await _submissionsClient.ListAsync(submissionIds, workflowIds, submissionFilter, limit)).GetAwaiter().GetResult();

            return JsonConvert.SerializeObject(submissions, new StringEnumConverter());
        }

    }
}
