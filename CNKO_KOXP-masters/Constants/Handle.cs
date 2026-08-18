using KOXP.Win32;

namespace KOXP.Constants
{
    public class Handle : Win32Api
    {
        public static int GamePID { get; set; }
        public static IntPtr GameProcessHandle { get; set; }

        public static bool AttachProccess(string WindowsName)
        {
            GameProcessHandle = GetHandle(WindowsName);

            if (GameProcessHandle == IntPtr.Zero)
            {
                _ = MessageBox.Show("Handle error.");
            }
            else
                return true;

            return false;
        }

        public static IntPtr GetHandle(string title)
        {
            GetWindowThreadProcessId(FindWindow(null, title), out int pid);
            GamePID = pid; // For close the client.
            return OpenProcess(ProcessAccessFlags.All, false, pid);
        }
    }
}
