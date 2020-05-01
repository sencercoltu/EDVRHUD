using System;
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

//#if UseOpenVR
            var path = Environment.CurrentDirectory + "\\openvr_api.dll";
            if (File.Exists(path)) File.Delete(path);
            File.WriteAllBytes(path, Properties.Resources.openvr_api);
//#endif //UseOpenVR

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            using (var code = new NotificationApp())
            {
                code.Run();
            }            
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
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
            }
            return ass;
        }
    }
}
