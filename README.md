#AE.Net.Mail
A C# POP/IMAP client library

##Background
These are text-based services... it's not that hard, and yet all the projects I
found out there were nasty, bloated, and severely error prone. So, I rebuilt 
one. This is based heavily on xemail-net. I simplified it quite a bit, and 
created standard methods for repeated code blocks and implemented a base class
to simplify the creation of the Pop3 client.

##Sample Usage

###IMAP
     using(var imap = new AE.Net.Mail.ImapClient(host, username, password, AE.Net.Mail.ImapClient.AuthMethods.Login, port, isSSL)) {
        var msgs = imap.SearchMessages(
          SearchCondition.Undeleted().And(
            SearchCondition.From("david"), 
            SearchCondition.SentSince(new DateTime(2000, 1, 1))
          ).Or(SearchCondition.To("andy"))
        );
          
        Assert.AreEqual(msgs[0].Value.Subject, "This is cool!");

        imap.NewMessage += (sender, e) => {
          var msg = imap.GetMessage(e.MessageCount - 1);
          Assert.AreEqual(msg.Subject, "IDLE support?  Yes, please!");
        };
    }

###POP
     using(var pop = new AE.Net.Mail.Pop3Client(host, username, password, port, isSSL)) {
       for(var i = pop.GetMessageCount() - 1; i >= 0; i--){
          var msg = pop.GetMessage(i, false);
          Assert.AreEqual(msg.Subject, "Standard API between different protocols?  Yes, please!");
          pop.DeleteMessage(i); //WE DON'T NEED NO STINKIN' EMAIL!
       }
    }

###License
http://andyedinborough.mit-license.org/