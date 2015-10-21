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

namespace TriAxis.RunSharp.Operands
{
	class Field : Operand
	{
		FieldInfo _fi;
		Operand _target;

		public Field(FieldInfo fi, Operand target)
		{
			this._fi = fi;
			this._target = target;
		}

		internal override void EmitGet(CodeGen g)
		{
			if (_fi.IsStatic)
				g.IL.Emit(OpCodes.Ldsfld, _fi);
			else
			{
				_target.EmitRef(g);
				g.IL.Emit(OpCodes.Ldfld, _fi);
			}
		}

		internal override void EmitSet(CodeGen g, Operand value, bool allowExplicitConversion)
		{
			if (!_fi.IsStatic)
				_target.EmitRef(g);

			g.EmitGetHelper(value, Type, allowExplicitConversion);
			g.IL.Emit(_fi.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, _fi);
		}

		internal override void EmitAddressOf(CodeGen g)
		{
			if (_fi.IsStatic)
				g.IL.Emit(OpCodes.Ldsflda, _fi);
			else
			{
				_target.EmitRef(g);
				g.IL.Emit(OpCodes.Ldflda, _fi);
			}
		}

		public override Type Type
		{
			get
			{
				return _fi.FieldType;
			}
		}

		internal override bool TrivialAccess
		{
			get
			{
				return true;
			}
		}
	}
}
