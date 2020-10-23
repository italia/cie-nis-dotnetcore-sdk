
namespace CIE.NIS.SDK.Smartcard
{
    using System;    
    using CryptoServices;
    using Infrastructure;
    using Extensions;    
    
    public class IASSod
    {        
        private ASN1Tag SignedDataObject { get; set; }
        private ASN1Tag SignedData { get; set; }
        private ASN1Tag DSCertificate { get; set; }
        private ASN1Tag IssuerName { get; set; }
        private ASN1Tag SerialNumber { get; set; }
        private ASN1Tag SignatureAlgoritmIdentifier { get; set; }
        private ASN1Tag SignerMessageDigest { get; set; }
        private ASN1Tag Signature { get; set; }
        private string OIDDSMessageDigestSignAlgo { get; set; }
        private string OIDDSMessageDigestHashAlgo { get; set; }        
        private IASSod(byte[] sod)
        {
            //Create asn1 structure of IAS sod object
            var asn1 = ASN1Tag.Parse(sod, true);
            //Take root element that represents all data signed 
            var root = asn1.Child(0);
            root.Child(0, 06).Verify(OID.ToByteArray(OID.OIDsignedData)); //06 Verify that result is a OBJECT IDENTIFIER
            //Struttura che negli altri elementi verrà firmata
            SignedDataObject = root.DeepChild(1, 0, 2);
            SignedDataObject.Child(0, 06).Verify(OID.ToByteArray(OID.OIDldsSecurityObject)); //06 Verify that result is a OBJECT IDENTIFIER
            /*GET SIGNED DATA THAT CONTAINS HASHED INFORMATION OF:
            - ID servizi
            - Seriale carta
            - Certificato utente
            - Chiave pubblica di internal authentication
            - Chiave pubblica di internal authentication per i servizi*/
            SignedData = SignedDataObject.DeepChild(1, 0).Child(0, 0x30); //0x30 Verify that result is a SEQUENCE
            //GET DOCUMENT SIGNER CERTIFICATE
            //This object can be parsed to X509Certificate2 class costrusctor
            DSCertificate = root.DeepChild(1, 0, 3).Child(0, 0x30); //0x30 Verify that result is a SEQUENCE            
            //GET SIGNER INFO
            var signerInfo = root.DeepChild(1, 0, 4).Child(0, 0x30); //0x30 Verify that result is a SEQUENCE
            /*GET INFORMATION OF ISSUER FROM SIGNER INFO
            La struttura dati contenuta si avvicina alla seguente:            
            0 SET (1 elem)
                0.0 SEQUENCE (2 elem)
                    0.0.0 OBJECT IDENTIFIER (2.5.4.3 commonName (X.520 DN component))
                    0.0.1 PrintableString (Italian Country Signer CA - TEST)
            1 SET (1 elem)
                1.0 SEQUENCE (2 elem)
                    1.0.0 OBJECT IDENTIFIER (2.5.4.11 organizationalUnitName (X.520 DN component))
                    1.0.1 PrintableString (National Electronic Center of State Police)
            2 SET (1 elem)
                2.0 SEQUENCE (2 elem)
                    2.0.0 OBJECT IDENTIFIER (2.5.4.10 organizationName (X.520 DN component))
                    2.0.1 PrintableString (Ministry of Interior)
            3 SET (1 elem)
                3.0 SEQUENCE (2 elem)
                    3.0.0 OBJECT IDENTIFIER (2.5.4.6 countryName (X.520 DN component))
                    3.0.1 PrintableString (IT)*/
            IssuerName = signerInfo.DeepChild(1).Child(0, 0x30); //0x30 Verify that result is a SEQUENCE            
            /*GET INFORMATION OF SIGNER CERTIFICATE SERIAL NUMBER FROM DS CERT
            La struttura dati contenuta si avvicina alla seguente:            
            0 INTEGER (value of serial number)*/
            SerialNumber = signerInfo.DeepChild(1).Child(1, 0x02); //02 Verify that result is a INTEGER            
            /*GET INFORMATION ABOUT SIGNER FROM DS CERT
            La struttura dati contenuta si avvicina alla seguente:            
            0 SEQUENCE (2 elem)
                0.0 OBJECT IDENTIFIER (1.2.840.113549.1.9.3 OIDcontentType)
                0.1 SET (1 elem)
                    0.1.0 OBJECT IDENTIFIER (2.23.136.1.1.1 Security)
            1 SEQUENCE (2 elem)
                1.0 OBJECT IDENTIFIER (1.2.840.113549.1.9.4 OIDmessageDigest)
                1.1 SET (1 elem)
                    1.1.0 OCTET STRING (F92C82AE7A944DCFFE4CCD0B6CAFA9D0134ED16EBEEFE6D9A44B641920F99BA9... (hash)) */
            SignatureAlgoritmIdentifier = signerInfo.Child(3, 0xA0); //0xA0 Verify that result is a CONTEXT_SPECIFIC
            //Verify data structure of SignerInfo object
            SignatureAlgoritmIdentifier.DeepChild(0, 0).Verify(OID.ToByteArray(OID.OIDcontentType)); //Verify that node 0.0 is a OIDcontentType Object identifier 
            SignatureAlgoritmIdentifier.DeepChild(0, 1, 0).Verify(OID.ToByteArray(OID.OIDldsSecurityObject)); //Verify that node 0.1.0 is a OIDldsSecurityObject Object identifier 
            SignatureAlgoritmIdentifier.DeepChild(1, 0).Verify(OID.ToByteArray(OID.OIDmessageDigest)); //Verify that node 1.0 is a OIDmessageDigest Object identifier 
            //Get message digest of Signer that is in the octect string object
            SignerMessageDigest = SignatureAlgoritmIdentifier.DeepChild(1, 1).Child(0, 04); //04 Verify that result is a OCTECT STRING
            //GET MESSAGE DIGEST SIGN ALGORYTM AND HASH ALGORYTM         
            OIDDSMessageDigestSignAlgo = OID.ToString(signerInfo.DeepChild(4).Child(0, 06).Data); //06 Verify that result is a OBJECT IDENTIFIER
            OIDDSMessageDigestHashAlgo = OID.ToString(signerInfo.DeepChild(2).Child(0, 06).Data); //06 Verify that result is a OBJECT IDENTIFIER
            //Verify Signed data hash algoritm
            //SignedData.Child(0, 02).Verify(new byte[] { 0 });
            SignedData.DeepChild(1).Child(0, 06).Verify(OID.Encode(OIDDSMessageDigestHashAlgo)); //Verify hash algoritm
            /*GET SIGNATURE
            La struttura dati contenuta si avvicina alla seguente: 
            0 OCTET STRING (42C600B32BD5EFF0A684F65BD4526872AD3D4EADA3017A6E836736340BCCDA7DB9622...)*/
            Signature = signerInfo.Child(5, 04); //04 Verify that result is a OCTECT STRING
        }
        public static IASSod Create(byte[] sod)
        {
            return new IASSod(sod);
        }
        public bool Verify(byte[] nis)
        {
            var digestAlgo = string.Empty;            
            switch (OIDDSMessageDigestHashAlgo)
            {
                case "2.16.840.1.101.3.4.2.1":
                    digestAlgo = "SHA256";
                    break;
                case "2.16.840.1.101.3.4.2.3":
                    digestAlgo = "SHA512";
                    break;
                default:
                    throw new Exception("hash algoritm not valid");
            }
            /*Get hashed data of
            - ID servizi
            - Seriale carta
            - Certificato utente
            - Chiave pubblica di internal authentication
            - Chiave pubblica di internal authentication per i servizi
            contained in SignedDataObject and calculate hash of hash*/
            var digestRecalculation = SignedDataObject.DeepChild(1, 0).Data.ToMessageDigest(digestAlgo);
            //Compare recalculation of hash with data contained in signer message digest object to verify equality            
            if (digestRecalculation.ToHexString() != SignerMessageDigest.Data.ToHexString())
                return false; // Digest SOD not correspond to data

            //Get Document Signer public key from SignedCert data           
            var dsPubKey = DSCertificate.DeepChild(0, 6, 1, 0);
            //Get modulus and exponent from Document Signer public key
            var dsModPubKey = dsPubKey.Child(0, 0x02).Data; // modulus. 0x02 Verify that result is a INTEGER
            var dsExpPubKey = dsPubKey.Child(1, 0x02).Data; // exp. 0x02 Verify that result is a INTEGER
            
            //Verify SOD sign using DS public key and sign algoritm
            using (var rsa = new RSAService(dsModPubKey, dsExpPubKey))
            {
                var isValidSod = false;
                switch (OIDDSMessageDigestSignAlgo)
                {
                    case "1.2.840.113549.1.1.5":
                        isValidSod = rsa.CheckSHA1withRSAEncryption(Signature.Data, SignatureAlgoritmIdentifier.MakeTag(0x31));
                        break;
                    case "1.2.840.113549.1.1.10":
                        isValidSod = rsa.CheckRSASSAPSS(Signature.Data, SignatureAlgoritmIdentifier.MakeTag(0x31), digestAlgo);
                        break;
                    default:
                        throw new Exception("Sign aloritm not supported");
                }
                if (!isValidSod)
                    throw new Exception("SOD sign wrong"); //SOD sign wrong
            }

            //Get issuer information from Document Signer certificate and compare it with data in IssuerName ASN1 tag
            var issuer = DSCertificate.DeepChild(0).Child(3, 0x30); //0x30 Verify that result is a SEQUENCE
            //Compare issuer information obtained from DS certificate with information stored directly in IssuerName tag 
            if (issuer.Data.ToHexString() != IssuerName.Data.ToHexString())
                return false; //Issuer name check failed

            //Get serial number information from Document Signer certificate and compare it with data in SignerCertSerialNumber ASN1 tag  
            var serialNumber = DSCertificate.DeepChild(0).Child(1, 0x02); //02 Verify that result is a INTEGER
            if (serialNumber.Data.ToHexString() != SerialNumber.Data.ToHexString())
                return false; // Serial Number of certificate wrong

            //Get hash of NIS and verify it with NIS stored in file system
            var dgsHash = SignedData.Child(2, 0x30); //0x30 Verify that result is a SEQUENCE
            var isValidNis = false;
            foreach (var dg in dgsHash.children)
            {
                /*
                0xa1 : IdServizi
                0xa2 : Seriale carta
                0xa3 : Certificato utente
                0xa4 : Chiave pubblica di Internal Authentication
                0xa5 : Chiave pubblica di Internal Authentication per i Servizi
                0x1b : Parametri DH
                */
                var oID = dg.Child(0, 02); //02 Verify that result is a INTEGER
                if (oID.Data.ToHexString() == "a1")
                {
                    //Get hashed NIS from DG
                    var hashedNis = dg.Child(1, 04).Data;  //04 Verify that result is a OCTECT STRING                    
                    //Calculate hash of file nis and compare it with the other in dg
                    if (hashedNis.ToHexString() == nis.ToMessageDigest(digestAlgo).ToHexString())
                        isValidNis = true;
                }
            }

            //TODO: Check that certificate is issued by https://csca-ita.interno.gov.it/index_ITA.htm (CSCA certificate)

            return isValidNis;
        }
    }
}
