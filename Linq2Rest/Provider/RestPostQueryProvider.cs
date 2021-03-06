// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestPostQueryProvider.cs" company="Reimers.dk">
//   Copyright � Reimers.dk 2014
//   This source is subject to the Microsoft Public License (Ms-PL).
//   Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//   All other rights reserved.
// </copyright>
// <summary>
//   Defines the RestPostQueryProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Linq2Rest.Provider
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Linq.Expressions;
	using Linq2Rest.Provider.Writers;

	internal class RestPostQueryProvider<T> : RestQueryProvider<T>
	{
		private readonly Stream _inputData;

		public RestPostQueryProvider(IRestClient client, ISerializerFactory serializerFactory, IExpressionProcessor expressionProcessor, IMemberNameResolver memberNameResolver, IEnumerable<IValueWriter> valueWriters, Stream inputData, Type sourceType)
			: base(client, serializerFactory, expressionProcessor, memberNameResolver, valueWriters, sourceType)
		{
			Contract.Requires(client != null);
			Contract.Requires(serializerFactory != null);
			Contract.Requires(expressionProcessor != null);
			Contract.Requires(memberNameResolver != null);
			Contract.Requires(valueWriters != null);
			Contract.Requires(sourceType != null);
			Contract.Requires(inputData != null);

			_inputData = inputData;
		}

		protected override Func<IRestClient, ISerializerFactory, IMemberNameResolver, IEnumerable<IValueWriter>, Expression, Type, IQueryable<TResult>> CreateQueryable<TResult>()
		{
			return InnerCreateQueryable<TResult>;
		}

		protected override IEnumerable<T> GetResults(ParameterBuilder builder)
		{
			var fullUri = builder.GetFullUri();
			var response = Client.Post(fullUri, _inputData);
			var serializer = GetSerializer(builder.SourceType);
			var resultSet = serializer.DeserializeList(response);

			Contract.Assume(resultSet != null);

			return resultSet;
		}

		protected override IEnumerable GetIntermediateResults(Type type, ParameterBuilder builder)
		{
			var fullUri = builder.GetFullUri();
			var response = Client.Post(fullUri, _inputData);

			dynamic serializer = GetSerializer(type, builder.SourceType);
			var resultSet = serializer.DeserializeList(response);

			return resultSet;
		}

		private IQueryable<TResult> InnerCreateQueryable<TResult>(IRestClient client, ISerializerFactory serializerFactory, IMemberNameResolver memberNameResolver, IEnumerable<IValueWriter> valueWriters, Expression expression, Type sourceType)
		{
			Contract.Requires(client != null);
			Contract.Requires(serializerFactory != null);
			Contract.Requires(memberNameResolver != null);
			Contract.Requires(valueWriters != null);
			Contract.Requires(expression != null);
			Contract.Requires(sourceType != null);

			return new RestDeleteQueryable<TResult>(client, serializerFactory, memberNameResolver, valueWriters, expression, sourceType);
		}

		[ContractInvariantMethod]
		private void Invariants()
		{
			Contract.Invariant(_inputData != null);
		}
	}
}