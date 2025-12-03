using System;
using System.Windows.Forms;

namespace SystemManager.Modules
{
    /// <summary>
    /// Interface for all modules in the system
    /// Ensures 100% modular architecture - unused modules are not loaded
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Module name displayed in UI
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Module description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Whether the module is currently enabled
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Initialize the module (only called when enabled)
        /// </summary>
        void Initialize();

        /// <summary>
        /// Start the module's operations
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the module's operations
        /// </summary>
        void Stop();

        /// <summary>
        /// Dispose of module resources
        /// </summary>
        void Dispose();

        /// <summary>
        /// Get the UI control for this module
        /// </summary>
        Control GetControl();

        /// <summary>
        /// Module configuration changed event
        /// </summary>
        event EventHandler ConfigurationChanged;
    }
}
