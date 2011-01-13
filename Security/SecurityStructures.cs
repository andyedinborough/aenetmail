/*
 *   Mentalis.org Security Library
 * 
 *     Copyright © 2002-2005, The Mentalis.org Team
 *     All rights reserved.
 *     http://www.mentalis.org/
 *
 *
 *   Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions
 *   are met:
 *
 *     - Redistributions of source code must retain the above copyright
 *        notice, this list of conditions and the following disclaimer. 
 *
 *     - Neither the name of the Mentalis.org Team, nor the names of its contributors
 *        may be used to endorse or promote products derived from this
 *        software without specific prior written permission. 
 *
 *   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 *   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 *   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 *   FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
 *   THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 *   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 *   (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 *   SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 *   HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 *   STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 *   ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 *   OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Runtime.InteropServices;

namespace Org.Mentalis.Security {
	/// <summary>
	/// The DataBlob structure contains an array of bytes. 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct DataBlob { //CRYPT_DATA_BLOB, CRYPTOAPI_BLOB
		public int cbData;
		public IntPtr pbData;
	}
	/// <summary>
	/// The CertificateInfo structure contains a certificate's information.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct CertificateInfo { //CERT_INFO
		public int dwVersion;
		public int SerialNumbercbData;
		public IntPtr SerialNumberpbData; // BYTE*
		public IntPtr SignatureAlgorithmpszObjId; // LPSTR
		public int SignatureAlgorithmParameterscbData;
		public IntPtr SignatureAlgorithmParameterspbData; // BYTE*
		public int IssuercbData;
		public IntPtr IssuerpbData; // BYTE*
		public long NotBefore; // FILETIME
		public long NotAfter; // FILETIME
		public int SubjectcbData;
		public IntPtr SubjectpbData; // BYTE*
		public IntPtr SubjectPublicKeyInfoAlgorithmpszObjId; // LPSTR
		public int SubjectPublicKeyInfoAlgorithmParameterscbData;
		public IntPtr SubjectPublicKeyInfoAlgorithmParameterspbData; // BYTE*
		public int SubjectPublicKeyInfoPublicKeycbData;
		public IntPtr SubjectPublicKeyInfoPublicKeypbData; // BYTE*
		public int SubjectPublicKeyInfoPublicKeycUnusedBits;
		public int IssuerUniqueIdcbData;
		public IntPtr IssuerUniqueIdpbData; // BYTE*
		public int IssuerUniqueIdcUnusedBits;
		public int SubjectUniqueIdcbData;
		public IntPtr SubjectUniqueIdpbData; // BYTE*
		public int SubjectUniqueIdcUnusedBits;
		public int cExtension;
		public IntPtr rgExtension; // /PCERT_EXTENSION/
	}
	/// <summary>
	/// The CERT_PUBLIC_KEY_INFO structure contains a public key and its algorithm.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct CERT_PUBLIC_KEY_INFO {
		public CERT_PUBLIC_KEY_INFO(CertificateInfo info) {
			pszObjId = info.SubjectPublicKeyInfoAlgorithmpszObjId;
			agcbData = info.SubjectPublicKeyInfoAlgorithmParameterscbData;
			agpbData = info.SubjectPublicKeyInfoAlgorithmParameterspbData;
			pkcbData = info.SubjectPublicKeyInfoPublicKeycbData;
			pkpbData = info.SubjectPublicKeyInfoPublicKeypbData;
			pkcUnusedBits = info.SubjectPublicKeyInfoPublicKeycUnusedBits;
		}
		public IntPtr pszObjId;
		public int agcbData;
		public IntPtr agpbData;
		public int pkcbData;
		public IntPtr pkpbData;
		public int pkcUnusedBits;
	}
	/// <summary>
	/// The CertificateContext structure contains both the encoded and decoded representations of a certificate.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct CertificateContext { //CERT_CONTEXT
		public int dwCertEncodingType;
		public IntPtr pbCertEncoded; // BYTE*
		public int cbCertEncoded;
		public IntPtr pCertInfo; // PCERT_INFO
		public IntPtr hCertStore; // HCERTSTORE
	}
	/// <summary>
	/// The TrustListUsage structure contains an array of Object Identifiers (OIDs) for Certificate Trust List (CTL) extensions. CTL_USAGE structures are used in functions that search for CTLs for specific uses.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct TrustListUsage { //CTL_USAGE
		public int cUsageIdentifier;
		public IntPtr rgpszUsageIdentifier;
	}
	/// <summary>
	/// The CertificateExtension structure contains the extension information for a certificate, Certificate Revocation List (CRL) or Certificate Trust List (CTL).
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct CertificateExtension { //CERT_EXTENSION
		public IntPtr pszObjId; //LPSTR
		public int fCritical;
		public int cbData;
		public IntPtr pbData;
	}
	/// <summary>
	/// The CertificateNameValue structure contains a relative distinguished name (RDN) attribute value. It is like the CERT_RDN_ATTR structure, except that it does not include the object identifier member that is a member of CERT_RDN_ATTR. As in CERT_RDN_ATTR, the interpretation of the Value member depends on dwValueType.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct CertificateNameValue { //CERT_NAME_VALUE
		public int dwValueType;
		public int cbData;
		public IntPtr pbData;
	}
	/// <summary>
	/// The CertificateNameInfo structure contains subject or issuer names. The information is represented as an array of CERT_RDN structures.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct CertificateNameInfo { //CERT_NAME_INFO
		public int cRDN;
		public IntPtr rgRDN; //PCERT_RDN
	}
	/// <summary>
	/// The RelativeDistinguishedName structure contains a relative distinguished name (RDN) consisting of an array of CERT_RDN_ATTR structures.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct RelativeDistinguishedName { //CERT_RDN
		public int cRDNAttr;
		public IntPtr rgRDNAttr;
	}
	/// <summary>
	/// The RdnAttribute structure contains a single attribute of a relative distinguished name (RDN). A whole RDN is expressed in a CERT_RDN structure that contains an array of CERT_RDN_ATTR structures.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct RdnAttribute { //CERT_RDN_ATTR
		public IntPtr pszObjId; //LPSTR
		public int dwValueType;
		public int cbData;
		public IntPtr pbData;
	}
	/// <summary>
	/// The ChainParameters structure establishing the searching and matching criteria to be used in building a certificate chain.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct ChainParameters { //CERT_CHAIN_PARA
		public int cbSize;
		public int RequestedUsagedwType;
		public int RequestedUsagecUsageIdentifier;
		public IntPtr RequestedUsagergpszUsageIdentifier;
		//public int RequestedIssuancePolicydwType;
		//public int RequestedIssuancePolicycUsageIdentifier;
		//public IntPtr RequestedIssuancergpszPolicyIdentifier;
		//public int dwUrlRetrievalTimeout;
		//public int fCheckRevocationFreshnessTime;
		//public int dwRevocationFreshnessTime;
	}
	/// <summary>
	/// The ChainPolicyStatus structure holds certificate chain status information returned by CertVerifyCertificateChainPolicy from the verification of certificate chains.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct ChainPolicyStatus { //CERT_CHAIN_POLICY_STATUS
		public int cbSize;
		public int dwError;
		public int lChainIndex;
		public int lElementIndex;
		public IntPtr pvExtraPolicyStatus; 
	}
	/// <summary>
	/// The ChainPolicyParameters structure contains information used in CertVerifyCertificateChainPolicy to establish policy criteria for the verification of certificate chains.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct ChainPolicyParameters { //CERT_CHAIN_POLICY_PARA
		public int cbSize;         // sizeof(CERT_CHAIN_POLICY_PARA);
		public int dwFlags;
		public IntPtr pvExtraPolicyPara;  
	}
	/// <summary>
	/// The SslPolicyParameters structure contains extra policy options.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct SslPolicyParameters { //HTTPSPolicyCallbackData or SSL_EXTRA_CERT_CHAIN_POLICY_PARA 
		public int cbSize;         // sizeof(HTTPSPolicyCallbackData);
		public int dwAuthType;
		public int fdwChecks;
		public IntPtr pwszServerName; // pointer to a Unicode string // used to check against CN=xxxx
	}
	/// <summary>
	/// The CRYPT_KEY_PROV_INFO structure contains fields that are passed as the arguments to CryptAcquireContext to acquire a handle to a particular key container within a particular cryptographic service provider (CSP), or to create or destroy a key container.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
	struct CRYPT_KEY_PROV_INFO {
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pwszContainerName;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pwszProvName;
		public int dwProvType;
		public int dwFlags;
		public int cProvParam;
		public IntPtr rgProvParam;
		public int dwKeySpec;
	}
	/// <summary>
	/// Union of the PUBLICKEYSTRUC [=BLOBHEADER] and RSAPUBKEY structures 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
	struct PUBLIC_KEY_BLOB {
		public byte bType;
		public byte bVersion;
		public short reserved;
		public int aiKeyAlg;
		public int magic;
		public int bitlen;
		public int pubexp;
	}
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	struct PROV_ENUMALGS_EX {
		public int aiAlgid;
		public int dwDefaultLen;
		public int dwMinLen;
		public int dwMaxLen;
		public int dwProtocols;
		public int dwNameLen;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=20)]
		public string szName;
		public int dwLongNameLen;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=40)]
		public string szLongName;
	}
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	struct CERT_EXTENSION {
		public IntPtr pszObjId;
		public int fCritical;
		public int ValuecbData;
		public IntPtr ValuepbData;
	}
}