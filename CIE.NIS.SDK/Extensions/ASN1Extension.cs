
namespace CIE.NIS.SDK.Extensions
{
    using System;
    using System.Text;
    using Smartcard;

    public static class ASN1Extension
    {
        public static ASN1Tag DeepChild(this ASN1Tag asn1, params int[] sequence)
        {
            ASN1Tag tag = asn1;
            foreach(var idx in sequence)
            {
                tag = tag.children[idx];
            }
            return tag;
        }
        public static byte[] MakeTag(this ASN1Tag asn1, ulong tag)
        {
            //Build tag
            byte[] newTag;
            if (tag <= 0xff)            
                newTag = new byte[] { (byte)tag };            
            else if (tag <= 0xffff)            
                newTag = new byte[] { (byte)(tag >> 8), (byte)(tag & 0xff) };            
            else if (tag <= 0xffffff)            
                newTag = new byte[] { (byte)(tag >> 16), (byte)((tag >> 8) & 0xff), (byte)(tag & 0xff) };            
            else if (tag <= 0xffffffff)            
                newTag = new byte[] { (byte)(tag >> 24), (byte)((tag >> 16) & 0xff), (byte)((tag >> 8) & 0xff), (byte)(tag & 0xff) };
            else 
                throw new Exception("Tag too long");

            //Build length
            var length = (ulong)asn1.Data.Length;
            byte[] newLength;
            if (length < 0x80)            
                newLength = new byte[] { (byte)length };            
            else if (length <= 0xff)            
                newLength = new byte[] { 0x81, (byte)length };            
            else if (length <= 0xffff)
                newLength = new byte[] { 0x82, (byte)(length >> 8), (byte)(length & 0xff) };            
            else if (length <= 0xffffff)
                newLength = new byte[] { 0x83, (byte)(length >> 16), (byte)((length >> 8) & 0xff), (byte)(length & 0xff) };            
            else if (length <= 0xffffffff)
                newLength = new byte[] { 0x84, (byte)(length >> 24), (byte)((length >> 16) & 0xff), (byte)((length >> 8) & 0xff), (byte)(length & 0xff) };
            else
                throw new Exception("Data too long");

            byte[] data = new byte[newTag.Length + newLength.Length + asn1.Data.Length];
            Array.Copy(newTag, 0, data, 0, newTag.Length);
            Array.Copy(newLength, 0, data, newTag.Length, newLength.Length);
            Array.Copy(asn1.Data, 0, data, newTag.Length + newLength.Length, asn1.Data.Length);
            
            return data;
        }
        public static string ToReadableString(this ASN1Tag asn1)
        {
            var data = asn1.Data.SubArray(0, Math.Min(30, asn1.Data.Length));
            if (data == null)
                return "";
            StringBuilder sb = new StringBuilder(data.Length * 3);
            for (int i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("X02") + " ");
            return sb.ToString();
        }
    }
}
