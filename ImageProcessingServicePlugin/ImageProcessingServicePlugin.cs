using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingServicePlugin
{
    public interface IIPSPlugin : IDisposable
    {
        string Name { get; }
        string Description { get; }
        string Process(string uri);
        string Process(string uri, Action<double> callback);
    }

    public static class TypeExt
    {
        public static bool IsPlugin(this Type t)
        {
            Type pType = typeof(IIPSPlugin);
            return
                t.IsClass &&
                !t.IsAbstract &&
                t.IsPublic &&
                !t.IsNested &&
                t.GetInterface(pType.FullName) != null;
        }
    }

    public class PluginManager
    {
        private const string dependencyDir = "_Dependency";
        private List<Type> plugins = new List<Type>();
        private string PluginPath;

        public PluginManager(string pluginPath)
        {
            PluginPath = pluginPath;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            foreach (var dir in Directory.GetDirectories(pluginPath))
            {
                string dirName = Path.GetFileName(dir);
                if (dirName == dependencyDir)
                    continue;
                string file = Path.Combine(dir, $"{dirName}.dll");
                AssemblyName an = AssemblyName.GetAssemblyName(file);
                Assembly assembly = Assembly.Load(an);
                var types = assembly?.GetTypes() ?? Enumerable.Empty<Type>();
                plugins.AddRange(types.Where(t => t.IsPlugin()));
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dll = args.Name.Substring(0, args.Name.IndexOf(','));
            System.Diagnostics.Debug.WriteLine($"Resolve: {dll} ({args.Name})");
            return Assembly.LoadFrom(Path.Combine(PluginPath, $"_Dependency\\{dll}.dll"));
        }

        public IReadOnlyList<Type> Plugins => plugins;

        public IIPSPlugin Create(string methodName)
        {
            try
            {
                return Activator.CreateInstance(plugins.SingleOrDefault(p => p.Name == methodName)) as IIPSPlugin;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"'{nameof(methodName)}' not exist.", e);
            }
        }
    }
}