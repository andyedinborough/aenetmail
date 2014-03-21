using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AE.Net.Mail {
	public class ImapClientExceptionEventArgs : System.EventArgs {
		public ImapClientExceptionEventArgs(System.Exception Exception) {
			this.Exception = Exception;
		}

		public System.Exception Exception { get; set; }
	}
}
