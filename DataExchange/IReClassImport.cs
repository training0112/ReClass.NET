﻿using System;
using System.Diagnostics.Contracts;
using ReClassNET.Logger;

namespace ReClassNET.DataExchange
{
	[ContractClass(typeof(IReClassImportContract))]
	public interface IReClassImport
	{
		SchemaBuilder Load(string filePath, ILogger logger);
	}

	[ContractClassFor(typeof(IReClassImport))]
	internal abstract class IReClassImportContract : IReClassImport
	{
		public SchemaBuilder Load(string filePath, ILogger logger)
		{
			Contract.Requires(filePath != null);
			Contract.Requires(logger != null);

			throw new NotImplementedException();
		}
	}
}
