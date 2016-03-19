using System;
using System.Linq;

using Mono.Linux.I2C;

namespace Mono.Linux.I2C.Console
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			System.Console.WriteLine("Usage: i2c bus device register [end-register]");
			System.Console.WriteLine("Example: i2c 0x01 0x2c 0x90 0x97");

			byte bus;
			byte device;
			byte startRegister;
			byte? endRegister=null;

			if (!(args.Length >= 1 && TryParseByte(args[0], out bus)))
			{
				System.Console.WriteLine("Missing bus argument");
				return;
			}
			if (!(args.Length >= 2 && TryParseByte(args[1], out device)))
			{
				System.Console.WriteLine("Missing device argument");
				return;
			}
			if (!(args.Length >= 3 && TryParseByte(args[2], out startRegister)))
			{
				System.Console.WriteLine("Missing register argument");
				return;
			}
			byte end;
			if (args.Length >= 4 && TryParseByte(args[3], out end))
			{
				if (end > startRegister)
				{
					endRegister = end;
				}
				else
				{
					System.Console.WriteLine("End register must be after the start register");
					return;
				}
			}

			byte[] bytes;
			using (var i2cBus = new I2CBus(bus))
			{
				var i2cDevice = new I2CDevice(i2cBus, device);

				if (endRegister.HasValue)
				{
					bytes = new byte[endRegister.Value - startRegister + 1];
					var read = i2cDevice.ReadBytes(startRegister, (byte)(endRegister.Value - startRegister + 1), bytes);
				}
				else
				{
					bytes = new byte[1];
					bytes[0] = i2cDevice.ReadByte(startRegister);
				}
			}

			for (int b = startRegister; b == startRegister || (endRegister.HasValue && b <= endRegister.Value);b++)
			{
				System.Console.WriteLine("0x{0:XX}:0x{1:XX}", b, bytes[b - startRegister]);
			}
		}

		public static byte[] HexToByteArray(string hex) 
		{
			if (hex.StartsWith("0x"))
			{
				hex = hex.Substring(2);
			}
			hex = hex.ToLower();
			return Enumerable.Range(0, hex.Length)
				     .Where(x => x % 2 == 0)
				     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
				     .ToArray();
		}

		public static byte HexToByte(string hex)
		{
			var bytes = HexToByteArray(hex);
			if (bytes.Length == 1)
			{
				return bytes.Single();
			}
			else
			{
				throw new FormatException("input string was more than one byte");
			}
		}

		public static bool TryParseByte(string hex, out byte b)
		{
			try
			{
				b = HexToByte(hex);
				return true;
			}
			catch
			{
				b = 0;
				return false;
			}
		}
	}
}
