using System;

namespace AE.Net.Mail {
	public class WarningEventArgs : EventArgs {
		public string Message { get; set; }
		public MailMessage MailMessage { get; set; }
	}
}
