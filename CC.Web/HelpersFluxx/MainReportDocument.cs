using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.IO;
using System.Net;



namespace CC.Web.HelpersFluxx
{
    public static class MainReportDocument
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);


        static string userName;
        static string password;
        static string domainName;
        static string uri;

        static MainReportDocument()
        {
            //path
            uri = System.Configuration.ConfigurationManager.AppSettings["MainReportUri"];
            if (string.IsNullOrEmpty(uri))
            {
                throw new Exception("Missing path for main reports folder \"MainReportUri\"at config");
            }

            //credentials 
            userName = System.Configuration.ConfigurationManager.AppSettings["windows_UserName"];
            password = System.Configuration.ConfigurationManager.AppSettings["windows_Password"];
            domainName = System.Configuration.ConfigurationManager.AppSettings["windows_Domain"];


            var errors = new List<string>() {
                string.IsNullOrEmpty(userName) ? "windows_UserName" : null,
                string.IsNullOrEmpty(password) ? "windows_Password" : null,
                string.IsNullOrEmpty(domainName) ? "windows_Domain" : null

            }.Where(p => !string.IsNullOrEmpty(p))
            .ToList();

            if (errors.Any())
            {
                throw new Exception(string.Format("Missing credentials {0} for window user at config. ",
                    string.Join(", ", errors)));
            }

        }

        public static void SaveBytesToFile_NetworkConnection(string filename, byte[] bytesToWrite)
        {

            var credentials = new NetworkCredential(string.Format(@"{0}\{1}", domainName, userName), password);
            using (new NetworkConnection(uri, credentials))
            {

                if (filename != null && filename.Length > 0 && bytesToWrite != null)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(filename)))
                        Directory.CreateDirectory(Path.GetDirectoryName(filename));

                    FileStream file = System.IO.File.Create(filename);
                    file.Write(bytesToWrite, 0, bytesToWrite.Length);
                    file.Close();
                }
            }

        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static void SaveBytesToFile_Impersonation(string filename, byte[] bytesToWrite)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            const int LOGON32_LOGON_INTERACTIVE = 2;
            SafeTokenHandle safeTokenHandle;
            bool returnValue = false;
            try
            {

                returnValue = LogonUser(userName, domainName, password,
                    LOGON32_LOGON_INTERACTIVE,
                    LOGON32_PROVIDER_DEFAULT,
                    out safeTokenHandle);

                if (false == returnValue)
                {
                    int ret = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(ret);
                }

                //Impersonation, save file under windows user
                using (safeTokenHandle)
                {
                    using (WindowsIdentity newId = new WindowsIdentity(safeTokenHandle.DangerousGetHandle()))
                    {
                        using (WindowsImpersonationContext win_user = newId.Impersonate())
                        {
                            if (filename != null && filename.Length > 0 && bytesToWrite != null)
                            {
                                if (!Directory.Exists(Path.GetDirectoryName(filename)))
                                    Directory.CreateDirectory(Path.GetDirectoryName(filename));

                                FileStream file = System.IO.File.Create(filename);
                                file.Write(bytesToWrite, 0, bytesToWrite.Length);
                                file.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (returnValue ? " (Access to folder) "
                    : " (Create a windows user) "), ex.InnerException);

            }

        }
    }


    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }

}
