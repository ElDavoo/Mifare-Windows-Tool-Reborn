using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCT_Windows
{
    internal delegate void UserCallBack(string data);
    public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);

    public class FixedProcess : Process
    {
        internal AsyncStreamReader output;
        internal AsyncStreamReader error;
        public event DataReceivedEventHandler OutputDataReceived;
        public event DataReceivedEventHandler ErrorDataReceived;

        public new void BeginOutputReadLine()
        {
            Stream baseStream = StandardOutput.BaseStream;
            this.output = new AsyncStreamReader(this, baseStream, new UserCallBack(this.FixedOutputReadNotifyUser), StandardOutput.CurrentEncoding);
            this.output.BeginReadLine();
        }

        public void BeginErrorReadLine()
        {
            Stream baseStream = StandardError.BaseStream;
            this.error = new AsyncStreamReader(this, baseStream, new UserCallBack(this.FixedErrorReadNotifyUser), StandardError.CurrentEncoding);
            this.error.BeginReadLine();
        }

        internal void FixedOutputReadNotifyUser(string data)
        {
            DataReceivedEventHandler outputDataReceived = this.OutputDataReceived;
            if (outputDataReceived != null)
            {
                DataReceivedEventArgs dataReceivedEventArgs = new DataReceivedEventArgs(data);
                if (this.SynchronizingObject != null && this.SynchronizingObject.InvokeRequired)
                {
                    this.SynchronizingObject.Invoke(outputDataReceived, new object[]
                    {
                        this,
                        dataReceivedEventArgs
                    });
                    return;
                }
                outputDataReceived(this, dataReceivedEventArgs);
            }
        }

        internal void FixedErrorReadNotifyUser(string data)
        {
            DataReceivedEventHandler errorDataReceived = this.ErrorDataReceived;
            if (errorDataReceived != null)
            {
                DataReceivedEventArgs dataReceivedEventArgs = new DataReceivedEventArgs(data);
                if (this.SynchronizingObject != null && this.SynchronizingObject.InvokeRequired)
                {
                    this.SynchronizingObject.Invoke(errorDataReceived, new object[]
                    {
                        this,
                        dataReceivedEventArgs
                    });
                    return;
                }
                errorDataReceived(this, dataReceivedEventArgs);
            }
        }
    }

    internal class AsyncStreamReader : IDisposable
    {
        internal const int DefaultBufferSize = 1024;
        private const int MinBufferSize = 128;
        private Stream stream;
        private Encoding encoding;
        private Decoder decoder;
        private byte[] byteBuffer;
        private char[] charBuffer;
        private int _maxCharsPerBuffer;
        private Process process;
        private UserCallBack userCallBack;
        private bool cancelOperation;
        private ManualResetEvent eofEvent;
        private Queue messageQueue;
        private StringBuilder sb;
        private bool bLastCarriageReturn;
        public virtual Encoding CurrentEncoding
        {
            get
            {
                return this.encoding;
            }
        }
        public virtual Stream BaseStream
        {
            get
            {
                return this.stream;
            }
        }
        internal AsyncStreamReader(Process process, Stream stream, UserCallBack callback, Encoding encoding)
            : this(process, stream, callback, encoding, 1024)
        {
        }
        internal AsyncStreamReader(Process process, Stream stream, UserCallBack callback, Encoding encoding, int bufferSize)
        {
            this.Init(process, stream, callback, encoding, bufferSize);
            this.messageQueue = new Queue();
        }
        private void Init(Process process, Stream stream, UserCallBack callback, Encoding encoding, int bufferSize)
        {
            this.process = process;
            this.stream = stream;
            this.encoding = encoding;
            this.userCallBack = callback;
            this.decoder = encoding.GetDecoder();
            if (bufferSize < 128)
            {
                bufferSize = 128;
            }
            this.byteBuffer = new byte[bufferSize];
            this._maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
            this.charBuffer = new char[this._maxCharsPerBuffer];
            this.cancelOperation = false;
            this.eofEvent = new ManualResetEvent(false);
            this.sb = null;
            this.bLastCarriageReturn = false;
        }
        public virtual void Close()
        {
            this.Dispose(true);
        }
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.stream != null)
            {
                this.stream.Close();
            }
            if (this.stream != null)
            {
                this.stream = null;
                this.encoding = null;
                this.decoder = null;
                this.byteBuffer = null;
                this.charBuffer = null;
            }
            if (this.eofEvent != null)
            {
                this.eofEvent.Close();
                this.eofEvent = null;
            }
        }
        internal void BeginReadLine()
        {
            if (this.cancelOperation)
            {
                this.cancelOperation = false;
            }
            if (this.sb == null)
            {
                this.sb = new StringBuilder(1024);
                this.stream.BeginRead(this.byteBuffer, 0, this.byteBuffer.Length, new AsyncCallback(this.ReadBuffer), null);
                return;
            }
            this.FlushMessageQueue();
        }
        internal void CancelOperation()
        {
            this.cancelOperation = true;
        }
        private void ReadBuffer(IAsyncResult ar)
        {
            int num;
            try
            {
                num = this.stream.EndRead(ar);
            }
            catch (IOException)
            {
                num = 0;
            }
            catch (OperationCanceledException)
            {
                num = 0;
            }
            if (num == 0)
            {
                lock (this.messageQueue)
                {
                    if (this.sb.Length != 0)
                    {
                        this.messageQueue.Enqueue(this.sb.ToString());
                        this.sb.Length = 0;
                    }
                    this.messageQueue.Enqueue(null);
                }
                try
                {
                    this.FlushMessageQueue();
                    return;
                }
                finally
                {
                    this.eofEvent.Set();
                }
            }
            int chars = this.decoder.GetChars(this.byteBuffer, 0, num, this.charBuffer, 0);
            this.sb.Append(this.charBuffer, 0, chars);
            this.GetLinesFromStringBuilder();
            this.stream.BeginRead(this.byteBuffer, 0, this.byteBuffer.Length, new AsyncCallback(this.ReadBuffer), null);
        }
        private void GetLinesFromStringBuilder()
        {
            int i = 0;
            int num = 0;
            int length = this.sb.Length;
            if (this.bLastCarriageReturn && length > 0 && this.sb[0] == '\n')
            {
                i = 1;
                num = 1;
                this.bLastCarriageReturn = false;
            }
            while (i < length)
            {
                char c = this.sb[i];
                if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && i + 1 < length && this.sb[i + 1] == '\n')
                    {
                        i++;
                    }

                    string obj = this.sb.ToString(num, i + 1 - num);

                    num = i + 1;

                    lock (this.messageQueue)
                    {
                        this.messageQueue.Enqueue(obj);
                    }
                }
                i++;
            }

            // Flush Fix: Send Whatever is left in the buffer
            string endOfBuffer = this.sb.ToString(num, length - num);
            lock (this.messageQueue)
            {
                this.messageQueue.Enqueue(endOfBuffer);
                num = length;
            }
            // End Flush Fix

            if (this.sb[length - 1] == '\r')
            {
                this.bLastCarriageReturn = true;
            }
            if (num < length)
            {
                this.sb.Remove(0, num);
            }
            else
            {
                this.sb.Length = 0;
            }
            this.FlushMessageQueue();
        }
        private void FlushMessageQueue()
        {
            while (this.messageQueue.Count > 0)
            {
                lock (this.messageQueue)
                {
                    if (this.messageQueue.Count > 0)
                    {
                        string data = (string)this.messageQueue.Dequeue();
                        if (!this.cancelOperation)
                        {
                            this.userCallBack(data);
                        }
                    }
                    continue;
                }
                break;
            }
        }
        internal void WaitUtilEOF()
        {
            if (this.eofEvent != null)
            {
                this.eofEvent.WaitOne();
                this.eofEvent.Close();
                this.eofEvent = null;
            }
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        internal string _data;
        /// <summary>Gets the line of characters that was written to a redirected <see cref="T:System.Diagnostics.Process" /> output stream.</summary>
        /// <returns>The line that was written by an associated <see cref="T:System.Diagnostics.Process" /> to its redirected <see cref="P:System.Diagnostics.Process.StandardOutput" /> or <see cref="P:System.Diagnostics.Process.StandardError" /> stream.</returns>
        /// <filterpriority>2</filterpriority>
        public string Data
        {
            get
            {
                return this._data;
            }
        }
        internal DataReceivedEventArgs(string data)
        {
            this._data = data;
        }
    }
}

