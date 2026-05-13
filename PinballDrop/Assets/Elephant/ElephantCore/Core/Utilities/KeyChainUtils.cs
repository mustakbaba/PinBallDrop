using System;
using System.Runtime.InteropServices;

namespace ElephantSDK
{
    public class KeyChainUtils
    {
#if UNITY_IOS
        private static string PtrToString(IntPtr ptr)
        {
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
        }
        
        public static bool KeyExists(string key)
        {
            return ElephantIOS.keyExistsInKeyChain(key);
        }
        
        public static string GetValue(string key)
        {
            var valuePtr = ElephantIOS.getValueForKey(key);
            return PtrToString(valuePtr);
        }
        
        public static void SaveValue(string key, string value)
        {
            ElephantIOS.saveValueForKey(key, value);
        }
#endif
    }
}