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

namespace Org.Mentalis.Security.Certificates {
	/// <summary>
	/// Defines the different hash type values.
	/// </summary>
	public enum HashType : int {
		/// <summary>The certificate will be hashed using the SHA1 algorithm.</summary>
		SHA1 = SecurityConstants.CERT_SHA1_HASH_PROP_ID,
		/// <summary>The certificate will be hashed using the MD5 algorithm.</summary>
		MD5 = SecurityConstants.CERT_MD5_HASH_PROP_ID,
		/// <summary>The certificate will be hashed using the default hashing algorithm.</summary>
		Default = SecurityConstants.CERT_HASH_PROP_ID
	}
	/// <summary>
	/// Defines the different key usage values.
	/// </summary>
	[Flags]
	public enum KeyUsage : int {
		/// <summary>The key can be used for data encipherment.</summary>
		DataEncipherment = SecurityConstants.CERT_DATA_ENCIPHERMENT_KEY_USAGE,
		/// <summary>The key can be used to sign data.</summary>
		DigitalSignature = SecurityConstants.CERT_DIGITAL_SIGNATURE_KEY_USAGE,
		/// <summary>The key can be used in key agreement algorithms.</summary>
		KeyAgreement = SecurityConstants.CERT_KEY_AGREEMENT_KEY_USAGE,
		/// <summary>The key can be used to sign certificates.</summary>
		KeyCertSign = SecurityConstants.CERT_KEY_CERT_SIGN_KEY_USAGE,
		/// <summary>The key can be used for key encipherment.</summary>
		KeyEncipherment = SecurityConstants.CERT_KEY_ENCIPHERMENT_KEY_USAGE,
		/// <summary>The key can be used for electronic non-repudiation.</summary>
		NonRepudiation = SecurityConstants.CERT_NON_REPUDIATION_KEY_USAGE,
		/// <summary>The key can be used to sign certificate revocation lists.</summary>
		CrlSign = SecurityConstants.CERT_OFFLINE_CRL_SIGN_KEY_USAGE
	}
	/// <summary>
	/// Defines the different authentication type values.
	/// </summary>
	public enum AuthType : int {
		/// <summary>The certificate is a client certificate.</summary>
		Client = SecurityConstants.AUTHTYPE_CLIENT, // used to validate a certificate that comes from a client
		/// <summary>The certificate is a server certificate.</summary>
		Server = SecurityConstants.AUTHTYPE_SERVER // used to validate a certificate that comes from a server
	}
	/// <summary>
	/// Defines the different certificate status values.
	/// </summary>
	public enum CertificateStatus : int {
		/// <summary>The certificate is valid.</summary>
		ValidCertificate = 0,
		/// <summary>A required certificate is not within its validity period.</summary>
		Expired = SecurityConstants.CERT_E_EXPIRED,
		/// <summary>The certificate's basic constraints are invalid or missing.</summary>
		InvalidBasicConstraints = SecurityConstants.TRUST_E_BASIC_CONSTRAINTS,
		/// <summary>A chain of certificates was not correctly created.</summary>
		InvalidChain = SecurityConstants.CERT_E_CHAINING,
		/// <summary>The validity periods of the certification chain do not nest correctly.</summary>
		InvalidNesting = SecurityConstants.CERT_E_VALIDITYPERIODNESTING,
		/// <summary>A certificate is being used for a non permitted purpose.</summary>
		InvalidPurpose = SecurityConstants.CERT_E_PURPOSE,
		/// <summary>A certificate that can only be used as an end-entity is being used as a CA or visa versa.</summary>
		InvalidRole = SecurityConstants.CERT_E_ROLE,
		/// <summary>The signature of the certificate cannot be verified.</summary>
		InvalidSignature = SecurityConstants.TRUST_E_CERT_SIGNATURE,
		/// <summary>The certificate's CN name does not match the passed value.</summary>
		NoCNMatch = SecurityConstants.CERT_E_CN_NO_MATCH,
		/// <summary>A certificate in the chain has been explicitly revoked by its issuer.</summary>
		ParentRevoked = SecurityConstants.CERT_E_REVOKED,
		/// <summary>The revocation process could not continue. The certificates could not be checked.</summary>
		RevocationFailure = SecurityConstants.CERT_E_REVOCATION_FAILURE,
		/// <summary>Since the revocation server was offline, the called function was not able to complete the revocation check.</summary>
		RevocationServerOffline = SecurityConstants.CRYPT_E_REVOCATION_OFFLINE,
		/// <summary>The certificate or signature has been revoked.</summary>
		Revoked = SecurityConstants.CRYPT_E_REVOKED,
		/// <summary>A certification chain processed correctly but terminated in a root certificate not trusted by the trust provider.</summary>
		UntrustedRoot = SecurityConstants.CERT_E_UNTRUSTEDROOT,
		/// <summary>The root certificate is a testing certificate and policy settings disallow test certificates.</summary>
		UntrustedTestRoot = SecurityConstants.CERT_E_UNTRUSTEDTESTROOT,
		/// <summary>The certificate is not valid for the requested usage.</summary>
		WrongUsage = SecurityConstants.CERT_E_WRONG_USAGE,
		/// <summary>The certificate is invalid.</summary>
		OtherError = -1,
	}
	/// <summary>
	/// Defines the different certificate store values.
	/// </summary>
	public enum CertificateStoreType : int {
		/// <summary>The certificate store should be saved as a serializes store.</summary>
		SerializedStore = SecurityConstants.CERT_STORE_SAVE_AS_STORE,
		/// <summary>The certificate store should be saved as a signed PKCS7 message.</summary>
		Pkcs7Message = SecurityConstants.CERT_STORE_SAVE_AS_PKCS7
	}
	/// <summary>
	/// Defines additional options for building a certificate chain.
	/// </summary>
	public enum CertificateChainOptions : int {
		/// <summary>The default chain options</summary>
		Default = 0,
		/// <summary>Revocation checking is done on the end certificate and only the end certificate.</summary>
		RevocationCheckEndCert = SecurityConstants.CERT_CHAIN_REVOCATION_CHECK_END_CERT,
		/// <summary>Revocation checking is done on all of the certificates in every chain.</summary>
		RevocationCheckChain = SecurityConstants.CERT_CHAIN_REVOCATION_CHECK_CHAIN,
		/// <summary>Revocation checking in done on all certificates in all of the chains except the root certificate.</summary>
		RevocationCheckChainExcludeRoot = SecurityConstants.CERT_CHAIN_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT,
		/// <summary>When this flag is set, the end certificate is cached, which might speed up the chain-building process. By default, the end certificate is not cached and it would need to be verified each time a chain is built for it.</summary>
		RevocationCacheEndCert = SecurityConstants.CERT_CHAIN_CACHE_END_CERT,
		/// <summary>Revocation checking only accesses cached URLs and does not hit the wire to do any revocation URL retrieval. Supported on Windows Me, Windows 2000 and later.</summary>
		RevocationCheckCacheOnly = SecurityConstants.CERT_CHAIN_REVOCATION_CHECK_CACHE_ONLY,
		/// <summary>Uses only cached URLs in building a certificate chain. The Internet and Intranet are not searched for URL-based objects. Note, not applicable to revocation checking. Set CERT_CHAIN_REVOCATION_CHECK_CACHE_ONLY to use only cached URLs for revocation checking.</summary>
		CacheOnlyUrlRetrieval = SecurityConstants.CERT_CHAIN_CACHE_ONLY_URL_RETRIEVAL,
		/// <summary>For performance reasons, the second pass of chain building only considers potential chain paths that have quality greater than or equal to the highest quality determined during the first pass. The first pass only considers valid signature, complete chain and trusted roots to calculate chain quality. This flag can be set to disable this optimization and consider all potential chain paths during the second pass.</summary>
		DisablePass1QualityFiltering = SecurityConstants.CERT_CHAIN_DISABLE_PASS1_QUALITY_FILTERING,
		/// <summary>The default is to return only the highest quality chain path. Setting this flag will return the lower quality chains. These are returned in the chain context's cLowerQualityChainContext and rgpLowerQualityChainContext fields.</summary>
		ReturnLowerQualityContexts = SecurityConstants.CERT_CHAIN_RETURN_LOWER_QUALITY_CONTEXTS,
		/// <summary>Setting this flag inhibits the auto update of third party roots from the Windows Update Web Server.</summary>
		DisableAuthRootAutoUpdate = SecurityConstants.CERT_CHAIN_DISABLE_AUTH_ROOT_AUTO_UPDATE,
	}
	/// <summary>
	/// Defines the different verificateion flags values.
	/// </summary>
	/// <remarks>
	/// You can specify more VerificationFlags at once by combining them with the OR operator.
	/// </remarks>
	[Flags]
	public enum VerificationFlags : int {
		/// <summary>No flags.</summary>
		None = 0,
		/// <summary>Ignore an invalid time.</summary>
		IgnoreTimeNotValid = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_NOT_TIME_VALID_FLAG,
		/// <summary>Ignore an invalid time of the certificate trust list.</summary>
		IgnoreCtlTimeNotValid = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_CTL_NOT_TIME_VALID_FLAG,
		/// <summary>Ignore an invalid time nesting.</summary>
		IgnoreTimeNotNested = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_NOT_TIME_NESTED_FLAG,
		/// <summary>Ignore invalid basic contraints.</summary>
		IgnoreInvalidBasicContraints = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_INVALID_BASIC_CONSTRAINTS_FLAG,
		/// <summary>Ignore all time checks.</summary>
		IgnoreAllTimeChecks = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_ALL_NOT_TIME_VALID_FLAGS,
		/// <summary>Allow an unknown certificate authority.</summary>
		AllowUnknownCA = SecurityConstants.CERT_CHAIN_POLICY_ALLOW_UNKNOWN_CA_FLAG,
		/// <summary>Ignore the wrong usage of a certificate.</summary>
		IgnoreWrongUsage = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_WRONG_USAGE_FLAG,
		/// <summary>Ignore an invalid name.</summary>
		IgnoreInvalidName = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_INVALID_NAME_FLAG,
		/// <summary>Ignore an invalid policy.</summary>
		IgnoreInvalidPolicy = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_INVALID_POLICY_FLAG,
		/// <summary>Ignore an unknown revocation status of the end certificate.</summary>
		IgnoreEndRevUnknown = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_END_REV_UNKNOWN_FLAG,
		/// <summary>Ignore an unknown revocation status of the signer certificate.</summary>
		IgnoreSignerRevUnknown = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_CTL_SIGNER_REV_UNKNOWN_FLAG,
		/// <summary>Ignore an unknown revocation status of the certificate authority.</summary>
		IgnoreCARevUnknown = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_CA_REV_UNKNOWN_FLAG,
		/// <summary>Ignore an unknown revocation status of the root certificate.</summary>
		IgnoreRootRevUnknown = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_ROOT_REV_UNKNOWN_FLAG,
		/// <summary>Ignore an unknown revocation status of any of the certificates.</summary>
		IgnoreAllRevUnknown = SecurityConstants.CERT_CHAIN_POLICY_IGNORE_ALL_REV_UNKNOWN_FLAGS,
		/// <summary>Allow a test root.</summary>
		AllowTestroot = SecurityConstants.CERT_CHAIN_POLICY_ALLOW_TESTROOT_FLAG,
		/// <summary>Trust a test root.</summary>
		TrustTestroot = SecurityConstants.CERT_CHAIN_POLICY_TRUST_TESTROOT_FLAG
	}
	/// <summary>
	/// Specifies the location of the X.509 certificate store.
	/// </summary>
	public enum StoreLocation {
		/// <summary>The certificate store for the current service.</summary>
		CurrentService = SecurityConstants.CERT_SYSTEM_STORE_CURRENT_SERVICE,
		/// <summary>The certificate store for the currently logged-on user.</summary>
		CurrentUser = SecurityConstants.CERT_SYSTEM_STORE_CURRENT_USER,
		/// <summary>The certificate store for the currently logged-on group.</summary>
		CurrentUserGroupPolicy = SecurityConstants.CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY,
		/// <summary>The certificate store for the local computer.</summary>
		LocalMachine = SecurityConstants.CERT_SYSTEM_STORE_LOCAL_MACHINE,
		/// <summary>The certificate store for the local machine enterprise downloaded from a network setting.</summary>
		LocalMachineEnterprise = SecurityConstants.CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE,
		/// <summary>The certificate store for the local machine group policy downloaded from a network setting.</summary>
		LocalMachineGroupPolicy = SecurityConstants.CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY,
		/// <summary>The certificate store for a specified service account; for example, an Alerter or the Event Log.</summary>
		Services = SecurityConstants.CERT_SYSTEM_STORE_SERVICES,
		/// <summary>The location is unknown.</summary>
		Unknown = 0,
		/// <summary>The certificate store for the users group of this computer.</summary>
		Users = SecurityConstants.CERT_SYSTEM_STORE_CURRENT_USER
	}
	/// <summary>
	/// Defines a structure that represents one attribute of a relative distinguished name.
	/// </summary>
	public struct NameAttribute {
		/// <summary>
		/// Initializes a new <see cref="NameAttribute"/> instance.
		/// </summary>
		/// <param name="oid">The object identifier of the attribute.</param>
		/// <param name="val">The decoded value of the attribute.</param>
		public NameAttribute(string oid, string val) {
			this.ObjectID = oid;
			this.Value = val;
		}
		/// <summary>
		/// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="NameAttribute"/>.
		/// </summary>
		/// <param name="obj">The Object to compare with the current Object. </param>
		/// <returns><b>true</b> if the specified Object is equal to the current NameAttribute; otherwise, <b>false</b>.</returns>
		public override bool Equals(object obj) {
			try {
				NameAttribute o = (NameAttribute)obj;
				return (o.ObjectID == this.ObjectID) && (o.Value == this.Value);
			} catch {
				return false;
			}
		}
		/// <summary>
		/// Serves as a hash function for a <see cref="NameAttribute"/> type, suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>A hash code for the current Object.</returns>
		public override int GetHashCode() {
			if (this.ObjectID == null && this.Value == null)
				return 0;
			else if (this.ObjectID == null)
				return this.Value.GetHashCode();
			else if (this.Value == null)
				return this.ObjectID.GetHashCode();
			else
				return this.Value.GetHashCode() ^ this.ObjectID.GetHashCode();
		}
		/// <summary>
		/// Returns a <see cref="String"/> that represents the current <see cref="NameAttribute"/>.
		/// </summary>
		/// <returns>A String that represents the current NameAttribute.</returns>
		public override string ToString() {
			if (ObjectID == null && Value == null)
				return "N/A: N/A";
			else if (ObjectID == null)
				return "N/A: " + Value;
			else if (Value == null)
				return ObjectID + ": N/A";
			else
				return ObjectID + ": " + Value;
		}
		/// <summary>
		/// The object identifier of the attribute.
		/// </summary>
		public string ObjectID;
		/// <summary>
		/// The value of the attribute.
		/// </summary>
		public string Value;
	}
	/// <summary>
	/// Represents an encoded certificate extension. 
	/// </summary>
	/// <remarks>These extensions can be decoded with the Certificate.DecodeExtension() method.</remarks>
	public class Extension {
		/// <summary>
		/// Initializes a new instance of the <see cref="Extension"/> class.
		/// </summary>
		/// <param name="oid">The object identifier of the extension.</param>
		/// <param name="critical"><b>true</b> if it is a critical extension, <b>false</b> otherwise.</param>
		/// <param name="val">A byte array that contains the encoded extension.</param>
		public Extension(string oid, bool critical, byte[] val) {
			this.ObjectID = oid;
			this.Critical = critical;
			this.EncodedValue = val;
		}
		/// <summary>
		/// The object identifier of the extension.
		/// </summary>
		public string ObjectID;
		/// <summary>
		/// <b>true</b> if it is a critical extension, <b>false</b> otherwise.
		/// </summary>
		public bool Critical;
		/// <summary>
		/// A byte array that contains the encoded extension.
		/// </summary>
		public byte[] EncodedValue;
	}
	/// <summary>
	/// Defines the different keyset locations.
	/// </summary>
	public enum KeysetLocation : int {
		/// <summary>The private keys are stored in the default location.</summary>
		Default = 0,
		/// <summary>The private keys are stored under local machine and not the current user.</summary>
		LocalMachine = SecurityConstants.CRYPT_MACHINE_KEYSET,
		/// <summary>The private keys are stored under the current user and not the local machine even if the PFX BLOB specifies they should go into local machine.</summary>
		CurrentUser = SecurityConstants.CRYPT_USER_KEYSET
	}
}