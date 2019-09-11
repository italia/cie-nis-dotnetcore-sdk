
namespace CIE.NIS.SDK.Extensions
{
    using System;
    using System.Numerics;
    using System.Security.Cryptography;
    public static class RSAExtension
    {
        private static byte[] SHA1Algo = new byte[] { 0x30, 0x21, 0x30, 0x09, 0x06, 0x05, 0x2b, 0x0e, 0x03, 0x02, 0x1a, 0x05, 0x00, 0x04, 0x14 };
        private static byte[] SHA256Algo = new byte[] { 0x30, 0x31, 0x30, 0x0D, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x01, 0x05, 0x00, 0x04, 0x20 };

        public static byte[] PureRSA(this RSA rsa, byte[] data, byte[] modulus, byte[] exp)
        {
            //Normalize data
            int i = 0;
            for (i = 0; i < modulus.Length; i++)
                if (modulus[i] != 0)
                    break;
            if (i > 0)
                modulus = modulus.SubArray(i, modulus.Length - i);

            //Set length of modulus for future filler operation
            var modulusLength = modulus.Length;

            //Apply reverse
            modulus = modulus.Reverse();
            exp = exp.Reverse();
            data = data.Reverse();

            //Check if modulus is negative
            if ((modulus[modulus.Length - 1] & 0x80) != 0)
                //modulus a negative biginteger, so append 0 to end of byte array
                modulus = modulus.Combine(0);

            //Check if data is negative
            if ((data[data.Length - 1] & 0x80) != 0)
                //modulus a negative biginteger, so append 0 to end of byte array
                data = data.Combine(0);

            /* Create three biginteger that satisfy RSA decrypt computation
             * x = y^c mod n 
             * where:
             * x --> result
             * y --> encrypted data
             * c --> exponent
             * n --> modulus
             */
            var y = new BigInteger(data);
            var c = new BigInteger(exp);
            var n = new BigInteger(modulus);
            //Compute RSA
            var x = BigInteger.ModPow(y, c, n);

            //Convert biginteger to octect string
            byte[] octS = x.ToByteArray();
            //Reverse data 
            octS = octS.Reverse();
            //Normalize data
            for (i = 0; i < octS.Length; i++)
                if (data[i] != 0)
                    break;
            if (i != 0)
                octS = octS.SubArray(i, octS.Length - i);

            if (octS.Length < modulusLength)
                octS = octS.Fill(1, modulusLength - octS.Length, 0);

            //Remove padding from result to get original data encrypted
            var result = octS.RemoveRsaPadding();

            return result;
        }
        public static bool PureVerifyData(this RSA rsa, byte[] data, byte[]signature, byte[] modulus, byte[] exp)
        {
            //Perform RSA operation
            var result = PureRSA(rsa, signature, modulus, exp);
            
            //Compare result and original data
            return result.ToHexString() == data.ToHexString();
        }
        public static byte[] RemoveRsaPadding(this byte[] source)
        {
            //Verify that first byte is 0x00
            if (source[0] != 0)
                throw new Exception("Padding BT is not valid");

            //Veryfy BT (block type. 00, 01, 02) 
            if(source[1] == 0x01)
            {
                //Encryption block is of type: EB = 00 || 01 || FF FF FF ... FF || 00 || D
                var i = 0;
                for (i = 2; i < source.Length - 1; i++)
                {
                    if (source[i] != 0xff)
                    {
                        if (source[i] != 0x00)
                            throw new Exception("Padding BT 0x01 is not valid");
                        else
                        {
                            i = i + 1;
                            break;
                        }
                    }
                }
                return source.SubArray(i, source.Length - i);
            }
            else if(source[1] == 0x02)
            {
                var i = 0;
                for (i = 2; i < source.Length - 1; i++)
                {
                    if(source[i] == 0x00)
                    {
                        i = i + 1;
                        break;
                    }
                }
                return source.SubArray(i, source.Length - i);
            }
            else if(source[1] == 0x00)
            {
                throw new NotImplementedException("Not supporrted yet");
            }
            else
            { throw new Exception("Padding BT not found"); }
        }
        public static byte[] RemoveHashAlgorithm(this byte[] data, string hashAlgo)
        {            
            byte[] algo = null;
            switch (hashAlgo)
            {
                case "SHA1":
                    algo = SHA1Algo;
                    break;
                case "SHA256":
                    algo = SHA256Algo;
                    break;
                default:
                    throw new Exception("Algorithm not supported");
            }
            if (algo.Length > data.Length)
                return data;

            //Get first byte of data array that is type of algorithm
            var dataAlgo = data.SubArray(0, algo.Length);

            //Compare data algorithm with chosen algorithm
            if (dataAlgo.ToHexString() != algo.ToHexString())
                throw new Exception("Algorithm not identified");

            //Return data without algorithm
            return data.SubArray(algo.Length, data.Length - algo.Length);
        }
    }
}

