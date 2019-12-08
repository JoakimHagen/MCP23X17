using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
  public class MockHardware
  {
    public byte[] Registers { get; } = new byte[22];

    public byte ReadRegister(int address)
    {
      return Registers[address];
    }

    public void WriteRegister(int address, byte values)
    {
      Registers[address] = values;
    }
  }
}
