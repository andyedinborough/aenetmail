using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace AE.Net.Mail.Imap
{
    public class Namespace
    {
        #region Constructors

        public Namespace(string prefix, string delimiter)
        {
            Prefix = prefix;
            Delimiter = delimiter;
        }

        public Namespace()
        {
        }

        #endregion

        #region Properties

        public virtual string Delimiter { get; internal set; }

        public virtual string Prefix { get; internal set; }

        #endregion
    }

    public class Namespaces
    {
        #region Fields

        private Collection<Namespace> _servernamespace = new Collection<Namespace>();
        private Collection<Namespace> _sharednamespace = new Collection<Namespace>();
        private Collection<Namespace> _usernamespace = new Collection<Namespace>();

        #endregion

        #region Properties

        public virtual Collection<Namespace> ServerNamespace
        {
            get { return this._servernamespace; }
        }

        public virtual Collection<Namespace> SharedNamespace
        {
            get { return this._sharednamespace; }
        }

        public virtual Collection<Namespace> UserNamespace
        {
            get { return this._usernamespace; }
        }

        #endregion
    }
}