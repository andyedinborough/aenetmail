
namespace AE.Net.Mail.Imap {
    public class Mailbox {
        public Mailbox() : this(string.Empty) { }
        public Mailbox(string name) {
            Name = name;
            Flags = new string[0];
        }
        public string Name { get; internal set; }
        public int NumNewMsg { get; internal set; }
        public int NumMsg { get; internal set; }
        public int NumUnSeen { get; internal set; }
        public string[] Flags { get; internal set; }
        public bool IsWritable { get; internal set; }

        internal void SetFlags(string flags) {
            Flags = flags.Split(' ');
        }
    }
}

