namespace StdbModule.Utils;

public static class Sha256
    {
        private static readonly uint[] K = new uint[]
        {
            0x428a2f98,0x71374491,0xb5c0fbcf,0xe9b5dba5,0x3956c25b,0x59f111f1,0x923f82a4,0xab1c5ed5,
            0xd807aa98,0x12835b01,0x243185be,0x550c7dc3,0x72be5d74,0x80deb1fe,0x9bdc06a7,0xc19bf174,
            0xe49b69c1,0xefbe4786,0x0fc19dc6,0x240ca1cc,0x2de92c6f,0x4a7484aa,0x5cb0a9dc,0x76f988da,
            0x983e5152,0xa831c66d,0xb00327c8,0xbf597fc7,0xc6e00bf3,0xd5a79147,0x06ca6351,0x14292967,
            0x27b70a85,0x2e1b2138,0x4d2c6dfc,0x53380d13,0x650a7354,0x766a0abb,0x81c2c92e,0x92722c85,
            0xa2bfe8a1,0xa81a664b,0xc24b8b70,0xc76c51a3,0xd192e819,0xd6990624,0xf40e3585,0x106aa070,
            0x19a4c116,0x1e376c08,0x2748774c,0x34b0bcb5,0x391c0cb3,0x4ed8aa4a,0x5b9cca4f,0x682e6ff3,
            0x748f82ee,0x78a5636f,0x84c87814,0x8cc70208,0x90befffa,0xa4506ceb,0xbef9a3f7,0xc67178f2
        };

        public static byte[] ComputeHash(byte[] data)
        {
            // 初始化哈希值
            uint h0 = 0x6a09e667;
            uint h1 = 0xbb67ae85;
            uint h2 = 0x3c6ef372;
            uint h3 = 0xa54ff53a;
            uint h4 = 0x510e527f;
            uint h5 = 0x9b05688c;
            uint h6 = 0x1f83d9ab;
            uint h7 = 0x5be0cd19;

            // 填充数据
            int origLen = data.Length;
            ulong bitLen = (ulong)origLen * 8;
            int padLen = (56 - (origLen + 1) % 64 + 64) % 64;
            byte[] padded = new byte[origLen + 1 + padLen + 8];
            Buffer.BlockCopy(data, 0, padded, 0, origLen);
            padded[origLen] = 0x80;
            for (int i = 0; i < 8; i++)
                padded[padded.Length - 1 - i] = (byte)(bitLen >> (8 * i));

            // 处理每个 512-bit block
            for (int chunk = 0; chunk < padded.Length; chunk += 64)
            {
                uint[] w = new uint[64];
                for (int i = 0; i < 16; i++)
                    w[i] = (uint)(padded[chunk + 4 * i] << 24 |
                                  padded[chunk + 4 * i + 1] << 16 |
                                  padded[chunk + 4 * i + 2] << 8 |
                                  padded[chunk + 4 * i + 3]);

                for (int i = 16; i < 64; i++)
                {
                    uint s0 = RotateRight(w[i - 15], 7) ^ RotateRight(w[i - 15], 18) ^ (w[i - 15] >> 3);
                    uint s1 = RotateRight(w[i - 2], 17) ^ RotateRight(w[i - 2], 19) ^ (w[i - 2] >> 10);
                    w[i] = unchecked(w[i - 16] + s0 + w[i - 7] + s1);
                }

                uint a = h0, b = h1, c = h2, d = h3, e = h4, f = h5, g = h6, h = h7;

                for (int i = 0; i < 64; i++)
                {
                    uint S1 = RotateRight(e, 6) ^ RotateRight(e, 11) ^ RotateRight(e, 25);
                    uint ch = (e & f) ^ (~e & g);
                    uint temp1 = unchecked(h + S1 + ch + K[i] + w[i]);
                    uint S0 = RotateRight(a, 2) ^ RotateRight(a, 13) ^ RotateRight(a, 22);
                    uint maj = (a & b) ^ (a & c) ^ (b & c);
                    uint temp2 = unchecked(S0 + maj);

                    h = g; g = f; f = e; e = unchecked(d + temp1);
                    d = c; c = b; b = a; a = unchecked(temp1 + temp2);
                }

                h0 = unchecked(h0 + a);
                h1 = unchecked(h1 + b);
                h2 = unchecked(h2 + c);
                h3 = unchecked(h3 + d);
                h4 = unchecked(h4 + e);
                h5 = unchecked(h5 + f);
                h6 = unchecked(h6 + g);
                h7 = unchecked(h7 + h);
            }

            byte[] hash = new byte[32];
            Array.Copy(BitConverter.GetBytes(h0).Reverse().ToArray(), 0, hash, 0, 4);
            Array.Copy(BitConverter.GetBytes(h1).Reverse().ToArray(), 0, hash, 4, 4);
            Array.Copy(BitConverter.GetBytes(h2).Reverse().ToArray(), 0, hash, 8, 4);
            Array.Copy(BitConverter.GetBytes(h3).Reverse().ToArray(), 0, hash, 12, 4);
            Array.Copy(BitConverter.GetBytes(h4).Reverse().ToArray(), 0, hash, 16, 4);
            Array.Copy(BitConverter.GetBytes(h5).Reverse().ToArray(), 0, hash, 20, 4);
            Array.Copy(BitConverter.GetBytes(h6).Reverse().ToArray(), 0, hash, 24, 4);
            Array.Copy(BitConverter.GetBytes(h7).Reverse().ToArray(), 0, hash, 28, 4);
            return hash;
        }

        private static uint RotateRight(uint x, int n) => (x >> n) | (x << (32 - n));
    }