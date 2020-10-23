
namespace CIE.NIS.SDK.Smartcard.Infrastructure
{
    public interface IMaskAlgo
    {
        byte[] Mask(byte[] seed, int maskLen);
    }
}
