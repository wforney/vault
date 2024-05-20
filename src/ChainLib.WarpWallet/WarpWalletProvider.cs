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
public class WarpWalletProvider : IWalletProvider
{
    private readonly IWalletRepository _repository;
    private readonly IWalletSecretProvider _secrets;
    private readonly IWalletAddressProvider _addresses;
    private readonly IWalletFactoryProvider _factory;

    public WarpWalletProvider(IWalletRepository repository, IWalletAddressProvider addresses, IWalletFactoryProvider factory)
    {
        this._repository = repository;
        this._secrets = new WarpWalletSecretProvider();
        this._addresses = addresses;
        this._factory = factory;
    }

    public string GenerateAddress(Wallet wallet) => this._addresses.GenerateAddress(wallet);

    public byte[] GenerateSecret(params object[] args) => this._secrets.GenerateSecret(args);

    public Wallet Create(params object[] args) => this._factory.Create(args);

    public Task<IEnumerable<Wallet>> GetAllAsync() => this._repository.GetAllAsync();

    public Task<Wallet> GetByIdAsync(string id) => this._repository.GetByIdAsync(id);

    public Task<Wallet> AddAsync(Wallet wallet) => this._repository.AddAsync(wallet);

    public Task SaveAddressesAsync(Wallet wallet) => this._repository.SaveAddressesAsync(wallet);
}