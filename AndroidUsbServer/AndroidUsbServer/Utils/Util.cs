using System;

namespace AndroidUsbServer.Utils
{
    public static class Util
    {
        public static string ErrorString(Exception ex) => ex.GetType().FullName + ": " + ex.Message + "\nInner: " + ex.InnerException?.Message + "\nStack: " + ex.StackTrace + "\n";
    }
}
