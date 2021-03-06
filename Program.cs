// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage : resgen <pathToResxFile.resx> [<resourcesNamespace>] [<classFullName>] [<outCsFilePath>]");
                Console.WriteLine("Example : resgen StringResources.resx System.Fabric.Strings.StringResources System.Fabric.Strings.StringResources StringResources.cs");
                Environment.Exit(1);
            }

            // setup variables.
            string resxPath = args[0];
            string folder = Path.GetDirectoryName(args[0]);
            string resourcesNamespace = (args.Length > 1) ? args[1] : Path.GetFileName(folder);
            string classFullName = (args.Length > 2) ? args[2] : Path.GetFileNameWithoutExtension(resxPath);
            string outPath = (args.Length > 3) ? args[3] : Path.Combine(folder, classFullName + ".cs");

            Console.WriteLine("Resource Namespace : {0}, ClassFullName : {1}, CsFilePath : {2}", resourcesNamespace, classFullName, outPath);

            // generate code.
            string sourceCode = GetStronglyTypeCsFileForResx(resxPath, resourcesNamespace, classFullName);

            // write C# file.
            Console.WriteLine("ResGen for " + outPath);
            File.WriteAllText(outPath, sourceCode);
        }

        private static string GetStronglyTypeCsFileForResx(string xmlPath, string resourcesNamespace, string classFullName)
        {
            // Example
            //
            // classFullName = Full.Name.Of.The.ClassFoo
            // shortClassName = ClassFoo
            // namespaceName = Full.Name.Of.The

            string shortClassName = classFullName;
            string namespaceName = null;
            int lastIndexOfDot = classFullName.LastIndexOf('.');
            if (lastIndexOfDot != -1)
            {
                namespaceName = classFullName.Substring(0, lastIndexOfDot);
                shortClassName = classFullName.Substring(lastIndexOfDot + 1);
            }

            var entries = new StringBuilder();
            XElement root = XElement.Parse(File.ReadAllText(xmlPath));
            foreach (var data in root.Elements("data"))
            {
                string value = data.Value.Replace("\n", "\n    ///");
                string name = data.Attribute("name").Value.Replace(' ', '_');
                entries.AppendFormat(ENTRY, name, value);
            }

            string bodyCode = string.Format(BODY, shortClassName, resourcesNamespace, entries.ToString());
            if (namespaceName != null)
            {
                bodyCode = string.Format(NAMESPACE, namespaceName, bodyCode);
            }

            string resultCode = string.Format(BANNER, bodyCode).Replace("\r\n?|\n", "\r\n");
            return resultCode;
        }

        private static readonly string BANNER = @"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a dotnet run from src\ResGen folder.
//     To add or remove a member, edit your .resx file then rerun src\ResGen.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

{0}
";

        private static readonly string NAMESPACE = @"
namespace {0} {{
{1}
}}
";
        private static readonly string BODY = @"
using System;
using System.Reflection;

/// <summary>
///   A strongly-typed resource class, for looking up localized strings, etc.
/// </summary>
[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""System.Resources.Tools.StronglyTypedResourceBuilder"", ""4.0.0.0"")]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]

internal class {0} {{

    private static global::System.Resources.ResourceManager resourceMan;

    private static global::System.Globalization.CultureInfo resourceCulture;

    [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute(""Microsoft.Performance"", ""CA1811:AvoidUncalledPrivateCode"")]
    internal {0}() {{
    }}

    /// <summary>
    ///   Returns the cached ResourceManager instance used by this class.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
    internal static global::System.Resources.ResourceManager ResourceManager {{
        get {{
            if (object.ReferenceEquals(resourceMan, null)) {{
                global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager(""{1}"", typeof({0}).Assembly);
                resourceMan = temp;
            }}
            return resourceMan;
        }}
    }}

    /// <summary>
    ///   Overrides the current threads CurrentUICulture property for all
    ///   resource lookups using this strongly typed resource class.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
    internal static global::System.Globalization.CultureInfo Culture {{
        get {{
            return resourceCulture;
        }}
        set {{
            resourceCulture = value;
        }}
    }}
    {2}
}}
";

    private static readonly string ENTRY = @"

    /// <summary>
    ///   Looks up a localized string similar to {1}
    /// </summary>
    internal static string {0} {{
        get {{
            return ResourceManager.GetString(""{0}"", resourceCulture);
        }}
    }}
";

    }
}