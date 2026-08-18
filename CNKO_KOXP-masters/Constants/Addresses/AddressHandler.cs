using System.Text;
using static KOXP.Constants.Address;
using static KOXP.Core.Helper;
using static KOXP.Core.Processor.CharFunctions;

namespace KOXP.Constants.Addresses
{
    public class AddressHandler : Handle
    {
        public static void WriteFloat(IntPtr Handle, IntPtr Address, float Value)
        {
            WriteProcessMemory(Handle, Address, BitConverter.GetBytes(Value), 4, 0);
        }

        public static void Write4Byte(IntPtr Handle, IntPtr Address, int Value)
        {
            WriteProcessMemory(Handle, Address, BitConverter.GetBytes(Value), 4, 0);
        }

        public static void WriteFloat(long Address, float Value)
        {
            WriteFloat(GameProcessHandle, new IntPtr(Address), Value);
        }

        public static void Write4Byte(long Address, int Value)
        {
            Write4Byte(GameProcessHandle, new IntPtr(Address), Value);
        }

        public static void WriteString(IntPtr Address, string Value)
        {
            byte[] data = Encoding.Default.GetBytes(Value + "\0");
            IntPtr Zero = IntPtr.Zero;
            WriteProcessMemory(GameProcessHandle, Address, data, data.Length, (uint)Zero);
        }

        public static void ExecuteRemoteCode(string Code)
        {
            IntPtr CodePtr = VirtualAllocEx(GameProcessHandle, IntPtr.Zero, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            byte[] CodeByte = StringToByte(Code);

            WriteProcessMemory(GameProcessHandle, CodePtr, CodeByte, CodeByte.Length, 0);
            IntPtr Thread = CreateRemoteThread(GameProcessHandle, IntPtr.Zero, 0, CodePtr, IntPtr.Zero, 0, IntPtr.Zero);
            if (Thread != IntPtr.Zero)
            {
                WaitForSingleObject(Thread, uint.MaxValue);
            }

            CloseHandle(Thread);
            VirtualFreeEx(GameProcessHandle, CodePtr, 0, MEM_RELEASE);
        }

        public static void SendPacket(byte[] Packet, int ExecutionAfterWait = 0)
        {
            if (DC())
                return;

            IntPtr PacketPtr = VirtualAllocEx(GameProcessHandle, IntPtr.Zero, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

            WriteProcessMemory(GameProcessHandle, PacketPtr, Packet, Packet.Length, 0);
            ExecuteRemoteCode("608B0D" + AlignDWORD(KO_PTR_PKT) + "68" + AlignDWORD(Packet.Length) + "68" + AlignDWORD(PacketPtr) + "BF" + AlignDWORD(KO_PTR_SND) + "FFD7C605" + AlignDWORD(KO_PTR_PKT + 0xC5) + "0061C3");
            VirtualFreeEx(GameProcessHandle, PacketPtr, 0, MEM_RELEASE);
        }

        public static void SendPacket(string Packet, int ExecutionAfterWait = 0)
        {
            SendPacket(StringToByte(Packet), ExecutionAfterWait);
        }
    }
}
