﻿/*
 * Copyright (c) 2010, Stefan Simek
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

// Google Code Issue 17:	Operand.EQ(null) doesn't work
// Reported by qwertie256, Aug 18, 2010

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace TriAxis.RunSharp.Tests.Bugs
{
	[TestFixture]
    public class X17_IsNull : TestBase
    {
        [Test]
        public void TestGenIsNull()
        {
            TestingFacade.GetTestsForGenerator(GenIsNull, @">>> GEN TriAxis.RunSharp.Tests.Bugs.X17_IsNull.GenIsNull
=== RUN TriAxis.RunSharp.Tests.Bugs.X17_IsNull.GenIsNull
a is not null
b is null
<<< END TriAxis.RunSharp.Tests.Bugs.X17_IsNull.GenIsNull

").RunAll();
        }

        public static void GenIsNull(AssemblyGen ag)
		{
			TypeGen Test = ag.Class("Test");
			{
				CodeGen g = Test.Static.Method(typeof(void), "Main");
				{
					var a = g.Local(typeof(object), "notnull");
					var b = g.Local(typeof(object), null);

					g.If(a == null);
					{
						g.WriteLine("a is null");
					}
					g.Else();
					{
						g.WriteLine("a is not null");
					}
					g.End();

					g.If(b == null);
					{
						g.WriteLine("b is null");
					}
					g.Else();
					{
						g.WriteLine("b is not null");
					}
					g.End();
				}
			}
		}
	}
}
