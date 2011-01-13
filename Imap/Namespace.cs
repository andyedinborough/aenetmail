using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace AE.Net.Mail.Imap {
    public class Namespaces {
        private Collection<Namespace> _servernamespace = new Collection<Namespace>();
        private Collection<Namespace> _usernamespace = new Collection<Namespace>();
        private Collection<Namespace> _sharednamespace = new Collection<Namespace>();
 
        public Collection<Namespace> ServerNamespace {
            get { return this._servernamespace; }
        }
        public Collection<Namespace> UserNamespace {
            get { return this._usernamespace; }
        }
        public Collection<Namespace> SharedNamespace {
            get { return this._sharednamespace; }
        }
    }

    public class Namespace {
        public Namespace(string prefix, string delimiter) {
            Prefix = prefix;
            Delimiter = delimiter;
        }
        public Namespace() { }
        public string Prefix { get; internal set; }
        public string Delimiter { get; internal set; }
    }
}