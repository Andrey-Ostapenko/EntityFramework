﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.FunkyDataModel;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class FunkyDataQuerySqlServerFixture : FunkyDataQueryFixtureBase<SqlServerTestStore>
    {
        public const string DatabaseName = "FunkyDataQueryTest";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = SqlServerTestStore.CreateConnectionString(DatabaseName);

        public FunkyDataQuerySqlServerFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override SqlServerTestStore CreateTestStore()
        {
            return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseSqlServer(_connectionString, b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new FunkyDataContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureCreated();
                        FunkyDataModelInitializer.Seed(context);

                        TestSqlLoggerFactory.Reset();
                    }
                });
        }

        public override FunkyDataContext CreateContext(SqlServerTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            optionsBuilder
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(_serviceProvider)
                .UseSqlServer(testStore.Connection, b => b.ApplyConfiguration());

            var context = new FunkyDataContext(optionsBuilder.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
