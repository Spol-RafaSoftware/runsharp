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
using System.Collections;
using System.Text;
using NUnit.Framework;
using TriAxis.RunSharp;

namespace TriAxis.RunSharp.Tests
{
    [TestFixture]
	public class _13_Events : TestBase
    {
        [Test]
        public void TestGenEvents1()
        {
            TestingFacade.GetTestsForGenerator(GenEvents1, @">>> GEN TriAxis.RunSharp.Tests.13_Events.GenEvents1
=== RUN TriAxis.RunSharp.Tests.13_Events.GenEvents1
This is called when the event fires.
This is called when the event fires.
<<< END TriAxis.RunSharp.Tests.13_Events.GenEvents1

").RunAll();
        }

        // example based on the MSDN Events Sample (events1.cs)
        public static void GenEvents1(AssemblyGen ag)
        {
            var st = ag.StaticFactory;
            var exp = ag.ExpressionFactory;

            ITypeMapper m = ag.TypeMapper;
		    TypeGen ChangedEventHandler, ListWithChangedEvent;

		    using (ag.Namespace("MyCollections"))
			{
				// A delegate type for hooking up change notifications.
				ChangedEventHandler = ag.Delegate(typeof(void), "ChangedEventHandler").Parameter(typeof(object), "sender").Parameter(typeof(EventArgs), "e");

				// A class that works just like ArrayList, but sends event
				// notifications whenever the list changes.
				ListWithChangedEvent = ag.Public.Class("ListWithChangedEvent", typeof(ArrayList));
				{
					// An event that clients can use to be notified whenever the
					// elements of the list change.
					EventGen Changed = ListWithChangedEvent.Public.Event(ChangedEventHandler, "Changed");

					// Invoke the Changed event; called whenever list changes
					CodeGen g = ListWithChangedEvent.Protected.Virtual.Method(typeof(void), "OnChanged").Parameter(typeof(EventArgs), "e");
					{
						g.If(Changed != null);
						{
							g.InvokeDelegate(Changed, g.This(), g.Arg("e"));
						}
						g.End();
					}

					// Override some of the methods that can change the list;
					// invoke event after each
					g = ListWithChangedEvent.Public.Override.Method(typeof(int), "Add").Parameter(typeof(object), "value");
					{
                        var i = g.Local(g.Base().Invoke("Add", new[] { g.Arg("value") }));
						g.Invoke(g.This(), "OnChanged", st.Field(typeof(EventArgs), "Empty"));
						g.Return(i);
					}

					g = ListWithChangedEvent.Public.Override.Method(typeof(void), "Clear");
					{
						g.Invoke(g.Base(), "Clear");
						g.Invoke(g.This(), "OnChanged", st.Field(typeof(EventArgs), "Empty"));
					}

					g = ListWithChangedEvent.Public.Override.Indexer(typeof(object)).Index(typeof(int), "index").Setter();
					{
						g.Assign(g.Base()[g.Arg("index")], g.PropertyValue());
						g.Invoke(g.This(), "OnChanged", st.Field(typeof(EventArgs), "Empty"));
					}
				}
			}

			using (ag.Namespace("TestEvents"))
			{
				TypeGen EventListener = ag.Class("EventListener");
				{
					FieldGen List = EventListener.Field(ListWithChangedEvent, "List");

					// This will be called whenever the list changes.
					CodeGen g = EventListener.Private.Method(typeof(void), "ListChanged").Parameter(typeof(object), "sender").Parameter(typeof(EventArgs), "eventArgs");
					{
						g.WriteLine("This is called when the event fires.");
					}

					g = EventListener.Public.Constructor().Parameter(ListWithChangedEvent, "list");
					{
						g.Assign(List, g.Arg("list"));
						// Add "ListChanged" to the Changed event on "List".
						g.SubscribeEvent(List, "Changed", exp.NewDelegate(ChangedEventHandler, g.This(), "ListChanged"));
					}

					g = EventListener.Public.Method(typeof(void), "Detach");
					{
						// Detach the event and delete the list
						g.UnsubscribeEvent(List, "Changed", exp.NewDelegate(ChangedEventHandler, g.This(), "ListChanged"));
						g.Assign(List, null);
					}
				}

				TypeGen Test = ag.Class("Test");
				{
					// Test the ListWithChangedEvent class.
					CodeGen g = Test.Public.Static.Method(typeof(void), "Main");
					{
                        // Create a new list.
                        var list = g.Local(exp.New(ListWithChangedEvent));

                        // Create a class that listens to the list's change event.
                        var listener = g.Local(exp.New(EventListener, list));

						// Add and remove items from the list.
						g.Invoke(list, "Add", "item 1");
						g.Invoke(list, "Clear");
						g.Invoke(listener, "Detach");
					}
				}
			}
		}
	}
}
