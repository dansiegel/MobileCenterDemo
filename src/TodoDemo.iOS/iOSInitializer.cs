using System;
using DryIoc;
using Prism.DryIoc;

namespace TodoDemo.iOS
{
    public class iOSInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainer container)
        {
            // Register Any Platform Specific Implementations that you cannot 
            // access from Shared Code
        }
    }
}
