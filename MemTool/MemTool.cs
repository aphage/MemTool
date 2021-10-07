using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace MemTool
{
    public class Mem
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
             uint processAccess,
             bool bInheritHandle,
             int processId
        );
        public static IntPtr OpenProcess(Process proc, ProcessAccessFlags flags)
        {
            return OpenProcess((uint)flags, false, proc.Id);
        }
        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(
             IntPtr hProcess,
             IntPtr lpBaseAddress,
             byte[] lpBuffer,
             Int32 nSize,
             out IntPtr lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(
             IntPtr hProcess,
             IntPtr lpBaseAddress,
             [In, MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
             Int32 nSize,
             out IntPtr lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, 
            UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, AllocationType dwFreeType);

        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION64
        {
            public ulong BaseAddress;
            public ulong AllocationBase;
            public int AllocationProtect;
            public int __alignment1;
            public ulong RegionSize;
            public int State;
            public int Protect;
            public int Type;
            public int __alignment2;
        }

        public enum VirtualMemoryState : uint
        {
            MEM_COMMIT = 0x00001000,
            MEM_RESERVE = 0x00002000,
            MEM_DECOMMIT = 0x00004000,
            MEM_RELEASE = 0x00008000,
            MEM_FREE = 0x00010000,
            MEM_PRIVATE = 0x00020000,
            MEM_MAPPED = 0x00040000,
            MEM_RESET = 0x00080000,
            MEM_TOP_DOWN = 0x00100000,
            MEM_WRITE_WATCH = 0x00200000,
            MEM_PHYSICAL = 0x00400000,
            MEM_ROTATE = 0x00800000,
            MEM_DIFFERENT_IMAGE_BASE_OK = 0x00800000,
            MEM_RESET_UNDO = 0x01000000,
            MEM_LARGE_PAGES = 0x20000000,
            MEM_DOS_LIM = 0x40000000,
            MEM_4MB_PAGES = 0x80000000,
            MEM_IMAGE = 0x01000000,
        }

        public enum AllocationProtect : uint
        {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        private IntPtr handle = IntPtr.Zero;

        public Mem(int processId)
        {
            this.handle = OpenProcess(Process.GetProcessById(processId), 
                ProcessAccessFlags.VirtualMemoryRead | ProcessAccessFlags.VirtualMemoryWrite |
                ProcessAccessFlags.VirtualMemoryOperation | ProcessAccessFlags.QueryInformation);
        }

        public Mem(IntPtr handle)
        {
            this.handle = handle;
        }

        public IntPtr Alloc(IntPtr addr, uint size)
        {
            var ptr = VirtualAllocEx(handle, addr, size, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
            return ptr;
        }

        public IntPtr Alloc(uint size)
        {
            return Alloc(new IntPtr(0), size);
        }

        public bool Free(IntPtr addr)
        {
            return VirtualFreeEx(handle, addr, 0, AllocationType.Release);
        }

        public (bool,uint) PageERW(IntPtr addr, uint size)
        {
            uint old_protect = 0;
            var b = VirtualProtectEx(handle, addr, new UIntPtr(size), (int)AllocationProtect.PAGE_EXECUTE_READWRITE, out old_protect);
            return (b, old_protect);
        }

        public bool PageRestore(IntPtr addr, uint size,uint protect)
        {
            return VirtualProtectEx(handle, addr, new UIntPtr(size), protect, out var old_protect);
        }

        public byte[] Rpm(IntPtr lpBaseAddress, int size)
        {
            var buffer = new byte[size];
            ReadProcessMemory(handle, lpBaseAddress, buffer, size, out var out_size);
            return buffer;
        }
        public T Rpm<T>(IntPtr lpBaseAddress) where T : struct
        {
            T[] buffer = new T[1];
            ReadProcessMemory(handle, lpBaseAddress, buffer, Marshal.SizeOf<T>(), out var bytesread);
            return buffer.First(); // [0] would be faster, but First() is safer. E.g. of buffer[0] ?? default(T)
        }

        public T Rpm<T>(IntPtr lpBaseAddress, int[] offsets) where T : struct
        {
            IntPtr address = lpBaseAddress;

            var lastOffset = offsets.Last();
            for(int i = 0; i < offsets.Length - 1; i++)
            {
                address = Rpm<IntPtr>(IntPtr.Add(address, offsets[i]));
            }

            return Rpm<T>(IntPtr.Add(address, lastOffset));
        }
        public T Rpm<T>(IntPtr lpBaseAddress, List<int> offsets) where T : struct
        {
            IntPtr address = lpBaseAddress;

            var lastOffset = offsets.Last();
            offsets.RemoveAt(offsets.Count - 1);

            foreach (var offset in offsets)
            {
                address = Rpm<IntPtr>(IntPtr.Add(address, offset));
            }

            return Rpm<T>(IntPtr.Add(address, lastOffset));
        }

        public bool Wpm(IntPtr address, byte[] buffer, int size)
        {
            return WriteProcessMemory(handle, address, buffer, size, out var out_size);
        }
        public bool Wpm<T>(IntPtr lpBaseAddress, T v) where T : struct
        {
            T[] buffer = new T[] { v };

            return WriteProcessMemory(handle, lpBaseAddress, buffer, Marshal.SizeOf<T>(), out var byteswrite);
        }
        public bool Wpm<T>(IntPtr lpBaseAddress, int[] offsets, T v) where T : struct
        {
            IntPtr address = lpBaseAddress;

            var lastOffset = offsets.Last();
            for (int i = 0; i < offsets.Length - 1; i++)
            {
                address = Rpm<IntPtr>(IntPtr.Add(address, offsets[i]));
            }

            return Wpm<T>(IntPtr.Add(address, lastOffset), v);
        }
        public bool Wpm<T>(IntPtr lpBaseAddress, List<int> offsets, T v) where T : struct
        {
            IntPtr address = lpBaseAddress;

            var lastOffset = offsets.Last();
            offsets.RemoveAt(offsets.Count - 1);

            foreach (var offset in offsets)
            {
                address = Rpm<IntPtr>(IntPtr.Add(address, offset));
            }

            return Wpm<T>(IntPtr.Add(address, lastOffset), v);
        }

        public List<MEMORY_BASIC_INFORMATION> EnumerateMemory(IntPtr address, IntPtr max_address,Func<IntPtr, IntPtr, MEMORY_BASIC_INFORMATION, bool> func)
        {
            List<MEMORY_BASIC_INFORMATION> r = new List<MEMORY_BASIC_INFORMATION>();

            IntPtr p_next;
            for (IntPtr p_region = address; (UInt64)p_region.ToInt64() < (UInt64)max_address.ToInt64(); p_region = p_next)
            {
                MEMORY_BASIC_INFORMATION m = new MEMORY_BASIC_INFORMATION();
                if (VirtualQueryEx(handle, p_region, out m, (uint)Marshal.SizeOf(m)) <= 0)
                {
                    break;
                }

                r.Add(m);

                p_next = new IntPtr(m.BaseAddress.ToInt64() + m.RegionSize.ToInt64());
                if (VirtualMemoryState.MEM_FREE.Is(m.State) || VirtualMemoryState.MEM_RESERVE.Is(m.State))
                {
                    continue;
                }

                if ((m.Protect & (uint)AllocationProtect.PAGE_GUARD) > 0 || (m.Protect & (uint)AllocationProtect.PAGE_NOCACHE) > 0)
                {
                    continue;
                }

                if (m.Protect == (uint)AllocationProtect.PAGE_NOACCESS)
                {
                    continue;
                }

                if(!func(p_region, p_next, m))
                {
                    break;
                }
            }

            return r;
        }

        public List<IntPtr> Search(IntPtr address, IntPtr max_address, string pattern)
        {
            var r = new ConcurrentQueue<IntPtr>();
            List<WaitHandle> waitHandles = new List<WaitHandle>();
            var (is_ok, patterns) = PatternFind.PatternTransform(pattern);
            if (!is_ok)
            {
                return r.ToList();
            }

            EnumerateMemory(address, max_address, (addr_start, addr_end, m) => {

                Int64 size = m.RegionSize.ToInt64();
                byte[] bytes = new byte[size];
                if (ReadProcessMemory(handle, m.BaseAddress, bytes, (int)size, out var n))
                {
                    WaitHandle wait = new AutoResetEvent(false);
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict["wait"] = wait;
                    dict["m"] = m;
                    dict["bytes"] = bytes;
                    var b = ThreadPool.QueueUserWorkItem((state) => {
                        var _dict = (Dictionary<string, object>)state;
                        var _m = (MEMORY_BASIC_INFORMATION)_dict["m"];
                        var _w = (AutoResetEvent)_dict["wait"];
                        var _b = (byte[])_dict["bytes"];

                        var addr = _m.BaseAddress;
                        Int64 i = PatternFind.Find(_b, patterns);
                        Int64 new_arrays_index = 0;
                        while (i != -1)
                        {
                            r.Enqueue(new IntPtr(addr.ToInt64() + i + new_arrays_index));
                            new_arrays_index += i + patterns.Count;

                            var bs = _b.Skip((int)new_arrays_index).ToArray();

                            i = PatternFind.Find(bs, patterns);
                        }
                        _w.Set();
                    }, dict);

                    if (b)
                    {
                        waitHandles.Add(wait);
                    }
                }

                return true;
            });
            foreach(var w in waitHandles)
            {
                w.WaitOne();
            }

            return r.ToList();
        }

        public List<IntPtr> Search<T>(IntPtr address, IntPtr max_address, T v)
        {
            var r = new ConcurrentQueue<IntPtr>();
            List<WaitHandle> waitHandles = new List<WaitHandle>();
            var patterns = v.ToBytes();

            EnumerateMemory(address, max_address, (addr_start, addr_end, m) => {

                Int64 size = m.RegionSize.ToInt64();
                byte[] bytes = new byte[size];
                if (ReadProcessMemory(handle, m.BaseAddress, bytes, (int)size, out var n))
                {
                    WaitHandle wait = new AutoResetEvent(false);
                    Dictionary<string, Object> dict = new Dictionary<string, object>();
                    dict["wait"] = wait;
                    dict["m"] = m;
                    dict["bytes"] = bytes;
                    var b = ThreadPool.QueueUserWorkItem((state) => {
                        var dict_ = (Dictionary<string, Object>)state;
                        var m_ = (MEMORY_BASIC_INFORMATION)dict_["m"];
                        var w_ = (AutoResetEvent)dict_["wait"];
                        var b_ = (byte[])dict_["bytes"];

                        var addr = m_.BaseAddress;
                        Int64 i = PatternFind.Find(b_, patterns);
                        Int64 new_arrays_index = 0;
                        while (i != -1)
                        {
                            r.Enqueue(new IntPtr(addr.ToInt64() + i + new_arrays_index));
                            new_arrays_index += i + patterns.Length;

                            var bs = b_.Skip((int)new_arrays_index).ToArray();

                            i = PatternFind.Find(bs, patterns);
                        }
                        w_.Set();
                    }, dict);

                    if (b)
                    {
                        waitHandles.Add(wait);
                    }
                }

                return true;
            });
            foreach (var w in waitHandles)
            {
                w.WaitOne();
            }

            return r.ToList();
        }
        public void TestVirtualQueryEx()
        {
            long MaxAddress = 0x00007fffffffffff;
            long address = 0;
            Console.WriteLine("{0}", Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
            do
            {
                MEMORY_BASIC_INFORMATION m;
                int result = VirtualQueryEx(handle, (IntPtr)address, out m, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
                Console.WriteLine("{0:X}-{1:X} : {2:X} {4:X} bytes result={3}", (ulong)m.BaseAddress, (ulong)m.BaseAddress + (ulong)m.RegionSize - 1, (ulong)m.RegionSize, result, (ulong)m.AllocationBase);
                if (address == (long)m.BaseAddress + (long)m.RegionSize)
                    break;
                address = (long)m.BaseAddress + (long)m.RegionSize;
            } while (address <= MaxAddress);
            Console.ReadLine();
        }
    }

    public static class VirtualMemoryStateExtensions
    {
        public static bool Is(this Mem.VirtualMemoryState state, uint v)
        {
            return (uint)state == v;
        }
    }
}
