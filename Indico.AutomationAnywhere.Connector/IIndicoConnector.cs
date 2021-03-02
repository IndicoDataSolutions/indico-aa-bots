using System.Collections.Generic;

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
        /// Method gets result of the submission.
        /// </summary>
        /// <param name="submissionId">Id of the submission to get.</param>
        /// <param name="checkStatus">Optional parameter that causes wait until submissions is in certain state.</param>
        /// <param name="checkIntervalMilliseconds">Interval in miliseconds of checking submission status.</param>
        /// <param name="timeoutMilliseconds">Time in miliseconds before timeout of the function.</param>
        /// <returns>The result of submission in form of a JSON.</returns>
        string SubmissionResult(int submissionId, string checkStatus, int checkIntervalMilliseconds, int timeoutMilliseconds);
    }
}
