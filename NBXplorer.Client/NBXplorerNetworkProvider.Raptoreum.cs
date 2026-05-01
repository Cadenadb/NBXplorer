using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;

namespace NBXplorer
{
    /// <summary>
    /// NBXplorer network provider for Raptoreum (RTM).
    /// Raptoreum is based on Dash and uses the GhostRider proof-of-work algorithm.
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
    ///   PubKey address:  60  → addresses start with 'R'
    ///   Script address:  16  → addresses start with '7'
    ///   Secret key:      128
    ///   BIP44 coin type: 200
    ///
    /// NOTE: Raptoreum uses the GhostRider PoW algorithm. Block header hashing
    /// uses SHA256D as a placeholder for development. A full GhostRider
    /// implementation in C# is required for production block validation.
    /// NBXplorer trusts the connected full node for chain validation,
    /// so this does not affect basic payment processing functionality.
    /// </summary>
    public class Raptoreum : NetworkSetBase
    {
        public static Raptoreum Instance { get; } = new Raptoreum();

        public override string CryptoCode => "RTM";

        private Raptoreum() { }

        // ── Mainnet ──────────────────────────────────────────────────────────
        protected override NetworkBuilder CreateMainnet()
        {
            var builder = new NetworkBuilder();
            builder.SetName("raptoreum-main")
                   .AddAlias("RTM-mainnet")
                   .AddAlias("raptoreum")
                   .SetConsensusFactory(RaptoreumConsensusFactory.Instance)
                   // Magic bytes: r=0x72, t=0x74, m=0x6d, .=0x2e
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
            builder.SetName("raptoreum-test")
                   .AddAlias("RTM-testnet")
                   .SetConsensusFactory(RaptoreumConsensusFactory.Instance)
                   .SetMagic(0x74746d72)   // "rtmt"
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
            builder.SetName("raptoreum-reg")
                   .AddAlias("RTM-regtest")
                   .SetConsensusFactory(RaptoreumConsensusFactory.Instance)
                   .SetMagic(0x726d7472)   // "rtmr"
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

        // ── Consensus factory ────────────────────────────────────────────────
        public class RaptoreumConsensusFactory : ConsensusFactory
        {
            public static RaptoreumConsensusFactory Instance { get; } = new RaptoreumConsensusFactory();

            private RaptoreumConsensusFactory() { }

            public override BlockHeader CreateBlockHeader() => new RaptoreumBlockHeader();
            public override Block CreateBlock() => new Block(new RaptoreumBlockHeader());

            public override ProtocolCapabilities GetProtocolCapabilities(uint protocolVersion)
            {
                var caps = base.GetProtocolCapabilities(protocolVersion);
                // Raptoreum (Dash-based) does not support SegWit witness
                caps.SupportWitness = false;
                return caps;
            }
        }

        // ── Block header ─────────────────────────────────────────────────────
        /// <summary>
        /// Raptoreum block header.
        /// TODO: Replace SHA256D with GhostRider algorithm for production use.
        /// GhostRider is a complex multi-algorithm PoW. For development and
        /// payment processing purposes, SHA256D is sufficient since NBXplorer
        /// delegates chain validation to the full node via RPC.
        /// </summary>
        public class RaptoreumBlockHeader : BlockHeader
        {
            // Using default SHA256D from base class as placeholder.
            // GhostRider implementation pending.
        }
    }
}