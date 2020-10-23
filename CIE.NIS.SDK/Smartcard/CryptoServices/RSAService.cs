
namespace CIE.NIS.SDK.Smartcard.CryptoServices
{    
    using System;
    using System.Security.Cryptography;
    using Extensions;
    using Infrastructure;

    public class RSAService: IDisposable
    {
        private readonly byte[] _mod;
        private readonly byte[] _exp;
        public RSAService(byte[] mod, byte[] exp)
        {
            _mod = mod;
            _exp = exp;
        }
        public bool CheckSHA1withRSAEncryption(byte[] data, byte[] toSign)
        {
            byte[] signature;
            // Use public key of DS certificate to perform an RSA operation
            using (var rsa = RSA.Create())
            {
                signature = rsa.PureRSA(
                    data,
                    _mod,
                    _exp
                    );                
            }
            //Remove algorithm from signature            
            signature = signature.RemoveHashAlgorithm("SHA1");
            //Calculate message digest and compare it with signature                        
            var digestSignature = toSign.ToMessageDigest("SHA1");
            if (digestSignature.ToHexString() != signature.ToHexString())
                return false; // SOD sign wrong

            return true;
        }
        public bool CheckRSASSAPSS(byte[] data, byte[] toSign, string hashAlgo)
        {
            byte[] signature;            
            var mask = new MGF1(hashAlgo);
            // Use public key of DS certificate to perform an RSA operation
            using (var rsa = RSA.Create())
            {
                signature = rsa.RawRSA(
                    data,
                    _mod,
                    _exp
                    );
            }
            if (signature.Right(1)[0] != 0xbc)
                return false;

            var maskedDB = signature.Left(signature.Length - mask.DigestSize - 1);
            var H = signature.SubArray(maskedDB.Length, mask.DigestSize);
            var DBMask = mask.Mask(H, maskedDB.Length);
            var DB = maskedDB.Xor(DBMask);
            // RFC 3447 specifica che il controlo della firma va fatto con:
            // Result = EMSA-PSS-VERIFY (M, EM, modBits - 1).
            // quindi il numero di bit del messaggio è di uno inferiore alla lunghezza del modulo
            // quindi il passo "Set the leftmost 8emLen - emBits bits of the leftmost octet in DB to zero." 
            // richiede di impostare a 0 il bit più a sinistra
            DB[0] &= 0x7F;
            var hayStackPos = maskedDB.Length - mask.DigestSize - 1;
            for (int i = 0; i < hayStackPos; i++)
                if (DB[i] != 0)
                    return false;
            if (DB[(int)(hayStackPos)] != 1)
                return false;
            var salt = DB.Right(mask.DigestSize);

            //Calculate message digest and compare it with signature                        
            var digestSignature = toSign.ToMessageDigest(hashAlgo);

            var H1 = new byte[8];
            //Ensure to fill 0 in H1 and perform digest operation
            H1 = H1.Combine(digestSignature).Combine(salt).ToMessageDigest(hashAlgo);

            if (H1.ToHexString() != H.ToHexString())
                return false; // SOD sign wrong

            return true;
        }
        public void Dispose()
        {
            
        }
    }
}
