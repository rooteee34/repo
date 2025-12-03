using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemManager.Modules
{
    /// <summary>
    /// Manages all modules in the system
    /// Ensures only enabled modules are loaded and running
    /// </summary>
    public class ModuleManager
    {
        private readonly Dictionary<string, IModule> _modules;
        private readonly Dictionary<string, bool> _moduleStates;

        public ModuleManager()
        {
            _modules = new Dictionary<string, IModule>();
            _moduleStates = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Register a module
        /// </summary>
        public void RegisterModule(IModule module)
        {
            if (!_modules.ContainsKey(module.ModuleName))
            {
                _modules.Add(module.ModuleName, module);
                _moduleStates.Add(module.ModuleName, false);
            }
        }

        /// <summary>
        /// Enable a module (loads and initializes only when enabled)
        /// </summary>
        public void EnableModule(string moduleName)
        {
            if (_modules.ContainsKey(moduleName) && !_moduleStates[moduleName])
            {
                var module = _modules[moduleName];
                module.IsEnabled = true;
                module.Initialize();
                module.Start();
                _moduleStates[moduleName] = true;
            }
        }

        /// <summary>
        /// Disable a module (completely unloads and disposes)
        /// </summary>
        public void DisableModule(string moduleName)
        {
            if (_modules.ContainsKey(moduleName) && _moduleStates[moduleName])
            {
                var module = _modules[moduleName];
                module.Stop();
                module.Dispose();
                module.IsEnabled = false;
                _moduleStates[moduleName] = false;
            }
        }

        /// <summary>
        /// Get a module by name
        /// </summary>
        public IModule GetModule(string moduleName)
        {
            return _modules.ContainsKey(moduleName) ? _modules[moduleName] : null;
        }

        /// <summary>
        /// Get all registered modules
        /// </summary>
        public IEnumerable<IModule> GetAllModules()
        {
            return _modules.Values;
        }

        /// <summary>
        /// Get all enabled modules
        /// </summary>
        public IEnumerable<IModule> GetEnabledModules()
        {
            return _modules.Values.Where(m => m.IsEnabled);
        }

        /// <summary>
        /// Check if module is enabled
        /// </summary>
        public bool IsModuleEnabled(string moduleName)
        {
            return _moduleStates.ContainsKey(moduleName) && _moduleStates[moduleName];
        }

        /// <summary>
        /// Dispose all modules
        /// </summary>
        public void DisposeAll()
        {
            foreach (var module in _modules.Values.Where(m => m.IsEnabled))
            {
                module.Stop();
                module.Dispose();
            }
            _modules.Clear();
            _moduleStates.Clear();
        }
    }
}
