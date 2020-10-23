
namespace CIE.NIS.SDK.Smartcard.Infrastructure
{
    using System;
    using System.IO;
    public static class OID
    {
        public static string OIDsignedData { get { return "1.2.840.113549.1.7.2"; } }
        public static string OIDldsSecurityObject { get { return "2.23.136.1.1.1"; } }
        public static string OIDcontentType { get { return "1.2.840.113549.1.9.3"; } }
        public static string OIDmessageDigest { get { return "1.2.840.113549.1.9.4"; } }
        public static string OIDSigningTime { get { return "1.2.840.113549.1.9.5"; } }
        public static string OIDid_mgf1 { get { return "1.2.840.113549.1.1.8"; } }
        public static string OIDid_sha1 { get { return "1.3.14.3.2.26"; } }
        public static string OIDid_sha256 { get { return "2.16.840.1.101.3.4.2.1"; } }
        public static string OIDid_sha512 { get { return "2.16.840.1.101.3.4.2.3"; } }
        public static string OIDid_sha224 { get { return "2.16.840.1.101.3.4.2.4"; } }
        public static string OIDrsaEncryption { get { return "1.2.840.113549.1.1.1"; } }
        public static string OIDsha1WithRSAEncryption { get { return "1.2.840.113549.1.1.5"; } }
        public static string OIDsha256WithRSAEncryption { get { return "1.2.840.113549.1.1.11"; } }
        public static string OIDsha512WithRSAEncryption { get { return "1.2.840.113549.1.1.13"; } }
        public static string OIDsha224WithRSAEncryption { get { return "1.2.840.113549.1.1.14"; } }
        public static string OIDid_RSASSA_PSS { get { return "1.2.840.113549.1.1.10"; } }
        public static string OIDEcdsaPlainSHA1 { get { return "0.4.0.127.0.7.1.1.4.1.1"; } }
        public static string OIDEcdsaPlainSHA256 { get { return "0.4.0.127.0.7.1.1.4.1.3"; } }
        public static byte[] ToByteArray(string oid) { return Encode(oid); }
        public static string ToString(byte[] oid) { return Decode(oid); }
        public static byte[] Encode(string oidStr)
        {
            MemoryStream ms = new MemoryStream();
            Encode(ms, oidStr);
            ms.Position = 0;
            byte[] retval = new byte[ms.Length];
            ms.Read(retval, 0, retval.Length);
            ms.Close();
            return retval;
        }
        private static void Encode(Stream bt, string oidStr)
        {
            string[] oidList = oidStr.Split('.');
            if (oidList.Length < 2) throw new Exception("Invalid OID string.");
            ulong[] values = new ulong[oidList.Length];
            for (int i = 0; i < oidList.Length; i++)
            {
                values[i] = Convert.ToUInt64(oidList[i]);
            }
            bt.WriteByte((byte)(values[0] * 40 + values[1]));
            for (int i = 2; i < values.Length; i++)
                EncodeValue(bt, values[i]);
        }
        private static void EncodeValue(Stream bt, ulong v)
        {
            for (int i = (BitPrecision(v) - 1) / 7; i > 0; i--)
            {
                bt.WriteByte((byte)(0x80 | ((v >> (i * 7)) & 0x7f)));
            }
            bt.WriteByte((byte)(v & 0x7f));
        }
        public static string Decode(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            ms.Position = 0;
            string retval = Decode(ms);
            ms.Close();
            return retval;
        }
        private static string Decode(Stream bt)
        {
            string retval = "";
            byte b;
            ulong v = 0;
            b = (byte)bt.ReadByte();
            retval += Convert.ToString(b / 40);
            retval += "." + Convert.ToString(b % 40);
            while (bt.Position < bt.Length)
            {
                try
                {
                    DecodeValue(bt, ref v);
                    retval += "." + v.ToString();
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to decode OID value: " + e.Message);
                }
            }
            return retval;
        }
        private static int DecodeValue(Stream bt, ref ulong v)
        {
            byte b;
            int i = 0;
            v = 0;
            while (true)
            {
                b = (byte)bt.ReadByte();
                i++;
                v <<= 7;
                v += (ulong)(b & 0x7f);
                if ((b & 0x80) == 0)
                    return i;
            }
        }
        private static int BitPrecision(ulong ivalue)
        {
            if (ivalue == 0) return 0;
            int l = 0, h = 8 * 4; // 4: sizeof(ulong)
            while (h - l > 1)
            {
                int t = (int)(l + h) / 2;
                if ((ivalue >> t) != 0)
                    l = t;
                else
                    h = t;
            }
            return h;
        }
    }
}
