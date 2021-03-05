using System;
using System.Linq;
using System.Threading.Tasks;
using IndicoV2.Extensions.SubmissionResult;
using IndicoV2.IntegrationTests.Utils.DataHelpers.Workflows;
using IndicoV2.Submissions;
using IndicoV2.Submissions.Models;
using Newtonsoft.Json.Linq;

namespace Indico.AutomationAnywhere.Connector.IntegrationTests.Utils.DataHelpers.Submissions
{
    public class SubmissionHelper
    {
        private readonly ISubmissionsClient _submissions;
        private readonly ISubmissionResultAwaiter _submissionResultAwaiter;
        private readonly WorkflowHelper _workflows;

        public SubmissionHelper(ISubmissionsClient submissions, ISubmissionResultAwaiter submissionResultAwaiter, WorkflowHelper workflows)
        {
            _submissions = submissions;
            _submissionResultAwaiter = submissionResultAwaiter;
            _workflows = workflows;
        }

        public async Task<ISubmission> GetAny() => (await _submissions.ListAsync(null, null, null, 1)).Single();

        public async Task<int> GetNewId()
        {
            var submissionIds = 
                await _submissions.CreateAsync(
                    (await _workflows.GetAny()).Id, 
                    new Uri[] { new Uri("https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf") });

            return submissionIds.First();
        }

        public async Task<JObject> GetNewResult() => await GetResult(await GetNewId());

        public async Task<JObject> GetResult(int submissionId) => await _submissionResultAwaiter.WaitReady(submissionId, TimeSpan.FromMilliseconds(200));
    }
}
