namespace ChainLib.Node.Models;

using ChainLib.Models;
using ChainLib.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class Peer
{
    private readonly string _host;
    private readonly int _port;
    private readonly ICollection<Peer> _peers;
    private readonly Blockchain _blockchain;
    private readonly JsonSerializerSettings _jsonSettings;
    private readonly ILogger<Peer> _logger;
    private readonly HttpClient _http;

    private string Url => $"http://{this._host}:{this._port}";

    public IEnumerable<Peer> Peers => this._peers;

    public Peer(string host, int port, Blockchain blockchain, JsonSerializerSettings jsonSettings, ILogger<Peer> logger, IEnumerable<string> peers)
    {
        this._host = host;
        this._port = port;
        if (peers != null)
        {
            this._peers = new HashSet<Peer>(peers.Select(x =>
                {
                    Uri url = new(x);
                    return new Peer(url.Host, url.Port, blockchain, jsonSettings, logger, null);
                }));
        }

        this._blockchain = blockchain;
        this._jsonSettings = jsonSettings;
        this._logger = logger;

        this._http = new HttpClient();
    }

    public async Task ConnectToPeersAsync(params Peer[] newPeers)
    {
        foreach (Peer peer in newPeers)
        {
            if (peer.Url == this.Url)
            {
                continue;
            }

            // If it already has that peer, ignore.
            bool hasPeer = false;
            if (this._peers.Any(existing => existing.Url == peer.Url))
            {
                this._logger?.LogInformation($"Peer {peer.Url} not added to connections, because I already have it.");
                hasPeer = true;
            }

            if (hasPeer)
            {
                continue;
            }

            await this.SendPeerAsync(peer, this);
            this._logger?.LogInformation($"Peer {peer.Url} added to connections.");
            this._peers.Add(peer);

            await this.InitConnectionAsync(peer);
            this.Broadcast(async node => await this.SendPeerAsync(peer, node));
        }
    }

    public async Task SendPeerAsync(Peer peer, Peer peerToSend)
    {
        string url = $"{peer.Url}/node/peers";
        this._logger.LogInformation($"Sending {peerToSend.Url} to peer {url}.");

        try
        {
            HttpResponseMessage response = await this._http.PostAsync(url, null);
            string json = await response.Content.ReadAsStringAsync();
            Block block = JsonConvert.DeserializeObject<Block>(json, this._jsonSettings);

            _ = await this.CheckReceivedBlocksAsync(block);
        }
        catch (Exception e)
        {
            this._logger?.LogWarning(e, $"Unable to send me to peer {url}: {e.Message}");
        }
    }

    public async Task InitConnectionAsync(Peer peer) => await this.GetLatestBlockAsync(peer);

    public async Task GetLatestBlockAsync(Peer peer)
    {
        string url = $"{peer.Url}/blockchain/blocks/latest";
        this._logger?.LogInformation($"Getting latest block from: {url}");

        try
        {
            HttpResponseMessage response = await this._http.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            Block block = JsonConvert.DeserializeObject<Block>(json, this._jsonSettings);

            _ = await this.CheckReceivedBlocksAsync(block);
        }
        catch (Exception e)
        {
            this._logger?.LogWarning(e, $"Unable to get latest block from {url}: {e.Message}");
        }
    }

    public async Task SendLatestBlockAsync(Peer peer, Block block)
    {
        string url = $"{peer.Url}/blockchain/blocks/latest";
        this._logger?.LogInformation($"Sending latest block to: {url}");

        try
        {
            string body = JsonConvert.SerializeObject(block, this._jsonSettings);
            HttpResponseMessage response = await this._http.PutAsync(url, new StringContent(body, Encoding.UTF8));
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Status code was {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            this._logger?.LogWarning(e, $"Unable to send latest block to {url}: {e.Message}");
        }
    }

    public async Task GetBlocksAsync(Peer peer)
    {
        string url = $"{peer.Url}/blockchain/blocks";
        this._logger?.LogInformation($"Getting blocks from: {url}");

        try
        {
            HttpResponseMessage response = await this._http.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            Block block = JsonConvert.DeserializeObject<Block>(json, this._jsonSettings);

            _ = await this.CheckReceivedBlocksAsync(block);
        }
        catch (Exception e)
        {
            this._logger?.LogWarning(e, $"Unable to get blocks from {url}: {e.Message}");
        }
    }

    public void Broadcast(Action<Peer> closure)
    {
        foreach (Peer peer in this._peers)
        {
            closure(peer);
        }
    }

    public async Task<bool?> CheckReceivedBlocksAsync(params Block[] blocks)
    {
        List<Block> receivedBlocks = blocks.OrderBy(x => x.Index).ToList();
        Block latestBlockReceived = receivedBlocks[^1];
        Block latestBlockHeld = await this._blockchain.GetLastBlockAsync();

        // If the received blockchain is not longer than blockchain. Do nothing.
        if (latestBlockReceived.Index <= latestBlockHeld.Index)
        {
            this._logger?.LogInformation($"Received blockchain is not longer than blockchain. Do nothing.");
            return false;
        }

        this._logger?.LogInformation($"Blockchain possibly behind. We got: {latestBlockHeld.Index}, Peer got: {latestBlockReceived.Index}");

        if (latestBlockHeld.Hash == latestBlockReceived.PreviousHash)
        {
            // We can append the received block to our chain
            this._logger?.LogInformation("Appending received block to our chain");
            _ = await this._blockchain.AddBlockAsync(latestBlockReceived);
            return true;
        }

        if (receivedBlocks.Count == 1)
        {
            // We have to query the chain from our peer
            this._logger?.LogInformation("Querying chain from our peers");
            this.Broadcast(async node => await this.GetBlocksAsync(node));
            return null;
        }

        // Received blockchain is longer than current blockchain
        this._logger?.LogInformation("Received blockchain is longer than current blockchain");
        await this._blockchain.ReplaceChainAsync(receivedBlocks);
        return true;
    }
}
