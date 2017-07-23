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
        private List<Type> plugins = new List<Type>();

        public PluginManager(string pluginPath)
        {
            foreach (var file in Directory.GetFiles(pluginPath, "*.dll"))
            {
                AssemblyName an = AssemblyName.GetAssemblyName(file);
                Assembly assembly = Assembly.Load(an);
                var types = assembly?.GetTypes() ?? Enumerable.Empty<Type>();
                plugins.AddRange(types.Where(t => t.IsPlugin()));
            }
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