#region license
// boo - an extensible programming language for the CLI
// Copyright (C) 2004 Rodrigo B. de Oliveira
//
// Permission is hereby granted, free of charge, to any person 
// obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, 
// publish, distribute, sublicense, and/or sell copies of the Software, 
// and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included 
// in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Contact Information
//
// mailto:rbo@acm.org
#endregion

namespace Boo.Lang.Compiler.Bindings
{
	using System;
	using Boo.Lang.Compiler.Ast;
	using Boo.Lang.Compiler.Services;

	public class ModuleBinding : INamespace, IBinding
	{
		DefaultBindingService _bindingService;
		
		Module _module;
		
		INamespace _moduleClassNamespace = NullNamespace.Default;
		
		INamespace[] _using;
		
		string _namespace;
		
		public ModuleBinding(DefaultBindingService bindingManager, Module module)
		{
			_bindingService = bindingManager;
			_module = module;			
			if (null == module.Namespace)
			{
				_namespace = "";
			}
			else
			{
				_namespace = module.Namespace.Name;
			}
		}
		
		public BindingType BindingType
		{
			get
			{
				return BindingType.Module;
			}
		}
		
		public string Name
		{
			get
			{
				return _module.Name;
			}
		}
		
		public string FullName
		{
			get
			{
				return _module.FullName;
			}
		}
		
		public string Namespace
		{
			get
			{
				return _namespace;
			}
		}
		
		public IBinding ResolveMember(string name)
		{
			IBinding binding = ResolveModuleMember(name);
			if (null == binding)
			{
				binding = ResolveModuleClassMember(name);
			}
			return binding;
		}
		
		public INamespace ParentNamespace
		{
			get
			{
				return (INamespace)DefaultBindingService.GetBinding(_module.ParentNode);
			}
		}
		
		public IBinding Resolve(string name)
		{
			IBinding binding = ResolveMember(name);
			if (null == binding)
			{	
				if (null == _using)
				{
					_using = new INamespace[_module.Imports.Count];
					for (int i=0; i<_using.Length; ++i)
					{
						_using[i] = (INamespace)DefaultBindingService.GetBinding(_module.Imports[i]);
					}
				}
				
				foreach (INamespace ns in _using)
				{
					// todo: resolve name in all namespaces...
					binding = ns.Resolve(name);
					if (null != binding)
					{					
						break;
					}
				}
			}
			return binding;
		}
		
		IBinding ResolveModuleMember(string name)
		{
			TypeMember member = _module.Members[name];
			if (null != member)
			{
				ITypeBinding typeBinding = (ITypeBinding)member.Binding;
				if (null == typeBinding)
				{
					if (NodeType.EnumDefinition == member.NodeType)
					{
						typeBinding = new EnumTypeBinding(_bindingService, (EnumDefinition)member);
					}
					else
					{
						typeBinding = new InternalTypeBinding(_bindingService, (TypeDefinition)member);
					}
					DefaultBindingService.Bind(member, typeBinding);
				}
				return _bindingService.AsTypeReference(typeBinding);
			}
			return null;
		}
		
		IBinding ResolveModuleClassMember(string name)
		{
			return _moduleClassNamespace.Resolve(name);
		}
	}
}
