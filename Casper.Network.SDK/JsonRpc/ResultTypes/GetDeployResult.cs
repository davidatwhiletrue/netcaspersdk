using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc.ResultTypes
{
    /// <summary>
    /// Result for "info_get_deploy" RPC response.
    /// </summary>
    public class GetDeployResult : RpcResult
    {
        /// <summary>
        /// The deploy.
        /// </summary>
        [JsonPropertyName("deploy")]
        public Deploy Deploy { get; init; }

        /// <summary>
        /// Execution info, if available.
        /// </summary>
        [JsonPropertyName("execution_info")]
        public ExecutionInfo ExecutionInfo { get; init; }
    }
}
