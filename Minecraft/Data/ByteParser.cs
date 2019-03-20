using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Minecraft.Data {

    public static class ByteParser {

        private delegate Object Converter (byte[] B);
        private static Dictionary<Type, Converter> Handlers = new Dictionary<Type, Converter>() {

            { typeof(short), B => BitConverter.ToInt16(B, 0) },
            { typeof(int), B => BitConverter.ToInt32(B, 0) },
            { typeof(long), B => BitConverter.ToInt64(B, 0) },
            { typeof(UInt16), B => BitConverter.ToUInt16(B, 0) },
            { typeof(UInt32), B => BitConverter.ToUInt32(B, 0) },
            { typeof(UInt64), B => BitConverter.ToUInt64(B, 0) },
            { typeof(float), B => BitConverter.ToSingle(B, 0) },
            { typeof(double), B => BitConverter.ToDouble(B, 0) },
            { typeof(bool), B => BitConverter.ToBoolean(B, 0) },
            { typeof(char), B => Convert.ToChar(B[0]) },
            { typeof(string), B => UTF8Encoding.UTF8.GetString(B) },
            { typeof(byte), B => B[0] }
        };

        public static Dictionary<Type, int> Size = new Dictionary<Type, int>() {

            { typeof(short), sizeof(short) },
            { typeof(int), sizeof(int) },
            { typeof(long), sizeof(long) },
            { typeof(UInt16), sizeof(UInt16) },
            { typeof(UInt32), sizeof(UInt32) },
            { typeof(UInt64), sizeof(UInt64) },
            { typeof(float), sizeof(float) },
            { typeof(double), sizeof(double) },
            { typeof(bool), sizeof(bool) },
            { typeof(char), 1 },
            { typeof(byte), 1 }
        };

        public static byte[] GetBytes(Stream S, uint count) {

            byte[] buffer = new byte[count];
            S.Read(buffer, 0, Convert.ToInt32(count));
            return buffer;
        }

        public static T ConvertBytes<T>(byte[] B) {

            if (Size.ContainsKey(typeof(T)) && B.Length != Size[typeof(T)])
                throw new ArgumentException("Array length doesn't match the type");

            return (T)Handlers[typeof(T)](B);
        }
    }
}
