namespace MxUtilities.FileHandling.Models
{
    public class MaxStream
    {
        private byte[] _buffer;
        private long _position;

        public string StreamName { get; }

        public MaxStream(int initialCapacity)
        {
            _buffer = new byte[initialCapacity];
            _position = 0;
        }

        public MaxStream(byte[] buffer, string streamName)
        {
            _buffer = buffer;
            _position = 0;
            StreamName = streamName;
        }

        public byte[] GetBuffer() => _buffer;

        public long Size => _buffer.Length;
        public long Position => _position;

        public void Write(byte[] data)
        {
            if (_position + data.Length > _buffer.Length)
            {
                Array.Resize(ref _buffer, _buffer.Length * 2);
            }

            Array.Copy(data, 0, _buffer, _position, data.Length);
            _position += data.Length;
        }

        public void Trim()
        {
            Array.Resize(ref _buffer, (int)_position);
        }

        public int Read(byte[] destination, int count)
        {
            if (_position + count > _buffer.Length)
            {
                throw new EndOfStreamException("Attempt to read beyond the buffer length.");
            }

            Array.Copy(_buffer, _position, destination, 0, count);
            _position += count;

            return count;
        }
    }
}
