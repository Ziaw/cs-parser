using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpParser;
using Nemerle.Core;
using Nemerle.Peg;

namespace CsParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = args.Length > 0 ? args[0] : @"../../Program.cs";
            
            var preParser = new PreParser(); // preprocessor parser

            SourceSnapshot src; // parsed source
            using (var reader = new StreamReader(fileName))
            {
                src = new SourceSnapshot(reader.ReadToEnd()); 
            }

            var preAst = preParser.Parse(src); 
            
            var preprocessed = Preprocessor.Run(preAst.Value, new string[0]); // transform AST with preprocessor

            var parser = new Parser(); // C# parser

            var parsingResult = parser.Parse(preprocessed.Source);
            
            var csAST = parsingResult.Value; 

            Console.WriteLine("Usings: ");
            foreach (var usingDirective in csAST.UsingDirectives)
            {
                var ns = usingDirective as UsingDirective.Namespace;
                if (ns != null)
                    Console.WriteLine(ns.name);
                var alias = usingDirective as UsingDirective.Alias;
                if (alias != null)
                    Console.WriteLine("{0} = {1}", alias.name, alias.alias);
            }

            Console.WriteLine("\nClasses: ");
            ShowMembers(csAST.Members);
        }

        private static void ShowMembers(IEnumerable<NamespaceNode> namespaceNodes)
        {
            foreach (var namespaceNode in namespaceNodes)
            {
                var ns = namespaceNode as NamespaceNode.Namespace;
                if (ns != null)
                    ShowMembers(ns.members);

                var typeDecl = namespaceNode as NamespaceNode.TypeDeclaration;
                if (typeDecl != null)
                    Console.WriteLine(typeDecl.decl.Name);
            }

        }
    }

    internal class TestClass {}
}
