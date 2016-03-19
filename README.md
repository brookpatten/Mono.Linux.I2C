# Mono.Linux.I2C
A lightweight Mono/.Net/C# Wrapper for interacting with i2c devices on linux.

```C#
//open i2c bus 1, aka /dev/i2c-1
using (var i2cBus = new I2CBus(0x01))
{
  //create a device at address 0x3c
  var i2cDevice = new I2CDevice(i2cBus, 0x3c);
  var bytes = new byte[2];
  //read 2 bytes starting at register 0x00
  i2cDevice.ReadBytes(0x00, 2, bytes);
  //do someting useful with the bytes
}
```
