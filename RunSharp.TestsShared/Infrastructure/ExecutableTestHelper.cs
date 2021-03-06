/*
Copyright(c) 2009, Stefan Simek
Copyright(c) 2016, Vladyslav Taranov

MIT License

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
#if FEAT_IKVM
using IKVM.Reflection;
#endif
using Assembly = System.Reflection.Assembly;
using BindingFlags = System.Reflection.BindingFlags;
using MethodInfo = System.Reflection.MethodInfo;
using ParameterInfo = System.Reflection.ParameterInfo;
using Type = System.Type;

namespace TriAxis.RunSharp
{
    public class ExecutableTestHelper
    {
        public delegate void Generator(AssemblyGen ag);

        internal string GetTestName(Generator g)
        {
            MethodInfo methodInfo = g.Method;
            return GetTestName(methodInfo);
        }

        internal static string GetTestName(MethodInfo methodInfo)
        {

            Type declType = methodInfo.DeclaringType;
            var r = declType.Namespace + "." + declType.Name.TrimStart('_') + "." + methodInfo.Name;
            return r.IndexOfAny(Path.GetInvalidPathChars()) == -1 ? r : declType.Namespace + "." + Guid.NewGuid();
        }

        string[] GetTestArguments(Generator g)
        {
            return (Attribute.GetCustomAttribute(g.Method, typeof(TestArgumentsAttribute)) as TestArgumentsAttribute)?.Arguments;
        }

        public void RunTest(MethodInfo testMethod, bool exe)
        {
            if (testMethod == null) throw new ArgumentNullException(nameof(testMethod));
            Generator gen;

            ParameterInfo[] pi = testMethod.GetParameters();

            if (pi.Length == 1 && pi[0].ParameterType == typeof(AssemblyGen))
            {
                gen = ((Generator)Delegate.CreateDelegate(typeof(Generator), testMethod, true));
            }
            else throw new ArgumentException("Wrong test method signature", nameof(testMethod));


            RunTest(gen, exe);
        }

        public void RunTest(Generator test, bool exe, string testName = null)
        {
            if (test == null) throw new ArgumentNullException(nameof(test));
            testName = testName ?? GetTestName(test);
            Console.WriteLine(">>> GEN {0}", testName);
            string name = testName;

            string exeDir = string.Empty;
            string exeFilePath = string.Empty;
            if (exe)
            {
                exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                exeFilePath = Path.Combine(exeDir, name + ".exe");
                Directory.CreateDirectory(exeDir);
            }

            AssemblyGen asm;
            asm = !exe
                      ? new AssemblyGen(name, new CompilerOptions())
                      : new AssemblyGen(name, new CompilerOptions() { OutputPath = exeFilePath });
            test(asm);
            if (exe)
            {
                asm.Save();
                PEVerify.AssertValid(exeFilePath);
            }
            Console.WriteLine("=== RUN {0}", testName);

            string[] testArguments = GetTestArguments(test);
#if !FEAT_IKVM
            if (!exe)
            {
#if SILVERLIGHT
                Type entryType = asm.GetAssembly().DefinedTypes.First(t => t.GetMethod("Main", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null);
#else
                Type entryType = ((TypeBuilder)asm.GetAssembly().EntryPoint.DeclaringType).CreateType();
#endif

                MethodInfo entryMethod = entryType.GetMethod(asm.GetAssembly().EntryPoint?.Name ?? "Main", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                object[] entryArgs = null;
                if (entryMethod.GetParameters().Length == 1)
                {
                    entryArgs = new object[] { testArguments };
                }
                entryMethod.Invoke(null, entryArgs);
            }
            else
#endif
            {
                try
                {
                    AppDomain.CurrentDomain.ExecuteAssembly(exeFilePath, null, testArguments);
                }
                catch (System.BadImageFormatException)
                {
                    throw;
                }
                catch (AppDomainUnloadedException)
                {
                    throw;
                }
                catch (Exception e)
                {

#if SILVERLIGHT
                    throw;
#else
                    throw new TargetInvocationException(e);
#endif
                }
            }

            Console.WriteLine("<<< END {0}", testName);
            Console.WriteLine();

            if (exe)
            {
                try
                {
                    File.Delete(exeFilePath);
                }
                catch { }
            }
        }
    }
}