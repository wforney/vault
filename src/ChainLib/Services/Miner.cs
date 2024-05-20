namespace ChainLib.Services
{
    using ChainLib.Models;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    public class Miner
    {
        private readonly IBlockchain<Block> _blockchain;
        private readonly IProofOfWork _proofOfWork;
        private readonly ILogger<Miner> _logger;

        public Miner(IBlockchain blockchain, IProofOfWork proofOfWork, ILogger<Miner> logger)
        {
            this._blockchain = blockchain;
            this._proofOfWork = proofOfWork;
            this._logger = logger;
        }

        public async Task<Block> MineAsync(byte[] address)
        {
            Block lastBlock = await this._blockchain.GetLastBlockAsync();
            Block baseBlock = GenerateNextBlock(lastBlock);
            uint difficulty = this._blockchain.GetDifficulty(baseBlock.Index.GetValueOrDefault());

            return this._proofOfWork.ProveWorkFor(baseBlock, difficulty);
        }

        private static Block GenerateNextBlock(Block previousBlock)
        {
            long? index = previousBlock.Index + 1;
            byte[] previousHash = previousBlock.Hash;
            uint timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return new Block
            {
                Index = index,
                Nonce = 0,
                PreviousHash = previousHash,
                Timestamp = timestamp
            };
        }
    }
}