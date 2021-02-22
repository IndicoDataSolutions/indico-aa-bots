﻿using System.Collections.Generic;

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
    }
}
