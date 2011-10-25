AE.Net.Mail
-----------
A C# POP/IMAP client library

Background
----------
These are text-based services... it’s not that hard, and yet all the projects I found out there were nasty—bloated and severely error prone. So, I rebuilt one. This is based heavily on xemail-net. I simplified it quite a bit—created standard methods for repeated code blocks and implemented a base class to simplify the creation of the Pop3 client.

*The SecureSocket library from Mentalis (http://www.mentalis.org/soft/projects/ssocket/) is embed in this project for SSL support.*