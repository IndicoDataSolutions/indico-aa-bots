﻿using System.Linq;
using System.Threading.Tasks;
using IndicoV2.DataSets;
using IndicoV2.Workflows;
using IndicoV2.Workflows.Models;

namespace IndicoV2.IntegrationTests.Utils.DataHelpers.Workflows
{
    public class WorkflowHelper
    {
        private readonly IDataSetClient _dataSets;
        private readonly IWorkflowsClient _workflows;

        public WorkflowHelper(IDataSetClient dataSets, IWorkflowsClient workflows)
        {
            _dataSets = dataSets;
            _workflows = workflows;
        }

        public async Task<IWorkflow> GetAny()
        {
            var dataSets = await _dataSets.ListAsync();
            var workflows = await _workflows.ListAsync(dataSets.First().Id);

            return workflows.First();
        }
    }
}
