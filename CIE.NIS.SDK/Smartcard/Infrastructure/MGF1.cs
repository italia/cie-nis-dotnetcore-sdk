
namespace CIE.NIS.SDK.Smartcard.Infrastructure
{
    using System;
    using Extensions;
    public class MGF1 : IMaskAlgo
    {
        private readonly string _hashAlgo;
        public int DigestSize { get; }
        public MGF1(string hashAlgo)
        {
            _hashAlgo = hashAlgo;
            switch (_hashAlgo)
            {
                case "SHA1":
                    DigestSize = 20;
                    break;
                case "SHA256":
                    DigestSize = 32;
                    break;
                case "SHA224":
                    DigestSize = 28;
                    break;
                case "SHA512":
                    DigestSize = 64;
                    break;
                default:
                    throw new Exception("MGF1 hash agoritm not supported");
            }
        }       
        public byte[] Mask(byte[] seed, int maskLen)
        {
            byte[] T = null;
            int iterations = (int)Math.Ceiling(maskLen / (double)DigestSize) - 1;
            for (int c = 0; c <= iterations; c++)
            {
                byte[] S = (byte[])seed.Clone();
                byte[] P = null;
                P = P.PadInt((ulong)c, 4);
                S = S.Combine(P);
                T = T.Combine(S.ToMessageDigest(_hashAlgo));
            }
            return T.Left(maskLen);
        }
    }
}
