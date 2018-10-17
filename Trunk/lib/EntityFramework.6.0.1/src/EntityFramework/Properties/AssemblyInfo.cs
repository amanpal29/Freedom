// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright("© Microsoft Corporation.  All rights reserved.")]
[assembly: AssemblyDefaultAlias("EntityFramework.dll")]
[assembly: AssemblyDescription("EntityFramework.dll")]
[assembly: AssemblyFileVersion("6.0.0.0")]
[assembly: AssemblyInformationalVersion("6.0.1")]
[assembly: AssemblyMetadata("Serviceable", "True")]
[assembly: AssemblyProduct("Microsoft Entity Framework")]
[assembly: AssemblyTitle("EntityFramework")]
[assembly: AssemblyVersion("6.0.0.0")]

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: SatelliteContractVersion("6.0.0.0")]

#if NET40

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    internal sealed class AssemblyMetadataAttribute : Attribute
    {
        public AssemblyMetadataAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}

#endif

