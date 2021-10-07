using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MemTool
{
    public static class StructToBytes
    {
        public static byte[] ToBytes<T>(this T v)
        {
            var size = Marshal.SizeOf(v);
            var ptr = Marshal.AllocHGlobal(size);
            var bytes = new byte[size];

            Marshal.StructureToPtr(v, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            return bytes;
        }
    }
}
