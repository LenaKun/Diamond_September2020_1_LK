using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace CC.Web.HelpersFluxx
{
    
    public class CommandLineApp
    {
        string exe_full_name { get; set;}
        string args { get; set; }

        public bool is_fluxx_api { get; set; }
        
        public CommandLineApp(string is_fluxx_api, string exe_full_name, string args)
        {
            //is_fluxx_api
            var is_fluxx_api_ = System.Configuration.ConfigurationManager.AppSettings[is_fluxx_api];

            if (string.IsNullOrEmpty(is_fluxx_api_))
            {
                throw new Exception("Missing \"" + is_fluxx_api + "\" for CommandLineApp at config");
            }
            bool is_fluxx_api_temp;
            var result = bool.TryParse(is_fluxx_api_, out is_fluxx_api_temp);

            if (!result)
            {
                throw new Exception("Field for CommandLineApp must bool type  \"" + is_fluxx_api + "\"  at config");
            }
            this.is_fluxx_api = is_fluxx_api_temp;


            //exe_full_name
            this.exe_full_name = System.Configuration.ConfigurationManager.AppSettings[exe_full_name];

            if (string.IsNullOrEmpty(this.exe_full_name))
            {
                throw new Exception("Missing \"" + exe_full_name+ "\" for CommandLineApp at config");
            }


            //args
            this.args = System.Configuration.ConfigurationManager.AppSettings[args];

            if (string.IsNullOrEmpty(this.args))
            {
                throw new Exception("Missing  \"" + args + "\" for  CommandLineApp at config");
            }

           
        }

        public void Launch(int? id = null)
        {
            lock (this)
            {               
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = exe_full_name;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.Arguments = args;
                if (id != null)
                {
                    startInfo.Arguments = args + " id=" + id;
                }
                
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
        }
    }
}