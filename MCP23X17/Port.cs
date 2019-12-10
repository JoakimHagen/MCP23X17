/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;

namespace MCP23X17
{
  /// <summary>
  /// One of the chip's two sides. Contains a set of 10 8bit registers. Supports caching and write commits.
  /// </summary>
  public class Port
  {
    protected readonly Func<int, byte> readHardware;
    protected readonly Action<int, byte> writeHardware;
    protected readonly byte?[] Cache = new byte?[11];

    // IOCON and GPIO cannot be written to using this class so max 9 writes can be queued.
    protected readonly Queue<int> WriteQueue = new Queue<int>(9);
    private readonly Func<int, int> _getHwAdr;

    internal readonly Side port;
    internal bool Bank { get; set; }

    public bool UseCaching { get; set; }

    public Port(Func<int, byte> readAddressByte, Action<int, byte> writeAddressByte, Side port)
    {
      readHardware = readAddressByte;
      writeHardware = writeAddressByte;

      this.port = port;
      if (port == Side.A)
      {
        _getHwAdr = i => Bank ? i : i << 1;
      }
      else // Side.B
      {
        _getHwAdr = i => Bank ? i | 0x10 : (i << 1) | 1;
      }
    }

    /// <summary>
    /// If <see cref="UseCaching"/> is true and register is in cache, reads cache, else reads from hardware.
    /// </summary>
    /// <param name="register"></param>
    /// <returns></returns>
    public byte Read(McpReg register) => Read((int)register);

    internal byte Read(int index)
    {
      if (UseCaching && Cache[index].HasValue) return Cache[index].Value;

      var val = readHardware(_getHwAdr(index));
      Cache[index] = val;
      return val;
    }

    /// <summary>
    /// If <see cref="UseCaching"/> is true, writes to cache and queue, else writes to hardware.
    /// </summary>
    /// <param name="register"></param>
    /// <param name="values"></param>
    public void Write(McpReg register, byte values)
    {
      // Consider GPIO readonly
      if (register == McpReg.GPIO)
      {
        register = McpReg.OLAT;
      }

      Write((int)register, values);
    }

    internal void Write(int i, byte values)
    {
      if (Cache[i] == values) return;

      if (UseCaching && i == (int)McpReg.OLAT)
      {
        SimulateGPIOWrite(values);
      }

      Cache[i] = values;
      if (UseCaching)
      {
        if (!WriteQueue.Contains(i)) WriteQueue.Enqueue(i);
      }
      else
      {
        writeHardware(_getHwAdr(i), values);
      }
    }

    public void MaskedWrite(McpReg register, bool value, int mask)
    {
      // Consider GPIO readonly
      if (register == McpReg.GPIO)
      {
        register = McpReg.OLAT;
      }

      MaskedWrite((int)register, value, mask);
    }

    internal void MaskedWrite(int register, bool value, int mask)
    {
      Write(register, value
        ? (byte)(Read(register) | mask)
        : (byte)(Read(register) & ~mask));
    }

    /// <summary>
    /// Will modify <see cref="McpReg.GPIO"/> in cache using <see cref="McpReg.IODIR"/> as mask
    /// </summary>
    /// <param name="olat"></param>
    private void SimulateGPIOWrite(byte olat)
    {
      var iodir = Cache[(int)McpReg.IODIR];
      if (iodir == 0x00)
      {
        // If all IO are outputs we write olat directly
        Cache[(int)McpReg.GPIO] = olat;
      }
      else if (iodir != null)
      {
        // If IO direction is mixed we must preserve input values if existing
        var gpio = Cache[(int)McpReg.GPIO];
        if (gpio != null)
        {
          Cache[(int)McpReg.GPIO] = (byte)((gpio & iodir) | (olat & ~iodir));
        }
      }
    }

    /// <summary>
    /// Reads fresh register data from hardware into cache. Returns update difference.
    /// </summary>
    /// <param name="register"></param>
    public byte Update(McpReg register)
    {
      var i = (int)register;
      var values = readHardware(_getHwAdr(i));

      var diff = Cache[i] == null ? (byte)0xFF : (byte)(Cache[i] ^ values);
      Cache[i] = values;
      return diff;
    }

    /// <summary>
    /// Writes all changed registers in cache to chip
    /// </summary>
    public void Commit()
    {
      while (WriteQueue.Count > 0)
      {
        var i = WriteQueue.Dequeue();
        writeHardware(_getHwAdr(i), Cache[i].Value);
      }
    }

    /// <summary>
    /// Sets all registers to their default values
    /// </summary>
    public void Reset()
    {
      Write(McpReg.IODIR,   0xFF);
      Write(McpReg.IPOL,    0x00);
      Write(McpReg.GPINTEN, 0x00);
      Write(McpReg.DEFVAL,  0x00);
      Write(McpReg.INTCON,  0x00);
      Write(McpReg.GPPU,    0x00);
      Write(McpReg.INTF,    0x00);
      Write(McpReg.INTCAP,  0x00);
      // Write to GPIO is skipped because it only modifies OLAT
      Write(McpReg.OLAT,    0x00);
      Commit();
    }

    public override string ToString() => $"Port{port}";

    /// <summary>
    /// Read/Write <see cref="McpReg.IODIR"/>
    /// </summary>
    public byte IODIR
    {
      get => Read(McpReg.IODIR);
      set => Write(McpReg.IODIR, value);
    }
    /// <summary>
    /// Read/Write <see cref="McpReg.IPOL"/>
    /// </summary>
    public byte IPOL
    {
      get => Read(McpReg.IPOL);
      set => Write(McpReg.IPOL, value);
    }
    /// <summary>
    /// Read/Write <see cref="McpReg.GPINTEN"/>
    /// </summary>
    public byte GPINTEN
    {
      get => Read(McpReg.GPINTEN);
      set => Write(McpReg.GPINTEN, value);
    }
    /// <summary>
    /// Read/Write <see cref="McpReg.DEFVAL"/>
    /// </summary>
    public byte DEFVAL
    {
      get => Read(McpReg.DEFVAL);
      set => Write(McpReg.DEFVAL, value);
    }
    /// <summary>
    /// Read/Write <see cref="McpReg.INTCON"/>
    /// </summary>
    public byte INTCON
    {
      get => Read(McpReg.INTCON);
      set => Write(McpReg.INTCON, value);
    }
    /// <summary>
    /// Read/Write <see cref="McpReg.GPPU"/>
    /// </summary>
    public byte GPPU
    {
      get => Read(McpReg.GPPU);
      set => Write(McpReg.GPPU, value);
    }
    /// <summary>
    /// Read/Write <see cref="McpReg.INTF"/>
    /// </summary>
    public byte INTF
    {
      get => Read(McpReg.INTF);
      set => Write(McpReg.INTF, value);
    }
    /// <summary>
    /// Read/Write <see cref="McpReg.INTCAP"/>
    /// </summary>
    public byte INTCAP
    {
      get => Read(McpReg.INTCAP);
      set => Write(McpReg.INTCAP, value);
    }
    /// <summary>
    /// Read/Write <see cref="McpReg.GPIO"/>
    /// </summary>
    public byte GPIO
    {
      get => Read(McpReg.GPIO);
      set => Write(McpReg.GPIO, value);
    }
    /// <summary>
    /// Read/Write <see cref="McpReg.OLAT"/>
    /// </summary>
    public byte OLAT
    {
      get => Read(McpReg.OLAT);
      set => Write(McpReg.OLAT, value);
    }

    public enum Side { A, B }
  }

}
