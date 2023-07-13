﻿using Trinity.Core.Math.Hash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Trinity.Core.Cache
{
    public class GFPakHashCache
    {
        private const string CachePath = "GFPAKHashCache.bin";
        private const string LatestCachePath = "hashes_inside_fd.txt";
        private static Dictionary<ulong, string> Cache = new Dictionary<ulong, string>();

        public static void Init(string path = CachePath)
        {
            Cache = new Dictionary<ulong, string>();
            if (File.Exists(LatestCachePath))
            {
                using (StreamReader streamReader = new StreamReader(LatestCachePath))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var hashEntry = line.Split(' ');
                        ulong hash;
                        string name;

                        if (hashEntry.Length == 2 && !string.IsNullOrEmpty(hashEntry[0]) && !string.IsNullOrEmpty(hashEntry[1]))
                        {
                            try
                            {
                                hash = Convert.ToUInt64(hashEntry[0], 16);
                                name = hashEntry[1].TrimEnd('\r', '\n');
                            }
                            catch
                            {
                                continue;
                            }

                            Cache.TryAdd(hash, name);
                        }
                    }
                }
            }
            else if (File.Exists(path)) 
            {
                BinaryReader br = new BinaryReader(File.OpenRead(path));
                var version = br.ReadUInt64();
                var count = br.ReadUInt32();
                for (int i = 0; i < count; i++)
                {
                    var hash = br.ReadUInt64();
                    var length = br.ReadByte();
                    var name = new String(br.ReadChars(length));
                    Cache.TryAdd(hash, name);
                    //Cache.Add(hash, name);
                }
            }
        }

        public static void Write(string path = CachePath)
        {
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(path));
            bw.Write(GFFNV.Hash(""));
            bw.Write((uint)Cache.Count);
            foreach(KeyValuePair<ulong, string> pair in Cache)
            {
                bw.Write(pair.Key);
                bw.Write(pair.Value);
            }
        }

        public static void AddHashName(UInt64 hash, string name)
        {
            UInt64 hashCheck = GFFNV.Hash(name);
            if (hashCheck == hash)
            {
                Cache[hash] = name;
            }
        }

        public static void AddHash(string name)
        {
            UInt64 hash = GFFNV.Hash(name);
            Cache[hash] = name;
        }

        public static string? GetName(UInt64 hash)
        {
            string? str = null;
            Cache.TryGetValue(hash, out str);
            return str;
        }

    }
}
