using Xunit;
using MCP23X17;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
  public class PortTests
  {
    const int IODIRA_ADR   = 0x00;
    const int IPOLA_ADR    = 0x02;
    const int GPINTENA_ADR = 0x04;
    const int DEFVALA_ADR  = 0x06;
    const int INTCONA_ADR  = 0x08;
    const int IOCONA_ADR   = 0x0A;
    const int GPPUA_ADR    = 0x0C;
    const int INTFA_ADR    = 0x0E;
    const int INTCAPA_ADR  = 0x10;
    const int GPIOA_ADR    = 0x12;
    const int OLATA_ADR    = 0x14;

    const int IODIRB_ADR   = 0x01;
    const int IPOLB_ADR    = 0x03;
    const int GPINTENB_ADR = 0x05;
    const int DEFVALB_ADR  = 0x07;
    const int INTCONB_ADR  = 0x09;
    const int IOCONB_ADR   = 0x0B;
    const int GPPUB_ADR    = 0x0D;
    const int INTFB_ADR    = 0x0F;
    const int INTCAPB_ADR  = 0x11;
    const int GPIOB_ADR    = 0x13;
    const int OLATB_ADR    = 0x15;

    const bool LOW  = false;
    const bool HIGH = true;

    private MockHardware hw;

    public PortTests()
    {
      hw = new MockHardware();
    }

    public static IEnumerable<object[]> RegisterArgs()
    {
      yield return new object[] { Port.Side.A, McpReg.IODIR, IODIRA_ADR };
      yield return new object[] { Port.Side.A, McpReg.IPOL, IPOLA_ADR };
      yield return new object[] { Port.Side.A, McpReg.GPINTEN, GPINTENA_ADR };
      yield return new object[] { Port.Side.A, McpReg.DEFVAL, DEFVALA_ADR };
      yield return new object[] { Port.Side.A, McpReg.INTCON, INTCONA_ADR };
      yield return new object[] { Port.Side.A, McpReg.GPPU, GPPUA_ADR };
      yield return new object[] { Port.Side.A, McpReg.INTF, INTFA_ADR };
      yield return new object[] { Port.Side.A, McpReg.INTCAP, INTCAPA_ADR };
      yield return new object[] { Port.Side.A, McpReg.OLAT, OLATA_ADR };

      yield return new object[] { Port.Side.B, McpReg.IODIR, IODIRB_ADR };
      yield return new object[] { Port.Side.B, McpReg.IPOL, IPOLB_ADR };
      yield return new object[] { Port.Side.B, McpReg.GPINTEN, GPINTENB_ADR };
      yield return new object[] { Port.Side.B, McpReg.DEFVAL, DEFVALB_ADR };
      yield return new object[] { Port.Side.B, McpReg.INTCON, INTCONB_ADR };
      yield return new object[] { Port.Side.B, McpReg.GPPU, GPPUB_ADR };
      yield return new object[] { Port.Side.B, McpReg.INTF, INTFB_ADR };
      yield return new object[] { Port.Side.B, McpReg.INTCAP, INTCAPB_ADR };
      yield return new object[] { Port.Side.B, McpReg.OLAT, OLATB_ADR };
    }

    [Theory]
    [MemberData(nameof(RegisterArgs))]
    [InlineData(Port.Side.A, McpReg.GPIO, GPIOA_ADR)]
    [InlineData(Port.Side.B, McpReg.GPIO, GPIOB_ADR)]
    public void ReadsFromAddress(Port.Side side, McpReg register, int address)
    {
      var port = new Port(hw.ReadRegister, hw.WriteRegister, side);
      hw.Registers[address] = 200;
      var read = port.Read(register);
      Assert.Equal(200, read);
    }

    [Theory]
    [MemberData(nameof(RegisterArgs))]
    [InlineData(Port.Side.A, McpReg.GPIO, OLATA_ADR)]
    [InlineData(Port.Side.B, McpReg.GPIO, OLATB_ADR)]
    public void WritesToAddress(Port.Side side, McpReg register, int address)
    {
      var port = new Port(hw.ReadRegister, hw.WriteRegister, side);
      port.Write(register, 200);
      Assert.Equal(200, hw.Registers[address]);
    }

    [Theory]
    [MemberData(nameof(RegisterArgs))]
    [InlineData(Port.Side.A, McpReg.GPIO, 0)]
    [InlineData(Port.Side.B, McpReg.GPIO, 0)]
    public void WriteIsIsolated(Port.Side side, McpReg register, int address)
    {
      var port = new Port(hw.ReadRegister, hw.WriteRegister, side);
      port.Write(register, 200);
      Assert.Single(hw.Registers.Where(x => x == 200));
    }

    [Theory]
    [MemberData(nameof(RegisterArgs))]
    [InlineData(Port.Side.A, McpReg.GPIO, OLATA_ADR)]
    [InlineData(Port.Side.B, McpReg.GPIO, OLATB_ADR)]
    public void MaskAppliedOnWriteHigh(Port.Side side, McpReg register, int address)
    {
      var port = new Port(hw.ReadRegister, hw.WriteRegister, side);
      hw.Registers[address] = 0x0F;
      port.MaskedWrite(register, HIGH, 0x55);
      Assert.Equal(0x5F, hw.Registers[address]);
    }

    [Theory]
    [MemberData(nameof(RegisterArgs))]
    [InlineData(Port.Side.A, McpReg.GPIO, OLATA_ADR)]
    [InlineData(Port.Side.B, McpReg.GPIO, OLATB_ADR)]
    public void MaskAppliedOnWriteLow(Port.Side side, McpReg register, int address)
    {
      var port = new Port(hw.ReadRegister, hw.WriteRegister, side);
      hw.Registers[address] = 0x0F;
      port.MaskedWrite(register, LOW, 0x55);
      Assert.Equal(0x0A, hw.Registers[address]);
    }

    [Theory]
    [MemberData(nameof(RegisterArgs))]
    [InlineData(Port.Side.A, McpReg.GPIO, GPIOA_ADR)]
    [InlineData(Port.Side.B, McpReg.GPIO, GPIOB_ADR)]
    public void ReadsFromCache(Port.Side side, McpReg register, int address)
    {
      var port = new Port(hw.ReadRegister, hw.WriteRegister, side);
      port.UseCaching = true;
      hw.Registers[address] = 200;
      port.Read(register);
      hw.Registers[address] = 5;
      Assert.Equal(200, port.Read(register));
    }

    [Theory]
    [MemberData(nameof(RegisterArgs))]
    [InlineData(Port.Side.A, McpReg.GPIO, GPIOA_ADR)]
    [InlineData(Port.Side.B, McpReg.GPIO, GPIOB_ADR)]
    public void UpdatesCache(Port.Side side, McpReg register, int address)
    {
      var port = new Port(hw.ReadRegister, hw.WriteRegister, side);
      port.UseCaching = true;
      hw.Registers[address] = 200;
      port.Read(register);
      hw.Registers[address] = 5;
      port.Update(register);
      Assert.Equal(5, port.Read(register));
    }

    [Theory]
    [MemberData(nameof(RegisterArgs))]
    [InlineData(Port.Side.A, McpReg.GPIO, GPIOA_ADR)]
    [InlineData(Port.Side.B, McpReg.GPIO, GPIOB_ADR)]
    public void UpdateCalculatesDiff(Port.Side side, McpReg register, int address)
    {
      var port = new Port(hw.ReadRegister, hw.WriteRegister, side);
      port.UseCaching = true;
      hw.Registers[address] = 0x0F;
      port.Read(register);
      hw.Registers[address] = 0x55;
      var diff = port.Update(register);
      Assert.Equal(0x5A, diff);
    }

    [Theory]
    [MemberData(nameof(RegisterArgs))]
    [InlineData(Port.Side.A, McpReg.GPIO, OLATA_ADR)]
    [InlineData(Port.Side.B, McpReg.GPIO, OLATB_ADR)]
    public void WritesToCache(Port.Side side, McpReg register, int address)
    {
      var port = new Port(hw.ReadRegister, hw.WriteRegister, side);
      port.UseCaching = true;
      port.Write(register, 200);
      Assert.Equal(0, hw.Registers[address]);
    }

    [Theory]
    [MemberData(nameof(RegisterArgs))]
    [InlineData(Port.Side.A, McpReg.GPIO, OLATA_ADR)]
    [InlineData(Port.Side.B, McpReg.GPIO, OLATB_ADR)]
    public void CommittingWrite(Port.Side side, McpReg register, int address)
    {
      var port = new Port(hw.ReadRegister, hw.WriteRegister, side);
      port.UseCaching = true;
      port.Write(register, 200);
      port.Commit();
      Assert.Equal(200, hw.Registers[address]);
    }

    [Fact]
    public void ResetWithCacheWriteDefaults()
    {
      var port = new Port(hw.ReadRegister, hw.WriteRegister, Port.Side.A);
      port.UseCaching = true;
      hw.Registers[IODIRA_ADR] = 0x55;
      hw.Registers[IPOLA_ADR] = 0x55;
      hw.Registers[GPINTENA_ADR] = 0x55;
      port.Write(McpReg.IODIR, 30);
      port.Write(McpReg.IPOL, 30);
      port.Reset();
      Assert.Equal(0xFF, hw.Registers[IODIRA_ADR]);
      Assert.Equal(0x00, hw.Registers[IPOLA_ADR]);
      Assert.Equal(0x00, hw.Registers[GPINTENA_ADR]);
    }
  }
}
