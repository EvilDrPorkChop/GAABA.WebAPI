using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GAABA.WebAPI
{
    /// <summary>
    /// This abstract class represents a code example.
    /// </summary>
    public abstract class ExampleBase
    {
        /// <summary>
        /// Returns a description about the code example.
        /// </summary>
        public abstract string Description
        {
            get;
        }
    }
}