using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetDllSummary
{
    class GenerateSummary
    {
        public void DumpField(FieldDefinition field)
        {
            if (!field.IsPublic)
                return;
            if (field.IsSpecialName || field.IsRuntimeSpecialName)
                return;
            var s = " F: " + field.FullName;
            System.Console.WriteLine(s);
        }

        public void DumpFields(TypeDefinition type)
        {
            if (!type.HasFields)
                return;
            foreach (FieldDefinition f in type.Fields)
            {
                DumpField(f);
            }
        }

        public void DumpEnum(TypeDefinition type)
        {
            var s = "E:" + type.FullName;
            if (type.BaseType != null)
            {
                s = s + " : " + type.BaseType.FullName;
            }
            Console.WriteLine(s);
            DumpFields(type);
        }

        public void DumpTypeMethods(TypeDefinition type)
        {
            if (!type.HasMethods)
                return;
            foreach (var m in type.Methods)
            {
                if (!m.IsPublic)
                    continue;
                Console.WriteLine(" M:" + m.FullName);
            }
        }

        public void DumpClass(TypeDefinition type)
        {
            Console.WriteLine("C:" + type.FullName);
            DumpTypeMethods(type);
        }

        public void DumpValueType(TypeDefinition type)
        {
            Console.WriteLine("VT:" + type.FullName);
            DumpTypeMethods(type);
        }

        public void DumpInterface(TypeDefinition type)
        {
            Console.WriteLine("I:" + type.FullName);
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
            string dir = @"c:\Windows\Microsoft.NET\Framework\v4.0.30319";
            var gs = new GenerateSummary();
            gs.SummaryForDir(dir);
        }
    }
}
