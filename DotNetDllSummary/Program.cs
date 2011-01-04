using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetDllSummary
{
    class GenerateSummary
    {
        public string[] namespacesToIgnore = new string[] {
            "Microsoft.Build",
            "Microsoft.Aspnet",
            "Microsoft.JScript",
            "Microsoft.VisualBasic",
            "Microsoft.VisualC",
            "Microsoft.Vsa"
        };

        public bool IsIgnoredNamespace(string s)
        {
            foreach (var ns in namespacesToIgnore)
            {
                if (s.StartsWith(ns))
                    return true;
            }
            return false;
        }

        public string ShortenTypes(string s)
        {
            var rd = new string[] {
                "System.Void", "void",
                "System.Object", "object",
                "System.Char", "char",
                "System.String", "string",
                "System.Byte", "byte",
                "System.Guid", "guid",
                "System.Int16", "i16",
                "System.Int32", "i32",
                "System.Int64", "i64",
                "System.UInt16", "u16",
                "System.UInt32", "u32",
                "System.UInt64", "u64",
                "System.Boolean", "bool",
                "System.Double", "double",
                "System.Single", "float",
                "System.Decimal", "decimal",
                "System.DateTime", "datetime",
                "System.Array", "array",
                "System.Type", "type"
            };
            for (int i = 0; i < rd.Length / 2; i++)
            {
                s = s.Replace(rd[i*2], rd[i*2 + 1]);
            }
            s = s.Replace(",", ", ");
            return s;
        }

        public void DumpField(FieldDefinition field)
        {
            if (!field.IsPublic)
                return;
            if (field.IsRuntimeSpecialName)
                Debug.Assert(field.IsSpecialName);
            if (field.IsSpecialName)
                return;
            var s = " F: " + ShortenTypes(field.FieldType.FullName) + " " + field.Name;
            System.Console.WriteLine(s);
        }

        public void DumpTypeFields(TypeDefinition type)
        {
            if (!type.HasFields)
                return;
            foreach (FieldDefinition f in type.Fields)
            {
                DumpField(f);
            }
        }

        public void DumpEvent(EventDefinition e)
        {
            if (e.IsSpecialName)
                return;
            var s = " E: " + ShortenTypes(e.EventType.FullName) + " " + e.Name;
            System.Console.WriteLine(s);
        }

        public void DumpTypeEvents(TypeDefinition type)
        {
            if (!type.HasEvents)
                return;
            foreach (EventDefinition e in type.Events)
            {
                DumpEvent(e);
            }
        }

        public void DumpEnum(TypeDefinition type)
        {
            if (IsIgnoredNamespace(type.FullName))
                return;
            var s = "E:" + type.FullName;
            if (type.BaseType != null)
            {
                s = s + " : " + ShortenTypes(type.BaseType.FullName);
            }
            Console.WriteLine(s);
            DumpTypeFields(type);
        }

        public string GetMethodName(MethodDefinition m)
        {
            string s = "";
            if (m.IsStatic)
                s += "static ";
            if (m.IsFinal)
                s += "final ";
            if (m.IsAbstract)
                s += "abstract ";
            if (m.IsVirtual)
                s += "virtual ";
            s += ShortenTypes(m.ReturnType.FullName) + " ";
            s += m.Name + "(";
            if (m.HasParameters)
            {
                bool first = true;
                foreach (var p in m.Parameters)
                {
                    if (first)
                        first = false;
                    else
                        s += ", ";
                    s += ShortenTypes(p.ParameterType.FullName) + " " + p.Name;
                }
            }
            return s + ")";
        }

        public void DumpMethod(MethodDefinition m)
        {
            if (m.IsPrivate)
                return;
            if (m.IsRuntimeSpecialName)
                Debug.Assert(m.IsSpecialName);
            if (m.IsSpecialName)
                return;
#if NOT_DISABLED
            if (m.Name.StartsWith("add_") || m.Name.StartsWith("remove_"))
            {
                Debug.Assert(false);
                continue;
            }
#endif
            Console.WriteLine(" M:" + GetMethodName(m));
        }

        public void DumpTypeMethods(TypeDefinition type)
        {
            if (!type.HasMethods)
                return;
            foreach (var m in type.Methods)
            {
                DumpMethod(m);
            }
        }

        public string GetPropertyName(PropertyDefinition p)
        {
            // TODO: more detailed?
            string s = ShortenTypes(p.PropertyType.FullName);
            s += " " + p.Name;
            return s;
        }

        public void DumpTypeProperties(TypeDefinition type)
        {
            if (!type.HasProperties)
                return;
            foreach (PropertyDefinition p in type.Properties)
            {
                if (p.IsRuntimeSpecialName)
                    Debug.Assert(p.IsSpecialName);
                if (p.IsSpecialName)
                    continue;
                Console.WriteLine(" P:" + GetPropertyName(p));
            }
        }

        public string GetTypeName(TypeDefinition type)
        {
            var s = type.FullName;
            TypeDefinition t = type;
            while (t.BaseType != null)
            {
                s = s + " : " + ShortenTypes(t.BaseType.FullName);
                TypeReference tr = t.BaseType;
                t = tr.Resolve();
            }
            return s;
        }

        public void DumpClass(TypeDefinition type)
        {
            if (IsIgnoredNamespace(type.FullName))
                return;
            Console.WriteLine("C:" + GetTypeName(type));
            DumpTypeFields(type);
            DumpTypeProperties(type);
            DumpTypeEvents(type);
            DumpTypeMethods(type);
        }

        public void DumpValueType(TypeDefinition type)
        {
            if (IsIgnoredNamespace(type.FullName))
                return;
            Console.WriteLine("S:" + GetTypeName(type));
            DumpTypeFields(type);
            DumpTypeProperties(type);
            DumpTypeMethods(type);
        }

        public void DumpInterface(TypeDefinition type)
        {
            if (IsIgnoredNamespace(type.FullName))
                return;
            Debug.Assert(type.BaseType == null);
            Debug.Assert(!type.HasFields);
            Console.WriteLine("I:" + type.FullName);
            DumpTypeProperties(type);
            DumpTypeMethods(type);
        }

        public void DumpType(TypeDefinition type)
        {
            if (!type.IsPublic)
                return;
            if (type.IsEnum)
            {
                DumpEnum(type);
                return;
            }
            if (type.IsValueType)
            {
                DumpValueType(type);
                return;
            }
            if (type.IsClass)
            {
                DumpClass(type);
                return;
            }
            if (type.IsInterface)
            {
                DumpInterface(type);
                return;
            }
            Console.WriteLine("UNKOWN TYPE:" + type.FullName);
            DumpTypeMethods(type);
        }

        public void ProcessDll(string path)
        {
            System.Console.WriteLine("DLL: " + path);
            try
            {
                ModuleDefinition module = ModuleDefinition.ReadModule(path);
                foreach (TypeDefinition type in module.Types)
                {
                    DumpType(type);
                }
            }
            catch (BadImageFormatException)
            {
                return;
            }
        }

        public void SummaryForDir(string dir)
        {
            var di = new DirectoryInfo(dir);
            foreach (var fi in di.EnumerateFiles())
            {
                if (fi.Name.EndsWith(".dll"))
                {
                    ProcessDll(Path.Combine(dir, fi.Name));
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var gs = new GenerateSummary();
            gs.SummaryForDir(@"c:\Windows\Microsoft.NET\Framework\v4.0.30319");
            gs.SummaryForDir(@"c:\Windows\Microsoft.NET\Framework\v4.0.30319\WPF");
        }
    }
}
