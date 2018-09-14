// <copyright file="Application.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace XmlAbstraction
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    /// A hacked together class to avoid referencing System.Windows.Forms.
    /// </summary>
    // class to hack away to dependency on "System.Windows.Forms".
    public static class Application
    {
        private static string startupPath;
        private static HandleRef nullHandleRef = new HandleRef(null, IntPtr.Zero);

        /// <summary>
        /// Gets the application startup path.
        /// </summary>
        public static string StartupPath
        {
            get
            {
                if (startupPath == null)
                {
                    var sb = UnsafeNativeMethods.GetModuleFileNameLongPath(nullHandleRef);
                    startupPath = Path.GetDirectoryName(sb.ToString());
                }

                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, startupPath).Demand();
                return startupPath;
            }
        }

        private static class UnsafeNativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [ResourceExposure(ResourceScope.None)]
            public static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);

            public static StringBuilder GetModuleFileNameLongPath(HandleRef hModule)
            {
                var buffer = new StringBuilder(260);
                var noOfTimes = 1;
                var length = 0;

                // Iterating by allocating chunk of memory each time we find the length is not sufficient.
                // Performance should not be an issue for current MAX_PATH length due to this change.
                while (((length = GetModuleFileName(hModule, buffer, buffer.Capacity)) == buffer.Capacity)
                    && Marshal.GetLastWin32Error() == 122
                    && buffer.Capacity < short.MaxValue)
                {
                    noOfTimes += 2; // Increasing buffer size by 520 in each iteration.
                    var capacity = noOfTimes * 260 < short.MaxValue ? noOfTimes * 260 : short.MaxValue;
                    buffer.EnsureCapacity(capacity);
                }

                buffer.Length = length;
                return buffer;
            }
        }
    }
}
