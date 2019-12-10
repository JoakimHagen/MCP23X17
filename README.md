# MCP23X17
C# device driver for MCP23017/MCP23S17

There are 3 different interfaces to use
- 1 bit individual pin manipulation with the Pin class
- 8 bit port register manipulation with the Port class
- 16 bit full microchip manipulation with the Device class

Each port supports read/write from cache and to queue with commits independently.

When the `UseCaching` property is not set (which is default), read and write commands will be issued for every interaction that requires it.

When using caching, you are responsible for calling `Commit()` when done writing. Switching `UseCaching` off will not commit any queued writes, but you can still call `Commit()`.
Reads will reflect the latest written values even before committing and queued writes cannot be discarded, but can be overwritten with new values before committing to hardware.
Use `Update(register)` to update the cache with fresh reads. This is required to detect changes to `GPIO` when `UseCaching` is set.

### Current limitations

- This driver does not come with an I²C or SPI controller. It is an abstraction of top of any communication pipeline implementation of your chosing.
- Support for IOCON register manipulation is not yet implemented but underway.
- Sequential operations are not implemented.
- OnChange C# events planned but not yet implemented.