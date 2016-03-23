# Mono.Linux.I2C
A lightweight Mono/.Net/C# Wrapper for interacting with i2c devices on linux.

[![Build Status](https://travis-ci.org/brookpatten/Mono.Linux.I2C.svg?branch=master)](https://travis-ci.org/brookpatten/Mono.Linux.I2C)

**be sure that your user/process has access to the relevant device file(s) either through `sudo` or permissions**  
*hint, if you're running through monodevelop, it probably doesn't*

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
}//note that disposing the bus closes the file
```
