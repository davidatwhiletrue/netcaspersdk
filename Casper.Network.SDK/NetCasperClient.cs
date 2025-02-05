using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK
{
    /// <summary>
    /// Client to communicate with a Casper node.
    /// </summary>
    public class NetCasperClient : ICasperClient, IDisposable
    {
        private volatile bool _disposed;

        private readonly RpcClient _rpcClient;

        private int _nodeVersion;

        /// <summary>
        /// Create a new instance of the Casper client for a specific node in the network determined
        /// by the node address. Optionally, indicate a logging handler to log the requests and responses
        /// exchanged with tne node.
        /// </summary>
        /// <param name="nodeAddress">URL of the node. Example: 'http://127.0.0.1:7777/rpc'.</param>
        /// <param name="loggingHandler">Optional. An instance of a logging handler to log the requests
        /// and responses exchanged with the network.</param>
        public NetCasperClient(string nodeAddress, RpcLoggingHandler loggingHandler = null)
        {
            _rpcClient = new RpcClient(nodeAddress, loggingHandler);
        }
        
        public NetCasperClient(string nodeAddress, HttpClient httpClient)
        {
            _rpcClient = new RpcClient(nodeAddress, httpClient);
        }

        private Task<RpcResponse<TRpcResult>> SendRpcRequestAsync<TRpcResult>(RpcMethod method)
        {
            return _rpcClient.SendRpcRequestAsync<TRpcResult>(method);
        }

        private async Task<int> GetNodeVersion()
        {
            if (_nodeVersion > 0)
                return _nodeVersion;

            var response = await this.GetNodeStatus();
            var result = response.Parse();
            _nodeVersion = result.BuildVersion.StartsWith("2")
                    ? 2
                    : 1;

            return _nodeVersion;
        }
        
        /// <summary>
        /// Request the state root hash at a given Block.
        /// </summary>
        /// <param name="blockHash">Block hash for which the state root is queried. Null for the most recent.</param>
        /// <returns></returns>
        public async Task<string> GetStateRootHash(string blockHash = null)
        {
            var method = new GetStateRootHash(blockHash);
            var rpcResponse = await SendRpcRequestAsync<GetStateRootHashResult>(method);
            return rpcResponse.Result.GetProperty("state_root_hash").GetString();
        }

        /// <summary>
        /// Request the state root hash at a given Block.
        /// </summary>
        /// <param name="blockHeight">Block height for which the state root is queried.</param>
        /// <returns></returns>
        public async Task<string> GetStateRootHash(ulong blockHeight)
        {
            var method = new GetStateRootHash(blockHeight);
            var rpcResponse = await SendRpcRequestAsync<GetStateRootHashResult>(method);
            return rpcResponse.Result.GetProperty("state_root_hash").GetString();
        }

        /// <summary>
        /// Request the current status of the node. 
        /// </summary>
        public async Task<RpcResponse<GetNodeStatusResult>> GetNodeStatus()
        {
            var method = new GetNodeStatus();
            return await SendRpcRequestAsync<GetNodeStatusResult>(method);
        }

        /// <summary>
        /// Request a list of peers connected to the node.
        /// </summary>
        public async Task<RpcResponse<GetNodePeersResult>> GetNodePeers()
        {
            var method = new GetNodePeers();
            return await SendRpcRequestAsync<GetNodePeersResult>(method);
        }

        /// <summary>
        /// Request the bids and validators at a given block.
        /// </summary>
        /// <param name="blockHash">Block hash for which the auction info is queried. Null for the most recent auction info.</param>
        /// <returns></returns>
        public async Task<RpcResponse<GetAuctionInfoResult>> GetAuctionInfo(string blockHash = null)
        {
            var nodeVersion = await GetNodeVersion();
            RpcMethod method = null;
            
            if (nodeVersion == 1)
                method = new GetAuctionInfo(blockHash);
            else if (blockHash == null)
                method = new GetAuctionInfoV2(blockHash);
            else 
            {
                var getBlockResponse = await GetBlock(blockHash);
                if (getBlockResponse.Parse().Block.Version == 2)
                    method = new GetAuctionInfoV2(blockHash);
                else
                    method = new GetAuctionInfo(blockHash);
            }
            
            return await SendRpcRequestAsync<GetAuctionInfoResult>(method);                
        }

        /// <summary>
        /// Request the bids and validators at a given block. 
        /// </summary>
        /// <param name="blockHeight">Block height for which the auction info is queried.</param>
        public async Task<RpcResponse<GetAuctionInfoResult>> GetAuctionInfo(ulong blockHeight)
        {
            var nodeVersion = await GetNodeVersion();
            RpcMethod method = null;
            
            if (nodeVersion == 1)
                method = new GetAuctionInfo(blockHeight);
            else 
            {
                var getBlockResponse = await GetBlock(blockHeight);
                if (getBlockResponse.Parse().Block.Version == 2)
                    method = new GetAuctionInfoV2(blockHeight);
                else
                    method = new GetAuctionInfo(blockHeight);
            }
            
            return await SendRpcRequestAsync<GetAuctionInfoResult>(method);   
        }

        /// <summary>
        /// Request the information of an Account in the network.
        /// </summary>
        /// <param name="publicKey">The public key of the account.</param>
        /// <param name="blockHash">A block hash for which the information of the account is queried. Null for most recent information.</param>
        public async Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(PublicKey publicKey,
            string blockHash = null)
        {
            var method = new GetAccountInfo(publicKey, blockHash);
            return await SendRpcRequestAsync<GetAccountInfoResult>(method);
        }

        /// <summary>
        /// Request the information of an Account in the network.
        /// </summary>
        /// <param name="accountHash">The account hash of the account.</param>
        /// <param name="blockHash">A block hash for which the information of the account is queried. Null for most recent information.</param>
        public async Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(AccountHashKey accountHash,
            string blockHash = null)
        {
            var method = new GetAccountInfo(accountHash, blockHash);
            return await SendRpcRequestAsync<GetAccountInfoResult>(method);
        }

        /// <summary>
        /// Request the information of an Account in the network.
        /// </summary>
        /// <param name="publicKey">The public key of the account formatted as a string.</param>
        /// <param name="blockHash">A block hash for which the information of the account is queried. Null for most recent information.</param>
        [Obsolete("For Casper node v1.5.5 or newer use the new method signature with PublicKey or AccountHashKey, ", false)]
        public async Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(string publicKey, 
            string blockHash = null)
        {
            var method = new GetAccountInfo(publicKey, blockHash);
            return await SendRpcRequestAsync<GetAccountInfoResult>(method);
        }

        /// <summary>
        /// Request the information of an Account in the network.
        /// </summary>
        /// <param name="publicKey">The public key of the account.</param>
        /// <param name="blockHeight">A block height for which the information of the account is queried.</param>
        public async Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(PublicKey publicKey, ulong blockHeight)
        {
            var method = new GetAccountInfo(publicKey, blockHeight);
            return await SendRpcRequestAsync<GetAccountInfoResult>(method);
        }

        /// <summary>
        /// Request the information of an Account in the network.
        /// </summary>
        /// <param name="accountHash">The account hash of the account.</param>
        /// <param name="blockHeight">A block height for which the information of the account is queried.</param>
        public async Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(AccountHashKey accountHash, ulong blockHeight)
        {
            var method = new GetAccountInfo(accountHash, blockHeight);
            return await SendRpcRequestAsync<GetAccountInfoResult>(method);
        }

        /// <summary>
        /// Request the information of an Account in the network.
        /// </summary>
        /// <param name="publicKey">The public key of the account formatted as an hex-string.</param>
        /// <param name="blockHeight">A block height for which the information of the account is queried.</param>
        [Obsolete("For Casper node v1.5.5 or newer use the new method signature with PublicKey or AccountHashKey, ", false)]
        public async Task<RpcResponse<GetAccountInfoResult>> GetAccountInfo(string publicKey, ulong blockHeight)
        {
            var method = new GetAccountInfo(publicKey, blockHeight);
            return await SendRpcRequestAsync<GetAccountInfoResult>(method);
        }

        /// <summary>
        /// Returns an AddressableEntity or a legacy Accountfrom the network for a Block from the network
        /// </summary>
        /// <param name="entityIdentifier">A PublicKey, an AccoountHashKey, or an AddressableEntityKey</param>
        /// <param name="blockHash">A block hash for which the information of the entity is queried. Null for most recent information.</param>
        public async Task<RpcResponse<GetEntityResult>> GetEntity(IEntityIdentifier entityIdentifier, string blockHash = null)
        {
            var method = new GetEntity(entityIdentifier, blockHash != null ? new BlockIdentifier(blockHash) : null);
            return await SendRpcRequestAsync<GetEntityResult>(method);
        }
        
        /// <summary>
        /// Returns an AddressableEntity or a legacy Accountfrom the network for a Block from the network
        /// </summary>
        /// <param name="entityIdentifier">A PublicKey, an AccoountHashKey, or an AddressableEntityKey</param>
        /// <param name="blockHeight">A block height for which the information of the entity is queried..</param>
        public async Task<RpcResponse<GetEntityResult>> GetEntity(IEntityIdentifier entityIdentifier, ulong blockHeight)
        {
            var method = new GetEntity(entityIdentifier, new BlockIdentifier(blockHeight));
            return await SendRpcRequestAsync<GetEntityResult>(method);
        }
        
        /// <summary>
        /// Returns an AddressableEntity or a legacy Accountfrom the network for a Block from the network
        /// </summary>
        /// <param name="entityAddr">The entity address to get information of.</param>
        /// <param name="blockHash">A block hash for which the information of the entity is queried. Null for most recent information.</param>
        public async Task<RpcResponse<GetEntityResult>> GetEntity(string entityAddr, string blockHash = null)
        {
            var method = new GetEntity(entityAddr, blockHash != null ? new BlockIdentifier(blockHash) : null);
            return await SendRpcRequestAsync<GetEntityResult>(method);
        }
        
        /// <summary>
        /// Returns an AddressableEntity or a legacy Accountfrom the network for a Block from the network
        /// </summary>
        /// <param name="entityAddr">The entity address to get information of.</param>
        /// <param name="blockHeight">A block height for which the information of the entity is queried..</param>
        public async Task<RpcResponse<GetEntityResult>> GetEntity(string entityAddr, ulong blockHeight)
        {
            var method = new GetEntity(entityAddr, new BlockIdentifier(blockHeight));
            return await SendRpcRequestAsync<GetEntityResult>(method);
        }

        /// <summary>
        /// Returns a Package from the network
        /// </summary>
        /// <param name="packageHash">The entity address to get information of.</param>
        /// <param name="blockHash">A block hash for which the information of the entity is queried. Null for most recent information.</param>
        public async Task<RpcResponse<GetPackageResult>> GetPackage(string packageHash, string blockHash = null)
        {
            var method = new GetPackage(packageHash.StartsWith("package-")
                    ? PackageIdentifier.FromPackageAddr(packageHash)
                    : PackageIdentifier.FromContractPackageHash(packageHash),
                blockHash != null ? new BlockIdentifier(blockHash) : null);
            return await SendRpcRequestAsync<GetPackageResult>(method);
        }

        /// <summary>
        /// Returns a Package from the network
        /// </summary>
        /// <param name="packageHash">The package address or contract package hash to get information of.</param>
        /// <param name="blockHeight">A block height for which the information of the package is queried.</param>
        public async Task<RpcResponse<GetPackageResult>> GetPackage(string packageHash, ulong blockHeight)
        {
            var method = new GetPackage(packageHash.StartsWith("package-")
                    ? PackageIdentifier.FromPackageAddr(packageHash)
                    : PackageIdentifier.FromContractPackageHash(packageHash),
                new BlockIdentifier(blockHeight));
            return await SendRpcRequestAsync<GetPackageResult>(method);
        }

        /// <summary>
        /// Request a stored value from the network. This RPC is deprecated, use `QueryGlobalState` instead.
        /// </summary>
        /// <param name="keyHash">A global state key formatted as a string</param>
        /// <param name="path">The path components starting from the key as base (use '/' as separator).</param>
        /// <param name="stateRootHash"></param>
        /// <returns></returns>
        [Obsolete("Use QueryGlobalState() to retrieve stored values from the network.", false)]
        public async Task<RpcResponse<GetItemResult>> QueryState(string keyHash, List<string> path = null,
            string stateRootHash = null)
        {
            if (stateRootHash == null)
                stateRootHash = await GetStateRootHash();

            var method = new GetItem(keyHash, stateRootHash, path);
            return await SendRpcRequestAsync<GetItemResult>(method);
        }

        /// <summary>
        /// Request the stored value in a global state key.
        /// </summary>
        /// <param name="key">The global state key formatted as a string to query the value from the network.</param>
        /// <param name="height">Height of the block to check the stored value in.</param>
        /// <param name="path">The path components starting from the key as base (use '/' as separator).</param>
        public async Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalState(string key, ulong height,
            string path = null)
        {            
            var method = new QueryGlobalState(key, StateIdentifier.WithBlockHeight(height), path?.Split(new char[] {'/'}));
            return await SendRpcRequestAsync<QueryGlobalStateResult>(method);
        }
        
        /// <summary>
        /// Request the stored value in a global state key.
        /// </summary>
        /// <param name="key">The global state key formatted as a string to query the value from the network.</param>
        /// <param name="stateRootHash">Hash of the state root. Null for the most recent stored value..</param>
        /// <param name="path">The path components starting from the key as base (use '/' as separator).</param>
        public async Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalState(string key, string stateRootHash = null,
            string path = null)
        {            
            var method = new QueryGlobalState(key, 
                stateRootHash != null? StateIdentifier.WithStateRootHash(stateRootHash) : null, 
                path?.Split(new char[] {'/'}));
            return await SendRpcRequestAsync<QueryGlobalStateResult>(method);
        }

        /// <summary>
        /// Request the stored value in a global state key.
        /// </summary>
        /// <param name="key">The global state key to query the value from the network.</param>
        /// <param name="stateRootHash">Hash of the state root. Null for the most recent stored value..</param>
        /// <param name="path">The path components starting from the key as base (use '/' as separator).</param>
        public async Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalState(GlobalStateKey key, string stateRootHash = null,
            string path = null)
        {
            return await QueryGlobalState(key.ToString(), stateRootHash, path);
        }
        
        /// <summary>
        /// Request the stored value in a global state key.
        /// </summary>
        /// <param name="key">The global state key formatted as a string to query the value from the network.</param>
        /// <param name="blockHash">The block hash.</param>
        /// <param name="path">The path components starting from the key as base (use '/' as separator).</param>
        public async Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalStateWithBlockHash(string key, string blockHash,
            string path = null)
        {
            var method = new QueryGlobalState(key, StateIdentifier.WithBlockHash(blockHash), path?.Split(new char[] {'/'}));
            return await SendRpcRequestAsync<QueryGlobalStateResult>(method);
        }

        /// <summary>
        /// Request the stored value in a global state key.
        /// </summary>
        /// <param name="key">The global state key to query the value from the network.</param>
        /// <param name="blockHash">The block hash.</param>
        /// <param name="path">The path components starting from the key as base (use '/' as separator).</param>
        public async Task<RpcResponse<QueryGlobalStateResult>> QueryGlobalStateWithBlockHash(GlobalStateKey key,
            string blockHash, string path = null)
        {
            return await QueryGlobalStateWithBlockHash(key.ToString(), blockHash, path);
        }
        
        /// <summary>
        /// Request a purse's balance from the network.
        /// </summary>
        /// <param name="purseURef">Purse URef formatted as a string.</param>
        /// <param name="stateRootHash">Hash of the state root. Null to get latest available.</param>
        public async Task<RpcResponse<GetBalanceResult>> GetBalance(string purseURef,
            string stateRootHash = null)
        {
            var method = new GetBalance(purseURef, stateRootHash);
            return await SendRpcRequestAsync<GetBalanceResult>(method);
        }
        
        /// <summary>
        /// Request the balance information from a PublicKey, AccountHashKey, URef or EntityAddr.
        /// </summary>
        /// <param name="purseIdentifier">A PublicKey, AccountHashKey, URef or EntityAddr to identify a purse.</param>
        /// <param name="blockHash">Hash of the block. Null to get latest available.</param>
        public async Task<RpcResponse<QueryBalanceResult>> QueryBalance(IPurseIdentifier purseIdentifier, 
            string blockHash = null)
        {
            var method = new QueryBalance(purseIdentifier, blockHash != null ? StateIdentifier.WithBlockHash(blockHash) : null);
            return await SendRpcRequestAsync<QueryBalanceResult>(method);
        }
        
        /// <summary>
        /// Request the balance information from a PublicKey, AccountHashKey, URef or EntityAddr.
        /// </summary>
        /// <param name="purseIdentifier">A PublicKey, AccountHashKey, URef or EntityAddr to identify a purse.</param>
        /// <param name="blockHeight">Height of the block.</param>
        public async Task<RpcResponse<QueryBalanceResult>> QueryBalance(IPurseIdentifier purseIdentifier, 
            ulong blockHeight)
        {
            var method = new QueryBalance(purseIdentifier, StateIdentifier.WithBlockHeight(blockHeight));
            return await SendRpcRequestAsync<QueryBalanceResult>(method);
        }
        
        /// <summary>
        /// Request the balance information from a PublicKey, AccountHashKey, URef or EntityAddr.
        /// </summary>
        /// <param name="purseIdentifier">A PublicKey, AccountHashKey, URef or EntityAddr to identify a purse.</param>
        /// <param name="stateRootHash">the state root hash.</param>
        public async Task<RpcResponse<QueryBalanceResult>> QueryBalanceWithStateRootHash(IPurseIdentifier purseIdentifier,
            string stateRootHash)
        {
            var method = new QueryBalance(purseIdentifier, StateIdentifier.WithStateRootHash(stateRootHash));
            return await SendRpcRequestAsync<QueryBalanceResult>(method);
        }
        
        /// <summary>
        /// Queries the balance information including total, available, and holds.
        /// </summary>
        /// <param name="purseIdentifier">A PublicKey, AccountHashKey, URef or EntityAddr to identify a purse.</param>
        /// <param name="blockHash">Hash of the block. Null to get latest available.</param>
        public async Task<RpcResponse<QueryBalanceDetailsResult>> QueryBalanceDetails(IPurseIdentifier purseIdentifier,
            string blockHash = null)
        {
            var method = new QueryBalanceDetails(purseIdentifier, blockHash != null ? StateIdentifier.WithBlockHash(blockHash) : null);
            return await SendRpcRequestAsync<QueryBalanceDetailsResult>(method);
        }
        
        /// <summary>
        /// Queries the balance information including total, available, and holds.
        /// </summary>
        /// <param name="purseIdentifier">A PublicKey, AccountHashKey, URef or EntityAddr to identify a purse.</param>
        /// <param name="blockHeight">Height of the block.</param>
        public async Task<RpcResponse<QueryBalanceDetailsResult>> QueryBalanceDetails(IPurseIdentifier purseIdentifier,
            ulong blockHeight)
        {
            var method = new QueryBalanceDetails(purseIdentifier, StateIdentifier.WithBlockHeight(blockHeight));
            return await SendRpcRequestAsync<QueryBalanceDetailsResult>(method);
        }
        
        /// <summary>
        /// Queries the balance information including total, available, and holds.
        /// </summary>
        /// <param name="purseIdentifier">A PublicKey, AccountHashKey, URef or EntityAddr to identify a purse.</param>
        /// <param name="stateRootHash">the state root hash.</param>
        public async Task<RpcResponse<QueryBalanceDetailsResult>> QueryBalanceDetailsWithStateRootHash(IPurseIdentifier purseIdentifier,
            string stateRootHash)
        {
            var method = new QueryBalanceDetails(purseIdentifier, StateIdentifier.WithStateRootHash(stateRootHash));
            return await SendRpcRequestAsync<QueryBalanceDetailsResult>(method);
        }

        /// <summary>
        /// Send a Deploy to the network for its execution.
        /// </summary>
        /// <param name="deploy">The deploy object.</param>
        /// <exception cref="System.Exception">Throws an exception if the deploy is not signed.</exception>
        public async Task<RpcResponse<PutDeployResult>> PutDeploy(Deploy deploy)
        {
            if (deploy.Approvals.Count == 0)
                throw new Exception("Sign the deploy before sending it to the network.");

            var method = new PutDeploy(deploy);
            return await SendRpcRequestAsync<PutDeployResult>(method);
        }
        
        /// <summary>
        /// Request a Deploy object from the network by the deploy hash.
        /// When a cancellation token is included this method waits until the deploy is
        /// executed, i.e. until the deploy contains the execution results information.
        /// </summary>
        /// <param name="deployHash">Hash of the deploy to retrieve.</param>
        /// <param name="cancellationToken">A CancellationToken. Do not include this parameter to return
        /// with the first deploy object returned by the network, even it's not executed.</param>
        /// <exception cref="TaskCanceledException">The token has cancelled the operation before the deploy has been executed.</exception>
        public async Task<RpcResponse<GetDeployResult>> GetDeploy(string deployHash,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetDeploy(deployHash, false, cancellationToken);
        }

        /// <summary>
        /// Request a Deploy object from the network by the deploy hash.
        /// When a cancellation token is included this method waits until the deploy is
        /// executed, i.e. until the deploy contains the execution results information.
        /// </summary>
        /// <param name="deployHash">Hash of the deploy to retrieve.</param>
        /// <param name="finalizedApprovals">Whether to return the deploy with the finalized approvals
        /// substituted. If `false` or omitted, returns the deploy with the approvals that were originally
        /// received by the node.</param>
        /// <param name="cancellationToken">A CancellationToken. Do not include this parameter to return
        /// with the first deploy object returned by the network, even it's not executed.</param>
        /// <exception cref="TaskCanceledException">The token has cancelled the operation before the deploy has been executed.</exception>
        public async Task<RpcResponse<GetDeployResult>> GetDeploy(string deployHash,
            bool finalizedApprovals,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = new GetDeploy(deployHash, finalizedApprovals);

            while (!cancellationToken.IsCancellationRequested)
            {
                var response = await SendRpcRequestAsync<GetDeployResult>(method);
                if (!cancellationToken.CanBeCanceled)
                    return response;
                
                // Casper >= v2.0.0 processed deploy contains execution_info with data
                if(response.Result.TryGetProperty("execution_info", out var executionInfo) &&
                   executionInfo.ValueKind != JsonValueKind.Null)
                    return response;
             
                // Casper < v2.0.0 processed deploy contains execution_results with data
                if(response.Result.TryGetProperty("execution_results", out var executionResults) &&
                   executionResults.ValueKind == JsonValueKind.Array &&
                   executionResults.GetArrayLength() > 0)
                    return response;
                
                await Task.Delay(4000);
            }

            throw new TaskCanceledException("GetDeploy operation canceled");
        }

        /// <summary>
        /// Send a Transaction to the network for its execution.
        /// </summary>
        /// <param name="transaction">The transaction object.</param>
        /// <exception cref="System.Exception">Throws an exception if the transaction is not signed.</exception>
        public async Task<RpcResponse<PutTransactionResult>> PutTransaction(TransactionV1 transaction)
        {
            if (transaction.Approvals.Count == 0)
                throw new Exception("Sign the transaction before sending it to the network.");

            var method = new PutTransaction(transaction);
            return await SendRpcRequestAsync<PutTransactionResult>(method);
        }
        
        /// <summary>
        /// Send a Transaction to the network for its execution.
        /// </summary>
        /// <param name="transaction">The transaction object.</param>
        /// <exception cref="System.Exception">Throws an exception if the transaction is not signed.</exception>
        public async Task<RpcResponse<PutTransactionResult>> PutTransaction(Transaction transaction)
        {
            RpcMethod method;
            
            if (transaction.Approvals.Count == 0)
                throw new Exception("Sign the transaction before sending it to the network.");

            if(transaction.Version == TransactionVersion.Deploy)
                method = new PutTransaction((Deploy)transaction);
            else
                method = new PutTransaction((TransactionV1)transaction);
            return await SendRpcRequestAsync<PutTransactionResult>(method);
        }
        
        /// <summary>
        /// Request a Transaction object from the network by the transaction (or deploy) hash.
        /// When a cancellation token is included this method waits until the transaction is
        /// executed, i.e. until the transaction contains the execution result information.
        /// </summary>
        /// <param name="transactionHash">An v1 transaction hash or a deploy hash</param>
        /// <param name="finalizedApprovals">Whether to return the transaction with the finalized approvals
        /// substituted. If `false` or omitted, returns the transaction with the approvals that were originally
        /// received by the node.</param>
        /// <param name="cancellationToken">A CancellationToken. Do not include this parameter to return
        /// with the first transaction object returned by the network, even it's not executed.</param>
        /// <exception cref="TaskCanceledException">The token has cancelled the operation before the deploy has been executed.</exception>
        public async Task<RpcResponse<GetTransactionResult>> GetTransaction(TransactionHash transactionHash,
            bool finalizedApprovals,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = new GetTransaction(transactionHash, finalizedApprovals);

            while (!cancellationToken.IsCancellationRequested)
            {
                var response = await SendRpcRequestAsync<GetTransactionResult>(method);
                if (!cancellationToken.CanBeCanceled ||
                    response.Result.GetProperty("execution_info").ValueKind != JsonValueKind.Null)
                    return response;
                await Task.Delay(4000);
            }

            throw new TaskCanceledException("GetDeploy operation canceled");
        }
        
        /// <summary>
        /// Request a Transaction object from the network by the transaction (or deploy) hash.
        /// When a cancellation token is included this method waits until the transaction is
        /// executed, i.e. until the transaction contains the execution result information.
        /// </summary>
        /// <param name="transactionHash">An v1 transaction hash or a deploy hash</param>
        /// <param name="cancellationToken">A CancellationToken. Do not include this parameter to return
        /// with the first transaction object returned by the network, even it's not executed.</param>
        /// <exception cref="TaskCanceledException">The token has cancelled the operation before the deploy has been executed.</exception>
        public async Task<RpcResponse<GetTransactionResult>> GetTransaction(TransactionHash transactionHash,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetTransaction(transactionHash, false, cancellationToken);
        }
        
        /// <summary>
        /// Request a Transaction object from the network by the transaction hash.
        /// When a cancellation token is included this method waits until the transaction is
        /// executed, i.e. until the transaction contains the execution result information.
        /// </summary>
        /// <param name="transactionV1Hash">A TransactionV1 hash</param>
        /// <param name="finalizedApprovals">Whether to return the transaction with the finalized approvals
        /// substituted. If `false` or omitted, returns the transaction with the approvals that were originally
        /// received by the node.</param>
        /// <param name="cancellationToken">A CancellationToken. Do not include this parameter to return
        /// with the first transaction object returned by the network, even it's not executed.</param>
        /// <exception cref="TaskCanceledException">The token has cancelled the operation before the deploy has been executed.</exception>
        public async Task<RpcResponse<GetTransactionResult>> GetTransaction(string transactionV1Hash,
            bool finalizedApprovals,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.GetTransaction(new TransactionHash { Version1 = transactionV1Hash }, finalizedApprovals,
                cancellationToken);
        }
        
        /// <summary>
        /// Request a Transaction object from the network by the transaction hash.
        /// When a cancellation token is included this method waits until the transaction is
        /// executed, i.e. until the transaction contains the execution result information.
        /// </summary>
        /// <param name="transactionV1Hash">A TransactionV1 hash</param>
        /// <param name="cancellationToken">A CancellationToken. Do not include this parameter to return
        /// with the first transaction object returned by the network, even it's not executed.</param>
        /// <exception cref="TaskCanceledException">The token has cancelled the operation before the deploy has been executed.</exception>
        public async Task<RpcResponse<GetTransactionResult>> GetTransaction(string transactionV1Hash,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.GetTransaction(new TransactionHash { Version1 = transactionV1Hash }, false,
                cancellationToken);
        }

        /// <summary>
        /// Retrieves a Block from the network by its hash. 
        /// </summary>
        /// <param name="blockHash">Hash of the block to retrieve. Null for the most recent block.</param>
        public async Task<RpcResponse<GetBlockResult>> GetBlock(string blockHash = null)
        {
            var method = new GetBlock(blockHash);
            return await SendRpcRequestAsync<GetBlockResult>(method);
        }

        /// <summary>
        /// Request a Block from the network by its height number.
        /// </summary>
        /// <param name="blockHeight">Height of the block to retrieve.</param>
        public async Task<RpcResponse<GetBlockResult>> GetBlock(ulong blockHeight)
        {
            var method = new GetBlock(blockHeight);
            return await SendRpcRequestAsync<GetBlockResult>(method);
        }

        /// <summary>
        /// Request all transfers for a Block by its block hash.
        /// </summary>
        /// <param name="blockHash">Hash of the block to retrieve the transfers from. Null for the most recent block</param>
        public async Task<RpcResponse<GetBlockTransfersResult>> GetBlockTransfers(string blockHash = null)
        {
            var method = new GetBlockTransfers(blockHash);
            return await SendRpcRequestAsync<GetBlockTransfersResult>(method);
        }

        /// <summary>
        /// Request all transfers for a Block by its height number.
        /// </summary>
        /// <param name="blockHeight">Height of the block to retrieve the transfers from.</param>
        public async Task<RpcResponse<GetBlockTransfersResult>> GetBlockTransfers(ulong blockHeight)
        {
            var method = new GetBlockTransfers(blockHeight);
            return await SendRpcRequestAsync<GetBlockTransfersResult>(method);
        }

        /// <summary>
        /// Request an EraInfo from the network given a switch block.
        /// For a non-switch block this method returns an empty response.
        /// </summary>
        /// <param name="blockHash">Block hash of a switch block. Null for the latest block.</param>
        public async Task<RpcResponse<GetEraInfoBySwitchBlockResult>> GetEraInfoBySwitchBlock(string blockHash = null)
        {
            var method = new GetEraInfoBySwitchBlock(blockHash);
            return await SendRpcRequestAsync<GetEraInfoBySwitchBlockResult>(method);
        }

        /// <summary>
        /// Request an EraInfo from the network given a switch block.
        /// For a non-switch block this method returns an empty response.
        /// </summary>
        /// <param name="blockHeight">Block height of a switch block.</param>
        public async Task<RpcResponse<GetEraInfoBySwitchBlockResult>> GetEraInfoBySwitchBlock(ulong blockHeight)
        {
            var method = new GetEraInfoBySwitchBlock(blockHeight);
            return await SendRpcRequestAsync<GetEraInfoBySwitchBlockResult>(method);
        }
        
        /// <summary>
        /// Request current Era Info from the network given a block hash
        /// </summary>
        /// <param name="blockHash">Block hash. Null for the latest block.</param>
        public async Task<RpcResponse<GetEraSummaryResult>> GetEraSummary(string blockHash = null)
        {
            var method = new GetEraSummary(blockHash);
            return await SendRpcRequestAsync<GetEraSummaryResult>(method);
        }

        /// <summary>
        /// Request current Era Info from the network given a block hash
        /// </summary>
        /// <param name="blockHeight">Block height.</param>
        public async Task<RpcResponse<GetEraSummaryResult>> GetEraSummary(ulong blockHeight)
        {
            var method = new GetEraSummary(blockHeight);
            return await SendRpcRequestAsync<GetEraSummaryResult>(method);
        }

        /// <summary>
        /// Lookup a dictionary item from its dictionary key.
        /// </summary>
        /// <param name="dictionaryKey">The dictionary key to retrieve.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public async Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItem(string dictionaryKey, string stateRootHash = null)
        {
            if(stateRootHash == null)
                stateRootHash = await GetStateRootHash();

            var method = new GetDictionaryItem(dictionaryKey, stateRootHash);
            return await SendRpcRequestAsync<GetDictionaryItemResult>(method);
        }

        /// <summary>
        /// Lookup a dictionary item via an Account's named keys.
        /// </summary>
        /// <param name="accountKey">The account key as a formatted string whose named keys contains dictionaryName.</param>
        /// <param name="dictionaryName">The named key under which the dictionary seed URef is stored.</param>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public async Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItemByAccount(string accountKey, string dictionaryName, 
            string dictionaryItem, string stateRootHash = null)
        {
            if(stateRootHash == null)
                stateRootHash = await GetStateRootHash();

            var method = new GetDictionaryItemByAccount(accountKey, dictionaryName, dictionaryItem, stateRootHash);
            return await SendRpcRequestAsync<GetDictionaryItemResult>(method);
        }
        
        /// <summary>
        /// Lookup a dictionary item via a Contract named keys.
        /// </summary>
        /// <param name="contractKey">The contract key as a formatted string whose named keys contains dictionaryName.</param>
        /// <param name="dictionaryName">The named key under which the dictionary seed URef is stored.</param>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public async Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItemByContract(string contractKey, string dictionaryName, 
            string dictionaryItem, string stateRootHash = null)
        {
            if(stateRootHash == null)
                stateRootHash = await GetStateRootHash();

            var method = new GetDictionaryItemByContract(contractKey, dictionaryName, dictionaryItem, stateRootHash);
            return await SendRpcRequestAsync<GetDictionaryItemResult>(method);
        }
        
        /// <summary>
        /// Lookup a dictionary item via its seed URef.
        /// </summary>
        /// <param name="seedURef">The dictionary's seed URef.</param>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public async Task<RpcResponse<GetDictionaryItemResult>> GetDictionaryItemByURef(string seedURef, 
            string dictionaryItem, string stateRootHash = null)
        {
            if(stateRootHash == null)
                stateRootHash = await GetStateRootHash();

            var method = new GetDictionaryItemByURef(seedURef, dictionaryItem, stateRootHash);
            return await SendRpcRequestAsync<GetDictionaryItemResult>(method);
        }
        
        /// <summary>
        /// Request the status changes of active validators.
        /// </summary>
        public async Task<RpcResponse<GetValidatorChangesResult>> GetValidatorChanges()
        {
            var method = new GetValidatorChanges();
            return await SendRpcRequestAsync<GetValidatorChangesResult>(method);
        }

        /// <summary>
        /// Returns the reward for a given era and a validator
        /// </summary>
        /// <param name="validator">The public key of the validator.</param>
        /// <param name="blockHash">Hash of the block to retrieve the rewards from. Null for the most recent era</param>
        public async Task<RpcResponse<GetRewardResult>> GetValidatorReward(PublicKey validator, string blockHash = null)
        {
            var method = new GetValidatorReward(validator, blockHash != null ? new BlockIdentifier(blockHash) : null);
            return await SendRpcRequestAsync<GetRewardResult>(method);
        }
        
        /// <summary>
        /// Returns the reward for a given era and a validator
        /// </summary>
        /// <param name="validator">The public key of the validator.</param>
        /// <param name="blockHeight">Height of the block to retrieve the rewards from.</param>
        public async Task<RpcResponse<GetRewardResult>> GetValidatorReward(PublicKey validator, ulong blockHeight)
        {
            var method = new GetValidatorReward(validator, new BlockIdentifier(blockHeight));
            return await SendRpcRequestAsync<GetRewardResult>(method);
        }
        
        /// <summary>
        /// Returns the reward for a given era and a validator
        /// </summary>
        /// <param name="validator">The public key of the validator.</param>
        /// <param name="eraId">Id of the Era to retrieve the rewards from.</param>
        public async Task<RpcResponse<GetRewardResult>> GetValidatorRewardWithEraId(PublicKey validator, ulong eraId)
        {
            var method = new GetValidatorReward(validator, eraId);
            return await SendRpcRequestAsync<GetRewardResult>(method);
        }

        /// <summary>
        /// Returns the reward for a given era and a delegator
        /// </summary>
        /// <param name="validator">The public key of the validator.</param>
        /// <param name="delegator">The public key of the delegator.</param>
        /// <param name="blockHash">Hash of the block to retrieve the rewards from. Null for the most recent era</param>
        public async Task<RpcResponse<GetRewardResult>> GetDelegatorReward(PublicKey validator, PublicKey delegator, string blockHash = null)
        {
            var method = new GetDelegatorReward(validator, delegator, blockHash != null ? new BlockIdentifier(blockHash) : null);
            return await SendRpcRequestAsync<GetRewardResult>(method);
        }
        
        /// <summary>
        /// Returns the reward for a given era and a delegator
        /// </summary>
        /// <param name="validator">The public key of the validator.</param>
        /// <param name="delegator">The public key of the delegator.</param>
        /// <param name="blockHeight">Height of the block to retrieve the rewards from.</param>
        public async Task<RpcResponse<GetRewardResult>> GetDelegatorReward(PublicKey validator, PublicKey delegator, ulong blockHeight)
        {
            var method = new GetDelegatorReward(validator, delegator, new BlockIdentifier(blockHeight));
            return await SendRpcRequestAsync<GetRewardResult>(method);
        }
        
        /// <summary>
        /// Returns the reward for a given era and a delegator
        /// </summary>
        /// <param name="validator">The public key of the validator.</param>
        /// <param name="delegator">The public key of the delegator.</param>
        /// <param name="eraId">Id of the Era to retrieve the rewards from.</param>
        public async Task<RpcResponse<GetRewardResult>> GetDelegatorRewardWithEraId(PublicKey validator, PublicKey delegator, ulong eraId)
        {
            var method = new GetDelegatorReward(validator, delegator, eraId);
            return await SendRpcRequestAsync<GetRewardResult>(method);
        }
        
        /// <summary>
        /// Request the RPC Json schema to the network node.
        /// </summary>
        public async Task<string> GetRpcSchema()
        {
            var method = new GetRpcSchema();
            var response = await SendRpcRequestAsync<RpcResult>(method);
            return response.Result.GetRawText();
        }

        /// <summary>
        /// Request the the chainspec.toml, genesis accounts.toml, and global_state.toml files of the node.
        /// </summary>
        public async Task<RpcResponse<GetChainspecResult>> GetChainspec()
        {
            var method = new GetChainspec();
            return await SendRpcRequestAsync<GetChainspecResult>(method);
        }

        /// <summary>
        /// Sends a "deploy dry run" to the network. It will execute the deploy on top of the specified block and return
        /// the results of the execution to the caller. The effects of the execution won't be committed to the trie
        /// (blockchain database/GlobalState).
        /// This method runs in a different port of the network (e.g.: 7778) and can be used for debugging, discovery.
        /// For example price estimation.
        /// </summary>
        /// <param name="deploy">The deploy to execute.</param>
        /// <param name="stateRootHash">Hash of the state root. null if deploy is to be executed on top of the latest block.</param>
        public async Task<RpcResponse<SpeculativeExecutionResult>> SpeceulativeExecution(Deploy deploy, string stateRootHash = null)
        {
            if (deploy.Approvals.Count == 0)
                throw new Exception("Sign the deploy before sending it to the network.");

            var method = new SpeculativeExecution(deploy, stateRootHash, isBlockHash: false );
            return await SendRpcRequestAsync<SpeculativeExecutionResult>(method);
        }

        /// <summary>
        /// Sends a "deploy dry run" to the network. It will execute the deploy on top of the specified block and return
        /// the results of the execution to the caller. The effects of the execution won't be committed to the trie
        /// (blockchain database/GlobalState).
        /// This method runs in a different port of the network (e.g.: 7778) and can be used for debugging, discovery.
        /// For example price estimation.
        /// </summary>
        /// <param name="deploy">The deploy to execute.</param>
        /// <param name="blockHash">Hash of the block on top of which the deploy is executed.</param>
        public async Task<RpcResponse<PutDeployResult>> SpeceulativeExecutionWithBlockHash(Deploy deploy, string blockHash = null)
        {
            if (deploy.Approvals.Count == 0)
                throw new Exception("Sign the deploy before sending it to the network.");

            var method = new SpeculativeExecution(deploy, blockHash, isBlockHash: true );
            return await SendRpcRequestAsync<PutDeployResult>(method);
        }

        /// <summary>
        /// Request the performance metrics of a node in the network.
        /// </summary>
        /// <param name="nodeAddress">URL of the performance metrics endpoint. Example: 'http://127.0.0.1:8888/metrics'.</param>
        public static async Task<string> GetNodeMetrics(string nodeAddress)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            try
            {
                var metrics = new StringBuilder();

                using (var streamReader =
                    new StreamReader(await client.GetStreamAsync(nodeAddress)))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var message = await streamReader.ReadLineAsync();
                        metrics.AppendLine(message);
                    }
                }

                return metrics.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not retrieve metrics from {nodeAddress}", ex);
            }
        }

        /// <summary>
        /// Request the performance metrics of a node in the network.
        /// </summary>
        /// <param name="host">IP of the network node.</param>
        /// <param name="port">Port of the performance metrics endpoint (usually 8888).</param>
        public static async Task<string> GetNodeMetrics(string host, int port)
        {
            var uriBuilder = new UriBuilder("http", host, port, "metrics");
            return await GetNodeMetrics(uriBuilder.Uri.ToString());
        }
        
        /// <summary>
        /// Returns the validator bid.
        /// </summary>
        /// <param name="validator">The public key of the validator.</param>
        /// <param name="blockHash">Hash of the block to retrieve the rewards from. Null for the most recent era</param>
        public async Task<RpcResponse<QueryGlobalStateResult>> GetValidatorBid(PublicKey validator, string blockHash = null)
        {
            var bidAddr = BidAddrKey.FromValidatorKey(new AccountHashKey(validator));
            return await QueryGlobalState(bidAddr, blockHash);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                _rpcClient.Dispose();
            }
        }
    }
}
