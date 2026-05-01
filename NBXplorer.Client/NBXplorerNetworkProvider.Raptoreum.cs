using System;
using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;

namespace NBXplorer
{
    /// <summary>
    /// NBXplorer network provider for Raptoreum (RTM).
    /// Raptoreum is a Dash-based cryptocurrency using the GhostRider proof-of-work algorithm.
    /// Reference: https://github.com/Raptor3um/raptoreum/blob/master/src/chainparams.cpp
    /// </summary>
    public partial class NBXplorerNetworkProvider
    {
        private void InitRaptoreum(ChainName networkType)
        {
            Add(new NBXplorerNetwork(Raptoreum.Instance, networkType)
            {
                MinRPCVersion = 140200,
                CoinType = networkType == ChainName.Mainnet ? new KeyPath("200'") : new KeyPath("1'")
            });
        }

        public NBXplorerNetwork GetRTM()
        {
            return GetFromCryptoCode(Raptoreum.Instance.CryptoCode);
        }
    }

    /// <summary>
    /// Raptoreum (RTM) network definition for NBitcoin.
    /// Based on Dash chainparams with RTM-specific parameters.
    ///
    /// Mainnet parameters (from chainparams.cpp):
    ///   Magic:           0x72 0x74 0x6d 0x2e  ("rtm.")
    ///   P2P Port:        10226
    ///   RPC Port:        9998
    ///   PubKey address:  60  → addresses start with 'r'
    ///   Script address:  16  → addresses start with '7'
    ///   Secret key:      128
    ///   BIP44 coin type: 200
    ///   Block spacing:   2 minutes
    ///   Genesis hash:    0xb79e5df07278b9567ada8fc655ffbfa9d3f586dc38da3dd93053686f41caeea0
    ///   Genesis ts:      "The Times 22/Jan/2018 Raptoreum is name of the game for new generation of firms"
    ///
    /// NOTE: Raptoreum uses the GhostRider PoW algorithm. Block header hashing
    /// uses SHA256D as a placeholder for development. A full GhostRider
    /// implementation in C# is required for production block validation.
    /// NBXplorer trusts the connected full node for chain validation via RPC,
    /// so this does not affect basic payment processing functionality.
    /// </summary>
    public class Raptoreum : NetworkSetBase
    {
        public static Raptoreum Instance { get; } = new Raptoreum();

        public override string CryptoCode => "RTM";

        private Raptoreum() { }

        // ── Consensus factory ────────────────────────────────────────────────
        public class RaptoreumConsensusFactory : ConsensusFactory
        {
            public static RaptoreumConsensusFactory Instance { get; } = new RaptoreumConsensusFactory();

            private RaptoreumConsensusFactory() { }

#pragma warning disable CS0618 // obsolete constructors used intentionally
            public override BlockHeader CreateBlockHeader() => new RaptoreumBlockHeader();
            public override Block CreateBlock() => new RaptoreumBlock(new RaptoreumBlockHeader());
#pragma warning restore CS0618

            public override ProtocolCapabilities GetProtocolCapabilities(uint protocolVersion)
            {
                var caps = base.GetProtocolCapabilities(protocolVersion);
                // Raptoreum (Dash-based) does not support SegWit witness
                caps.SupportWitness = false;
                return caps;
            }
        }

#pragma warning disable CS0618 // obsolete BlockHeader/Block constructors
        // ── Block header ─────────────────────────────────────────────────────
        /// <summary>
        /// Raptoreum block header.
        /// TODO: Replace SHA256D with GhostRider algorithm for production use.
        /// NBXplorer delegates chain validation to the full node via RPC,
        /// so SHA256D is sufficient for payment processing purposes.
        /// </summary>
        public class RaptoreumBlockHeader : BlockHeader
        {
            // SHA256D placeholder — GhostRider implementation pending for production.
        }

        // ── Block ─────────────────────────────────────────────────────────────
        public class RaptoreumBlock : Block
        {
            public RaptoreumBlock(RaptoreumBlockHeader header) : base(header) { }

            public override ConsensusFactory GetConsensusFactory()
            {
                return RaptoreumConsensusFactory.Instance;
            }
        }
#pragma warning restore CS0618

        // ── Mainnet ──────────────────────────────────────────────────────────
        // Genesis: CreateGenesisBlock(1614369600, 1130, 0x20001fff, 4, 5000*COIN)
        // Hash:    0xb79e5df07278b9567ada8fc655ffbfa9d3f586dc38da3dd93053686f41caeea0
        // Merkle:  0x87a48bc22468acdd72ee540aab7c086a5bbcddc12b51c6ac925717a74c269453
        protected override NetworkBuilder CreateMainnet()
        {
            var builder = new NetworkBuilder();
            builder.SetConsensus(new Consensus()
                   {
                       SubsidyHalvingInterval      = 210240,
                       MajorityEnforceBlockUpgrade = 750,
                       MajorityRejectBlockOutdated = 950,
                       MajorityWindow              = 1000,
                       BIP34Hash                   = new uint256("0xb79e5df07278b9567ada8fc655ffbfa9d3f586dc38da3dd93053686f41caeea0"),
                       PowLimit                    = new Target(new uint256("00ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                       MinimumChainWork            = new uint256("000000000000000000000000000000000000000000000000000eead474ccbc59"),
                       PowTargetTimespan           = TimeSpan.FromSeconds(24 * 60 * 60), // 1 day
                       PowTargetSpacing            = TimeSpan.FromSeconds(2 * 60),        // 2 minutes
                       PowAllowMinDifficultyBlocks = false,
                       CoinbaseMaturity            = 100,
                       PowNoRetargeting            = false,
                       RuleChangeActivationThreshold = 1916,
                       MinerConfirmationWindow     = 2016,
                       ConsensusFactory            = RaptoreumConsensusFactory.Instance,
                       SupportSegwit               = false
                   })
                   .SetName("raptoreum-main")
                   .AddAlias("RTM-mainnet")
                   .AddAlias("raptoreum")
                   // Magic bytes: r=0x72, t=0x74, m=0x6d, .=0x2e (little-endian)
                   .SetMagic(0x2e6d7472)
                   .SetPort(10226)
                   .SetRPCPort(10225)
                   .SetMaxP2PVersion(70220)
                   .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS,  new byte[] { 60  })
                   .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS,  new byte[] { 16  })
                   .SetBase58Bytes(Base58Type.SECRET_KEY,      new byte[] { 128 })
                   .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY,  new byte[] { 0x04, 0x88, 0xB2, 0x1E })
                   .SetBase58Bytes(Base58Type.EXT_SECRET_KEY,  new byte[] { 0x04, 0x88, 0xAD, 0xE4 })
                   .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("rtm"))
                   .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("rtm"))
                   .AddDNSSeeds(new[]
                   {
                       new DNSSeedData("lbdn.raptoreum.com", "lbdn.raptoreum.com"),
                   })
                   .AddSeeds(new NetworkAddress[0])
                   .SetGenesis("0400000000000000000000000000000000000000000000000000000000000000000000005394264ca7175792acc6512bc1ddbc5b6a087cab0a54ee72ddac6824c28ba48740533960ff1f00206a0400000101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff5804ffff001d01044c4f5468652054696d65732032322f4a616e2f3230313820526170746f7265756d206973206e616d65206f66207468652067616d6520666f72206e65772067656e65726174696f6e206f66206669726d73ffffffff010088526a740000004341040184710fa689ad5023690c80f3a49c8f13f8d45b8c857fbcbc8bc4a8e4d3eb4b10f4d4604fa08dce601aaf0f470216fe1b51850b4acf21b179c45070ac7b03a9ac00000000");
            return builder;
        }

        // ── Testnet ──────────────────────────────────────────────────────────
        // Genesis: CreateGenesisBlock(1711078237, 971, 0x20001fff, 4, 5000*COIN)
        // Hash:    0xbbab22066081d3b466abd734de914e8092abf4e959bcd0fff978297c41591b23
        // Merkle:  0x87a48bc22468acdd72ee540aab7c086a5bbcddc12b51c6ac925717a74c269453
        protected override NetworkBuilder CreateTestnet()
        {
            var builder = new NetworkBuilder();
            builder.SetConsensus(new Consensus()
                   {
                       SubsidyHalvingInterval      = 210240,
                       MajorityEnforceBlockUpgrade = 51,
                       MajorityRejectBlockOutdated = 75,
                       MajorityWindow              = 100,
                       BIP34Hash                   = new uint256("0xbbab22066081d3b466abd734de914e8092abf4e959bcd0fff978297c41591b23"),
                       PowLimit                    = new Target(new uint256("00ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                       PowTargetTimespan           = TimeSpan.FromSeconds(24 * 60 * 60),
                       PowTargetSpacing            = TimeSpan.FromSeconds(2 * 60),
                       PowAllowMinDifficultyBlocks = true,
                       CoinbaseMaturity            = 100,
                       PowNoRetargeting            = false,
                       RuleChangeActivationThreshold = 1512,
                       MinerConfirmationWindow     = 2016,
                       ConsensusFactory            = RaptoreumConsensusFactory.Instance,
                       SupportSegwit               = false
                   })
                   .SetName("raptoreum-test")
                   .AddAlias("RTM-testnet")
                   .SetMagic(0x74746d72)   // "rtmt" little-endian
                   .SetPort(11226)
                   .SetRPCPort(19998)
                   .SetMaxP2PVersion(70220)
                   .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS,  new byte[] { 140 })
                   .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS,  new byte[] { 19  })
                   .SetBase58Bytes(Base58Type.SECRET_KEY,      new byte[] { 239 })
                   .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY,  new byte[] { 0x04, 0x35, 0x87, 0xCF })
                   .SetBase58Bytes(Base58Type.EXT_SECRET_KEY,  new byte[] { 0x04, 0x35, 0x83, 0x94 })
                   .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("trtm"))
                   .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("trtm"))
                   .AddDNSSeeds(new DNSSeedData[0])
                   .AddSeeds(new NetworkAddress[0])
                   .SetGenesis("0400000000000000000000000000000000000000000000000000000000000000000000005394264ca7175792acc6512bc1ddbc5b6a087cab0a54ee72ddac6824c28ba4875dfbfc65ff1f0020cb0300000101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff5804ffff001d01044c4f5468652054696d65732032322f4a616e2f3230313820526170746f7265756d206973206e616d65206f66207468652067616d6520666f72206e65772067656e65726174696f6e206f66206669726d73ffffffff010088526a740000004341040184710fa689ad5023690c80f3a49c8f13f8d45b8c857fbcbc8bc4a8e4d3eb4b10f4d4604fa08dce601aaf0f470216fe1b51850b4acf21b179c45070ac7b03a9ac00000000");
            return builder;
        }

        // ── Regtest ──────────────────────────────────────────────────────────
        // Genesis: CreateGenesisBlock(1614369600, 2, 0x207fffff, 4, 5000*COIN)
        // Hash:    0x485491468e03c8ac23dd38f70fc1cda9f98cbd0bf58945e2da6c94c2a2d8b044
        // Merkle:  0x87a48bc22468acdd72ee540aab7c086a5bbcddc12b51c6ac925717a74c269453
        protected override NetworkBuilder CreateRegtest()
        {
            var builder = new NetworkBuilder();
            builder.SetConsensus(new Consensus()
                   {
                       SubsidyHalvingInterval      = 150,
                       MajorityEnforceBlockUpgrade = 750,
                       MajorityRejectBlockOutdated = 950,
                       MajorityWindow              = 1000,
                       PowLimit                    = new Target(new uint256("7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                       PowTargetTimespan           = TimeSpan.FromSeconds(24 * 60 * 60),
                       PowTargetSpacing            = TimeSpan.FromSeconds(2 * 60),
                       PowAllowMinDifficultyBlocks = true,
                       CoinbaseMaturity            = 100,
                       PowNoRetargeting            = true,
                       RuleChangeActivationThreshold = 108,
                       MinerConfirmationWindow     = 144,
                       ConsensusFactory            = RaptoreumConsensusFactory.Instance,
                       SupportSegwit               = false
                   })
                   .SetName("raptoreum-reg")
                   .AddAlias("RTM-regtest")
                   .SetMagic(0xdcb7c1fc)   // regtest magic from chainparams: fc=0xfc, c1, b7, dc
                   .SetPort(19899)
                   .SetRPCPort(19225)
                   .SetMaxP2PVersion(70220)
                   .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS,  new byte[] { 140 })
                   .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS,  new byte[] { 19  })
                   .SetBase58Bytes(Base58Type.SECRET_KEY,      new byte[] { 239 })
                   .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY,  new byte[] { 0x04, 0x35, 0x87, 0xCF })
                   .SetBase58Bytes(Base58Type.EXT_SECRET_KEY,  new byte[] { 0x04, 0x35, 0x83, 0x94 })
                   .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("rrtm"))
                   .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("rrtm"))
                   .AddDNSSeeds(new DNSSeedData[0])
                   .AddSeeds(new NetworkAddress[0])
                   .SetGenesis("0400000000000000000000000000000000000000000000000000000000000000000000005394264ca7175792acc6512bc1ddbc5b6a087cab0a54ee72ddac6824c28ba48740533960ffff7f20020000000101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff5804ffff001d01044c4f5468652054696d65732032322f4a616e2f3230313820526170746f7265756d206973206e616d65206f66207468652067616d6520666f72206e65772067656e65726174696f6e206f66206669726d73ffffffff010088526a740000004341040184710fa689ad5023690c80f3a49c8f13f8d45b8c857fbcbc8bc4a8e4d3eb4b10f4d4604fa08dce601aaf0f470216fe1b51850b4acf21b179c45070ac7b03a9ac00000000");
            return builder;
        }
    }
}
