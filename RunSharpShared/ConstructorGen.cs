/*
 * Copyright (c) 2015, Stefan Simek, Vladyslav Taranov
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

using System;
using System.Collections.Generic;
using System.Text;
#if FEAT_IKVM
using IKVM.Reflection;
using IKVM.Reflection.Emit;
using Type = IKVM.Reflection.Type;
using MissingMethodException = System.MissingMethodException;
using MissingMemberException = System.MissingMemberException;
using DefaultMemberAttribute = System.Reflection.DefaultMemberAttribute;
using Attribute = IKVM.Reflection.CustomAttributeData;
using BindingFlags = IKVM.Reflection.BindingFlags;
#else
using System.Reflection;
using System.Reflection.Emit;
#endif

namespace TriAxis.RunSharp
{
	public sealed class ConstructorGen : RoutineGen<ConstructorGen>
	{
		MethodAttributes _attributes;
		ConstructorBuilder _cb;
		MethodImplAttributes _implFlags;

		internal ConstructorBuilder GetConstructorBuilder()
		{
			LockSignature();
			return _cb;
		}

		internal ConstructorGen(TypeGen owner, MethodAttributes attributes, MethodImplAttributes implFlags)
			: base(owner, null)
		{
			this._attributes = attributes;
			this._implFlags = implFlags;

			owner.RegisterForCompletion(this);
		}

		protected override void CreateMember()
		{
			this._cb = Owner.TypeBuilder.DefineConstructor(_attributes | MethodAttributes.HideBySig, IsStatic ? CallingConventions.Standard : CallingConventions.HasThis, ParameterTypes);
			if (_implFlags != 0)
				_cb.SetImplementationFlags(_implFlags);
		}

		protected override void RegisterMember()
		{
			Owner.Register(this);
		}

		public TypeGen Type { get { return Owner; } }

		#region RoutineGen concrete implementation

		public override string Name
		{
			get { return IsStatic ? ".cctor" : ".ctor"; }
		}

		protected internal override bool IsStatic
		{
			get { return (_attributes & MethodAttributes.Static) != 0; }
		}

		protected internal override bool IsOverride
		{
			get { return false; }
		}

		protected override bool HasCode
		{
			get { return (_implFlags & MethodImplAttributes.Runtime) == 0; }
		}

		protected override ILGenerator GetILGenerator()
		{
			return _cb.GetILGenerator();
		}

		protected override ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string parameterName)
		{
			return _cb.DefineParameter(position, attributes, parameterName);
		}

		protected override MemberInfo Member
		{
			get { return _cb; }
		}

		protected override AttributeTargets AttributeTarget
		{
			get { return AttributeTargets.Constructor; }
		}

		protected override void SetCustomAttribute(CustomAttributeBuilder cab)
		{
			_cb.SetCustomAttribute(cab);
		}

		#endregion
	}
}
