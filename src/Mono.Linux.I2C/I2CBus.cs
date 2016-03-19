using System;
using System.IO;
using System.Runtime.InteropServices;

using Mono.Unix.Native;
using Mono.Unix;

namespace Mono.Linux.I2C
{
    public unsafe class I2CBus:IDisposable
    {
        public const string DefaultDeviceFileFormat = "/dev/i2c-{0}";
        public const int I2C_SLAVE = 0x0703;

        private string _device;
        private int _fd = -1;

        //TODO: change to safehandles
		[DllImport("libc.so.6",SetLastError=true)]
        extern private static int ioctl(int fd, int request, byte x);

		public I2CBus(int index)
		{
			_device = string.Format(DefaultDeviceFileFormat, index);
		}

        public I2CBus(int index,string deviceFileFormat)
        {
            _device = string.Format(deviceFileFormat, index);
        }
        
        public I2CBus(string deviceFile)
        {
            _device = deviceFile;
        }

		public bool IsOpen 
		{
			get 
			{
				return _fd != -1;
			}
		}

		private void OpenIfNotOpen ()
		{
			if (!IsOpen) 
			{
				Open ();
			}
		}

        public override string ToString()
        {
            return string.Format("I2C Bus {0} (File Descriptor {1})", _device, _fd);
        }

        protected void Open()
        {
			_fd = Syscall.open(_device, OpenFlags.O_RDWR);
			if (_fd < 0)
			{
				CheckAndThrowUnixIOException();
			}
        }

        protected void ChangeDevice(byte devAddr)
        {
			OpenIfNotOpen ();

            int ret = ioctl(_fd, I2C_SLAVE, devAddr);
			if (ret < 0)
			{
				CheckAndThrowUnixIOException();
			}
        }

        public byte ReadBytes(byte devAddr, byte regAddr, byte length, byte[] data, int offset=0, ushort timeout = 0)
        {
            if (length > 127)
                throw new IOException(_device + ": length > 127");

            ChangeDevice(devAddr);

            //fixed(byte* p = &regAddr)
            {
                int ret = (int)Syscall.write(_fd, &regAddr, 1);
				if (ret != 1)
				{
					CheckAndThrowUnixIOException();
				}
            }

            int count;
            fixed (byte* p = &data[offset])
            {
                count = (int)Syscall.read(_fd, p, (ulong)length);
				if (count < 0)
				{
					CheckAndThrowUnixIOException();
				}
				else if (count != length)
				{
					throw new IOException(_device + ": read short: length = " + length + " > " + count);
				}
            }

            return (byte)count;
        }

        /** Write multiple bytes to an 8-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr First register address to write to
        * @param length Number of bytes to write
        * @param data Buffer to copy new data from
        * @return Status of operation (true = success)
        */
        public void WriteBytes(byte devAddr, byte regAddr, byte length, byte[] data)
        {
			if (length > 127)
			{
				throw new IOException(_device + ": length > 127");
			}

            ChangeDevice(devAddr);

            byte[] buffer = new byte[128];
            buffer[0] = regAddr;
            Array.Copy(data, 0, buffer, 1, length);

            int count;
            fixed (byte* p = buffer)
            {
                count = (int)Syscall.write(_fd, p, (ulong)(length + 1));
            }

            if (count < 0)
            {
				CheckAndThrowUnixIOException();
            }
            else if (count != length + 1)
            {
                throw new IOException(_device + ": write short = " + count);
            }
        }

        public void WriteWords(byte devAddr, byte regAddr, byte length, ushort[] data)
        {
            int count = 0;
            byte[] buf = new byte[128];
            int i;

            // Should do potential byteswap and call writeBytes() really, but that
            // messes with the callers buffer

            if (length > 63)
            {
                throw new IOException(_device + ": length > 63");
            }

            ChangeDevice(devAddr);

            buf[0] = regAddr;
            for (i = 0; i < (int)length; i++)
            {
                buf[i * 2 + 1] = (byte)(data[i] >> 8);
                buf[i * 2 + 2] = (byte)data[i];
            }
            fixed (byte* p = buf)
            {
                count = (int)Syscall.write(_fd, p, (ulong)(length * 2 + 1));
            }
            if (count < 0)
            {
				CheckAndThrowUnixIOException();
            }
            else if (count != length * 2 + 1)
            {
                throw new IOException(_device + ": write short");
            }
        }

        public void Close()
        {
			if(IsOpen)
	        {
				int ret = Syscall.close(_fd);
				if (ret != 0)
				{
					CheckAndThrowUnixIOException();
				}
				_fd=-1;
	        }
        }
        
        public void Dispose()
        {
          Close();
        }

		private void CheckAndThrowUnixIOException()
		{
			var error = Marshal.GetLastWin32Error();
			throw new UnixIOException(error);
		}
    }
}
