
namespace CIE.NIS.SDK.Smartcard
{
    using System;    
    using System.Security.Cryptography;
    using PCSC;
    using PCSC.Iso7816;
    using PCSC.Utils;    
    using Extensions;   

    public class CIEOrchestrator
    {
        private readonly SCardReader _peripheral;        
        public CIEOrchestrator(SCardReader peripheral)
        {            
            _peripheral = peripheral;
        }

        public byte[] ReadNIS(string reader)
        {
            // Connection to reader
            var sc = _peripheral.Connect(reader, SCardShareMode.Shared, SCardProtocol.Any);
            if (sc != SCardError.Success)
            {
                throw new Exception(string.Format("Could not connect to reader {0}:\n{1}",
                    reader,
                    SCardHelper.StringifyError(sc)));
            }
            try
            {
                CommandApdu apdu;
                ResponseApdu rapdu;
                byte[] receiveBuffer;

                /* 00 A4 - 04 0C 0D - A0 00 00 00 30 80 00 00 00 09 81 60 01 Selezione Applet CIE  */
                receiveBuffer = new byte[2];
                apdu = new CommandApdu(IsoCase.Case4Short, _peripheral.ActiveProtocol)
                {
                    CLA = 0x00,
                    INS = 0XA4,
                    P1 = 0x04,
                    P2 = 0x0C,
                    Le = 0x0D,
                    Data = new byte[] { (byte)0xA0, 0x00, 0x00, 0x00, 0x30, (byte)0x80, 0x00, 0x00, 0x00, 0x09, (byte)0x81, 0x60, 0x01 }
                };
                sc = _peripheral.Transmit(
                        SCardPCI.GetPci(_peripheral.ActiveProtocol),
                        apdu.ToArray(),
                        new SCardPCI(),
                        ref receiveBuffer);

                /* 00 A4 - 04 0C 06 - A0 00 00 00 00 39 Selezione DF_CIE */
                receiveBuffer = new byte[2];
                apdu = new CommandApdu(IsoCase.Case4Short, _peripheral.ActiveProtocol)
                {
                    CLA = 0x00,
                    INS = 0XA4,
                    P1 = 0x04,
                    P2 = 0x0C,
                    Le = 0x00,
                    Data = new byte[] { (byte)0xA0, 0x00, 0x00, 0x00, 0x00, 0x39 }
                };
                sc = _peripheral.Transmit(
                        SCardPCI.GetPci(_peripheral.ActiveProtocol),
                        apdu.ToArray(),
                        new SCardPCI(),
                        ref receiveBuffer);

                /* 00 B0 - 81 00 00 Lettura NIS */
                receiveBuffer = new byte[14];
                apdu = new CommandApdu(IsoCase.Case2Short, _peripheral.ActiveProtocol)
                {
                    CLA = 0x00,
                    INS = 0XB0,
                    P1 = 0x81,
                    P2 = 0x00,
                    Le = 0x00
                };
                sc = _peripheral.Transmit(
                        SCardPCI.GetPci(_peripheral.ActiveProtocol),
                        apdu.ToArray(),
                        new SCardPCI(),
                        ref receiveBuffer);
                rapdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, _peripheral.ActiveProtocol);
                var NIS_ID = rapdu.GetData();

                /* 00 B0 - 85 00 00 Lettura chiave pubblica - 1*/
                receiveBuffer = new byte[233];
                apdu = new CommandApdu(IsoCase.Case2Short, _peripheral.ActiveProtocol)
                {
                    CLA = 0x00,
                    INS = 0XB0,
                    P1 = 0x85,
                    P2 = 0x00,
                    Le = 0x00
                };
                sc = _peripheral.Transmit(
                        SCardPCI.GetPci(_peripheral.ActiveProtocol),
                        apdu.ToArray(),
                        new SCardPCI(),
                        ref receiveBuffer);
                rapdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, _peripheral.ActiveProtocol);
                var pubkey1 = rapdu.GetData();

                /* 00 B0 - 85 E7 00 - Lettura chiave pubblica - 2 */
                receiveBuffer = new byte[233];
                apdu = new CommandApdu(IsoCase.Case2Short, _peripheral.ActiveProtocol)
                {
                    CLA = 0x00,
                    INS = 0XB0,
                    P1 = 0x85,
                    P2 = 0xE7,
                    Le = 0x00
                };
                sc = _peripheral.Transmit(
                        SCardPCI.GetPci(_peripheral.ActiveProtocol),
                        apdu.ToArray(),
                        new SCardPCI(),
                        ref receiveBuffer);
                rapdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, _peripheral.ActiveProtocol);
                var pubkey2 = rapdu.GetData();
                //Combine pubkey1 and pubkey2 to obtain ASN1 structure that contain 2 children, modulus and exponent
                //that is used to create RSA crypto service provider
                var pubKeyAsn1 = ASN1Tag.Parse(pubkey1.Combine(pubkey2));

                /* 00 22 - 41 A4 06 - 80 01 02 84 01 83 - Selezione chiave int-auth */
                receiveBuffer = new byte[2];
                apdu = new CommandApdu(IsoCase.Case4Short, _peripheral.ActiveProtocol)
                {
                    CLA = 0x00,
                    INS = 0x22,
                    P1 = 0x41,
                    P2 = 0xA4,
                    Le = 0x02,
                    Data = new byte[] { 0x80, 0x01, 0x02, 0x84, 0x01, 0x83 }
                };
                sc = _peripheral.Transmit(
                        SCardPCI.GetPci(_peripheral.ActiveProtocol),
                        apdu.ToArray(),
                        new SCardPCI(),
                        ref receiveBuffer);

                /* Generate random to perform sign and verify */
                var challenge = ByteArrayOperations.GenerateRandomByteArray(8);

                /* 00 88 - 00 00 08 - hashChallenge 00 int-auth*/
                receiveBuffer = new byte[258];
                apdu = new CommandApdu(IsoCase.Case4Short, _peripheral.ActiveProtocol)
                {
                    CLA = 0x00,
                    INS = 0x88,
                    P1 = 0x00,
                    P2 = 0x00,
                    Le = 0x00,
                    Data = challenge
                };
                sc = _peripheral.Transmit(
                        SCardPCI.GetPci(_peripheral.ActiveProtocol),
                        apdu.ToArray(),
                        new SCardPCI(),
                        ref receiveBuffer);
                rapdu = new ResponseApdu(receiveBuffer, IsoCase.Case4Short, _peripheral.ActiveProtocol);
                var signedData = rapdu.GetData();

                //Verify challenge with public key                
                using (var rsa = RSA.Create())
                {
                    var modulus = pubKeyAsn1.Child(0, 0x02).Data; // modulus. 02 Verify that result is a INTEGER
                    var exp = pubKeyAsn1.Child(1, 0x02).Data; // exp. 02 Verify that result is a INTEGER
                    if (!rsa.PureVerifyData(challenge, signedData, modulus, exp))
                        throw new Exception("Unable to verify challenge");
                }

                //Read SOD data record
                var idx = 0;
                var size = 0xe4;
                byte[] data;
                byte[] sodIASData = new byte[0];
                bool sodLoaded = false;
                while (!sodLoaded)
                {
                    var hexS = idx.ToString("X4");
                    receiveBuffer = new byte[233];
                    apdu = new CommandApdu(IsoCase.Case4Short, _peripheral.ActiveProtocol)
                    {
                        CLA = 0x00,
                        INS = 0xB1,
                        P1 = 0x00,
                        P2 = 0x06,
                        Le = 0x00,
                        Data = new byte[] {
                            0x54,
                            0x02,
                            byte.Parse(hexS.Substring(0,2), System.Globalization.NumberStyles.HexNumber),
                            byte.Parse(hexS.Substring(2, 2), System.Globalization.NumberStyles.HexNumber)}
                    };
                    sc = _peripheral.Transmit(
                            SCardPCI.GetPci(_peripheral.ActiveProtocol),
                            apdu.ToArray(),
                            new SCardPCI(),
                            ref receiveBuffer);
                    rapdu = new ResponseApdu(receiveBuffer, IsoCase.Case4Short, _peripheral.ActiveProtocol);
                    data = rapdu.GetData();
                    var offset = 2;
                    if (data[1] > 0x80)
                        offset = 2 + (data[1] - 0x80);
                    var buf = data.SubArray(offset, data.Length - offset);
                    sodIASData = sodIASData.Combine(buf);
                    idx += size;
                    if (data[2] != 0xe4)
                        sodLoaded = true;
                }
                //Create IAS ASN1 object
                var ias = IASSod.Create(sodIASData);
                var isValid = ias.Verify(NIS_ID);                

                //Verify integrity of data
                if (isValid)
                    return NIS_ID;

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally { _peripheral.Disconnect(SCardReaderDisposition.Reset); }
        }
    }
}
