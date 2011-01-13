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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;

namespace Org.Mentalis.Security.Ssl {
	internal class AsyncResult : IAsyncResult {
		internal AsyncResult(AsyncCallback callback, object stateObject, object owner) {
			m_StateObject = stateObject;
			m_Completed = false;
			m_Owner = owner;
			if (callback != null)
				this.Callback += callback;
		}
		// Thanks go out to John Doty for notifying us about a bug in this method
		public void Notify(Exception e) {
			if (!m_Completed) {
				m_AsyncException = e;
				m_Completed = true;
				if (Callback != null) {
					if (m_Owner != null) // exit the synchronization lock, if necessary
						Monitor.Exit(m_Owner);
					try {
						Callback(this);
					} finally {
						if (m_Owner != null) // acquire the synchronization lock, if necessary
							Monitor.Enter(m_Owner);
					}
				}
				if (m_WaitHandle != null)
					m_WaitHandle.Set();
			}
		}
		public void Notify() {
			Notify(this.AsyncException);
		}
		public Exception AsyncException {
			get {
				return m_AsyncException;
			}
			set {
				m_AsyncException = value;
			}
		}
		public bool IsCompleted {
			get {
				return m_Completed;
			}
		}
		public bool CompletedSynchronously {
			get {
				return false;
			}
		}
		public object AsyncState {
			get {
				return m_StateObject;
			}
		}
		// Thanks go out to Kevin Knoop for notifying us about a bug in this method
		public WaitHandle AsyncWaitHandle {
			get {
				if (m_WaitHandle == null)
					m_WaitHandle = new ManualResetEvent(m_Completed);
				if (m_Completed)
					m_WaitHandle.Set();
				return m_WaitHandle;
			}
		}
		private bool m_Completed;
		private object m_StateObject;
		private object m_Owner;
		private ManualResetEvent m_WaitHandle;
		private Exception m_AsyncException = null;
		public event AsyncCallback Callback;
	}
	internal class AsyncAcceptResult : AsyncResult {
		internal AsyncAcceptResult(AsyncCallback callback, object stateObject, object owner) : base(callback, stateObject, owner) {}
		public SecureSocket AcceptedSocket {
			get {
				return m_AcceptedSocket;
			}
			set {
				m_AcceptedSocket = value;
			}
		}
		private SecureSocket m_AcceptedSocket;
	}
}