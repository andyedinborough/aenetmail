AE.Net.Mail
-----------
A C# POP/IMAP client library

Sample Usage
------------
     using(var imap = new AE.Net.Mail.ImapClient(host, username, password, AE.Net.Mail.ImapClient.AuthMethods.Login, port, isSSL)) {
          var uids = imap.Search(
              SearchCondition.Undeleted().And(
                SearchCondition.From("david"), 
                SearchCondition.SentSince(new DateTime(2000, 1, 1))
              ).Or(SearchCondition.To("andy"))
          );
          
          var msg = imap.GetMessage(uids[0]);
     }

Background
----------
These are text-based services... it's not that hard, and yet all the projects I found out there were nasty—bloated and severely error prone. So, I rebuilt one. This is based heavily on xemail-net. I simplified it quite a bit—created standard methods for repeated code blocks and implemented a base class to simplify the creation of the Pop3 client.

*The SecureSocket library from Mentalis (http://www.mentalis.org/soft/projects/ssocket/) is embed in this project for SSL support.*