# Mono.Linux.I2C
A lightweight Mono/.Net/C# Wrapper over i2c for linux


```C#
using (var i2cBus = new I2CBus(0x01))
{
  var i2cDevice = new I2CDevice(i2cBus, 0x3c);
  var bytes = new byte[2];
  i2cDevice.ReadBytes(0x00, 2, bytes);
}
```
