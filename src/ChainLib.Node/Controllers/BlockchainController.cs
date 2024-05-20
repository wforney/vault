namespace ChainLib.Node.Controllers;

using ChainLib.Crypto;
using ChainLib.Models;
using ChainLib.Node.Models;
using ChainLib.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

/// <inheritdoc />
/// <summary>
/// Controls operations on the underlying blockchain.
/// </summary>
[Route("blockchain")]
public class BlockchainController : Controller
{
    private readonly Peer _node;
    private readonly Blockchain _blockchain;

    public BlockchainController(Peer node, Blockchain blockchain)
    {
        this._node = node;
        this._blockchain = blockchain;
    }

    /// <summary>
    /// Streams a complete copy of this node's blockchain to calling clients.
    /// </summary>
    /// <returns></returns>
    [HttpGet("blocks")]
    public IActionResult StreamAllBlocks(bool forwards, long startingAt = 0L)
    {
        System.Collections.Generic.IEnumerable<Block> blocks = this._blockchain.StreamAllBlocks(forwards, startingAt: startingAt);
        return !blocks.Any() ? this.NotFound() : this.Ok(blocks);
    }

    /// <summary>
    /// Gets the last block on the blockchain, according to this node.
    /// </summary>
    /// <returns></returns>
    [HttpGet("blocks/latest")]
    public async Task<IActionResult> GetLastBlock()
    {
        Block last = await this._blockchain.GetLastBlockAsync();
        return last == null
            ? this.NotFound(new
            {
                Message = "Last block not found"
            })
            : this.Ok(last);
    }

    /// <summary>
    /// Attempt to append the chain with the provided block. Used as a mechanism to sync peers. 
    /// </summary>
    [HttpPut("blocks/latest")]
    public async Task<IActionResult> VerifyLastBlock([FromBody] Block block)
    {
        bool? result = await this._node.CheckReceivedBlocksAsync(block);
        return result == null
            ? this.Accepted(new
            {
                Message = "Requesting blockchain to check."
            })
            : result.Value
            ? this.Ok(block)
            : (IActionResult)this.StatusCode((int)HttpStatusCode.Conflict, new
            {
                Message = "Blockchain is up to date."
            });
    }

    /// <summary>
    /// Retrieve a block by hash.
    /// </summary>
    [HttpGet("blocks/{hash}")]
    public async Task<IActionResult> GetBlockByHash(string hash)
    {
        Block blockFound = await this._blockchain.GetBlockByHashAsync(hash.FromHex());
        return blockFound == null
            ? this.NotFound(new
            {
                Message = $"Block not found with hash '{hash}'"
            })
            : this.Ok(blockFound);
    }

    /// <summary>
    /// Retrieve a block by index.
    /// </summary>
    [HttpGet("blocks/{index}")]
    public async Task<IActionResult> GetBlockByIndex(long index)
    {
        Block blockFound = await this._blockchain.GetBlockByIndexAsync(index);
        return blockFound == null
            ? this.NotFound(new
            {
                Message = $"Block not found with index '{index}'"
            })
            : this.Ok(blockFound);
    }
}
