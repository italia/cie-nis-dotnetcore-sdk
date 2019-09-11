
namespace CIE.NIS.SDK.Extensions
{
    using System;
    using System.Linq;

    public static class ByteArrayOperations
    {
        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
        public static byte[] GenerateRandomByteArray(int size)
        {
            var result = new byte[size];
            new Random(1).NextBytes(result);
            return result;
        }
    }
}
