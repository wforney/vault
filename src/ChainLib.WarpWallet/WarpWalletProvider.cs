namespace ChainLib.WarpWallet;

using ChainLib.Wallets;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Uses warpwallet's algorithm to produce a wallet secret:
/// <code>
///     s1 = scrypt(key=(passphrase||0x1), salt=(salt||0x1), N=2^18, r=8, p=1, dkLen=32)
///     s2 = pbkdf2(key=(passphrase||0x2), salt=(salt||0x2), c=2^16, dkLen=32, prf=HMAC_SHA256)
/// </code>
/// <see href="https://keybase.io/warp" />
/// </summary>
public class WarpWalletProvider(IWalletRepository repository, IWalletAddressProvider addresses, IWalletFactoryProvider factory) : IWalletProvider
{
    private readonly WarpWalletSecretProvider _secrets = new();

    public string GenerateAddress(Wallet wallet) => addresses.GenerateAddress(wallet);

    public byte[] GenerateSecret(params object[] args) => this._secrets.GenerateSecret(args);

    public Wallet Create(params object[] args) => factory.Create(args);

    public Task<IEnumerable<Wallet>> GetAllAsync() => repository.GetAllAsync();

    public Task<Wallet> GetByIdAsync(string id) => repository.GetByIdAsync(id);

    public Task<Wallet> AddAsync(Wallet wallet) => repository.AddAsync(wallet);

    public Task SaveAddressesAsync(Wallet wallet) => repository.SaveAddressesAsync(wallet);
}