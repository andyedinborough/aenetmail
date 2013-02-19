using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Portable.Utils.Security
{
    public class SslStream
    {
        public SslStream(Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback)
        {
            throw new NotImplementedException();
        }

        public SslStream(Stream innerStream, bool Open)
        {
            throw new NotImplementedException();
        }

        public void AuthenticateAsClient(string targetHost)
        {
            throw new NotImplementedException();
        }
    }
}
