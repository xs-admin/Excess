using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageExtension
{
    public class ResponseVisitor : SymbolVisitor
    {
        public static string Get(ITypeSymbol type, string data)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Object:
                case SpecialType.System_Void:
                case SpecialType.System_MulticastDelegate:
                case SpecialType.System_Delegate:
                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                case SpecialType.System_RuntimeArgumentHandle:
                case SpecialType.System_RuntimeFieldHandle:
                case SpecialType.System_RuntimeMethodHandle:
                case SpecialType.System_RuntimeTypeHandle:
                case SpecialType.System_IAsyncResult:
                case SpecialType.System_AsyncCallback:
                    return null; //unsupported type

                case SpecialType.System_Collections_IEnumerable:
                case SpecialType.System_Collections_Generic_IEnumerable_T:
                case SpecialType.System_Collections_Generic_IList_T:
                case SpecialType.System_Collections_Generic_ICollection_T:
                case SpecialType.System_Collections_IEnumerator:
                case SpecialType.System_Collections_Generic_IEnumerator_T:
                case SpecialType.System_Collections_Generic_IReadOnlyList_T:
                case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
                case SpecialType.System_Runtime_CompilerServices_IsVolatile:
                case SpecialType.System_ArgIterator:
                    return null; //unsupported collection type

                case SpecialType.System_IDisposable:
                case SpecialType.System_Enum:
                case SpecialType.System_TypedReference:
                case SpecialType.System_ValueType:
                case SpecialType.System_Nullable_T:
                case SpecialType.System_DateTime:
                case SpecialType.System_Array:
                    Debug.Assert(false); //td;
                    return null;

                case SpecialType.System_Boolean:
                    return $"{data} === 'true'";
                case SpecialType.System_Char:

                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_String:
                    return data;
            }

            var isCompilationType = type.DeclaringSyntaxReferences.Any();
            if (!isCompilationType)
                return data;

            var visitor = new ResponseVisitor(data);
            type.Accept(visitor);

            if (!visitor.Success)
                return null;

            return visitor.Result;
        }

        Stack<string> _data = new Stack<string>();
        private ResponseVisitor(string reader)
        {
            Success = true;
            _data.Push(reader);
        }

        public bool Success { get; private set; }
        public string Result { get { return _result.ToString(); } }


        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            _result.Append($"new {symbol.Name} ({{");
            foreach (var member in symbol.GetMembers())
            {
                switch (member.DeclaredAccessibility)
                {
                    case Accessibility.Protected:
                    case Accessibility.Public:
                    case Accessibility.Internal:
                        member.Accept(this);
                        break;
                }
            }
            _result.Append("})");
        }

        private StringBuilder _result = new StringBuilder();
        public override void VisitField(IFieldSymbol symbol)
        {
            addProperty(symbol.Type, symbol.Name);
        }

        public override void VisitProperty(IPropertySymbol symbol)
        {
            addProperty(symbol.Type, symbol.Name);
        }

        private void addProperty(ITypeSymbol type, string name)
        {
            _data.Push(name);
            try
            {
                var typeInstantiation = ResponseVisitor.Get(type, currentData());
                if (typeInstantiation == null)
                {
                    Success = false;
                    return;
                }

                _result.AppendLine($"{name} = {typeInstantiation},");
            }
            finally
            {
                _data.Pop();
            }
        }

        private string currentData()
        {
            return string.Join(".", _data
                .Reverse()
                .ToArray());
        }
    }
}
