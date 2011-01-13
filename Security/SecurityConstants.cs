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

namespace Org.Mentalis.Security {
	/// <summary>
	/// Defines the external methods of the CryptoAPI and SCHANNEL API.
	/// </summary>
	internal sealed class SecurityConstants {
		/// <summary>
		/// Defeat instantiation of this class.
		/// </summary>
		private SecurityConstants() {}
		// The SSPI constants used troughout the class library
		public const int X509_ASN_ENCODING = 0x1;
		public const int CERT_COMPARE_SHIFT = 16;
		public const int CERT_COMPARE_ANY = 0;
		public const int CERT_FIND_ANY = (CERT_COMPARE_ANY << CERT_COMPARE_SHIFT);
		public const int CERT_STORE_PROV_FILENAME_A = 7;
		public const int PKCS_7_ASN_ENCODING = 0x00010000;
		public const int CERT_SHA1_HASH_PROP_ID = 3;
		public const int CERT_MD5_HASH_PROP_ID = 4;
		public const int CERT_HASH_PROP_ID = CERT_SHA1_HASH_PROP_ID;
		public const int CERT_NAME_FRIENDLY_DISPLAY_TYPE = 5;
		public const int CERT_NAME_ISSUER_FLAG = 0x1;
		public const int CERT_NAME_DISABLE_IE4_UTF8_FLAG = 0x00010000;
		public const int PUBLICKEYBLOB = 0x6;
		public const int CRYPT_NEWKEYSET = 0x00000008;
		public const int NTE_BAD_KEYSET = -2146893802;
		public const int CERT_COMPARE_ENHKEY_USAGE = 10;
		public const int CERT_FIND_ENHKEY_USAGE = (CERT_COMPARE_ENHKEY_USAGE << CERT_COMPARE_SHIFT);
		public const int CERT_FIND_CTL_USAGE = CERT_FIND_ENHKEY_USAGE;
		public const int CERT_FRIENDLY_NAME_PROP_ID = 11;
		public const int ERROR_MORE_DATA = 234;
		public const int RSA_CSP_PUBLICKEYBLOB = 19;
		public const int X509_NAME = 7;
		public const int X509_UNICODE_NAME = 20;
		public const int CERT_DATA_ENCIPHERMENT_KEY_USAGE = 0x10;
		public const int CERT_DIGITAL_SIGNATURE_KEY_USAGE = 0x80;
		public const int CERT_KEY_AGREEMENT_KEY_USAGE = 0x08;
		public const int CERT_KEY_CERT_SIGN_KEY_USAGE = 0x04;
		public const int CERT_KEY_ENCIPHERMENT_KEY_USAGE = 0x20;
		public const int CERT_NON_REPUDIATION_KEY_USAGE = 0x40;
		public const int CERT_OFFLINE_CRL_SIGN_KEY_USAGE = 0x02;
		public const int CERT_CHAIN_POLICY_SSL = 4;
		public const int AUTHTYPE_CLIENT = 1;
		public const int AUTHTYPE_SERVER = 2;
		public const int CP_ACP = 0; // default to ANSI code page
		public const int TRUST_E_CERT_SIGNATURE = -2146869244; //0x80096004
		public const int CERT_E_UNTRUSTEDROOT = -2146762487; //0x800B0109
		public const int CERT_E_UNTRUSTEDTESTROOT = -2146762483; //0x800B010D
		public const int CERT_E_CHAINING = -2146762486; //0x800B010A
		public const int CERT_E_WRONG_USAGE = -2146762480; //0x800B0110
		public const int CERT_E_EXPIRED = -2146762495; //0x800B0101
		public const int CERT_E_VALIDITYPERIODNESTING = -2146762494; //0x800B0102
		public const int CERT_E_PURPOSE = -2146762490; //0x800B0106
		public const int TRUST_E_BASIC_CONSTRAINTS = -2146869223; //0x80096019
		public const int CERT_E_ROLE = -2146762493; //0x800B0103
		public const int CERT_E_CN_NO_MATCH = -2146762481; //0x800B010F
		public const int CRYPT_E_REVOKED = -2146885616; //0x80092010
		public const int CRYPT_E_REVOCATION_OFFLINE = -2146885613; //0x80092013
		public const int CERT_E_REVOKED = -2146762484; //0x800B010C
		public const int CERT_E_REVOCATION_FAILURE = -2146762482; //0x800B010E
		public const int CRYPT_ACQUIRE_COMPARE_KEY_FLAG = 0x00000004;
		public const int CRYPT_ACQUIRE_SILENT_FLAG = 0x00000040;
		public const int CERT_STORE_ADD_NEW = 1;
		public const int CRYPT_E_EXISTS = -2146885627;
		public const int EXPORT_PRIVATE_KEYS = 0x0004;
		public const int CERT_COMPARE_SHA1_HASH = 1;
		public const int CERT_COMPARE_MD5_HASH = 4;
		public const int CERT_FIND_SHA1_HASH = CERT_COMPARE_SHA1_HASH << CERT_COMPARE_SHIFT;
		public const int CERT_FIND_MD5_HASH = CERT_COMPARE_MD5_HASH << CERT_COMPARE_SHIFT;
		public const int CERT_FIND_HASH = CERT_FIND_SHA1_HASH;
		public const int CERT_STORE_PROV_MEMORY = 2;
		public const int CERT_STORE_SAVE_AS_STORE = 1;
		public const int CERT_STORE_SAVE_AS_PKCS7 = 2;
		public const int CERT_STORE_SAVE_TO_MEMORY = 2;
		public const int CERT_STORE_PROV_PKCS7 = 5;
		public const int CERT_STORE_PROV_SERIALIZED = 6;
		public const int CERT_CHAIN_POLICY_IGNORE_NOT_TIME_VALID_FLAG = 0x00000001;
		public const int CERT_CHAIN_POLICY_IGNORE_CTL_NOT_TIME_VALID_FLAG = 0x00000002;
		public const int CERT_CHAIN_POLICY_IGNORE_NOT_TIME_NESTED_FLAG = 0x00000004;
		public const int CERT_CHAIN_POLICY_IGNORE_INVALID_BASIC_CONSTRAINTS_FLAG = 0x00000008;
		public const int CERT_CHAIN_POLICY_IGNORE_ALL_NOT_TIME_VALID_FLAGS = (	CERT_CHAIN_POLICY_IGNORE_NOT_TIME_VALID_FLAG | CERT_CHAIN_POLICY_IGNORE_CTL_NOT_TIME_VALID_FLAG | CERT_CHAIN_POLICY_IGNORE_NOT_TIME_NESTED_FLAG);
		public const int CERT_CHAIN_POLICY_ALLOW_UNKNOWN_CA_FLAG = 0x00000010;
		public const int CERT_CHAIN_POLICY_IGNORE_WRONG_USAGE_FLAG = 0x00000020;
		public const int CERT_CHAIN_POLICY_IGNORE_INVALID_NAME_FLAG = 0x00000040;
		public const int CERT_CHAIN_POLICY_IGNORE_INVALID_POLICY_FLAG = 0x00000080;
		public const int CERT_CHAIN_POLICY_IGNORE_END_REV_UNKNOWN_FLAG = 0x00000100;
		public const int CERT_CHAIN_POLICY_IGNORE_CTL_SIGNER_REV_UNKNOWN_FLAG = 0x00000200;
		public const int CERT_CHAIN_POLICY_IGNORE_CA_REV_UNKNOWN_FLAG = 0x00000400;
		public const int CERT_CHAIN_POLICY_IGNORE_ROOT_REV_UNKNOWN_FLAG = 0x00000800;
		public const int CERT_CHAIN_POLICY_IGNORE_ALL_REV_UNKNOWN_FLAGS = (CERT_CHAIN_POLICY_IGNORE_END_REV_UNKNOWN_FLAG | CERT_CHAIN_POLICY_IGNORE_CTL_SIGNER_REV_UNKNOWN_FLAG | CERT_CHAIN_POLICY_IGNORE_CA_REV_UNKNOWN_FLAG | CERT_CHAIN_POLICY_IGNORE_ROOT_REV_UNKNOWN_FLAG);
		public const int CERT_CHAIN_POLICY_ALLOW_TESTROOT_FLAG = 0x00008000;
		public const int CERT_CHAIN_POLICY_TRUST_TESTROOT_FLAG = 0x00004000;
		public const int CERT_KEY_PROV_INFO_PROP_ID = 2;
		public const int CERT_COMPARE_NAME_STR_A = 7;
		public const int CERT_INFO_SUBJECT_FLAG = 7;
		public const int CERT_FIND_SUBJECT_STR_A = (CERT_COMPARE_NAME_STR_A << CERT_COMPARE_SHIFT | CERT_INFO_SUBJECT_FLAG);
		public const int SCHANNEL_CRED_VERSION = 0x4;
		public const int SECPKG_CRED_OUTBOUND = 0x2;
		public const int SEC_E_OK = 0x0;
		public const int SEC_E_NO_CREDENTIALS = -2146893042; //0x8009030E
		public const int LMEM_FIXED = 0x0;
		public const int LMEM_ZEROINIT = 0x40;
		public const int ISC_REQ_SEQUENCE_DETECT = 0x8;
		public const int ISC_REQ_REPLAY_DETECT = 0x4;
		public const int ISC_REQ_CONFIDENTIALITY = 0x10;
		public const int ISC_REQ_ALLOCATE_MEMORY = 0x100;
		public const int ISC_REQ_EXTENDED_ERROR = 0x4000;
		public const int ISC_REQ_STREAM = 0x8000;
		public const int ISC_RET_EXTENDED_ERROR = 0x4000;
		public const int SEC_I_CONTINUE_NEEDED = 0x90312;
		public const int SECBUFFER_TOKEN = 2;
		public const int SECBUFFER_VERSION = 0;
		public const int SECURITY_NATIVE_DREP = 0x10;
		public const int SECPKG_ATTR_ISSUER_LIST_EX = 0x59; // returns SecPkgContext_IssuerListInfoEx
		public const int CERT_CHAIN_FIND_BY_ISSUER = 1;
		public const int SEC_E_INCOMPLETE_MESSAGE = -2146893032; //0x80090318
		public const int SEC_E_ILLEGAL_MESSAGE = -2146893018; //0x80090326
		public const int SEC_I_INCOMPLETE_CREDENTIALS = 0x90320;
		public const int SEC_E_INVALID_TOKEN = -2146893048; //0x80090308
		public const int SECBUFFER_EMPTY = 0; // Undefined, replaced by provider
		public const int SECBUFFER_EXTRA = 5; // Extra data
		public const int SECPKG_ATTR_STREAM_SIZES = 4;
		public const int SECBUFFER_STREAM_TRAILER = 6; // Security Trailer
		public const int SECBUFFER_STREAM_HEADER = 7; // Security Header
		public const int SECBUFFER_DATA = 1; // Packet data
		public const int SEC_I_RENEGOTIATE = 0x90321;
		public const int SCHANNEL_SHUTDOWN = 1; // gracefully close down a connection
		public const int SEC_I_CONTEXT_EXPIRED = 0x90317;
		public const int ASC_REQ_SEQUENCE_DETECT = 0x8;
		public const int ASC_REQ_REPLAY_DETECT = 0x4;
		public const int ASC_REQ_CONFIDENTIALITY = 0x10;
		public const int ASC_REQ_EXTENDED_ERROR = 0x8000;
		public const int ASC_REQ_ALLOCATE_MEMORY = 0x100;
		public const int ASC_REQ_STREAM = 0x10000;
		public const int SECPKG_CRED_INBOUND = 0x1;
		public const int CERT_STORE_PROV_SYSTEM_A = 9;
		public const int PROV_RSA_FULL = 1;
		public const int PROV_RSA_AES = 24;
		public const int CRYPT_VERIFYCONTEXT = -268435456; // 0xF0000000
		public const int ALG_CLASS_DATA_ENCRYPT = (3 << 13);
		public const int ALG_TYPE_BLOCK = (3 << 9);
		public const int ALG_SID_AES_128 = 14;
		public const int ALG_SID_AES_192 = 15;
		public const int ALG_SID_AES_256 = 16;
		public const int ALG_SID_DH_SANDF = 1;
		public const int CALG_AES_128 = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_AES_128);
		public const int CALG_AES_192 = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_AES_192);
		public const int CALG_AES_256 = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_AES_256);
		public const int CALG_DH_EPHEM = ALG_CLASS_KEY_EXCHANGE | ALG_TYPE_DH | ALG_SID_DH_EPHEM;
		public const int CALG_DH_SF = (ALG_CLASS_KEY_EXCHANGE | ALG_TYPE_DH | ALG_SID_DH_SANDF);
		public const int CALG_RSA_KEYX = ALG_CLASS_KEY_EXCHANGE | ALG_TYPE_RSA | ALG_SID_RSA_ANY;
		public const int CRYPT_EXPORTABLE = 0x00000001;
		public const int PLAINTEXTKEYBLOB = 0x8;
		public const int SIMPLEBLOB = 0x1;
		public const int NTE_EXISTS = -2146893809; // 0x8009000F
		public const int AT_KEYEXCHANGE = 1;
		public const int PRIVATEKEYBLOB = 0x7;
		public const int ALG_CLASS_KEY_EXCHANGE = (5 << 13);
		public const int ALG_TYPE_DH = (5 << 9);
		public const int ALG_SID_DH_EPHEM = 2;
		public const int ALG_TYPE_RSA = (2 << 9);
		public const int ALG_SID_RSA_ANY = 0;
		public const int KP_KEYLEN = 9; // Length of key in bits
		public const int ALG_SID_3DES = 3;
		public const int CALG_3DES = ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_BLOCK | ALG_SID_3DES;
		public const int CRYPT_FIRST = 1;
		public const int PP_ENUMALGS_EX = 22;
		public const int KP_ALGID = 7; // Key algorithm
		public const int ALG_TYPE_STREAM = (4 << 9);
		public const int ALG_SID_AES = 17;
		public const int ALG_SID_DES = 1;
		public const int ALG_SID_RC2 = 2;
		public const int ALG_SID_RC4 = 1;
		public const int CALG_AES = (ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_BLOCK | ALG_SID_AES);
		public const int CALG_DES = (ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_BLOCK | ALG_SID_DES);
		public const int CALG_RC2 = (ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_BLOCK | ALG_SID_RC2);
		public const int CALG_RC4 = (ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_STREAM | ALG_SID_RC4);
		public const int KP_IV = 1; // Initialization vector
		public const int KP_MODE = 4; // Mode of the cipher
		public const int KP_MODE_BITS = 5; // Number of bits to feedback
		public const int SCH_CRED_USE_DEFAULT_CREDS = 0x40;
		public const int SCH_CRED_AUTO_CRED_VALIDATION = 0x20;
		public const int SCH_CRED_NO_DEFAULT_CREDS = 0x10;
		public const int SCH_CRED_MANUAL_CRED_VALIDATION = 0x8;
		public const int SCH_CRED_NO_SERVERNAME_CHECK = 0x4;
		public const int SECPKG_ATTR_REMOTE_CERT_CONTEXT = 0x53; // returns PCCERT_CONTEXT
		public const int SECPKG_ATTR_LOCAL_CERT_CONTEXT = 0x54; // returns PCCERT_CONTEXT
		public const int ISC_REQ_MUTUAL_AUTH = 0x2;
		public const int ISC_REQ_MANUAL_CRED_VALIDATION = 0x00080000;
		public const int KP_BLOCKLEN = 8;       // Block size of the cipher
		public const int KP_PADDING = 3;       // Padding values
		public const int PKCS5_PADDING = 1;       // PKCS 5 (sec 6.2) padding method
		public const int RANDOM_PADDING = 2;
		public const int ZERO_PADDING = 3;
		public const int SCHANNEL_RENEGOTIATE = 0;   // renegotiate a connection
		public const int CERT_KEY_IDENTIFIER_PROP_ID = 20;
		public const int CRYPT_FIND_SILENT_KEYSET_FLAG = 0x40;
		public const int CRYPT_SILENT = 0x40;
		public const int ALG_CLASS_HASH = (4 << 13);
		public const int ALG_TYPE_ANY = 0;
		public const int ALG_SID_MD2 = 1;
		public const int CALG_MD2 = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_MD2);
		public const int HP_HASHVAL = 0x0002;  // Hash value
		public const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
		public const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;
		public const int CRYPTPROTECT_VERIFY_PROTECTION = 0x40;
		public const int ALG_SID_SSL3SHAMD5 = 8;
		public const int CALG_SSL3_SHAMD5 = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_SSL3SHAMD5);
		public const int CMSG_CERT_PARAM = 12;
		public const int CERT_PVK_FILE_PROP_ID = 12;
		public const int CERT_KEY_PROV_HANDLE_PROP_ID = 1;
		public const int CERT_SYSTEM_STORE_LOCATION_SHIFT = 16;
		public const int CERT_SYSTEM_STORE_CURRENT_USER_ID = 1;
		public const int CERT_SYSTEM_STORE_LOCAL_MACHINE_ID = 2;
		public const int CERT_SYSTEM_STORE_CURRENT_SERVICE_ID = 4;
		public const int CERT_SYSTEM_STORE_SERVICES_ID = 5;
		public const int CERT_SYSTEM_STORE_USERS_ID = 6;
		public const int CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY_ID = 7;
		public const int CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY_ID = 8;
		public const int CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE_ID = 9;
		public const int CERT_SYSTEM_STORE_CURRENT_USER = (CERT_SYSTEM_STORE_CURRENT_USER_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT);
		public const int CERT_SYSTEM_STORE_LOCAL_MACHINE = (CERT_SYSTEM_STORE_LOCAL_MACHINE_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT);
		public const int CERT_SYSTEM_STORE_CURRENT_SERVICE = (CERT_SYSTEM_STORE_CURRENT_SERVICE_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT);
		public const int CERT_SYSTEM_STORE_SERVICES = (CERT_SYSTEM_STORE_SERVICES_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT);
		public const int CERT_SYSTEM_STORE_USERS = (CERT_SYSTEM_STORE_USERS_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT);
		public const int CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY = (CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT);
		public const int CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY = (CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT);
		public const int CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE = (CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT);
		public const int CERT_COMPARE_KEY_IDENTIFIER = 15;
		public const int CERT_FIND_KEY_IDENTIFIER = (CERT_COMPARE_KEY_IDENTIFIER << CERT_COMPARE_SHIFT);
		public const int CERT_COMPARE_NAME = 2;
		public const int CERT_FIND_SUBJECT_NAME = (CERT_COMPARE_NAME << CERT_COMPARE_SHIFT | CERT_INFO_SUBJECT_FLAG);
		public const int CERT_COMPARE_NAME_STR_W = 8;
		public const int CERT_FIND_SUBJECT_STR_W = (CERT_COMPARE_NAME_STR_W << CERT_COMPARE_SHIFT | CERT_INFO_SUBJECT_FLAG);
		public const int CERT_X500_NAME_STR = 3;
		public const int CERT_STORE_PROV_COLLECTION = 11;
		public const int X509_UNICODE_NAME_VALUE = 24;
		public const int X509_UNICODE_ANY_STRING = X509_UNICODE_NAME_VALUE;
		public const int X509_NAME_VALUE = 6;
		public const int X509_ANY_STRING = X509_NAME_VALUE;
		public const int CERT_RDN_ENCODED_BLOB = 1;
		public const int CERT_RDN_UNICODE_STRING = 12;
		public const int CERT_NAME_SIMPLE_DISPLAY_TYPE = 4;
		public const int CRYPT_MACHINE_KEYSET = 0x00000020;
		public const int CRYPT_USER_KEYSET = 0x00001000;
		public const int ALG_SID_MD4 = 2;
		public const int CALG_MD4 = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_MD4);
		public const int ALG_SID_MD5 = 3;
		public const int ALG_SID_SHA1 = 4;
		public const int CALG_MD5 = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_MD5);
		public const int CALG_SHA1 = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_SHA1);
		public const int CERT_CHAIN_REVOCATION_CHECK_END_CERT = 0x10000000;
		public const int CERT_CHAIN_REVOCATION_CHECK_CHAIN = 0x20000000;
		public const int CERT_CHAIN_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT = 0x40000000;
		public const int CERT_CHAIN_REVOCATION_CHECK_CACHE_ONLY = -2147483648; //0x80000000;
		public const int CERT_CHAIN_DISABLE_PASS1_QUALITY_FILTERING = 0x00000040;
		public const int CERT_CHAIN_RETURN_LOWER_QUALITY_CONTEXTS = 0x00000080;
		public const int CERT_CHAIN_DISABLE_AUTH_ROOT_AUTO_UPDATE = 0x00000100;
		public const int CERT_CHAIN_CACHE_END_CERT = 0x00000001;
		public const int CERT_CHAIN_CACHE_ONLY_URL_RETRIEVAL = 0x00000004;
		public const int PROV_DSS_DH = 13;
		public const int KP_P = 11;      // DSS/Diffie-Hellman P value
		public const int KP_G = 12;      // DSS/Diffie-Hellman G value
		public const int KP_X = 14;      // Diffie-Hellman X value
		public const int ALG_SID_CYLINK_MEK = 12;
		public const int CALG_CYLINK_MEK = (ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_BLOCK | ALG_SID_CYLINK_MEK);
		public const int CRYPT_PREGEN = 0x00000040;
		public const string szOID_COMMON_NAME = "2.5.4.3";  // case-ignore string
		public const string szOID_RSA_unstructName = "1.2.840.113549.1.9.2";
		public const string szOID_ORGANIZATION_NAME = "2.5.4.10"; // case-ignore string
		public const string KEY_CONTAINER = "{48959A69-B181-4cdd-B135-7565701307C5}";
	}
}