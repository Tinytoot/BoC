﻿using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.WebPages.Razor;
using System.Linq;
using System.Linq.Expressions;

namespace BoC.Web.Mvc.PrecompiledViews
{
	[BuildProviderAppliesTo(BuildProviderAppliesTo.Web | BuildProviderAppliesTo.Code)]
	public class CompiledRazorBuildProvider : RazorBuildProvider
	{
		private CompilerResults results;

		/// <summary>
		/// Returns a type generated by the build provider from the virtual path.
		/// </summary>
		/// <returns>
		/// The type that is generated by the build provider for the virtual path. The base class returns null.
		/// </returns>
		/// <param name="results">The compilation results for the build provider's virtual path.</param>
		public override Type GetGeneratedType(CompilerResults results)
		{
			if (IsCompiledFile)
			{
				var compiledFile = HostingEnvironment.VirtualPathProvider.GetFile(VirtualPath) as CompiledVirtualFile;
				if (compiledFile != null) return compiledFile.CompiledType;
			}
			return base.GetGeneratedType(results);
		}

		public override ICollection VirtualPathDependencies
		{
			get
			{
				if (base.VirtualPathDependencies == null || !IsCompiledFile)
					return base.VirtualPathDependencies;

				return base.VirtualPathDependencies.Cast<string>().Where(s => File.Exists(s) || Directory.Exists(s)).ToList();
			}
		}

		/// <summary>
		/// Generates source code for the virtual path of the build provider, and adds the source code to a specified assembly builder.
		/// </summary>
		/// <param name="assemblyBuilder">The assembly builder that references the source code generated by the build provider.</param>
		public override void GenerateCode(AssemblyBuilder assemblyBuilder)
		{
			if (!IsCompiledFile)
			{
				base.GenerateCode(assemblyBuilder);
			}
		}

		/// <summary>
		/// Represents the compiler type used by a build provider to generate source code for a custom file type.
		/// </summary>
		/// <returns>
		/// A read-only <see cref="T:System.Web.Compilation.CompilerType"/> representing the code generator, code compiler, and compiler settings used to build source code for the virtual path. The base class returns null.
		/// </returns>
		public override CompilerType CodeCompilerType
		{
			get
			{
				return !IsCompiledFile ? base.CodeCompilerType : null;
			}
		}

		private string virtualPathCache = null;
		private bool isCompiledFile = false;
		public virtual bool IsCompiledFile
		{
			get {
				if(virtualPathCache == VirtualPath)
			       	return isCompiledFile;
				
				virtualPathCache = VirtualPath;
				return (isCompiledFile = HostingEnvironment.VirtualPathProvider.GetFile(VirtualPath) is CompiledVirtualFile);
			}
		}

		protected override TextReader InternalOpenReader()
		{
			return !IsCompiledFile ? base.InternalOpenReader() : null;
		}

		protected override CodeCompileUnit GetCodeCompileUnit(out IDictionary linePragmasTable)
		{
			linePragmasTable = null;
			return IsCompiledFile ? null : base.GetCodeCompileUnit(out linePragmasTable);
		}
	}
}
