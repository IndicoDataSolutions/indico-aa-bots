using System.Collections.Generic;
using IndicoV2.Submissions.Models;

namespace Indico.AutomationAnywhere.Connector
{
    /// <summary>
    /// <c><see cref="IIndicoConnector"/></c> defines all AutomationAnywhere operations used by bots.
    /// </summary>
    public interface IIndicoConnector
    {
        /// <summary>
        /// Method initializes the Indico client.
        /// </summary>
        /// <param name="token">Indico API token.</param>
        /// <param name="uri">indico API url.</param>
        void Init(string token, string uri);

        /// <summary>
        /// Method submits files to the Indico app.
        /// </summary>
        /// <param name="filepaths"><c><see cref="IEnumerable{T}"/></c> containing paths of the files to submit.</param>
        /// <param name="uris"><c><see cref="IEnumerable{T}"/></c> containing uris to the files to submit.</param>
        /// <param name="workflowId"><c><see cref="int">Id</see></c> of the workflow to submit data to.</param>
        /// <returns>New submissions <c><see cref="int">ids</see></c>.</returns>
        int[] WorkflowSubmission(string[] filepaths, string[] uris, int workflowId);

        /// <summary>
        /// Method lists submissions.
        /// </summary>
        /// <param name="submissionIds">Array containing ids of the submissions to list.</param>
        /// <param name="workflowIds">Array containing ids of workflows to include submissions from.</param>
        /// <param name="inputFileName">File name used to filter result.</param>
        /// <param name="status">Submission status used to filter result.</param>
        /// <param name="retrieved">If submission retrieved, used to filter result.</param>
        /// <param name="limit">Submission count limit. Default value is 1000.</param>
        /// <returns>Submissions list in form of JSON.</returns>
        string ListSubmissions(int[] submissionIds, int[] workflowIds, string inputFileName, string status, string retrieved, int limit = 1000);

        /// <summary>
        /// Method submits submission review.
        /// </summary>
        /// <param name="submissionId">Id of the submission to submit review for.</param>
        /// <param name="changes">JSON changes to make to raw predictions.</param>
        /// <param name="rejected">Reject the predictions and place the submission in the review queue. Must be True if <c>changes</c> not provided.</param>
        /// <returns>Review result.</returns>
        string SubmitReview(int submissionId, string changes, bool rejected);

        /// <summary>
        /// Method submits submission review.
        /// </summary>
        /// <param name="submissionId">Id of the submission to submit review for.</param>
        /// <param name="changes">JSON changes to make to raw predictions.</param>
        /// <param name="rejected">Reject the predictions and place the submission in the review queue. Must be <c>true</c> if <c>changes</c> not provided.</param>
        /// <param name="forceComplete">Have this submission bypass the Review queue (or exceptions queue if <c>Rejected</c> is <c>true</c>) and mark as Complete. [NOT RECOMMENDED]</param>
        /// <returns>Review result.</returns>
        string SubmitReview(int submissionId, string changes, bool rejected, bool forceComplete);
    }
}
