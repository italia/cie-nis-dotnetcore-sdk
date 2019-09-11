
namespace CIE.NIS.SDK.Smartcard
{
    using System;
    using System.Security.Cryptography;    
    using Extensions;
    
    public class IASSod
    {        
        private readonly byte[] OID_rsawithSHA256 = new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x0b }; 
        private readonly byte[] OID_rsawithSHA1 = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x05 };
        private readonly byte[] OID_root = new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x07, 0x02 }; //1.2.840.113549.1.7.2 signedData (PKCS #7)
        private readonly byte[] OID_mRTDSignatureData = new byte[] { 0x67, 0x81, 0x08, 0x01, 0x01, 0x01 }; //2.23.136.1.1.1 mRTDSignatureData (ICAO MRTD)
        private readonly byte[] OID_contentType = new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x09, 0x03 }; // 1.2.840.113549.1.9.3 contentType (PKCS #9)
        private readonly byte[] OID_messageDigest = new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x09, 0x04 }; // 1.2.840.113549.1.9.4 messageDigest (PKCS #9)
        private readonly byte[] OID_SH256 = new byte[] { 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x01 };
        private readonly ASN1Tag iasSignature;

        private IASSod(byte[] sod)
        {                        
            //Create asn1 structure of IAS sod object
            var asn1Sod = ASN1Tag.Parse(sod, true);
            //Select and verify root node
            var root = asn1Sod.Child(0);
            root.Child(0, 06).Verify(OID_root); //1.2.840.113549.1.7.2 signedData (PKCS #7)
            iasSignature = root.DeepChild(1, 0, 2);
            iasSignature.Child(0, 06).Verify(OID_mRTDSignatureData); //2.23.136.1.1.1 mRTDSignatureData (ICAO MRTD)            
            /* 
            Get signed data that contains hashed information of:
            - ID servizi
            - Seriale carta
            - Certificato utente
            - Chiave pubblica di internal authentication
            - Chiave pubblica di internal authentication per i servizi
            La struttura dati contenuta è la seguente:
            SEQUENCE (3 elem)
            INTEGER 0
            SEQUENCE (1 elem)
                OBJECT IDENTIFIER 2.16.840.1.101.3.4.2.1 sha-256 (NIST Algorithm)
            SEQUENCE (6 elem)
                SEQUENCE (2 elem)
                INTEGER -91
                OCTET STRING (32 byte) 0000000000000000000000000000000000000000000000000000000000000000
                SEQUENCE (2 elem)
                INTEGER -92
                OCTET STRING (32 byte) 0000000000000000000000000000000000000000000000000000000000000000
                SEQUENCE (2 elem)
                INTEGER -95
                OCTET STRING (32 byte) 0000000000000000000000000000000000000000000000000000000000000000
                SEQUENCE (2 elem)
                INTEGER -93
                OCTET STRING (32 byte) 0000000000000000000000000000000000000000000000000000000000000000
                SEQUENCE (2 elem)
                INTEGER -94
                OCTET STRING (32 byte) 0000000000000000000000000000000000000000000000000000000000000000
                SEQUENCE (2 elem)
                INTEGER 27
                OCTET STRING (32 byte) 0000000000000000000000000000000000000000000000000000000000000000            
             */
            SignedData = iasSignature.DeepChild(1, 0).Child(0, 0x30); //0x30 Verify that result is a SEQUENCE            
            /* 
            Get information of certificate
            SEQUENCE (3 elem)
            SEQUENCE (8 elem)
                [0] (1 elem)
                INTEGER 2
                INTEGER (61 bit) 1505167906830807018
                SEQUENCE (2 elem)
                OBJECT IDENTIFIER 1.2.840.113549.1.1.5 sha1WithRSAEncryption (PKCS #1)
                NULL
                SEQUENCE (4 elem)
                SET (1 elem)
                    SEQUENCE (2 elem)
                    OBJECT IDENTIFIER 2.5.4.3 commonName (X.520 DN component)
                    UTF8String Italian Country Signer CA
                SET (1 elem)
                    SEQUENCE (2 elem)
                    OBJECT IDENTIFIER 2.5.4.11 organizationalUnitName (X.520 DN component)
                    UTF8String National Electronic Center of State Police
                SET (1 elem)
                    SEQUENCE (2 elem)
                    OBJECT IDENTIFIER 2.5.4.10 organizationName (X.520 DN component)
                    UTF8String Ministry of Interior
                SET (1 elem)
                    SEQUENCE (2 elem)
                    OBJECT IDENTIFIER 2.5.4.6 countryName (X.520 DN component)
                    PrintableString IT
                SEQUENCE (2 elem)
                UTCTime 2019-03-28 09:56:10 UTC
                UTCTime 2030-06-28 09:56:10 UTC
                SEQUENCE (5 elem)
                SET (1 elem)
                    SEQUENCE (2 elem)
                    OBJECT IDENTIFIER 2.5.4.3 commonName (X.520 DN component)
                    UTF8String eIdentityCardSigner
                SET (1 elem)
                    SEQUENCE (2 elem)
                    OBJECT IDENTIFIER 2.5.4.5 serialNumber (X.520 DN component)
                    PrintableString 00014
                SET (1 elem)
                    SEQUENCE (2 elem)
                    OBJECT IDENTIFIER 2.5.4.11 organizationalUnitName (X.520 DN component)
                    UTF8String Direz. Centr. per i Servizi Demografici - CNSD
                SET (1 elem)
                    SEQUENCE (2 elem)
                    OBJECT IDENTIFIER 2.5.4.10 organizationName (X.520 DN component)
                    UTF8String Ministry of Interior
                SET (1 elem)
                    SEQUENCE (2 elem)
                    OBJECT IDENTIFIER 2.5.4.6 countryName (X.520 DN component)
                    PrintableString IT
                SEQUENCE (2 elem)
                SEQUENCE (2 elem)
                    OBJECT IDENTIFIER 1.2.840.113549.1.1.1 rsaEncryption (PKCS #1)
                    NULL
                BIT STRING (1 elem)
                    SEQUENCE (2 elem)
                    INTEGER (2048 bit) 277154587434116957602280010100068985711032991137406187672091719020519...
                    INTEGER 65537
                [3] (1 elem)
                SEQUENCE (3 elem)
                    SEQUENCE (2 elem)
                    OBJECT IDENTIFIER 2.5.29.14 subjectKeyIdentifier (X.509 extension)
                    OCTET STRING (1 elem)
                        OCTET STRING (20 byte) 81FC88788E30CABBBA321D0ADDE2E7437316F44C
                    SEQUENCE (2 elem)
                    OBJECT IDENTIFIER 2.5.29.35 authorityKeyIdentifier (X.509 extension)
                    OCTET STRING (1 elem)
                        SEQUENCE (1 elem)
                        [0] (20 byte) 436CE3921D10922307EFD7A2F577ED7524467F1B
                    SEQUENCE (3 elem)
                    OBJECT IDENTIFIER 2.5.29.15 keyUsage (X.509 extension)
                    BOOLEAN true
                    OCTET STRING (1 elem)
                        BIT STRING (1 bit) 1
            SEQUENCE (2 elem)
                OBJECT IDENTIFIER 1.2.840.113549.1.1.5 sha1WithRSAEncryption (PKCS #1)
                NULL
            BIT STRING (4096 bit) 000011101010101100001000000001010010010010001011001111100011110001001...
            */
            SignedCert = root.DeepChild(1, 0, 3).Child(0, 0x30); //0x30 Verify that result is a SEQUENCE            
            /*
            Get information of issuer
            SEQUENCE (4 elem)
            SET (1 elem)
                SEQUENCE (2 elem)
                OBJECT IDENTIFIER 2.5.4.3 commonName (X.520 DN component)
                UTF8String Italian Country Signer CA
            SET (1 elem)
                SEQUENCE (2 elem)
                OBJECT IDENTIFIER 2.5.4.11 organizationalUnitName (X.520 DN component)
                UTF8String National Electronic Center of State Police
            SET (1 elem)
                SEQUENCE (2 elem)
                OBJECT IDENTIFIER 2.5.4.10 organizationName (X.520 DN component)
                UTF8String Ministry of Interior
            SET (1 elem)
                SEQUENCE (2 elem)
                OBJECT IDENTIFIER 2.5.4.6 countryName (X.520 DN component)
                PrintableString IT             
            */
            IssuerName = root.DeepChild(1, 0, 4, 0, 1).Child(0, 0x30); //0x30 Verify that result is a SEQUENCE            
            /*
               Get information of Signer certificate serial number
               INTEGER (61 bit) 0000000000000000...
             */
            SignerCertSerialNumber = root.DeepChild(1, 0, 4, 0, 1).Child(1, 02); //02 Verify that result is a INTEGER            
            /*
            Get information about signer.
            SEQUENCE (2 elem)
            OBJECT IDENTIFIER 1.2.840.113549.1.9.3 contentType (PKCS #9)
            SET (1 elem)
                OBJECT IDENTIFIER 2.23.136.1.1.1 mRTDSignatureData (ICAO MRTD)
            SEQUENCE (2 elem)
            OBJECT IDENTIFIER 1.2.840.113549.1.9.4 messageDigest (PKCS #9)
            SET (1 elem)
                OCTET STRING (32 byte) F92C82AE7A944DCFFE4CCD0B6CAFA9D0134ED16EBEEFE6D9A44B641920F99BA9
            */
            SignerInfo = root.DeepChild(1, 0, 4, 0).Child(3, 0xA0); //0xA0 Verify that result is a CONTEXT_SPECIFIC
            //Validate signer info
            SignerInfo.DeepChild(0, 0).Verify(OID_contentType); // 1.2.840.113549.1.9.3 contentType (PKCS #9)
            SignerInfo.DeepChild(0, 1, 0).Verify(OID_mRTDSignatureData); // //2.23.136.1.1.1 mRTDSignatureData (ICAO MRTD)
            SignerInfo.DeepChild(1, 0).Verify(OID_messageDigest); // 1.2.840.113549.1.9.4 messageDigest (PKCS #9)
            /* Get digest thar is octect string in SignerInfo */
            Digest = SignerInfo.DeepChild(1, 1).Child(0, 04); //04 Verify that result is a OCTECT STRING
            // Check sign digest algorithm
            var digestAlg = root.DeepChild(1, 0, 4, 0, 4).Child(0, 06); //06 Verify that result is a OBJECT
            if (digestAlg.Data.ToHexString() == OID_rsawithSHA1.ToHexString())
                DigestAlgo = "SHA1";
            else if ((digestAlg.Data.ToHexString() == OID_rsawithSHA256.ToHexString()))
                DigestAlgo = "SHA256";
            else
                throw new Exception("Unable to validate sign digest algorithm");            
            /* Get signature
               OCTET STRING (256 byte) 42C600B32BD5EFF0A684F65BD4526872AD3D4EADA3017A6E836736340BCCDA7DB9622...
            */
            Signature = root.DeepChild(1, 0, 4, 0).Child(5, 04); //04 Verify that result is a OCTECT STRING
        }
        public static IASSod Create(byte[] sod)
        {
            return new IASSod(sod);
        }

        public ASN1Tag SignedData { get; set; }
        public ASN1Tag SignedCert { get; set; }
        public ASN1Tag IssuerName { get; set; }
        public ASN1Tag SignerCertSerialNumber { get; set; }
        public ASN1Tag SignerInfo { get; set; } 
        public ASN1Tag Digest { get; set; }
        public string DigestAlgo { get; set; }
        public ASN1Tag Signature { get; set; }
        public bool Verify(byte[] nis)
        {
            /*
            Select hash of
            - ID servizi
            - Seriale carta
            - Certificato utente
            - Chiave pubblica di internal authentication
            - Chiave pubblica di internal authentication per i servizi
            and calculate hash of hash  
            */
            var digestRecalculation = iasSignature.DeepChild(1, 0).Data.ToMessageDigest("SHA256");
            //Check in recalculation of digest is equal of digest in sod
            if (ByteArrayExtension.ToHexString(digestRecalculation) != ByteArrayExtension.ToHexString(Digest.Data))
                return false; // Digest SOD not correspond to data
                        
            // Get Document Signer public key from SignedCert data           
            var dsPubKey = SignedCert.DeepChild(0, 6, 1, 0);

            // DigestAlgo contain SHA1 or SHA256
            if (string.IsNullOrEmpty(DigestAlgo))
                throw new Exception("Digest algorithm is not valid");

            byte[] signature;
            // Use public key of DS certificate to perform an RSA operation
            using (var rsa = RSA.Create()) {
                signature = rsa.PureRSA(
                    Signature.Data,
                    dsPubKey.Child(0, 02).Data, // modulus. 02 Verify that result is a INTEGER
                    dsPubKey.Child(1, 02).Data // exp. 02 Verify that result is a INTEGER
                    );
            }
            //Remove algorithm from signature            
            signature = signature.RemoveHashAlgorithm(DigestAlgo); //signature.RemoveAlgorithm(DigestAlgo);
                        
            //Calculate message digest and compare it with signature            
            //var digestSignature = Crypto.CalculateMessageDigest(new ByteArray(SignerInfo.Data).ASN1Tag(0x31).Data, DigestAlgo);
            var digestSignature = SignerInfo.MakeTag(0x31).ToMessageDigest(DigestAlgo);
            if (digestSignature.ToHexString() != signature.ToHexString())
                return false; // SOD sign wrong
            
            //Get issuer information from Document Signer certificate and compare it with data in IssuerName ASN1 tag
            var issuer = SignedCert.DeepChild(0).Child(3, 0x30); //0x30 Verify that result is a SEQUENCE
            //Compare issuer information obtained from DS certificate with information stored directly in IssuerName tag 
            if (issuer.Data.ToHexString() != IssuerName.Data.ToHexString())            
                return false; //Issuer name check failed
                        
            //Get serial number information from Document Signer certificate and compare it with data in SignerCertSerialNumber ASN1 tag  
            var serialNumber = SignedCert.DeepChild(0).Child(1, 0x02); //02 Verify that result is a INTEGER
            if (serialNumber.Data.ToHexString() != SignerCertSerialNumber.Data.ToHexString())
                return false; // Serial Number of certificate wrong

            //Verify child of signed data
            SignedData.Child(0, 02).Verify(new byte[] { 0 });
            SignedData.DeepChild(1).Child(0, 06).Verify(OID_SH256);

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
                var oID = dg.Child(0, 02); //2 Verify that result is a INTEGER
                if(oID.Data.ToHexString() == "a1")
                {
                    //Get hashed NIS from DG
                    var hashedNis = dg.Child(1, 04).Data;  //04 Verify that result is a OCTECT STRING                    
                    //Calculate hash of file nis and compare it with the other in dg
                    if (hashedNis.ToHexString() == nis.ToMessageDigest("SHA256").ToHexString())
                        isValidNis = true;
                }
            }

            //TODO: Check that certificate is issued by https://csca-ita.interno.gov.it/index_ITA.htm (CSCA certificate)

            return isValidNis;
        }
    }
}
