using System;
using System.Collections.Generic;
using System.IO;

namespace AE.Net.Mail {
  //http://stackoverflow.com/questions/842465/reading-a-line-from-a-streamreader-without-consuming
  public class PeekableTextReader : TextReader {
    private TextReader _Underlying;
    private Queue<string> _BufferedLines;

    public PeekableTextReader(TextReader underlying) {
      _Underlying = underlying;
      _BufferedLines = new Queue<string>();
    }

    public string PeekLine() {
      string line = _Underlying.ReadLine();
      if (line == null)
        return null;
      _BufferedLines.Enqueue(line);
      return line;
    }

    public override string ReadLine() {
      if (_BufferedLines.Count > 0)
        return _BufferedLines.Dequeue();
      return _Underlying.ReadLine();
    }

    public override int Peek() {
      return _Underlying.Peek();
    }

    public override void Close() {
      _Underlying.Close();
    }

    public override System.Runtime.Remoting.ObjRef CreateObjRef(Type requestedType) {
      return _Underlying.CreateObjRef(requestedType);
    }

    protected override void Dispose(bool disposing) {
      if (disposing) _Underlying.Dispose();
    }

    public override bool Equals(object obj) {
      return _Underlying.Equals(obj);
    }

    public override int GetHashCode() {
      return _Underlying.GetHashCode();
    }

    public override object InitializeLifetimeService() {
      return _Underlying.InitializeLifetimeService();
    }

    public override int Read() {
      return _Underlying.Read();
    }

    public override int Read(char[] buffer, int index, int count) {
      return _Underlying.Read(buffer, index, count);
    }

    public override int ReadBlock(char[] buffer, int index, int count) {
      return _Underlying.ReadBlock(buffer, index, count);
    }

    public override string ReadToEnd() {
      return _Underlying.ReadToEnd();
    }
  }
}
