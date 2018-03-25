using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Source-based implementation of <see cref="IAssemblyInfo"/>.
    /// </summary>
    class SourceAssemblyInfo : IAssemblyInfo
    {
        private readonly SemanticModel _semanticModel;
        private readonly CancellationToken _cancellation;

        private CompilationContext _compilationContext;
        private IEnumerable<ITypeInfo> _publicTypes;

        public SourceAssemblyInfo(SemanticModel semanticModel, CancellationToken cancellation)
        {
            _semanticModel = semanticModel;
            _cancellation = cancellation;
        }

        private CompilationContext CompilationContext
        {
            get
            {
                if (_compilationContext == null)
                    _compilationContext = new CompilationContext(this, _semanticModel.Compilation);

                return _compilationContext;
            }
        }

        string IAssemblyInfo.AssemblyPath => null;

        string IAssemblyInfo.Name => CompilationContext.AssemblyName;

        IEnumerable<IAttributeInfo> IAssemblyInfo.GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            if (CompilationContext.AssemblySymbol.TryGetMatchingAttributes(CompilationContext, assemblyQualifiedAttributeTypeName, out var matchingAttributes))
                return matchingAttributes;

            return Enumerable.Empty<IAttributeInfo>();
        }

        ITypeInfo IAssemblyInfo.GetType(string typeName)
        {
            throw new NotImplementedException();
        }

        IEnumerable<ITypeInfo> IAssemblyInfo.GetTypes(bool includePrivateTypes)
        {
            // Don't bother caching requests that include private types, because we don't usually ask for those,
            // meaning we can avoid creating ITypeInfos for the private types in the normal case...
            if (includePrivateTypes || _publicTypes == null)
            {
                var typeMap = new Dictionary<string, ITypeInfo>();

                foreach (var node in _semanticModel.SyntaxTree.GetRoot(_cancellation).DescendantNodes())
                {
                    if (_semanticModel.GetDeclaredSymbol(node, _cancellation) is INamedTypeSymbol type &&
                        // TODO: Below doesn't correctly handle accessibility for nested types...
                        (includePrivateTypes || type.DeclaredAccessibility == Accessibility.Public))
                    {
                        ITypeInfo typeInfo = new SourceTypeInfo(_compilationContext, type);
                        if (!typeMap.ContainsKey(typeInfo.Name))
                            typeMap.Add(typeInfo.Name, typeInfo);
                    }
                }

                if (!includePrivateTypes)
                    _publicTypes = typeMap.Values;

                return typeMap.Values;
            }

            return _publicTypes;
        }
    }
}
