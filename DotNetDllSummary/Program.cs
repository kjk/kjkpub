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

        public void ProcessField(FieldDefinition field)
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

        public void ProcessTypeFields(TypeDefinition type)
        {
            if (!type.HasFields)
                return;
            foreach (FieldDefinition f in type.Fields)
            {
                ProcessField(f);
            }
        }

        public void ProcessEvent(EventDefinition e)
        {
            if (e.IsSpecialName)
                return;
            var s = " E: " + ShortenTypes(e.EventType.FullName) + " " + e.Name;
            System.Console.WriteLine(s);
        }

        public void ProcessTypeEvents(TypeDefinition type)
        {
            if (!type.HasEvents)
                return;
            foreach (EventDefinition e in type.Events)
            {
                ProcessEvent(e);
            }
        }

        public bool ProcessEnum(TypeDefinition type)
        {
            if (!type.IsEnum)
                return false;
            if (IsIgnoredNamespace(type.FullName))
                return true;
            var s = "E:" + type.FullName;
            if (type.BaseType != null)
            {
                s = s + " : " + ShortenTypes(type.BaseType.FullName);
            }
            Console.WriteLine(s);
            ProcessTypeFields(type);
            return true;
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

        public void ProcessMethod(MethodDefinition m)
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

        public void ProcessTypeMethods(TypeDefinition type)
        {
            if (!type.HasMethods)
                return;
            foreach (var m in type.Methods)
            {
                ProcessMethod(m);
            }
        }

        public string GetPropertyName(PropertyDefinition p)
        {
            // TODO: more detailed?
            string s = ShortenTypes(p.PropertyType.FullName);
            s += " " + p.Name;
            return s;
        }

        public void ProcessTypeProperties(TypeDefinition type)
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

        public bool ProcessClass(TypeDefinition type)
        {
            if (!type.IsClass)
                return false;
            if (IsIgnoredNamespace(type.FullName))
                return true;
            Console.WriteLine("C:" + GetTypeName(type));
            ProcessTypeFields(type);
            ProcessTypeProperties(type);
            ProcessTypeEvents(type);
            ProcessTypeMethods(type);
            return true;
        }

        public bool ProcessValueType(TypeDefinition type)
        {
            if (!type.IsValueType)
                return false;
            if (IsIgnoredNamespace(type.FullName))
                return true;
            Console.WriteLine("S:" + GetTypeName(type));
            ProcessTypeFields(type);
            ProcessTypeProperties(type);
            ProcessTypeMethods(type);
            return true;
        }

        public bool ProcessInterface(TypeDefinition type)
        {
            if (!type.IsInterface)
                return false;
            if (IsIgnoredNamespace(type.FullName))
                return true;
            Debug.Assert(type.BaseType == null);
            Debug.Assert(!type.HasFields);
            Console.WriteLine("I:" + type.FullName);
            ProcessTypeProperties(type);
            ProcessTypeMethods(type);
            return true;
        }

        public void ProcessType(TypeDefinition type)
        {
            if (!type.IsPublic)
                return;
            if (ProcessEnum(type))
                return;
            if (ProcessValueType(type))
                return;
            if (ProcessClass(type))
                return;
            if (ProcessInterface(type))
                return;
            Console.WriteLine("UNKOWN TYPE:" + type.FullName);
            ProcessTypeMethods(type);
        }

        public void ProcessDll(string path)
        {
            System.Console.WriteLine("DLL: " + path);
            try
            {
                ModuleDefinition module = ModuleDefinition.ReadModule(path);
                foreach (TypeDefinition type in module.Types)
                {
                    ProcessType(type);
                }
            }
            catch (BadImageFormatException)
            {
                return;
            }
        }

        public void DoDir(string dir)
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

    class GenerateHtml
    {
        bool ProcessEnum(TypeDefinition type)
        {
            if (!type.IsEnum)
                return false;
            return true;
        }

        bool ProcessValueType(TypeDefinition type)
        {
            if (!type.IsValueType)
                return false;
            return true;
        }

        bool ProcessClass(TypeDefinition type)
        {
            if (!type.IsClass)
                return false;
            return true;
        }

        bool ProcessInterface(TypeDefinition type)
        {
            if (!type.IsInterface)
                return false;
            return true;
        }

        Dictionary<string, bool> enums = new Dictionary<string, bool>(1024 * 2);
        Dictionary<string, bool> valueTypes = new Dictionary<string, bool>(512);
        Dictionary<string, bool> classes = new Dictionary<string, bool>(1024 * 10);
        Dictionary<string, bool> interfaces = new Dictionary<string, bool>(1024);

        void RegisterType(TypeDefinition type)
        {
            if (!type.IsPublic)
                return;
            if (type.IsEnum)
                enums[type.FullName] = true;
            else if (type.IsValueType)
                valueTypes[type.FullName] = true;
            else if (type.IsClass)
                classes[type.FullName] = true;
            else if (type.IsInterface)
                interfaces[type.FullName] = true;
        }

        public void DumpCounts()
        {
            int n = enums.Count;
            Console.WriteLine("Enums:       " + n.ToString());
            n = valueTypes.Count;
            Console.WriteLine("Value types: " + n.ToString());
            n = classes.Count;
            Console.WriteLine("Classes    : " + n.ToString());
            n = interfaces.Count;
            Console.WriteLine("Interfaces : " + n.ToString());
        }

        public void ProcessType(TypeDefinition type)
        {
            RegisterType(type);
            if (!type.IsPublic)
                return;
            if (ProcessEnum(type))
                return;
            if (ProcessValueType(type))
                return;
            if (ProcessClass(type))
                return;
            if (ProcessInterface(type))
                return;
            Debug.Assert(false);
        }

        public void ProcessDll(string path)
        {
            System.Console.WriteLine("DLL: " + path);
            try
            {
                ModuleDefinition module = ModuleDefinition.ReadModule(path);
                foreach (TypeDefinition type in module.Types)
                {
                    ProcessType(type);
                }
            }
            catch (BadImageFormatException)
            {
                return;
            }
        }

        public void DoDir(string dir)
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
            var o = new GenerateHtml();
            o.DoDir(@"c:\Windows\Microsoft.NET\Framework\v4.0.30319");
            o.DoDir(@"c:\Windows\Microsoft.NET\Framework\v4.0.30319\WPF");
            o.DumpCounts();
        }
    }
}
