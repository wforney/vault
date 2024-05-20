namespace ChainLib.Wallets
{
    using ChainLib.Crypto;

    public interface IWalletAddressStorageFormat
    {
        KeyPair Import(Wallet wallet, string input, int len);
        string Export(Wallet wallet, byte[] address);
    }
}