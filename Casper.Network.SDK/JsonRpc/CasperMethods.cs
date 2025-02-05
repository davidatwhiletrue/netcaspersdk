using System;
using System.Collections.Generic;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.JsonRpc
{
    public class GetStateRootHash : RpcMethod
    {
        /// <summary>
        /// Returns a state root hash at a given Block
        /// </summary>
        /// <param name="blockHash">Block hash for which the state root is queried. Null for the most recent.</param>
        public GetStateRootHash(string blockHash = null) : base("chain_get_state_root_hash", blockHash)
        {
        }

        /// <summary>
        /// Returns the state root hash at a given Block
        /// </summary>
        /// <param name="height">Block height for which the state root is queried.</param>
        public GetStateRootHash(ulong height) : base("chain_get_state_root_hash", height)
        {
        }
    }

    public class GetNodeStatus : RpcMethod
    {
        /// <summary>
        /// Returns the current status of the node.
        /// </summary>
        public GetNodeStatus() : base("info_get_status")
        {
        }
    }

    public class GetNodePeers : RpcMethod
    {
        /// <summary>
        /// Returns a list of peers connected to the node.
        /// </summary>
        public GetNodePeers() : base("info_get_peers")
        {
        }
    }

    public class GetAuctionInfo : RpcMethod
    {
        /// <summary>
        /// Returns the bids and validators at a given block.
        /// </summary>
        /// <param name="blockHash">Block hash for which the auction info is queried. Null for the most recent auction info.</param>
        public GetAuctionInfo(string blockHash) : base("state_get_auction_info", blockHash)
        {
        }

        /// <summary>
        /// Returns the bids and validators at a given block.
        /// </summary>
        /// <param name="height">Block height for which the auction info is queried.</param>
        public GetAuctionInfo(ulong height) : base("state_get_auction_info", height)
        {
        }
    }
    
    public class GetAuctionInfoV2 : RpcMethod
    {
        /// <summary>
        /// Returns the bids and validators at a given block.
        /// </summary>
        /// <param name="blockHash">Block hash for which the auction info is queried. Null for the most recent auction info.</param>
        public GetAuctionInfoV2(string blockHash) : base("state_get_auction_info_v2", blockHash)
        {
        }

        /// <summary>
        /// Returns the bids and validators at a given block.
        /// </summary>
        /// <param name="height">Block height for which the auction info is queried.</param>
        public GetAuctionInfoV2(ulong height) : base("state_get_auction_info_v2", height)
        {
        }
    }

    public class GetAccountInfo : RpcMethod
    {
        /// <summary>
        /// Returns the information of an Account in the network.
        /// </summary>
        /// <param name="publicKey">The public key of the account.</param>
        /// <param name="blockHash">A block hash for which the information of the account is queried. Null for most recent information.</param>
        public GetAccountInfo(PublicKey publicKey, string blockHash = null) : base("state_get_account_info", blockHash)
        {
            this.Parameters.Add("account_identifier", publicKey);
        }

        /// <summary>
        /// Returns the information of an Account in the network.
        /// </summary>
        /// <param name="publicKey">The public key of the account.</param>
        /// <param name="height">A block height for which the information of the account is queried.</param>
        public GetAccountInfo(PublicKey publicKey, ulong height) : base("state_get_account_info", height)
        {
            this.Parameters.Add("account_identifier", publicKey);
        }
        
        /// <summary>
        /// Returns the information of an Account in the network.
        /// </summary>
        /// <param name="accountHash">The account hash of the account.</param>
        /// <param name="blockHash">A block hash for which the information of the account is queried. Null for most recent information.</param>
        public GetAccountInfo(AccountHashKey accountHash, string blockHash = null) : base("state_get_account_info", blockHash)
        {
            this.Parameters.Add("account_identifier", accountHash.ToString());
        }

        /// <summary>
        /// Returns the information of an Account in the network.
        /// </summary>
        /// <param name="accountHash">The account hash of the account.</param>
        /// <param name="height">A block height for which the information of the account is queried.</param>
        public GetAccountInfo(AccountHashKey accountHash, ulong height) : base("state_get_account_info", height)
        {
            // this.Parameters.Add("account_identifier", Hex.ToHexString(accountHash.GetBytes()));
            this.Parameters.Add("account_identifier", accountHash.ToString());
        }
        
        /// <summary>
        /// Returns the information of an Account in the network.
        /// </summary>
        /// <param name="publicKey">The public key of the account.</param>
        /// <param name="blockHash">A block hash for which the information of the account is queried. Null for most recent information.</param>
        [Obsolete("For Casper node v1.5.5 or newer use the new method signature with PublicKey or AccountHashKey, ", false)]
        public GetAccountInfo(string publicKey, string blockHash = null) : base("state_get_account_info", blockHash)
        {
            this.Parameters.Add("public_key", publicKey);
        }

        /// <summary>
        /// Returns the information of an Account in the network.
        /// </summary>
        /// <param name="publicKey">The public key of the account.</param>
        /// <param name="height">A block height for which the information of the account is queried.</param>
        [Obsolete("For Casper node v1.5.5 or newer use the new method signature with PublicKey or AccountHashKey", false)]
        public GetAccountInfo(string publicKey, ulong height) : base("state_get_account_info", height)
        {
            this.Parameters.Add("public_key", publicKey);
        }
    }

    public class GetEntity : RpcMethod
    {
        /// <summary>
        /// Returns an AddressableEntity from the network for a Block from the network
        /// </summary>
        /// <param name="entityIdentifier">A PublicKey, an AccoountHashKey, or an AddressableEntityKey</param>
        /// <param name="blockIdentifier">A a block identifier by hash or key. Null for the latest block</param>
        public GetEntity(IEntityIdentifier entityIdentifier, IBlockIdentifier blockIdentifier = null) : base("state_get_entity")
        {
            this.Parameters = new Dictionary<string, object>
            {
                { "entity_identifier", entityIdentifier.GetEntityIdentifier() }
            };
            
            if(blockIdentifier != null)
                this.Parameters.Add("block_identifier", blockIdentifier.GetBlockIdentifier());
        }

        /// <summary>
        /// Returns an AddressableEntity from the network for a Block from the network
        /// </summary>
        /// <param name="addressableEntity">A string with an addressable entity key.</param>
        /// <param name="blockIdentifier">A a block identifier by hash or key. Null for the latest block</param>
        public GetEntity(string addressableEntity, IBlockIdentifier blockIdentifier = null) 
            : this(new AddressableEntityKey(addressableEntity), blockIdentifier)
        {
        }
    }

    public class GetPackage : RpcMethod
    {
        public GetPackage(IPackageIdentifier packageIdentifier, IBlockIdentifier blockIdentifier = null) : base("state_get_package")
        {
            this.Parameters = new Dictionary<string, object>
            {
                { "package_identifier", packageIdentifier.GetPackageIdentifier() }
            };

            if(blockIdentifier != null)
                this.Parameters.Add("block_identifier", blockIdentifier.GetBlockIdentifier());
        }
    }
    
    public class GetItem : RpcMethod
    {
        /// <summary>
        /// Returns a stored value from the network. This RPC is deprecated, use `QueryGlobalState` instead.
        /// </summary>
        /// <param name="key">GlobalStateKey as a string.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        /// <param name="path">The path components starting from the key as base (use '/' as separator).</param>
        public GetItem(string key, string stateRootHash, List<string> path = null) : base("state_get_item")
        {
            if (path == null) path = new List<string>();
            this.Parameters = new Dictionary<string, object>
            {
                {"state_root_hash", stateRootHash},
                {"path", path},
                {"key", key}
            };
        }
    }

    public class QueryGlobalState : RpcMethod
    {
        /// <summary>
        /// A query to the global state that returns a stored value from the network.
        /// </summary>
        /// <param name="key">A global state key formatted as a string to query the value from the network.</param>
        /// <param name="stateIdentifier">(Optional) A block hash, a block height or a state root hash value.</param>
        /// <param name="path">The path components starting from the key as base (use '/' as separator).</param>
        public QueryGlobalState(string key, StateIdentifier stateIdentifier = null, string[] path = null) : base("query_global_state")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"path", path ?? new string[] { }},
                {"key", key}
            };
            if(stateIdentifier != null)
                this.Parameters.Add("state_identifier", stateIdentifier.GetParam());
        }
    }

    public class GetBalance : RpcMethod
    {
        /// <summary>
        /// Returns a purse's balance from the network.
        /// </summary>
        /// <param name="purseURef">Purse URef formatted as a string.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public GetBalance(string purseURef, string stateRootHash) : base("state_get_balance")
        {
            this.Parameters = new Dictionary<string, object>
            {
                { "state_root_hash", stateRootHash },
                { "purse_uref", purseURef }
            };
        }
    }

    public class QueryBalance : RpcMethod
    {
        /// <summary>
        /// Query for balance information using a purse identifier and a state identifier
        /// </summary>
        /// <param name="purseIdentifier">The identifier to obtain the purse corresponding to balance query.</param>
        /// <param name="stateIdentifier">The identifier for the state used for the query, if none is passed, the latest block will be used.</param>
        public QueryBalance(IPurseIdentifier purseIdentifier, StateIdentifier stateIdentifier = null) : base("query_balance")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"purse_identifier", purseIdentifier.GetPurseIdentifier()},
            };
            if(stateIdentifier != null)
                this.Parameters.Add("state_identifier", stateIdentifier.GetParam());
        }
    }

    public class QueryBalanceDetails : RpcMethod
    {
        /// <summary>
        /// Query for full balance information using a purse identifier and a state identifier
        /// </summary>
        /// <param name="purseIdentifier">The identifier to obtain the purse corresponding to balance query.</param>
        /// <param name="stateIdentifier">The identifier for the state used for the query, if none is passed, the latest block will be used.</param>
        public QueryBalanceDetails(IPurseIdentifier purseIdentifier, StateIdentifier stateIdentifier = null) : base("query_balance_details")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"purse_identifier", purseIdentifier.GetPurseIdentifier()},
            };
            if(stateIdentifier != null)
                this.Parameters.Add("state_identifier", stateIdentifier.GetParam());
        }
    }
    
    public class PutDeploy : RpcMethod
    {
        /// <summary>
        /// Sends a Deploy to the network for its execution.
        /// </summary>
        /// <param name="deploy">The deploy object.</param>
        public PutDeploy(Deploy deploy) : base("account_put_deploy")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"deploy", deploy}
            };
        }
    }

    public class GetDeploy : RpcMethod
    {
        /// <summary>
        /// Retrieves a Deploy from the network.
        /// </summary>
        /// <param name="deployHash">Hash of the deploy to retrieve.</param>
        /// <param name="finalizedApprovals">Whether to return the deploy with the finalized approvals
        /// substituted. If `false` or omitted, returns the deploy with the approvals that were originally
        /// received by the node.</param>
        public GetDeploy(string deployHash, bool finalizedApprovals = false) : base("info_get_deploy")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"deploy_hash", deployHash}
            };

            if (finalizedApprovals)
                this.Parameters.Add("finalized_approvals", true);
        }
    }
	
	
    public class PutTransaction : RpcMethod
    {
        /// <summary>
        /// Sends a Transaction to the network for its execution.
        /// </summary>
        /// <param name="transaction">The Transactionv1 object.</param>
        public PutTransaction(TransactionV1 transaction) : base("account_put_transaction")
        {
            var txParameter = new Dictionary<string, object>();
            txParameter.Add("Version1", transaction);
            
            this.Parameters = new Dictionary<string, object>
            {
                {
                    "transaction", txParameter
                }
            };
        }
        
        /// <summary>
        /// Sends a Transaction to the network for its execution.
        /// </summary>
        /// <param name="transaction">The Deploy object.</param>
        public PutTransaction(Deploy transaction) : base("account_put_transaction")
        {
            var txParameter = new Dictionary<string, object>();
            txParameter.Add("Deploy", transaction);
            
            this.Parameters = new Dictionary<string, object>
            {
                {
                    "transaction", txParameter
                }
            };
        }
    }
    
    public class GetTransaction : RpcMethod
    {
        public GetTransaction(TransactionHash transactionHash, bool finalizedApprovals = false) : base("info_get_transaction")
        {
            var hashDict = new Dictionary<string, object>();
            if(transactionHash.Deploy != null)
                hashDict.Add("Deploy", transactionHash.Deploy);
            if(transactionHash.Version1 != null)
                hashDict.Add("Version1", transactionHash.Version1);

            this.Parameters = new Dictionary<string, object>
            {
                {
                    "transaction_hash", hashDict
                },
            };
            if (finalizedApprovals)
                this.Parameters.Add("finalized_approvals", true);
        }
    }

    public class GetBlock : RpcMethod
    {
        /// <summary>
        /// Retrieves a Block from the network by its hash. 
        /// </summary>
        /// <param name="blockHash">Hash of the block to retrieve. Null for the most recent block.</param>
        public GetBlock(string blockHash) : base("chain_get_block", blockHash)
        {
        }

        /// <summary>
        /// Retrieves a Block from the network by its height number.
        /// </summary>
        /// <param name="height">Height of the block to retrieve.</param>
        public GetBlock(ulong height) : base("chain_get_block", height)
        {
        }
    }

    public class GetBlockTransfers : RpcMethod
    {
        /// <summary>
        /// Retrieves all transfers for a Block from the network
        /// </summary>
        /// <param name="blockHash">Hash of the block to retrieve the transfers from. Null for the most recent block</param>
        public GetBlockTransfers(string blockHash) : base("chain_get_block_transfers", blockHash)
        {
        }

        /// <summary>
        /// Retrieves all transfers for a Block from the network
        /// </summary>
        /// <param name="height">Height of the block to retrieve the transfers from.</param>
        public GetBlockTransfers(ulong height) : base("chain_get_block_transfers", height)
        {
        }
    }

    public class GetEraInfoBySwitchBlock : RpcMethod
    {
        /// <summary>
        /// Retrieves an EraInfo from the network given a switch block 
        /// </summary>
        /// <param name="blockHash">Block hash of a switch block.</param>
        public GetEraInfoBySwitchBlock(string blockHash) : base("chain_get_era_info_by_switch_block", blockHash)
        {
        }

        /// <summary>
        /// Retrieves an EraInfo from the network given a switch block.
        /// </summary>
        /// <param name="height">Block height of a switch block.</param>
        public GetEraInfoBySwitchBlock(ulong height) : base("chain_get_era_info_by_switch_block", height)
        {
        }
    }

    public class GetEraSummary : RpcMethod
    {
        /// <summary>
        /// Retrieves current era info from the network given a block hash 
        /// </summary>
        /// <param name="blockHash">Block hash.</param>
        public GetEraSummary(string blockHash) : base("chain_get_era_summary", blockHash)
        {
        }

        /// <summary>
        /// Retrieves current era info from the network given a block height 
        /// </summary>
        /// <param name="height">Block height.</param>
        public GetEraSummary(ulong height) : base("chain_get_era_summary", height)
        {
        }
    }

    public class GetDictionaryItem : RpcMethod
    {
        /// <summary>
        /// Lookup a dictionary item from its dictionary item key..
        /// </summary>
        /// <param name="dictionaryItem">The dictionary item key to retrieve.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public GetDictionaryItem(string dictionaryItem, string stateRootHash) : base("state_get_dictionary_item")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {
                    "dictionary_identifier", new Dictionary<string, object>
                    {
                        {"Dictionary", dictionaryItem}
                    }
                },
                {"state_root_hash", stateRootHash},
            };
        }
    }

    public class GetDictionaryItemByAccount : RpcMethod
    {
        /// <summary>
        /// Lookup a dictionary item via an Account's named keys.
        /// </summary>
        /// <param name="accountKey">The account key as a formatted string whose named keys contains dictionaryName.</param>
        /// <param name="dictionaryName">The named key under which the dictionary seed URef is stored.</param>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public GetDictionaryItemByAccount(string accountKey, string dictionaryName, string dictionaryItem,
            string stateRootHash) : base("state_get_dictionary_item")
        {
            var accountNamedKey = new Dictionary<string, string>
            {
                {"key", accountKey},
                {"dictionary_name", dictionaryName},
                {"dictionary_item_key", dictionaryItem}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {
                    "dictionary_identifier", new Dictionary<string, object>
                    {
                        {"AccountNamedKey", accountNamedKey}
                    }
                },
                {"state_root_hash", stateRootHash},
            };
        }
    }

    public class GetDictionaryItemByContract : RpcMethod
    {
        /// <summary>
        /// Lookup a dictionary item via a Contract named keys.
        /// </summary>
        /// <param name="contractKey">The contract key as a formatted string whose named keys contains dictionaryName.</param>
        /// <param name="dictionaryName">The named key under which the dictionary seed URef is stored.</param>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public GetDictionaryItemByContract(string contractKey, string dictionaryName, string dictionaryItem,
            string stateRootHash) : base("state_get_dictionary_item")
        {
            var contractNamedKey = new Dictionary<string, string>
            {
                {"key", contractKey},
                {"dictionary_name", dictionaryName},
                {"dictionary_item_key", dictionaryItem}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {
                    "dictionary_identifier", new Dictionary<string, object>
                    {
                        {"ContractNamedKey", contractNamedKey}
                    }
                },
                {"state_root_hash", stateRootHash},
            };
        }
    }

    public class GetDictionaryItemByURef : RpcMethod
    {
        /// <summary>
        /// Lookup a dictionary item via its seed URef.
        /// </summary>
        /// <param name="seedURef">The dictionary's seed URef.</param>
        /// <param name="dictionaryItem">The dictionary item key.</param>
        /// <param name="stateRootHash">Hash of the state root.</param>
        public GetDictionaryItemByURef(string seedURef, string dictionaryItem, string stateRootHash) : base(
            "state_get_dictionary_item")
        {
            var contractNamedKey = new Dictionary<string, string>
            {
                {"seed_uref", seedURef},
                {"dictionary_item_key", dictionaryItem}
            };

            this.Parameters = new Dictionary<string, object>
            {
                {
                    "dictionary_identifier", new Dictionary<string, object>
                    {
                        {"URef", contractNamedKey}
                    }
                },
                {"state_root_hash", stateRootHash},
            };
        }
    }

    public class GetValidatorChanges : RpcMethod
    {
        /// <summary>
        /// Returns status changes of active validators
        /// </summary>
        public GetValidatorChanges() : base("info_get_validator_changes")
        {
        }
    }

    public class GetValidatorReward : RpcMethod
    {
        /// <summary>
        /// Returns the reward for a given era and a validator
        /// </summary>
        /// <param name="validator">The public key of the validator.</param>
        /// <param name="blockIdentifier">The identifier for the state used for the query, if none is passed, the latest block will be used.</param>
        public GetValidatorReward(PublicKey validator, IBlockIdentifier blockIdentifier = null) : base("info_get_reward")
        {
            this.Parameters = new Dictionary<string, object>
            {
                { "validator", validator.ToString() }
            };

            if (blockIdentifier != null)
                this.Parameters.Add("era_identifier", new Dictionary<string, object>
                {
                    { "Block", blockIdentifier.GetBlockIdentifier() }
                });
        }
        
        /// <summary>
        /// Returns the reward for a given era and a validator
        /// </summary>
        /// <param name="validator">The public key of the validator.</param>
        /// <param name="eraId">Id of the Era to retrieve the rewards from.</param>
        public GetValidatorReward(PublicKey validator, ulong eraId ) : base("info_get_reward")
        {
            this.Parameters = new Dictionary<string, object>
            {
                { "validator", validator.ToString() },
                { "era_identifier", new Dictionary<string, object>
                    {
                        { "Era", eraId }
                    }
                }
            };
        }
    }
    
    public class GetDelegatorReward : RpcMethod
    {
        /// <summary>
        /// Returns the reward for a given era and a delegator
        /// </summary>
        /// <param name="validator">The public key of the validator.</param>
        /// <param name="delegator">The public key of the delegator.</param>
        /// <param name="blockIdentifier">The identifier for the state used for the query, if none is passed, the latest block will be used.</param>
        public GetDelegatorReward(PublicKey validator, PublicKey delegator, IBlockIdentifier blockIdentifier = null) : base("info_get_reward")
        {
            this.Parameters = new Dictionary<string, object>
            {
                { "validator", validator.ToString() },
                { "delegator", delegator.ToString() }
            };

            if (blockIdentifier != null)
                this.Parameters.Add("era_identifier", new Dictionary<string, object>
                {
                    { "Block", blockIdentifier.GetBlockIdentifier() }
                });
        }
        
        /// <summary>
        /// Returns the reward for a given era and a delegator
        /// </summary>
        /// <param name="validator">The public key of the validator.</param>
        /// <param name="delegator">The public key of the delegator.</param>
        /// <param name="eraId">Id of the Era to retrieve the rewards from.</param>
        public GetDelegatorReward(PublicKey validator, PublicKey delegator, ulong eraId ) : base("info_get_reward")
        {
            this.Parameters = new Dictionary<string, object>
            {
                { "validator", validator.ToString() },
                { "delegator", delegator.ToString() },
                { "era_identifier", new Dictionary<string, object>
                    {
                        { "Era", eraId }
                    }
                }
            };
        }
    }
    
    public class GetRpcSchema : RpcMethod
    {
        /// <summary>
        /// Returns the RPC Json schema.
        /// </summary>
        public GetRpcSchema() : base("rpc.discover")
        {
        }
    }

    public class GetChainspec : RpcMethod
    {
        /// <summary>
        /// Returns the chainspec.toml file of the node.
        /// </summary>
        public GetChainspec() : base("info_get_chainspec")
        {
        }
    }

    public class SpeculativeExecution : RpcMethod
    {
        /// <summary>
        /// Sends a "deploy dry run" to the network. It will execute the deploy on top of the specified block and return
        /// the results of the execution to the caller. The effects of the execution won't be committed to the trie
        /// (blockchain database/GlobalState).
        /// Endpoint can be used for debugging, discovery - for example price estimation.
        /// </summary>
        public SpeculativeExecution(Deploy deploy, string hash, bool isBlockHash) : base("speculative_exec")
        {
            this.Parameters = new Dictionary<string, object>
            {
                {"deploy", deploy}
            };
            if (hash != null)
            {
                this.Parameters.Add("state_identifier", new Dictionary<string, string>
                {
                    {isBlockHash ? "BlockHash" : "StateRootHash", hash}
                });
            }
        }
    }
}
