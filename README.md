# MCP23X17
C# device driver for MCP23017/MCP23S17

There are 3 different interfaces to use
- 1 bit individual pin manipulation with the Pin class
- 8 bit port register manipulation with the Port class
- 16 bit full microchip manipulation with the Device class

Each port supports read/write from cache and to queue with commits independently.
Switch this on by setting the UseCaching property. Switching off does not commit any queued writes.

### Current limitations

- This driver does not come with an I²C or SPI controller. It is an abstraction of top of any communication pipeline implementation of your chosing.
- Support for IOCON register manipulation is not yet implemented.