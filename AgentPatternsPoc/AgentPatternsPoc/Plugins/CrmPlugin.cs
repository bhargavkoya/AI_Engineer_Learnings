using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentPatternsPoc.Plugins
{
    public class CrmPlugin
    {
        [KernelFunction("find_affected_customers")]
        [Description("Returns list of customer IDs affected by a given service name")]
        public IEnumerable<string> FindAffectedCustomers(
            [Description("Service or component name")] string serviceName)
        {
            // Stub - real implementation queries CRM
            return ["CUST-001 (Enterprise)", "CUST-047 (Pro)", "CUST-112 (Free)"];
        }
    }
}
