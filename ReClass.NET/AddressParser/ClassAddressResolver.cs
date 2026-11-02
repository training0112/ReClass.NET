using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using ReClassNET.Memory;

namespace ReClassNET.AddressParser
{
	/// <summary>Resolves the {ClassName} part of an address formula.</summary>
	/// <remarks>
	/// The class is looked up every time the formula runs, not when it is compiled. Compiled formulas are cached
	/// by their text, so a renamed or later created class would otherwise never be seen by a cached formula.
	/// </remarks>
	public static class ClassAddressResolver
	{
		private static readonly ConcurrentDictionary<string, Func<IProcessReader, IntPtr>> formulaCache = new ConcurrentDictionary<string, Func<IProcessReader, IntPtr>>();

		[ThreadStatic]
		private static HashSet<string> classesInProgress;

		/// <summary>Gets the address formula of a class by its name. Returns null if there is no such class.</summary>
		public static Func<string, string> FormulaProvider { get; set; }

		/// <summary>Calculates the address of a class.</summary>
		/// <param name="className">The name of the class.</param>
		/// <param name="process">The process used to read memory.</param>
		/// <returns>The address, or <see cref="IntPtr.Zero"/> if the class is unknown, its formula is invalid or classes refer to each other in a cycle.</returns>
		public static IntPtr Resolve(string className, IProcessReader process)
		{
			Contract.Requires(className != null);
			Contract.Requires(process != null);

			var formula = FormulaProvider?.Invoke(className);
			if (formula == null)
			{
				return IntPtr.Zero;
			}

			classesInProgress ??= new HashSet<string>();
			if (!classesInProgress.Add(className))
			{
				return IntPtr.Zero;
			}

			try
			{
				var func = formulaCache.GetOrAdd(formula, f => DynamicCompiler.CompileExpression(Parser.Parse(f)));

				return func(process);
			}
			catch (ParseException)
			{
				return IntPtr.Zero;
			}
			finally
			{
				classesInProgress.Remove(className);
			}
		}
	}
}
