﻿using System;
using System.ComponentModel.Composition.Hosting;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ShieldVSExtension.Common.Services
{
    public static class Services
    {
        public static IServiceProvider UnitTestServiceProvider { get; set; }

        static Ret GetGlobalService<T, Ret>(IServiceProvider provider = null) where T : class where Ret : class
        {
            Ret ret = null;
            if (provider != null)
            {
                ret = provider.GetService(typeof(T)) as Ret;
            }

            if (ret != null)
            {
                return ret;
            }

            if (UnitTestServiceProvider != null)
            {
                return UnitTestServiceProvider.GetService(typeof(T)) as Ret;
            }

            return Package.GetGlobalService(typeof(T)) as Ret;
        }

        public static IComponentModel ComponentModel => GetGlobalService<SComponentModel, IComponentModel>();
        public static ExportProvider DefaultExportProvider => ComponentModel.DefaultExportProvider;

        public static DTE Dte => GetGlobalService<DTE, DTE>();

        public static DTE2 Dte2 => Dte as DTE2;

        public static IVsSolution GetSolution(this IServiceProvider provider) => GetGlobalService<SVsSolution, IVsSolution>(provider);

        public static IVsSolution GetSolution() => GetGlobalService<SVsSolution, IVsSolution>();
    }
}
