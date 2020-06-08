using System;
using System.Resources;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace EDVRHUD
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var path = Environment.CurrentDirectory + "\\openvr_api.dll";
            if (!File.Exists(path)) 
                File.WriteAllBytes(path, Properties.Resources.openvr_api);

            path = Environment.CurrentDirectory + "\\LICENSES.txt";
            if (!File.Exists(path)) 
                File.WriteAllText(path, Properties.Resources.LICENSES);

            path = Environment.CurrentDirectory + "\\Settings.json";
            if (!File.Exists(path)) 
                File.WriteAllBytes(path, Properties.Resources.Settings);

            path = Environment.CurrentDirectory + "\\Panels.json";
            if (!File.Exists(path))
                File.WriteAllBytes(path, Properties.Resources.Panels);

            using (var code = new NotificationApp())
            {
                code.Run();
            }            
        }

        

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = args.Name.Split(',').First();
            Assembly ass = Assembly.GetExecutingAssembly();
            switch (assemblyName)
            {
                case "SharpDX":
                    return Assembly.Load(Properties.Resources.SharpDX);
                case "SharpDX.Direct3D11":
                    return Assembly.Load(Properties.Resources.SharpDX_Direct3D11);
                case "SharpDX.DXGI":
                    return Assembly.Load(Properties.Resources.SharpDX_DXGI);
                case "SharpDX.DirectInput":
                    return Assembly.Load(Properties.Resources.SharpDX_DirectInput);
                case "LiteDB":
                    return Assembly.Load(Properties.Resources.LiteDB);
                case "WindowsInput":
                    return Assembly.Load(Properties.Resources.WindowsInput);
                    
            }
            return ass;
        }
    }
}
