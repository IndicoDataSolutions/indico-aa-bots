using System.Linq;
using System.Threading.Tasks;
using IndicoV2.Submissions;
using IndicoV2.Submissions.Models;

namespace Indico.AutomationAnywhere.Connector.IntegrationTests.Utils.DataHelpers.Submissions
{
    public class SubmissionHelper
    {
        private readonly ISubmissionsClient _submissions;

        public SubmissionHelper(ISubmissionsClient submissions) => _submissions = submissions;

        public async Task<ISubmission> GetAnyAsync() => (await _submissions.ListAsync(null, null, null, 1)).Single();
    }
}
