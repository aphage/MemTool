# MemTool
C# .net Memory Tool library

```c#
//open process
mem = new MemTool.Mem(6708);

//write
mem.Wpm<int>(new IntPtr(0x10000), 1234);
mem.Wpm<int>(new IntPtr(0x10000), new int[] { 0x20, 0x8 }, 2434);

//read
mem.Rpm<int>(new IntPtr(0x10000));
mem.Rpm<int>(new IntPtr(0x10000), new int[] { 0x20, 0x8 });

//search
var r = mem.Search(new IntPtr(0x10000), new IntPtr(0x00007fffffffffff), "00 FF 00 A2 00 03 00 B? 00 C5 00 ?? 00 07");
foreach(var addr in r)
{
    Console.WriteLine($"addr is 0x{addr.ToInt64():X}");
}

//alloc/free memory
var buf = mem.Alloc(4);
mem.Free(buf);

//memory protect
var (is_ok, old_protect) = mem.PageERW(new IntPtr(0x10000), 4);
//do something
mem.PageRestore(new IntPtr(0x10000), 4 , old_protect);
```
