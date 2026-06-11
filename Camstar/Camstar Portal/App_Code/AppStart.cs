// Copyright Siemens 2023  
using System;
using System.Web.Hosting;
using Camstar.WebPortal.WebFramework;

namespace Camstar.Portal
{
    /// <summary>
    /// Summary description for AppInit
    /// </summary>
    public static class AppStart
    {
        public static void AppInitialize()
        {
            HostingEnvironment.RegisterVirtualPathProvider(new PathProvider());
        }
    }
}
