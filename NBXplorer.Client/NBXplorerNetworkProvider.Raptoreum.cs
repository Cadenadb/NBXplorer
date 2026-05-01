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
        /// GhostRider is a complex multi-algorithm PoW. For development and
        /// payment processing purposes, SHA256D is sufficient because NBXplorer
        /// delegates chain validation to the full node via RPC.
        /// </summary>
        public class RaptoreumBlockHeader : BlockHeader
        {
            // Using default SHA256D from base class as a placeholder.
            // Full GhostRider implementation is pending for production.
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
        protected override NetworkBuilder CreateMainnet()
        {
            var builder = new NetworkBuilder();
            builder.SetConsensus(new Consensus()
                   {
                       SubsidyHalvingInterval    = 210240,
                       MajorityEnforceBlockUpgrade = 750,
                       MajorityRejectBlockOutdated = 950,
                       MajorityWindow            = 1000,
                       BIP34Hash                 = new uint256("0xb79e5df07278b9567ada8fc655ffbfa9d3f586dc38da3dd93053686f41caeea0"),
                       PowLimit                  = new Target(new uint256("00ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                       MinimumChainWork          = new uint256("000000000000000000000000000000000000000000000000000eead474ccbc59"),
                       PowTargetTimespan         = TimeSpan.FromSeconds(24 * 60 * 60), // 1 day
                       PowTargetSpacing          = TimeSpan.FromSeconds(2 * 60),        // 2 minutes
                       PowAllowMinDifficultyBlocks = false,
                       CoinbaseMaturity          = 100,
                       PowNoRetargeting          = false,
                       RuleChangeActivationThreshold = 1916,
                       MinerConfirmationWindow   = 2016,
                       ConsensusFactory          = RaptoreumConsensusFactory.Instance,
                       SupportSegwit             = false
                   })
                   .SetName("raptoreum-main")
                   .AddAlias("RTM-mainnet")
                   .AddAlias("raptoreum")
                   // Magic bytes: r=0x72, t=0x74, m=0x6d, .=0x2e  (little-endian: 0x2e6d7472)
                   .SetMagic(0x2e6d7472)
                   .SetPort(10226)
                   .SetRPCPort(9998)
                   .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS,  new byte[] { 60  })
                   .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS,  new byte[] { 16  })
                   .SetBase58Bytes(Base58Type.SECRET_KEY,      new byte[] { 128 })
                   .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY,  new byte[] { 0x04, 0x88, 0xB2, 0x1E })
                   .SetBase58Bytes(Base58Type.EXT_SECRET_KEY,  new byte[] { 0x04, 0x88, 0xAD, 0xE4 })
                   .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("rtm"))
                   .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("rtm"));
            return builder;
        }

        // ── Testnet ──────────────────────────────────────────────────────────
        protected override NetworkBuilder CreateTestnet()
        {
            var builder = new NetworkBuilder();
            builder.SetConsensus(new Consensus()
                   {
                       SubsidyHalvingInterval    = 210240,
                       MajorityEnforceBlockUpgrade = 51,
                       MajorityRejectBlockOutdated = 75,
                       MajorityWindow            = 100,
                       PowLimit                  = new Target(new uint256("00000fffff000000000000000000000000000000000000000000000000000000")),
                       PowTargetTimespan         = TimeSpan.FromSeconds(24 * 60 * 60),
                       PowTargetSpacing          = TimeSpan.FromSeconds(2 * 60),
                       PowAllowMinDifficultyBlocks = true,
                       CoinbaseMaturity          = 100,
                       PowNoRetargeting          = false,
                       RuleChangeActivationThreshold = 1512,
                       MinerConfirmationWindow   = 2016,
                       ConsensusFactory          = RaptoreumConsensusFactory.Instance,
                       SupportSegwit             = false
                   })
                   .SetName("raptoreum-test")
                   .AddAlias("RTM-testnet")
                   .SetMagic(0x74746d72)   // "rtmt" little-endian
                   .SetPort(11226)
                   .SetRPCPort(19998)
                   .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS,  new byte[] { 140 })
                   .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS,  new byte[] { 19  })
                   .SetBase58Bytes(Base58Type.SECRET_KEY,      new byte[] { 239 })
                   .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY,  new byte[] { 0x04, 0x35, 0x87, 0xCF })
                   .SetBase58Bytes(Base58Type.EXT_SECRET_KEY,  new byte[] { 0x04, 0x35, 0x83, 0x94 })
                   .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("trtm"))
                   .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("trtm"));
            return builder;
        }

        // ── Regtest ──────────────────────────────────────────────────────────
        protected override NetworkBuilder CreateRegtest()
        {
            var builder = new NetworkBuilder();
            builder.SetConsensus(new Consensus()
                   {
                       SubsidyHalvingInterval    = 150,
                       MajorityEnforceBlockUpgrade = 750,
                       MajorityRejectBlockOutdated = 950,
                       MajorityWindow            = 1000,
                       PowLimit                  = new Target(new uint256("7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                       PowTargetTimespan         = TimeSpan.FromSeconds(24 * 60 * 60),
                       PowTargetSpacing          = TimeSpan.FromSeconds(2 * 60),
                       PowAllowMinDifficultyBlocks = true,
                       CoinbaseMaturity          = 100,
                       PowNoRetargeting          = true,
                       RuleChangeActivationThreshold = 108,
                       MinerConfirmationWindow   = 144,
                       ConsensusFactory          = RaptoreumConsensusFactory.Instance,
                       SupportSegwit             = false
                   })
                   .SetName("raptoreum-reg")
                   .AddAlias("RTM-regtest")
                   .SetMagic(0x726d7472)   // "rtmr" little-endian
                   .SetPort(12226)
                   .SetRPCPort(29998)
                   .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS,  new byte[] { 140 })
                   .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS,  new byte[] { 19  })
                   .SetBase58Bytes(Base58Type.SECRET_KEY,      new byte[] { 239 })
                   .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY,  new byte[] { 0x04, 0x35, 0x87, 0xCF })
                   .SetBase58Bytes(Base58Type.EXT_SECRET_KEY,  new byte[] { 0x04, 0x35, 0x83, 0x94 })
                   .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("rrtm"))
                   .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("rrtm"));
            return builder;
        }
    }
}
