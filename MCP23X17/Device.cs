/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;

namespace MCP23X17
{
  public enum McpReg
  {
    /// <summary>
    /// Controls the direction of the data I/O.
    /// When a bit is set, the corresponding pin becomes an
    /// input. When a bit is clear, the corresponding pin
    /// becomes an output.
    /// </summary>
    IODIR = 0,
    /// <summary>
    /// This register allows the user to configure the polarity on
    /// the corresponding GPIO port bits.
    /// If a bit is set, the corresponding GPIO register bit will
    /// reflect the inverted value on the pin.
    /// </summary>
    IPOL = 1,
    /// <summary>
    /// The GPINTEN register controls the interrupt-onchange feature for each pin.
    /// If a bit is set, the corresponding pin is enabled for
    /// interrupt-on-change. The DEFVAL and INTCON
    /// registers must also be configured if any pins are
    /// enabled for interrupt-on-change.
    /// </summary>
    GPINTEN = 2,
    /// <summary>
    /// The default comparison value is configured in the
    /// DEFVAL register. If enabled (via GPINTEN and
    /// INTCON) to compare against the DEFVAL register, an
    /// opposite value on the associated pin will cause an
    /// interrupt to occur
    /// </summary>
    DEFVAL = 3,
    /// <summary>
    /// The INTCON register controls how the associated pin
    /// value is compared for the interrupt-on-change feature.
    /// If a bit is set, the corresponding I/O pin is compared
    /// against the associated bit in the DEFVAL register. If a
    /// bit value is clear, the corresponding I/O pin is compared
    /// against the previous value.
    /// </summary>
    INTCON = 4,
    /// <summary>
    /// The GPPU register controls the pull-up resistors for the
    /// port pins. If a bit is set and the corresponding pin is
    /// configured as an input, the corresponding port pin is
    /// internally pulled up with a 100 kΩ resistor.
    /// </summary>
    GPPU = 6,
    /// <summary>
    /// The INTF register reflects the interrupt condition on the
    /// port pins of any pin that is enabled for interrupts via the
    /// GPINTEN register. A ‘set’ bit indicates that the
    /// associated pin caused the interrupt.
    /// This register is ‘read-only’. Writes to this register will be
    /// ignored.
    /// </summary>
    INTF = 7,
    /// <summary>
    /// The INTCAP register captures the GPIO port value at
    /// the time the interrupt occurred. The register is ‘read
    /// only’ and is updated only when an interrupt occurs. The
    /// register will remain unchanged until the interrupt is
    /// cleared via a read of INTCAP or GPIO.
    /// </summary>
    INTCAP = 8,
    /// <summary>
    /// The GPIO register reflects the value on the port.
    /// Reading from this register reads the port. Writing to this
    /// register modifies the Output Latch (OLAT) register
    /// </summary>
    GPIO = 9,
    /// <summary>
    /// The OLAT register provides access to the output
    /// latches. A read from this register results in a read of the
    /// OLAT and not the port itself. A write to this register
    /// modifies the output latches that modifies the pins
    /// configured as outputs.
    /// </summary>
    OLAT = 10
  }

  /// <summary>
  /// A single MCP23017/MCP23S17 chip with 21 registers. See <see cref="http://ww1.microchip.com/downloads/en/devicedoc/20001952c.pdf">Datasheet</see> for documentation
  /// </summary>
  public class Device
  {
    const int IOCON_INDEX = 5;

    public Port PortA { get; }
    public Port PortB { get; }

    public Pin GPA0 { get; }
    public Pin GPA1 { get; }
    public Pin GPA2 { get; }
    public Pin GPA3 { get; }
    public Pin GPA4 { get; }
    public Pin GPA5 { get; }
    public Pin GPA6 { get; }
    public Pin GPA7 { get; }
    public Pin GPB0 { get; }
    public Pin GPB1 { get; }
    public Pin GPB2 { get; }
    public Pin GPB3 { get; }
    public Pin GPB4 { get; }
    public Pin GPB5 { get; }
    public Pin GPB6 { get; }
    public Pin GPB7 { get; }

    /// <summary>
    /// Creates a new driver instance with caching capabilities
    /// </summary>
    /// <param name="writeAddressByte"></param>
    /// <param name="readAddressByte"></param>
    public Device(Action<int, byte> writeAddressByte, Func<int, byte> readAddressByte)
    {
      PortA = new Port(readAddressByte, writeAddressByte, Port.Side.A);
      PortB = new Port(readAddressByte, writeAddressByte, Port.Side.B);

      GPA0 = new Pin(PortA, 0);
      GPA1 = new Pin(PortA, 1);
      GPA2 = new Pin(PortA, 2);
      GPA3 = new Pin(PortA, 3);
      GPA4 = new Pin(PortA, 4);
      GPA5 = new Pin(PortA, 5);
      GPA6 = new Pin(PortA, 6);
      GPA7 = new Pin(PortA, 7);
      GPB0 = new Pin(PortB, 0);
      GPB1 = new Pin(PortB, 1);
      GPB2 = new Pin(PortB, 2);
      GPB3 = new Pin(PortB, 3);
      GPB4 = new Pin(PortB, 4);
      GPB5 = new Pin(PortB, 5);
      GPB6 = new Pin(PortB, 6);
      GPB7 = new Pin(PortB, 7);
    }

    /// <summary>
    /// Reads values of registers where the mask is set. The mask is 16 bits. Low 8 is the A port, High 8 is the B port.
    /// </summary>
    /// <param name="register"></param>
    /// <param name="mask"></param>
    /// <returns></returns>
    public ushort Read(McpReg register, ushort mask = 0xFFFF)
    {
      ushort value = 0;
      if ((mask & 0x00FF) > 0)
      {
        value |= PortA.Read(register);
      }
      if ((mask & 0xFF00) > 0)
      {
        value |= (ushort)(PortB.Read(register) << 8);
      }
      return (ushort)(value & mask);
    }

    /// <summary>
    /// Writes a masked value to both ports' registers of the given type. The mask is 16 bits. Low 8 is the A port, High 8 is the B port.
    /// </summary>
    /// <param name="register"></param>
    /// <param name="value"></param>
    /// <param name="mask"></param>
    public void Write(McpReg register, bool value, ushort mask = 0xFFFF)
    {
      // Port A
      var lower = mask & 0x00FF;
      if (lower == 0x00FF)
      {
        // If all bits are overwritten
        PortA.Write(register, value ? (byte)0xFF : (byte)0x00);
      }
      else if (lower != 0)
      {
        // If some bits need to be preserved
        PortA.Write(register, value
          ? (byte)(PortA.Read(register) | mask)
          : (byte)(PortA.Read(register) & ~mask));
      }

      // Port B
      var upper = mask & 0xFF00;
      if (upper == 0xFF00)
      {
        // If all bits are overwritten
        PortB.Write(register, value ? (byte)0xFF : (byte)0x00);
      }
      else if (upper != 0)
      {
        // If some bits need to be preserved
        PortB.Write(register, value
          ? (byte)(PortB.Read(register) | (mask >> 8))
          : (byte)(PortB.Read(register) & ~(mask >> 8)));
      }
    }

    /// <summary>
    /// Writes a value to both ports' registers of the given type. Low 8 is the A port, High 8 is the B port.
    /// </summary>
    /// <param name="register"></param>
    /// <param name="values"></param>
    public void Write(McpReg register, ushort values)
    {
      PortA.Write(register, (byte)values);
      PortB.Write(register, (byte)(values >> 8));
    }

    /// <summary>
    /// Sets all registers to their default values
    /// </summary>
    public void Reset()
    {
      PortA.Reset();
      PortB.Reset();
    }

    /// <summary>
    /// Set all the flags in the IOCON register at once
    /// </summary>
    /// <param name="values"></param>
    public void WriteIOCON(byte values)
    {
      PortA.Write(IOCON_INDEX, values);
      var bank = (values & 0x80) > 0;
      PortA.Bank = bank;
      PortB.Bank = bank;
    }

    /// <summary>
    /// Interrupt Polarity flag
    /// </summary>
    public bool IntPol
    {
      get => (PortA.Read(IOCON_INDEX) & 0x02) > 0;
      set => PortA.MaskedWrite(IOCON_INDEX, value, 0x02);
    }

    /// <summary>
    /// Open Drain interrupt output flag
    /// </summary>
    public bool ODr
    {
      get => (PortA.Read(IOCON_INDEX) & 0x04) > 0;
      set => PortA.MaskedWrite(IOCON_INDEX, value, 0x04);
    }

    /// <summary>
    /// Hardware Address Enable flag
    /// </summary>
    public bool HAEn
    {
      get => (PortA.Read(IOCON_INDEX) & 0x08) > 0;
      set => PortA.MaskedWrite(IOCON_INDEX, value, 0x08);
    }

    /// <summary>
    /// SDA output Slew Rate disable flag
    /// </summary>
    public bool DisSlw
    {
      get => (PortA.Read(IOCON_INDEX) & 0x10) > 0;
      set => PortA.MaskedWrite(IOCON_INDEX, value, 0x10);
    }

    /// <summary>
    /// Sequential Operation flag
    /// </summary>
    public bool SeqOp
    {
      get => (PortA.Read(IOCON_INDEX) & 0x20) > 0;
      set => PortA.MaskedWrite(IOCON_INDEX, value, 0x20);
    }

    /// <summary>
    /// Interrupt pins mirror flag. Connects pins internally.
    /// </summary>
    public bool Mirror
    {
      get => (PortA.Read(IOCON_INDEX) & 0x40) > 0;
      set => PortA.MaskedWrite(IOCON_INDEX, value, 0x40);
    }

    /// <summary>
    /// Bank addressing scheme.
    /// </summary>
    public bool Bank
    {
      get => (PortA.Read(IOCON_INDEX) & 0x80) > 0;
      set
      {
        PortA.MaskedWrite(IOCON_INDEX, value, 0x80);
        PortA.Bank = value;
        PortB.Bank = value;
      }
    }
  }
}
