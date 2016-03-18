using System;
using System.Linq;

using Mono.Linux.I2C;

namespace Mono.Linux.I2C.Console
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			System.Console.WriteLine("Usage: i2c bus device register [end register]");

			byte bus;
			byte device;
			byte startRegister;
			byte? endRegister=null;

			if (!(args.Length >= 1 && byte.TryParse(args[0], out bus)))
			{
				System.Console.WriteLine("Missing bus argument");
				return;
			}
			if (!(args.Length >= 2 && byte.TryParse(args[1], out device)))
			{
				System.Console.WriteLine("Missing device argument");
				return;
			}
			if (!(args.Length >= 3 && byte.TryParse(args[2], out startRegister)))
			{
				System.Console.WriteLine("Missing register argument");
				return;
			}
			byte end;
			if (args.Length >= 4 && byte.TryParse(args[3], out end))
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

			var i2cBus = new I2CBus(bus);
			var i2cDevice = new I2CDevice(i2cBus, device);

			byte[] bytes;
			if (endRegister.HasValue)
			{
				bytes = new byte[endRegister.Value - startRegister];
				var read = i2cDevice.ReadBytes(startRegister, (byte)(endRegister.Value - startRegister), bytes);
			}
			else
			{
				bytes = new byte[1];
				bytes[0] = i2cDevice.ReadByte(startRegister);
			}

			for (int b = startRegister; b == startRegister || (endRegister.HasValue && b <= endRegister.Value);b++)
			{
				System.Console.WriteLine("{0:X}:{1:X}", b, bytes[b - startRegister]);
			}
		}


	}
}
