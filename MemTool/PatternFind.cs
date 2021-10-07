using System;
using System.Collections.Generic;

namespace MemTool
{
    public class PatternFind
    {
        public static long Find(byte[] data, string pattern)
        {
            var (b, patterns) = PatternTransform(pattern);
            if (!b)
            {
                return -1;
            }

            return Find(data, patterns);
        }

        public static long Find(byte[] data, byte[] pattern)
        {
            if(pattern.Length > data.Length)
            {
                return -1;
            }
            for(long i = 0, pos = 0; i < data.Length; i++)
            {
                if(data[i] == pattern[pos])
                {
                    pos++;
                    if(pos == pattern.Length)
                    {
                        return i - pattern.Length + 1;
                    }
                }
                else if(pos > 0)
                {
                    i -= pos;
                    pos = 0;
                }
            }

            return -1;
        }

        public static long Find<T>(byte[] data, T v)
        {
            var patterns = v.ToBytes();
            return Find(data, patterns);
        }

        private static bool patternmatchbyte(byte b, in PatternByte pattern)
        {
            int matched = 0;

            byte n1 = Convert.ToByte((b >> 4) & 0xF);
            if(pattern.nibble[0].wildcard)
                matched++;
            else if(pattern.nibble[0].data == n1)
                matched++;

            byte n2 = Convert.ToByte(b & 0xF);
            if(pattern.nibble[1].wildcard)
                matched++;
            else if(pattern.nibble[1].data == n2)
                matched++;

            return (matched == 2);
        }

        public static long Find(byte[] data, List<PatternByte> patterns)
        {
            long searchpatternsize = patterns.Count;
            for (long i = 0, pos = 0; i < data.Length; i++)
            {
                if(patternmatchbyte(data[i], patterns[(int)pos]))
                {
                    pos++;
                    if(pos == searchpatternsize)
                    {
                        return i - searchpatternsize + 1;
                    }
                }
                else if(pos > 0)
                {
                    i -= pos;
                    pos = 0;
                }
            }
            return -1;
        }

        public static (bool,List<PatternByte>) PatternTransform(string s)
        {
            s = s.Replace(" ", "").Replace("0x", "");
            foreach(var c in s)
            {
                if(c != '?' && !Uri.IsHexDigit(c))
                {
                    return (false, null);
                }
            }

            if(s.Length % 2 != 0)
            {
                s += '?';
            }

            List<PatternByte> patternBytes = new List<PatternByte>();
            byte i = 0;
            PatternByte patternByte = new PatternByte();
            foreach(var c in s)
            {
                if(c == '?')
                {
                    patternByte.nibble[i].wildcard = true;
                }
                else
                {
                    patternByte.nibble[i].wildcard = false;
                    patternByte.nibble[i].data = Convert.ToByte(c.ToString(), 16);
                }

                i++;
                if(i == 2)
                {
                    i = 0;
                    patternBytes.Add(patternByte);
                    patternByte = new PatternByte();
                }
            }

            return (true, patternBytes);
        }



        public class PatternNibble
        {
            public byte data { get; set; }
            public bool wildcard { get; set; }
        }

        public class PatternByte
        {
            public PatternNibble[] nibble;

            public PatternByte()
            {
                nibble = new PatternNibble[] { new PatternNibble(), new PatternNibble()};
            }
        }

    }
}
