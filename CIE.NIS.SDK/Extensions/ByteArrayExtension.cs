
namespace CIE.NIS.SDK.Extensions
{    
    using System;
    using System.Security.Cryptography;
    using System.Text;   

    public static class ByteArrayExtension
    {        
        public static byte[] SubArray(this byte[] source, int offset, int length)
        {
            byte[] rv = new byte[length];
            Buffer.BlockCopy(source, offset, rv, 0, length);
            return rv;
        }
        public static byte[] Left(this byte[] source, int num)
        {
            if (num > source.Length)
                return source;
            byte[] data = new byte[num];
            Array.Copy(source, data, num);
            return data;
        }
        public static byte[] Right(this byte[] source, int num)
        {
            if (num > source.Length)
                return source;
            byte[] data = new byte[num];
            Array.Copy(source, source.Length - num, data, 0, num);
            return data;
        }
        public static byte[] Xor(this byte[] source, byte[] mask)
        {
            if (mask.Length != source.Length)
                throw new Exception("Dimensione array non corretta");
            var xor = new byte[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                xor[i] = (byte)(source[i] ^ mask[i]);
            }
            return xor;
        }
        public static byte[] Reverse(this byte[] source)
        {
            var ba = new Byte[source.Length];            
            var end = source.Length - 1;
            for(var start = 0; start <= end; start++)
            {
                ba[start] = source[end - start];
            }
            return ba;
        }                               
        public static byte[] Combine(this byte[] source, byte[] data)
        {
            if (source == null)
                return data;

            return ByteArrayOperations.Combine(source, data);
        }
        public static byte[] Combine(this byte[] source, byte data)
        {
            return Combine(source, new byte[] { data });
        }        
        public static byte[] Fill(this byte[] source, int offset, int size, byte content)
        {            
            //Create filler array of byte
            var filler = new byte[size];
            for(var i = 0; i < size; i++)
            {
                filler[i] = content;
            }
            //Verify wrhere put filler array
            var splitIndex = offset - 1;
            if (splitIndex == 0)
                //Fill content array at the start of source array
                return ByteArrayOperations.Combine(filler, source);
            else if (splitIndex == source.Length - 1)
                //Fill content array at the end of source array
                return ByteArrayOperations.Combine(source, filler);
            else
            {
                //Fill the array at offset position that is not start nor end of source array
                throw new NotImplementedException("Not supported yet");
            }
        }
        public static byte[] PadInt(this byte[] source, ulong value, int size)
        {
            var sz = BitConverter.GetBytes(value).Reverse().Right(size);
            if (sz.Length < size)
                return source.Fill(0, size - sz.Length, 0);
            else
                return sz;
        }        
        public static string ToHexString(this byte[] source)
        {
            StringBuilder hex = new StringBuilder(source.Length * 2);
            foreach (byte b in source)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        public static string ToReadableString(this byte[] source)
        {
            if (source == null)
                return "";
            StringBuilder sb = new StringBuilder(source.Length * 3);
            for (int i = 0; i < source.Length; i++)
                sb.Append(source[i].ToString("X02") + " ");
            return sb.ToString();
        }
        public static byte[] ToMessageDigest(this byte[] source, string algo)
        {
            return HashAlgorithm.Create(algo).ComputeHash(source);
        }
    }
}
