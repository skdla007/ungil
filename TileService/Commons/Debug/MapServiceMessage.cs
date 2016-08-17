using Commons.Debug.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Debug
{
    public class MapServiceMessage : BaseMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapServiceMessage"/> class.
        /// </summary>
        public MapServiceMessage()
        {
            this.InitializeDefaultMessage("MapService");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapServiceMessage"/> class.
        /// </summary>
        /// <param name="paramInfo">
        /// </param>
        public MapServiceMessage(params string[] paramInfo)
        {
            this.InitializeDefaultMessage("MapService", paramInfo);
        }
    }
}
