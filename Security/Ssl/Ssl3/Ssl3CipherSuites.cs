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
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Ssl.Shared;
using System.Text;
using System.IO;

namespace Org.Mentalis.Security.Ssl.Ssl3 {
	internal sealed class Ssl3CipherSuites {
		private Ssl3CipherSuites() {}
		public static CipherSuite InitializeCipherSuite(byte[] master, byte[] clientrnd, byte[] serverrnd, CipherDefinition definition, ConnectionEnd entity) {
			CipherSuite ret = new CipherSuite();
			SymmetricAlgorithm bulk = (SymmetricAlgorithm)Activator.CreateInstance(definition.BulkCipherAlgorithm);
			if (definition.BulkIVSize > 0)
				bulk.Mode = CipherMode.CBC;
			bulk.Padding = PaddingMode.None;
			bulk.BlockSize = definition.BulkIVSize * 8;
			// get the keys and IVs
			byte[] client_mac, server_mac, client_key, server_key, client_iv, server_iv;
			Ssl3DeriveBytes prf = new Ssl3DeriveBytes(master, clientrnd, serverrnd, false);
			client_mac = prf.GetBytes(definition.HashSize);
			server_mac = prf.GetBytes(definition.HashSize);
			client_key = prf.GetBytes(definition.BulkKeySize);
			server_key = prf.GetBytes(definition.BulkKeySize);
			client_iv = prf.GetBytes(definition.BulkIVSize);
			server_iv = prf.GetBytes(definition.BulkIVSize);
			prf.Dispose();
			if (definition.Exportable) { // make some extra modifications if the keys are exportable
				MD5 md5 = new MD5CryptoServiceProvider();
				md5.TransformBlock(client_key, 0, client_key.Length, client_key, 0);
				md5.TransformBlock(clientrnd, 0, clientrnd.Length, clientrnd, 0);
				md5.TransformFinalBlock(serverrnd, 0, serverrnd.Length);
				client_key = new byte[definition.BulkExpandedSize];
				Array.Copy(md5.Hash, 0, client_key, 0, client_key.Length);
				md5.Initialize();
				md5.TransformBlock(server_key, 0, server_key.Length, server_key, 0);
				md5.TransformBlock(serverrnd, 0, serverrnd.Length, serverrnd, 0);
				md5.TransformFinalBlock(clientrnd, 0, clientrnd.Length);
				server_key = new byte[definition.BulkExpandedSize];
				Array.Copy(md5.Hash, 0, server_key, 0, server_key.Length);
				md5.Initialize();
				md5.TransformBlock(clientrnd, 0, clientrnd.Length, clientrnd, 0);
				md5.TransformFinalBlock(serverrnd, 0, serverrnd.Length);
				client_iv = new byte[definition.BulkIVSize];
				Array.Copy(md5.Hash, 0, client_iv, 0, client_iv.Length);
				md5.Initialize();
				md5.TransformBlock(serverrnd, 0, serverrnd.Length, serverrnd, 0);
				md5.TransformFinalBlock(clientrnd, 0, clientrnd.Length);
				server_iv = new byte[definition.BulkIVSize];
				Array.Copy(md5.Hash, 0, server_iv, 0, server_iv.Length);
				md5.Clear();
			}
			// generate the cipher objects
			if (entity == ConnectionEnd.Client) {
				ret.Encryptor = bulk.CreateEncryptor(client_key, client_iv);
				ret.Decryptor = bulk.CreateDecryptor(server_key, server_iv);
				ret.LocalHasher = new Ssl3RecordMAC(definition.HashAlgorithmType, client_mac);
				ret.RemoteHasher = new Ssl3RecordMAC(definition.HashAlgorithmType, server_mac);
			} else {
				ret.Encryptor = bulk.CreateEncryptor(server_key, server_iv);
				ret.Decryptor = bulk.CreateDecryptor(client_key, client_iv);
				ret.LocalHasher = new Ssl3RecordMAC(definition.HashAlgorithmType, server_mac);
				ret.RemoteHasher = new Ssl3RecordMAC(definition.HashAlgorithmType, client_mac);
			}
			// clear sensitive data
			Array.Clear(client_mac, 0, client_mac.Length);
			Array.Clear(server_mac, 0, server_mac.Length);
			Array.Clear(client_key, 0, client_key.Length);
			Array.Clear(server_key, 0, server_key.Length);
			Array.Clear(client_iv, 0, client_iv.Length);
			Array.Clear(server_iv, 0, server_iv.Length);
			return ret;
		}
		public static byte[] GenerateMasterSecret(byte[] premaster, byte[] clientRandom, byte[] serverRandom) {
			Ssl3DeriveBytes prf = new Ssl3DeriveBytes(premaster, clientRandom, serverRandom, true);
			byte[] ret = prf.GetBytes(48);
			prf.Dispose();
			return ret;
		}
	}
}