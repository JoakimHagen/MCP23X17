/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;

namespace MCP23X17
{
  public enum PinMode { Output = 0, Input = 1 }

  /// <summary>
  /// An abstraction around a single pin and it's configuration across multiple registers in isolation of other pins
  /// </summary>
  public class Pin
  {
    private readonly byte _index;
    private readonly byte _mask;
    private readonly Port _port;

    public Pin(Port port, int pinIndex)
    {
      if (pinIndex < 0 || pinIndex > 7)
      {
        throw new ArgumentException("There are 8 IO pins. Index is from 0-7. Got " + pinIndex);
      }
      _port = port;
      _index = (byte)pinIndex;
      _mask = (byte)(1 << pinIndex);
    }

    public override string ToString() => $"GPIO{_port.port}[{_index}]";

    /// <summary>
    /// Controls IO direction (<see cref="McpReg.IODIR"/>) and connects a pull-up resistor when in <see cref="PinMode.Input"/>
    /// </summary>
    public PinMode Mode
    {
      get => (_port.IODIR & _mask) > 0 ? PinMode.Input : PinMode.Output;
      set => _port.MaskedWrite(McpReg.IODIR, value == PinMode.Input, _mask);
    }

    /// <summary>
    /// Controls whether pin has bias towards 5.5V and not floating. <see cref="McpReg.GPPU"/>
    /// </summary>
    public bool ConnectPullUp
    {
      get => (_port.GPPU & _mask) > 0;
      set => _port.MaskedWrite(McpReg.GPPU, value, _mask);
    }

    /// <summary>
    /// Controls whether <see cref="Value"/> reports the opposite of the actual pin status.
    /// </summary>
    public bool OppositePolarity
    {
      get => (_port.IPOL & _mask) > 0;
      set => _port.MaskedWrite(McpReg.IPOL, value, _mask);
    }

    /// <summary>
    /// Reflects the current value of a <see cref="McpReg.GPIO"/> pin. If <see cref="Mode"/> is <see cref="PinMode.Output"/>, this can be set
    /// </summary>
    public bool Value
    {
      get => (_port.GPIO & _mask) > 0;
      set => _port.MaskedWrite(McpReg.OLAT, value, _mask);
    }

    /// <summary>
    /// <see cref="McpReg.OLAT"/>
    /// </summary>
    public bool OutputLatch
    {
      get => (_port.OLAT & _mask) > 0;
      set => _port.MaskedWrite(McpReg.OLAT, value, _mask);
    }
  }
}
