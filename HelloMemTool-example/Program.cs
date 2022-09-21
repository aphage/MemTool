using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HelloMemTool_eample
{
    class Program
    {
        static void Main(string[] args)
        {
            var mem = new MemTool.Mem(Process.GetCurrentProcess().Id);

            long v = 123;
            var size = Marshal.SizeOf(v);
            Console.WriteLine(size);

            var buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(v, buffer, false);

            mem.Wpm<long>(buffer, 233);

            Console.WriteLine("v is {0}", mem.Rpm<long>(buffer));

            Console.WriteLine("v is {0}", Marshal.ReadInt64(buffer));

            Console.WriteLine($"v is {BitConverter.ToInt64(mem.Rpm(buffer, sizeof(long)))}");

            Marshal.FreeHGlobal(buffer);


            IntPtr n = new IntPtr(Convert.ToInt64("0xFFFFFFFFFFF", 16));
            Console.WriteLine(n);

            Console.WriteLine(IntPtr.Add(n, 0x100));

            Console.WriteLine("Hello World!");

            //mem.TestVirtualQueryEx();

            //www.pinvoke.net

            //write
            mem.Wpm<int>(new IntPtr(0x10000), new int[] { 0x20, 0x8 }, 2434);

            //read
            mem.Rpm<int>(new IntPtr(0x10000), new int[] { 0x20, 0x8 });

            var index = MemTool.PatternFind.Find(new byte[]{ 0xFF, 0xA2, 0x03, 0xB4, 0xC5, 0xE6, 0x07}, "B? C5 E6");
            Console.WriteLine("index is {0}", index);


            //mem = new MemTool.Mem(6708);
            //var r = mem.Search(new IntPtr(0x10000), new IntPtr(0x00007fffffffffff), "00 FF 00 A2 00 03 00 B? 00 C5 00 ?? 00 07");
            //foreach(var addr in r)
            //{
                //Console.WriteLine($"addr is 0x{addr.ToInt64():X}");
            //}
        }
    }
}
