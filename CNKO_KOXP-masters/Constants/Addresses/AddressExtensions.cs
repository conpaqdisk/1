using KOXP.Win32;
using System.Text;
using static KOXP.Constants.Handle;

namespace KOXP.Constants.Addresses
{
    public class AddressExtensions : Win32Api
    {
        public static byte[] ReadByteArray(IntPtr Handle, int address, int length)
        {
            byte[] Buffer = new byte[length];
            ReadProcessMemory(Handle, new IntPtr(address), Buffer, length, 0);
            return Buffer;
        }

        public static int Read4Byte(IntPtr Handle, IntPtr Address)
        {
            byte[] Buffer = new byte[4];
            ReadProcessMemory(Handle, Address, Buffer, 4, 0);
            return BitConverter.ToInt32(Buffer, 0);
        }

        public static int Read4Byte(IntPtr Handle, long Address)
        {
            return Read4Byte(Handle, new IntPtr(Address));
        }

        public static int Read2Byte(IntPtr Handle, IntPtr Address)
        {
            byte[] Buffer = new byte[2];
            ReadProcessMemory(Handle, Address, Buffer, 2, 0);
            return BitConverter.ToInt32(Buffer, 0);
        }

        public static int Read2Byte(IntPtr Handle, long Address)
        {
            return Read2Byte(Handle, new IntPtr(Address));
        }

        public static short ReadByte(IntPtr Handle, IntPtr Address)
        {
            byte[] Buffer = new byte[2];
            ReadProcessMemory(Handle, Address, Buffer, 1, 0);
            return BitConverter.ToInt16(Buffer, 0);
        }

        public static float ReadFloat(IntPtr Handle, IntPtr Address)
        {
            byte[] Buffer = new byte[4];
            ReadProcessMemory(Handle, Address, Buffer, 4, 0);
            return BitConverter.ToSingle(Buffer, 0);
        }

        public static string ReadString(IntPtr Handle, IntPtr Address, int Size)
        {
            byte[] Buffer = new byte[Size];
            ReadProcessMemory(Handle, Address, Buffer, Size, 0);
            return Encoding.Default.GetString(Buffer);
        }

        public static string ReadString(IntPtr Handle, long Address, int Size)
        {
            return ReadString(Handle, new IntPtr(Address), Size);
        }

        public static byte[] ReadByteArray(int address, int length)
        {
            return ReadByteArray(GameProcessHandle, address, length);
        }

        public static int Read4Byte(IntPtr Address)
        {
            return Read4Byte(GameProcessHandle, Address);
        }

        public static int Read4Byte(long Address)
        {
            return Read4Byte(new IntPtr(Address));
        }

        public static int Read2Byte(IntPtr Address)
        {
            return Read4Byte(GameProcessHandle, Address);
        }

        public static int Read2Byte(long Address)
        {
            return Read4Byte(new IntPtr(Address));
        }

        public static short ReadByte(IntPtr Address)
        {
            return ReadByte(GameProcessHandle, Address);
        }

        public static short ReadByte(long Address)
        {
            return ReadByte(new IntPtr(Address));
        }

        public static float ReadFloat(IntPtr Address)
        {
            return ReadFloat(GameProcessHandle, Address);
        }

        public static float ReadFloat(long Address)
        {
            return ReadFloat(new IntPtr(Address));
        }

        public static string ReadString(IntPtr Address, int Size)
        {
            return ReadString(GameProcessHandle, Address, Size);
        }

        public static string ReadString(long Address, int Size)
        {
            return ReadString(new IntPtr(Address), Size);
        }
    }
}
