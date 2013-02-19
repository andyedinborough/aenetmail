using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Portable.Utils.Security
{
    [Flags]
    public enum SslPolicyErrors
    {
        None = 0,
        RemoteCertificateChainErrors = 4,
        RemoteCertificateNameMismatch = 2,
        RemoteCertificateNotAvailable = 1
    }

    public delegate bool RemoteCertificateValidationCallback(object sender, SslPolicyErrors sslPolicyErrors);
}
