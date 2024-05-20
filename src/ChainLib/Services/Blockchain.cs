namespace ChainLib.Services
{
    using ChainLib.Exceptions;
    using ChainLib.Extensions;
    using ChainLib.Models;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Blockchain : IBlockchain
    {
        private readonly IBlockStore<Block> _blocks;
        private readonly IProofOfWork _proofOfWork;
        private readonly IHashProvider _hashProvider;
        private readonly ILogger _logger;

        public Blockchain(IBlockStore<Block> blocks, IProofOfWork proofOfWork, IHashProvider hashProvider, ILogger<Blockchain> logger)
        {
            this._blocks = blocks;
            this._hashProvider = hashProvider;
            this._logger = logger;
            this._proofOfWork = proofOfWork;

            this.Init();
        }

        public void Init()
        {
            // Create the genesis block if the blockchain is empty
            long height = this._blocks.GetLengthAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (height == 0)
            {
                Block genesisBlock = this._blocks.GetGenesisBlockAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                this._blocks.AddAsync(genesisBlock).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        public IEnumerable<BlockHeader> StreamAllBlockHeaders(bool forwards, long startingAt = 0) => this._blocks.StreamAllBlockHeaders(forwards, startingAt);

        public IEnumerable<Block> StreamAllBlocks(bool forwards, long startingAt = 0) => this._blocks.StreamAllBlocks(forwards, startingAt);

        public IEnumerable<BlockObject> StreamAllBlockObjects(bool forwards, long startingAt = 0) => this._blocks.StreamAllBlockObjects(forwards, startingAt);

        public async Task<Block> GetBlockByIndexAsync(long index) => await this._blocks.GetByIndexAsync(index);

        public async Task<Block> GetBlockByHashAsync(byte[] hash) => await this._blocks.GetByHashAsync(hash);

        public async Task<Block> GetLastBlockAsync() => await this._blocks.GetLastBlockAsync();

        public uint GetDifficulty(long index) => this._proofOfWork.GetDifficulty(index);

        public async Task ReplaceChainAsync(List<Block> newBlockchain)
        {
            // It doesn't make sense to replace this blockchain by a smaller one
            if (newBlockchain.Count <= await this._blocks.GetLengthAsync())
            {
                string message = $"Blockchain shorter than the current blockchain";
                this._logger?.LogError(message);
                throw new BlockchainAssertionException(message);
            }

            // Verify if the new blockchain is correct
            _ = await this.ChainIsValid(newBlockchain);

            // Get the blocks that diverge from our blockchain
            this._logger?.LogInformation($"Received blockchain is valid. Replacing current blockchain with received blockchain");

            // Get the blocks that diverge from our blockchain
            int start = (int)(newBlockchain.Count - await this._blocks.GetLengthAsync());
            foreach (Block block in newBlockchain.Skip(start))
            {
                _ = await this.AddBlockAsync(block);
            }
        }

        public async Task<bool> ChainIsValid(IReadOnlyList<Block> blockchainToValidate)
        {
            // Check if the genesis block is the same
            if (this._hashProvider.ComputeHashString(blockchainToValidate[0]) !=
                this._hashProvider.ComputeHashString(await this._blocks.GetGenesisBlockAsync()))
            {
                string message = $"Genesis blocks aren't the same";
                this._logger?.LogError(message);
                throw new BlockchainAssertionException(message);
            }

            // Compare every block to the previous one (it skips the first one, because it was verified before)
            try
            {
                for (int i = 1; i < blockchainToValidate.Count; i++)
                {
                    _ = this.BlockIsValid(blockchainToValidate[i], blockchainToValidate[i - 1]);
                }
            }
            catch (Exception ex)
            {
                string message = $"Invalid block sequence";
                this._logger?.LogError(message);
                throw new BlockchainAssertionException(message, ex);
            }

            return true;
        }

        public async Task<Block> AddBlockAsync(Block block)
        {
            if (this.BlockIsValid(block, await this.GetLastBlockAsync()))
            {
                await this._blocks.AddAsync(block);

                this._logger?.LogInformation($"Block added: {block.Hash}");
                this._logger?.LogDebug($"Block added: {JsonConvert.SerializeObject(block)}");

                return block;
            }

            return null;
        }

        public bool BlockIsValid(Block newBlock, Block previousBlock)
        {
            if (previousBlock.Index + 1 != newBlock.Index)
            { // Check if the block is the last one
                string message = $"Invalid index: expected '{previousBlock.Index + 1}' but got '{newBlock.Index}'";
                this._logger?.LogError(message);
                throw new BlockAssertionException(message);
            }

            if (previousBlock.Hash != newBlock.PreviousHash)
            { // Check if the previous block is correct
                string message = $"Invalid previoushash: expected '{previousBlock.Hash}' got '{newBlock.PreviousHash}'";
                this._logger?.LogError(message);
                throw new BlockAssertionException(message);
            }

            byte[] blockHash = newBlock.ToHashBytes(this._hashProvider);
            if (blockHash != newBlock.Hash)
            { // Check if the hash is correct
                string message = $"Invalid hash: expected '{blockHash}' got '{newBlock.Hash}'";
                throw new BlockAssertionException(message);
            }

            byte[] merkleRootHash = newBlock.ComputeMerkleRoot(this._hashProvider);
            if (merkleRootHash != newBlock.MerkleRootHash)
            {
                string message = $"Invalid merkle root: expected '{merkleRootHash}' got '{newBlock.MerkleRootHash}'";
                throw new BlockAssertionException(message);
            }

            if (newBlock.Difficulty >= this.GetDifficulty(newBlock.Index ?? 0))
            { // If the difficulty level of the proof-of-work challenge is correct
                string message = $"Invalid difficulty: expected '${newBlock.Difficulty}' to be smaller than '${this.GetDifficulty(newBlock.Index ?? 0)}'";
                this._logger?.LogError(message);
                throw new BlockAssertionException(message);
            }

            return true;
        }
    }
}