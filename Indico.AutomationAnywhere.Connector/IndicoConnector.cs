using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IndicoV2;
using IndicoV2.Extensions.Jobs;
using IndicoV2.Reviews;
using IndicoV2.Submissions;
using IndicoV2.Submissions.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Indico.AutomationAnywhere.Connector
{
    public class IndicoConnector : IIndicoConnector
    {
        private ISubmissionsClient _submissionsClient;
        private IReviewsClient _reviewsClient;
        private IJobAwaiter _jobAwaiter;

        private TimeSpan _checkInterval = TimeSpan.FromSeconds(1);
        private TimeSpan _timeout = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Constructor for AutomationAnywhere
        /// </summary>
        public IndicoConnector()
        {

        }

        public IndicoConnector(ISubmissionsClient submissionsClient, IReviewsClient reviewsClient, IJobAwaiter jobAwaiter)
        {
            _submissionsClient = submissionsClient;
            _reviewsClient = reviewsClient;
            _jobAwaiter = jobAwaiter;
        }

        public void Init(string token, string uri)
        {
            var client = new IndicoV2.IndicoClient(token, new Uri(uri));
            _submissionsClient = client.Submissions();
            _reviewsClient = client.Reviews();
            _jobAwaiter = client.JobAwaiter();
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

        public string SubmitReview(int submissionId, string changes, bool rejected) => SubmitReview(submissionId, changes, rejected, null);

        public string SubmitReview(int submissionId, string changes, bool rejected, bool forceComplete) => SubmitReview(submissionId, changes, rejected, (bool?)forceComplete);

        private string SubmitReview(int submissionId, string changes, bool rejected, bool? forceComplete)
        {
            JObject parsedChanges = null;
            if (!string.IsNullOrEmpty(changes))
            {
                parsedChanges = JObject.Parse(changes);
            }

            var jobResult = Task.Run(() => SubmitReviewAsync(submissionId, parsedChanges, rejected, forceComplete))
                .GetAwaiter()
                .GetResult();

            return jobResult.ToString();
        }

        private async Task<JObject> SubmitReviewAsync(int submissionId, JObject changes, bool rejected, bool? forceComplete)
        {
            using (var tokenSource = new CancellationTokenSource(_timeout))
            {
                var jobId = await _reviewsClient.SubmitReviewAsync(submissionId, changes, rejected, forceComplete, tokenSource.Token);
                var jobResult = (JObject)await _jobAwaiter.WaitReadyAsync(jobId, _checkInterval, tokenSource.Token);

                return jobResult;
            }
        }
    }
}
