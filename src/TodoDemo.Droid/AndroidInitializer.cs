using System;
using DryIoc;
using Prism.DryIoc;

namespace TodoDemo.Droid
{
    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainer container)
        {
            // Register Any Platform Specific Implementations that you cannot 
            // access from Shared Code
        }
    }
}
