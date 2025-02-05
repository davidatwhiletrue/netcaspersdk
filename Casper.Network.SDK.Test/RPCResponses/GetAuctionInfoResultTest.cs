using System.IO;
using System.Linq;
using System.Numerics;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace NetCasperTest.RPCResponses
{
    public class GetAuctionInfoResultTest
    {
        [Test]
        public void GetBlockResultTest_v156()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-auction-info-v156.json");
            
            var result = RpcResult.Parse<GetAuctionInfoResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("1.5.6", result.ApiVersion);
            Assert.AreEqual("436d8e272b759838e6416d915efb3adc3a102489b8db1c0ce731043116c6f645", result.AuctionState.StateRootHash);
            Assert.AreEqual(3192405, result.AuctionState.BlockHeight);
            Assert.AreEqual(1, result.AuctionState.EraValidators.Count);
            Assert.AreEqual(13801, result.AuctionState.EraValidators[0].EraId);
            Assert.AreEqual(5, result.AuctionState.EraValidators[0].ValidatorWeights.Count);
            Assert.AreEqual("020377bc3ad54b5505971e001044ea822a3f6f307f8dc93fa45a05b7463c0a053bed", result.AuctionState.EraValidators[0].ValidatorWeights[4].PublicKey.ToString().ToLower());
            Assert.AreEqual(BigInteger.Parse("10567495110201092"), result.AuctionState.EraValidators[0].ValidatorWeights[4].Weight);
            
            Assert.AreEqual(3, result.AuctionState.Bids.Count);
            Assert.AreEqual("01001b79b9a6e13d2b96e916f7fa7dff40496ba5188479263ca0fb2ccf8b714305", result.AuctionState.Bids[0].Unified.PublicKey.ToString().ToLower());
            Assert.AreEqual(1, result.AuctionState.Bids[0].Unified.Delegators.Count);
            Assert.AreEqual("018b34b15e023844531621cb52d42e216a2ea56034f0f40bf7cee566c32eae4f83", result.AuctionState.Bids[0].Unified.Delegators[0].DelegatorPublicKey.ToString().ToLower());
            Assert.AreEqual(BigInteger.Parse("30268476029"), result.AuctionState.Bids[0].Unified.Delegators[0].StakedAmount);
            Assert.IsNull(result.AuctionState.Bids[0].Unified.Delegators[0].VestingSchedule);
        }
        
        [Test]
        public void GetBlockResultTest_v200()
        {
            string json = File.ReadAllText(TestContext.CurrentContext.TestDirectory +
                                           "/TestData/get-auction-info-v200.json");
            
            var result = RpcResult.Parse<GetAuctionInfoResult>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual("2.0.0", result.ApiVersion);
            Assert.AreEqual("c8228d6d6d45151766901ba3579461847a17db15a66ce9ef6ae2f3e3abffd132", result.AuctionState.StateRootHash);
            Assert.AreEqual(3480973, result.AuctionState.BlockHeight);
            Assert.AreEqual(3, result.AuctionState.EraValidators.Count);
            Assert.AreEqual(14912, result.AuctionState.EraValidators[2].EraId);
            Assert.AreEqual(4, result.AuctionState.EraValidators[2].ValidatorWeights.Count);
            Assert.AreEqual("017536433a73f7562526f3e9fcb8d720428ae2d28788a9909f3c6f637a9d848a4b", result.AuctionState.EraValidators[2].ValidatorWeights[3].PublicKey.ToString().ToLower());
            Assert.AreEqual(BigInteger.Parse("2030445261010189498"), result.AuctionState.EraValidators[2].ValidatorWeights[3].Weight);
            
            Assert.AreEqual(4, result.AuctionState.Bids.Count);
            
            Assert.IsNotNull(result.AuctionState.Bids[0].Validator);
            Assert.AreEqual("01358a7e107668ae2eb092dcfbeb97d2ec3cc8354d2a77bc8f232fff6630a826c3", result.AuctionState.Bids[0].Validator.PublicKey.ToString().ToLower());
            Assert.AreEqual("uref-027d909fa0818f8b426b905795f608a6301168476b8013d7b3f682786796096f-007", result.AuctionState.Bids[0].Validator.BondingPurse.ToString());
            Assert.AreEqual(500000000000, result.AuctionState.Bids[0].Validator.MinimumDelegationAmount);
            Assert.AreEqual(1000000000000000000, result.AuctionState.Bids[0].Validator.MaximumDelegationAmount);
            Assert.AreEqual(3, result.AuctionState.Bids[0].Validator.ReservedSlots);
            Assert.AreEqual(1, result.AuctionState.Bids[0].Validator.DelegationRate);
            Assert.AreEqual(BigInteger.Parse("2500000000"), result.AuctionState.Bids[0].Validator.StakedAmount);
            Assert.AreEqual(true, result.AuctionState.Bids[0].Validator.Inactive);
            
            Assert.IsNotNull(result.AuctionState.Bids[3].Delegator);
            Assert.AreEqual("01032146b0b9de01e26aaec7b0d1769920de94681dbd432c3530bfe591752ded6c", result.AuctionState.Bids[3].Delegator.ValidatorPublicKey.ToString().ToLower());
            Assert.AreEqual("uref-d34ee21f5fe61feee2d9f15e0e369367aba62ba1b689e59c1e6ddc581ca99fdf-007", result.AuctionState.Bids[3].Delegator.BondingPurse.ToString());
            Assert.AreEqual(BigInteger.Parse("1676515877735"), result.AuctionState.Bids[3].Delegator.StakedAmount);
            Assert.IsNull(result.AuctionState.Bids[3].Delegator.DelegatorKind.PublicKey);
            Assert.AreEqual("8af7b77811970792f98b806779dfc0d1a9fef5bad205c6be8bb884210d7d323c", result.AuctionState.Bids[3].Delegator.DelegatorKind.Purse);
        }
    }
}