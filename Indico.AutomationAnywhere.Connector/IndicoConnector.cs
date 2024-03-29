﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IndicoV2;
using IndicoV2.Extensions.Jobs;
using IndicoV2.Reviews;
using IndicoV2.Extensions.SubmissionResult;
using IndicoV2.Submissions;
using IndicoV2.Submissions.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Indico.AutomationAnywhere.Connector
{
    public class IndicoConnector : IIndicoConnector
    {
        private ISubmissionsClient _submissionsClient;
        private ISubmissionResultAwaiter _submissionResultAwaiter;
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

        public IndicoConnector(ISubmissionsClient submissionsClient, ISubmissionResultAwaiter submissionResultAwaiter, IReviewsClient reviewsClient, IJobAwaiter jobAwaiter)
        {
            _submissionsClient = submissionsClient;
            _submissionResultAwaiter = submissionResultAwaiter;
            _reviewsClient = reviewsClient;
            _jobAwaiter = jobAwaiter;
        }

        public void Init(string token, string uri)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("Provide token and host parameters.");
            }

            if (!uri.StartsWith("http"))
            {
                throw new ArgumentException("Please provide valid host url that starts with http or https");
            }

            var client = new IndicoV2.IndicoClient(token, new Uri(uri));
            _submissionsClient = client.Submissions();
            _submissionResultAwaiter = client.GetSubmissionResultAwaiter();
            _reviewsClient = client.Reviews();
            _jobAwaiter = client.JobAwaiter();
        }

        /// <summary>
        /// Init method if parameters are passed in as a dictionary
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Init(Dictionary<string,object> input)
        {
            if (input.ContainsKey("uri") && input.ContainsKey("token"))
            {
                var token = (string)input["token"];
                var uri = (string)input["uri"];
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(uri))
                {
                    throw new ArgumentException("Provide token and host parameters.");
                }

                if (!uri.StartsWith("http"))
                {
                    throw new ArgumentException("Please provide valid host url that starts with http or https");
                }

                var client = new IndicoV2.IndicoClient(token, new Uri(uri));
                _submissionsClient = client.Submissions();
                _submissionResultAwaiter = client.GetSubmissionResultAwaiter();
                _reviewsClient = client.Reviews();
                _jobAwaiter = client.JobAwaiter();
            }

            else
            {
                throw new ArgumentException("Provide token and host parameters.");
            }
            
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

            try
            {
                if (filepathsProvided)
                {
                    return Task.Run(async () => await _submissionsClient.CreateAsync(workflowId, filepaths)).GetAwaiter().GetResult().ToArray();
                }

                return Task.Run(async () => await _submissionsClient.CreateAsync(workflowId, uris.Select(u => new Uri(u)))).GetAwaiter().GetResult().ToArray();
            }
            catch (System.Exception ex)
            {
                //AA doesn't understand custom exception types and the only important thing is message, so method catches all exceptions and throw new generic one with the correct message.
                throw new System.Exception(ex.Message);
            }
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

            try
            {
                var submissions = Task.Run(async () => await _submissionsClient.ListAsync(submissionIds, workflowIds, submissionFilter, limit)).GetAwaiter().GetResult();

                return JsonConvert.SerializeObject(submissions, new StringEnumConverter());

            }
            catch (System.Exception ex)
            {
                //AA doesn't understand custom exception types and the only important thing is message, so method catches all exceptions and throw new generic one with the correct message.
                throw new System.Exception(ex.Message);
            }
        }

        public string SubmissionResult(int submissionId, string checkStatus)
        {
            SubmissionStatus? awaitStatus = null;

            if (!string.IsNullOrWhiteSpace(checkStatus))
            {
                if (Enum.TryParse(checkStatus, out SubmissionStatus parsedStatus))
                {
                    awaitStatus = parsedStatus;
                }
                else
                {
                    throw new ArgumentException("Wrong checkStatus value. Please pass one of valid values for Submission Status.");
                }
            }

            using (var timeoutTokenSource = new CancellationTokenSource(_timeout))
            {
                var cancellationToken = timeoutTokenSource.Token;
                
                try
                {
                    var getResult = awaitStatus == null
                            ? Task.Run(async () => await _submissionResultAwaiter.WaitReady(submissionId, _checkInterval, cancellationToken))
                            : Task.Run(async () => await _submissionResultAwaiter.WaitReady(submissionId, awaitStatus.Value, _checkInterval, cancellationToken));

                    var result = getResult
                        .GetAwaiter()
                        .GetResult();

                    return result.ToString();
                }
                catch (System.Exception ex)
                {
                    //AA doesn't understand custom exception types and the only important thing is message, so method catches all exceptions and throw new generic one with the correct message.
                    throw new System.Exception(ex.Message);
                }
            }
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

            try
            {
                var jobResult = Task.Run(() => SubmitReviewAsync(submissionId, parsedChanges, rejected, forceComplete))
                        .GetAwaiter()
                        .GetResult();

                return jobResult.ToString();
            }
            catch (System.Exception ex)
            {
                //AA doesn't understand custom exception types and the only important thing is message, so method catches all exceptions and throw new generic one with the correct message.
                throw new System.Exception(ex.Message);
            }
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
