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
	class ArrayAccess : Operand
	{
	    readonly Operand _array;
	    readonly Operand[] _indexes;

	    protected override void ResetLeakedStateRecursively()
	    {
	        base.ResetLeakedStateRecursively();
	        OperandExtensions.SetLeakedState(_array, false);
            OperandExtensions.SetLeakedState(_indexes, false);
        }

	    public ArrayAccess(Operand array, Operand[] indexes)
		{
			_array = array;
			_indexes = indexes;
		}

		void LoadArrayAndIndexes(CodeGen g)
		{
            if (_array.GetReturnType(g.TypeMapper).GetArrayRank() != _indexes.Length)
                throw new ArgumentException(Properties.Messages.ErrIndexCountMismatch);

            _array.EmitGet(g);

			foreach (Operand op in _indexes)
			    g.EmitGetHelper(op, g.TypeMapper.MapType(Helpers.AreTypesEqual(GetType(op, g.TypeMapper), typeof(int), g.TypeMapper) ? typeof(int) : typeof(long)), false);
		}

	    protected internal override void EmitGet(CodeGen g) 
	    {
		    OperandExtensions.SetLeakedState(this, false); 
			LoadArrayAndIndexes(g);

			if (_indexes.Length == 1)
			{
				g.EmitLdelemHelper(GetReturnType(g.TypeMapper));
			}
			else
            {
                g.EmitCallHelper(
                    (MethodBase)g.TypeMapper.TypeInfo.FindMethod(
                        _array.GetReturnType(g.TypeMapper),
                        "Get",
                        _indexes,
                        false).Method.Member,
                    _array);
            }
		}

	    protected internal override void EmitSet(CodeGen g, Operand value, bool allowExplicitConversion)
	    {
		    OperandExtensions.SetLeakedState(this, false); 
			LoadArrayAndIndexes(g);

	        if (_indexes.Length == 1)
	        {
	            g.EmitStelemHelper(GetReturnType(g.TypeMapper), value, allowExplicitConversion);
	        }
	        else
	        {
                g.EmitGetHelper(value, GetReturnType(g.TypeMapper), allowExplicitConversion);
	            g.EmitCallHelper(
	                (MethodBase)g.TypeMapper.TypeInfo.FindMethod(
	                    _array.GetReturnType(g.TypeMapper),
	                    "Set",
	                    ArrayUtils.Combine(_indexes, value),
	                    false).Method.Member,
	                _array);
	        }
	    }

		protected internal override void EmitAddressOf(CodeGen g)
		{
		    OperandExtensions.SetLeakedState(this, false);  
			LoadArrayAndIndexes(g);

			if (_indexes.Length == 1)
			{
				g.IL.Emit(OpCodes.Ldelema, GetReturnType(g.TypeMapper));
			}
			else
			{
				throw new NotImplementedException();
			}
		}

	    public override Type GetReturnType(ITypeMapper typeMapper) => _array.GetReturnType(typeMapper).GetElementType();

	    protected internal override bool TrivialAccess => true;
	}
}
