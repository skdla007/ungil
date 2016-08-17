using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    /// <summary>
    /// The run type.
    /// </summary>
    public enum RunType
    {
        /// <summary>
        /// The none.
        /// </summary>
        None,

        /// <summary>
        /// The console service.
        /// </summary>
        ConsoleService,

        /// <summary>
        /// The window service.
        /// </summary>
        WindowService
    }

    /// <summary>
    /// The service type info.
    /// </summary>
    public class ServiceTypeInfo
    {
        private static RunType appType;

        static ServiceTypeInfo()
        {
            appType = RunType.None;
        }

        /// <summary>
        /// The set type info.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        public static void SetTypeInfo(RunType type)
        {
            appType = type;
        }

        /// <summary>
        /// The get type info.
        /// </summary>
        /// <returns>
        /// The <see cref="RunType"/>.
        /// </returns>
        public static RunType GetTypeInfo()
        {
            return appType;
        }
    }
}
