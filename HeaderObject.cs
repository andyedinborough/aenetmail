
namespace AE.Net.Mail {
    public abstract class ObjectWHeaders {
        public string RawHeaders { get; internal set; }
        private HeaderCollection _Headers;
        public HeaderCollection Headers {
            get {
                return _Headers ?? (_Headers = HeaderCollection.Parse(RawHeaders));
            }
        }
    }
}
