using System;
using System.Security.Cryptography;
using System.Text;

namespace AE.Net.Mail
{
    public sealed class HMACMD5 : IDisposable
    {
        #region Fields

        private const int BLOCK_SIZE = 64;

        private byte[] _inner = null;
        private byte[] _Key = null;
        private byte[] _outer = null;

        private bool disposedValue = false;
        private MD5 MD5 = MD5.Create();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HMACMD5"/> class using the supplied key with UT8 encoding.
        /// </summary>
        /// <param name="key">The key.</param>
        public HMACMD5(string key)
            : this(key, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HMACMD5"/> class using the supplied key with supplied encoding.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="encoding">The encoding used to read the key.</param>
        public HMACMD5(string key, Encoding encoding)
            : this(encoding.GetBytes(key))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HMACMD5"/> class the supplied key.
        /// </summary>
        /// <param name="key">The key.</param>
        public HMACMD5(byte[] key)
        {
            InitializeKey(key);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public byte[] Key
        {
            get
            {
                return _Key;
            }
            set
            {
                InitializeKey(value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Computes the hash value for the specified string (UTF8 default encoding).
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for. </param>
        /// <returns>The computed hash code</returns>
        public byte[] ComputeHash(string buffer)
        {
            return ComputeHash(buffer, Encoding.UTF8);
        }

        /// <summary>
        /// Computes the hash value for the specified string.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>
        /// The computed hash code
        /// </returns>
        public byte[] ComputeHash(string buffer, Encoding encoding)
        {
            return ComputeHash(encoding.GetBytes(buffer));
        }

        /// <summary>
        /// Computes the hash value for the specified byte array.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <returns>
        /// The computed hash code
        /// </returns>
        public byte[] ComputeHash(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "The input cannot be null.");
            }

            return MD5.ComputeHash(Combine(_outer, MD5.ComputeHash(Combine(_inner, buffer))));
        }

        /// <summary>
        /// Computes the hash for the specified string (UTF8 default encoding) to base64 string.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <returns>The computed hash code in base64 string</returns>
        public string ComputeHashToBase64String(string buffer)
        {
            return Convert.ToBase64String(ComputeHash(buffer, Encoding.UTF8));
        }

        /// <summary>
        /// Computes the hash for the specified string to base64 string.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>
        /// The computed hash code in base64 string
        /// </returns>
        public string ComputeHashToBase64String(string buffer, Encoding encoding)
        {
            return Convert.ToBase64String(ComputeHash(buffer, encoding));
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Combines two array (a1 and a2).
        /// </summary>
        /// <param name="a1">The Array 1.</param>
        /// <param name="a2">The Array 2.</param>
        /// <returns>Combinaison of a1 and a2</returns>
        private byte[] Combine(byte[] a1, byte[] a2)
        {
            byte[] final = new byte[a1.Length + a2.Length];
            for (int i = 0; i < a1.Length; i++)
            {
                final[i] = a1[i];
            }

            for (int i = 0; i < a2.Length; i++)
            {
                final[i + a1.Length] = a2[i];
            }

            return final;
        }

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (MD5 != null)
                    {
                        MD5.Dispose();
                    }
                    MD5 = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        /// <summary>
        /// Initializes the key.
        /// </summary>
        /// <param name="key">The key.</param>
        private void InitializeKey(byte[] key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "The Key cannot be null.");
            }

            if (key.Length > BLOCK_SIZE)
            {
                _Key = MD5.ComputeHash(key);
            }
            else
            {
                _Key = key;
            }

            UpdateIOPadBuffers();
        }

        /// <summary>
        /// Updates the IO pad buffers.
        /// </summary>
        private void UpdateIOPadBuffers()
        {
            if (_inner == null)
            {
                _inner = new byte[BLOCK_SIZE];
            }

            if (_outer == null)
            {
                _outer = new byte[BLOCK_SIZE];
            }

            for (int i = 0; i < BLOCK_SIZE; i++)
            {
                _inner[i] = 54;
                _outer[i] = 92;
            }

            for (int i = 0; i < Key.Length; i++)
            {
                byte[] s1 = _inner;
                int s2 = i;
                s1[s2] ^= Key[i];
                byte[] s3 = _outer;
                int s4 = i;
                s3[s4] ^= Key[i];
            }
        }

        #endregion

        // To detect redundant calls
        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~HMACMD5() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }
    }
}
