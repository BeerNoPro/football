2026-05-11T14:22:55Z runner[4d895d5ec41638] sin [info]Machine started in 3.419s
2026-05-11T14:22:55Z proxy[4d895d5ec41638] sin [info]machine started in 3.571926497s
2026-05-11T14:23:00Z app[e2862692b79d68] sin [info][2026-05-11 14:22:58 FTL]  | Application terminated unexpectedly
2026-05-11T14:23:00Z app[e2862692b79d68] sin [info]System.InvalidOperationException: Current JobStorage instance has not been initialized yet. You must set it before using Hangfire Client or Server API. For .NET Core applications please call the `IServiceCollection.AddHangfire` extension method from Hangfire.NetCore or Hangfire.AspNetCore package depending on your application type when configuring the services and ensure service-based APIs are used instead of static ones, like `IBackgroundJobClient` instead of `BackgroundJob` and `IRecurringJobManager` instead of `RecurringJob`.
2026-05-11T14:23:00Z app[e2862692b79d68] sin [info]   at Hangfire.JobStorage.get_Current() in C:\projects\hangfire-525\src\Hangfire.Core\JobStorage.cs:line 42
2026-05-11T14:23:00Z app[e2862692b79d68] sin [info]   at Hangfire.RecurringJobManager..ctor() in C:\projects\hangfire-525\src\Hangfire.Core\RecurringJobManager.cs:line 42
2026-05-11T14:23:00Z app[e2862692b79d68] sin [info]   at Hangfire.RecurringJob.<>c.<.cctor>b__66_0() in C:\projects\hangfire-525\src\Hangfire.Core\RecurringJob.cs:line 28
2026-05-11T14:23:00Z app[e2862692b79d68] sin [info]   at Hangfire.RecurringJob.RemoveIfExists(String recurringJobId) in C:\projects\hangfire-525\src\Hangfire.Core\RecurringJob.cs:line 645
2026-05-11T14:23:00Z app[e2862692b79d68] sin [info]   at Program.<Main>$(String[] args) in /src/FootballBlog.API/Program.cs:line 291
2026-05-11T14:23:00Z app[4d895d5ec41638] sin [info]2026/05/11 14:23:00 INFO SSH listening listen_address=[fdaa:73:80de:a7b:85a:8a8b:a066:2]:22
2026-05-11T14:23:00Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.567650182s so far)
2026-05-11T14:23:01Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.972321866s so far)
2026-05-11T14:23:01Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.972462531s so far)
2026-05-11T14:23:01Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 6.047450542s so far)
2026-05-11T14:23:01Z app[e2862692b79d68] sin [info] INFO Main child exited normally with code: 0
2026-05-11T14:23:01Z app[e2862692b79d68] sin [info] INFO Starting clean up.
2026-05-11T14:23:02Z app[e2862692b79d68] sin [info][   20.307010] reboot: Restarting system
2026-05-11T14:23:02Z runner[e2862692b79d68] sin [info]machine exited with exit code 0, not restarting
error.message="failed to connect to machine: gave up after 15 attempts (in 8.727570155s)" 2026-05-11T14:23:04Z proxy[4d895d5ec41638] sin [error]
error.message="failed to connect to machine: gave up after 15 attempts (in 9.127735341s)" 2026-05-11T14:23:04Z proxy[4d895d5ec41638] sin [error]
error.message="failed to connect to machine: gave up after 15 attempts (in 9.128032973s)" 2026-05-11T14:23:04Z proxy[4d895d5ec41638] sin [error]
error.message="failed to connect to machine: gave up after 15 attempts (in 9.209151962s)" 2026-05-11T14:23:04Z proxy[4d895d5ec41638] sin [error]
2026-05-11T14:23:10Z app[4d895d5ec41638] sin [info][2026-05-11 14:23:09 FTL]  | Application terminated unexpectedly
2026-05-11T14:23:10Z app[4d895d5ec41638] sin [info]System.InvalidOperationException: Current JobStorage instance has not been initialized yet. You must set it before using Hangfire Client or Server API. For .NET Core applications please call the `IServiceCollection.AddHangfire` extension method from Hangfire.NetCore or Hangfire.AspNetCore package depending on your application type when configuring the services and ensure service-based APIs are used instead of static ones, like `IBackgroundJobClient` instead of `BackgroundJob` and `IRecurringJobManager` instead of `RecurringJob`.
2026-05-11T14:23:10Z app[4d895d5ec41638] sin [info]   at Hangfire.JobStorage.get_Current() in C:\projects\hangfire-525\src\Hangfire.Core\JobStorage.cs:line 42
2026-05-11T14:23:10Z app[4d895d5ec41638] sin [info]   at Hangfire.RecurringJobManager..ctor() in C:\projects\hangfire-525\src\Hangfire.Core\RecurringJobManager.cs:line 42
2026-05-11T14:23:10Z app[4d895d5ec41638] sin [info]   at Hangfire.RecurringJob.<>c.<.cctor>b__66_0() in C:\projects\hangfire-525\src\Hangfire.Core\RecurringJob.cs:line 28
2026-05-11T14:23:10Z app[4d895d5ec41638] sin [info]   at Hangfire.RecurringJob.RemoveIfExists(String recurringJobId) in C:\projects\hangfire-525\src\Hangfire.Core\RecurringJob.cs:line 645
2026-05-11T14:23:10Z app[4d895d5ec41638] sin [info]   at Program.<Main>$(String[] args) in /src/FootballBlog.API/Program.cs:line 291
2026-05-11T14:23:12Z app[4d895d5ec41638] sin [info] INFO Main child exited normally with code: 0
2026-05-11T14:23:12Z app[4d895d5ec41638] sin [info] INFO Starting clean up.
2026-05-11T14:23:12Z app[4d895d5ec41638] sin [info][   20.737751] reboot: Restarting system
2026-05-11T14:23:13Z runner[4d895d5ec41638] sin [info]machine exited with exit code 0, not restarting
2026-05-11T14:29:53Z runner[4d895d5ec41638] sin [info]Pulling container image registry.fly.io/footballblog-api@sha256:8b51487fd5422dbc69e8809986dff83832ff0735ca9c5327b6bcf88f042523f4
2026-05-11T14:29:54Z runner[e2862692b79d68] sin [info]Pulling container image registry.fly.io/footballblog-api@sha256:8b51487fd5422dbc69e8809986dff83832ff0735ca9c5327b6bcf88f042523f4
2026-05-11T14:29:58Z runner[4d895d5ec41638] sin [info]Successfully prepared image registry.fly.io/footballblog-api@sha256:8b51487fd5422dbc69e8809986dff83832ff0735ca9c5327b6bcf88f042523f4 (5.430280336s)
2026-05-11T14:29:59Z runner[4d895d5ec41638] sin [info]Configuring firecracker
2026-05-11T14:30:00Z runner[e2862692b79d68] sin [info]Successfully prepared image registry.fly.io/footballblog-api@sha256:8b51487fd5422dbc69e8809986dff83832ff0735ca9c5327b6bcf88f042523f4 (6.102068138s)
2026-05-11T14:30:01Z runner[e2862692b79d68] sin [info]Configuring firecracker
2026-05-11T14:31:17Z proxy[4d895d5ec41638] sin [info]Starting machine
2026-05-11T14:31:17Z app[4d895d5ec41638] sin [info]2026-05-11T14:31:17.893097469 [01KRBQ3HW6AEBKMDQK3FP5XA4M:main] Running Firecracker v1.14.4
2026-05-11T14:31:17Z app[4d895d5ec41638] sin [info]2026-05-11T14:31:17.893336616 [01KRBQ3HW6AEBKMDQK3FP5XA4M:main] Listening on API socket ("/fc.sock").
2026-05-11T14:31:18Z app[4d895d5ec41638] sin [info] INFO Starting init (commit: 3040ac0)...
2026-05-11T14:31:19Z app[4d895d5ec41638] sin [info] INFO Preparing to run: `dotnet FootballBlog.API.dll` as root
2026-05-11T14:31:19Z app[4d895d5ec41638] sin [info] INFO [fly api proxy] listening at /.fly/api
2026-05-11T14:31:19Z runner[4d895d5ec41638] sin [info]Machine started in 1.535s
2026-05-11T14:31:19Z proxy[4d895d5ec41638] sin [info]machine started in 1.715763621s
2026-05-11T14:31:19Z app[4d895d5ec41638] sin [info]2026/05/11 14:31:19 INFO SSH listening listen_address=[fdaa:73:80de:a7b:85a:8a8b:a066:2]:22
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info][2026-05-11 14:31:20 FTL]  | Application terminated unexpectedly
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]System.ArgumentException: Connection string is not valid (Parameter 'connectionString')
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info] ---> System.ArgumentException: Couldn't set postgresql://neondb_owner:npg_e0cpeja4rdow@ep-shy-voice-aqrdqaz6-pooler.c-8.us-east-1.aws.neon.tech/neondb?sslmode (Parameter 'postgresql://neondb_owner:npg_e0cpeja4rdow@ep-shy-voice-aqrdqaz6-pooler.c-8.us-east-1.aws.neon.tech/neondb?sslmode')
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info] ---> System.Collections.Generic.KeyNotFoundException: The given key was not present in the dictionary.
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder.GeneratedActions(GeneratedAction action, String keyword, Object& value)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder.set_Item(String keyword, Object value)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   --- End of inner exception stack trace ---
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder.set_Item(String keyword, Object value)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at System.Data.Common.DbConnectionStringBuilder.set_ConnectionString(String value)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder..ctor(String connectionString)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Hangfire.PostgreSql.Factories.NpgsqlInstanceConnectionFactoryBase.SetupConnectionStringBuilder(String connectionString)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   --- End of inner exception stack trace ---
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Hangfire.PostgreSql.Factories.NpgsqlInstanceConnectionFactoryBase.SetupConnectionStringBuilder(String connectionString)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Hangfire.PostgreSql.Factories.NpgsqlConnectionFactory..ctor(String connectionString, PostgreSqlStorageOptions options, Action`1 connectionSetup)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Hangfire.PostgreSql.PostgreSqlBootstrapperOptions.UseNpgsqlConnection(String connectionString, Action`1 connectionSetup)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Program.<>c__DisplayClass0_0.<<Main>$>b__33(PostgreSqlBootstrapperOptions o) in /src/FootballBlog.API/Program.cs:line 223
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Hangfire.PostgreSql.PostgreSqlBootstrapperConfigurationExtensions.UsePostgreSqlStorage(IGlobalConfiguration configuration, Action`1 configure, PostgreSqlStorageOptions options)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Program.<>c__DisplayClass0_0.<<Main>$>b__10(IGlobalConfiguration cfg) in /src/FootballBlog.API/Program.cs:line 218
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Hangfire.HangfireServiceCollectionExtensions.<>c__DisplayClass0_0.<AddHangfire>b__0(IServiceProvider provider, IGlobalConfiguration config) in C:\projects\hangfire-525\src\Hangfire.NetCore\HangfireServiceCollectionExtensions.cs:line 40
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Hangfire.HangfireServiceCollectionExtensions.<>c__DisplayClass1_0.<AddHangfire>b__14(IServiceProvider serviceProvider) in C:\projects\hangfire-525\src\Hangfire.NetCore\HangfireServiceCollectionExtensions.cs:line 103
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitRootCache(ServiceCallSite callSite, RuntimeResolverContext context)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSite(ServiceCallSite callSite, TArgument argument)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.Resolve(ServiceCallSite callSite, ServiceProviderEngineScope scope)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProvider.CreateServiceAccessor(ServiceIdentifier serviceIdentifier)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at System.Collections.Concurrent.ConcurrentDictionary`2.GetOrAdd(TKey key, Func`2 valueFactory)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProvider.GetService(ServiceIdentifier serviceIdentifier, ServiceProviderEngineScope serviceProviderEngineScope)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProvider.GetService(Type serviceType)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService[T](IServiceProvider provider)
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Hangfire.HangfireServiceCollectionExtensions.ThrowIfNotConfigured(IServiceProvider serviceProvider) in C:\projects\hangfire-525\src\Hangfire.NetCore\HangfireServiceCollectionExtensions.cs:line 307
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Hangfire.HangfireApplicationBuilderExtensions.UseHangfireDashboard(IApplicationBuilder app, String pathMatch, DashboardOptions options, JobStorage storage) in C:\projects\hangfire-525\src\Hangfire.AspNetCore\HangfireApplicationBuilderExtensions.cs:line 44
2026-05-11T14:31:20Z app[4d895d5ec41638] sin [info]   at Program.<Main>$(String[] args) in /src/FootballBlog.API/Program.cs:line 277
2026-05-11T14:31:21Z app[4d895d5ec41638] sin [info] INFO Main child exited normally with code: 0
2026-05-11T14:31:21Z app[4d895d5ec41638] sin [info] INFO Starting clean up.
2026-05-11T14:31:21Z app[4d895d5ec41638] sin [info][    3.215880] reboot: Restarting system
2026-05-11T14:31:21Z runner[4d895d5ec41638] sin [info]machine exited with exit code 0, not restarting
2026-05-11T14:31:24Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.18561466s so far)
2026-05-11T14:31:24Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.228696367s so far)
2026-05-11T14:31:24Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.259621417s so far)
2026-05-11T14:31:24Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.318953039s so far)
2026-05-11T14:31:24Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.452520564s so far)
2026-05-11T14:31:24Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.504482937s so far)
error.message="failed to connect to machine: gave up after 15 attempts (in 8.456981423s)" 2026-05-11T14:31:27Z proxy[4d895d5ec41638] sin [error]
2026-05-11T14:31:27Z proxy[e2862692b79d68] sin [info]Starting machine
error.message="failed to connect to machine: gave up after 15 attempts (in 8.507794792s)" 2026-05-11T14:31:27Z proxy[4d895d5ec41638] sin [error]
2026-05-11T14:31:28Z app[e2862692b79d68] sin [info]2026-05-11T14:31:28.029126912 [01KRBQ3HNZGMPSCDXTQANX4NQM:main] Running Firecracker v1.14.4
2026-05-11T14:31:28Z app[e2862692b79d68] sin [info]2026-05-11T14:31:28.029406428 [01KRBQ3HNZGMPSCDXTQANX4NQM:main] Listening on API socket ("/fc.sock").
2026-05-11T14:31:29Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 10.191192307s so far)
error.message="failed to connect to machine: gave up after 15 attempts (in 10.191363782s)" 2026-05-11T14:31:29Z proxy[4d895d5ec41638] sin [error]
error.message="failed to connect to machine: gave up after 15 attempts (in 10.219527824s)" 2026-05-11T14:31:29Z proxy[4d895d5ec41638] sin [error]
error.message="failed to connect to machine: gave up after 15 attempts (in 10.233641342s)" 2026-05-11T14:31:29Z proxy[4d895d5ec41638] sin [error]
error.message="failed to connect to machine: gave up after 15 attempts (in 10.250586434s)" 2026-05-11T14:31:29Z proxy[4d895d5ec41638] sin [error]
2026-05-11T14:31:29Z app[e2862692b79d68] sin [info]2026/05/11 14:31:29 INFO SSH listening listen_address=[fdaa:73:80de:a7b:85c:3bf:c1b4:2]:22
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info][2026-05-11 14:31:30 FTL]  | Application terminated unexpectedly
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]System.ArgumentException: Connection string is not valid (Parameter 'connectionString')
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info] ---> System.ArgumentException: Couldn't set postgresql://neondb_owner:npg_e0cpeja4rdow@ep-shy-voice-aqrdqaz6-pooler.c-8.us-east-1.aws.neon.tech/neondb?sslmode (Parameter 'postgresql://neondb_owner:npg_e0cpeja4rdow@ep-shy-voice-aqrdqaz6-pooler.c-8.us-east-1.aws.neon.tech/neondb?sslmode')
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info] ---> System.Collections.Generic.KeyNotFoundException: The given key was not present in the dictionary.
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder.GeneratedActions(GeneratedAction action, String keyword, Object& value)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder.set_Item(String keyword, Object value)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   --- End of inner exception stack trace ---
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder.set_Item(String keyword, Object value)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at System.Data.Common.DbConnectionStringBuilder.set_ConnectionString(String value)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder..ctor(String connectionString)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Hangfire.PostgreSql.Factories.NpgsqlInstanceConnectionFactoryBase.SetupConnectionStringBuilder(String connectionString)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   --- End of inner exception stack trace ---
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Hangfire.PostgreSql.Factories.NpgsqlInstanceConnectionFactoryBase.SetupConnectionStringBuilder(String connectionString)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Hangfire.PostgreSql.Factories.NpgsqlConnectionFactory..ctor(String connectionString, PostgreSqlStorageOptions options, Action`1 connectionSetup)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Hangfire.PostgreSql.PostgreSqlBootstrapperOptions.UseNpgsqlConnection(String connectionString, Action`1 connectionSetup)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Program.<>c__DisplayClass0_0.<<Main>$>b__33(PostgreSqlBootstrapperOptions o) in /src/FootballBlog.API/Program.cs:line 223
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Hangfire.PostgreSql.PostgreSqlBootstrapperConfigurationExtensions.UsePostgreSqlStorage(IGlobalConfiguration configuration, Action`1 configure, PostgreSqlStorageOptions options)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Program.<>c__DisplayClass0_0.<<Main>$>b__10(IGlobalConfiguration cfg) in /src/FootballBlog.API/Program.cs:line 218
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Hangfire.HangfireServiceCollectionExtensions.<>c__DisplayClass0_0.<AddHangfire>b__0(IServiceProvider provider, IGlobalConfiguration config) in C:\projects\hangfire-525\src\Hangfire.NetCore\HangfireServiceCollectionExtensions.cs:line 40
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Hangfire.HangfireServiceCollectionExtensions.<>c__DisplayClass1_0.<AddHangfire>b__14(IServiceProvider serviceProvider) in C:\projects\hangfire-525\src\Hangfire.NetCore\HangfireServiceCollectionExtensions.cs:line 103
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitRootCache(ServiceCallSite callSite, RuntimeResolverContext context)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSite(ServiceCallSite callSite, TArgument argument)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.Resolve(ServiceCallSite callSite, ServiceProviderEngineScope scope)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProvider.CreateServiceAccessor(ServiceIdentifier serviceIdentifier)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at System.Collections.Concurrent.ConcurrentDictionary`2.GetOrAdd(TKey key, Func`2 valueFactory)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProvider.GetService(ServiceIdentifier serviceIdentifier, ServiceProviderEngineScope serviceProviderEngineScope)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProvider.GetService(Type serviceType)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService[T](IServiceProvider provider)
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Hangfire.HangfireServiceCollectionExtensions.ThrowIfNotConfigured(IServiceProvider serviceProvider) in C:\projects\hangfire-525\src\Hangfire.NetCore\HangfireServiceCollectionExtensions.cs:line 307
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Hangfire.HangfireApplicationBuilderExtensions.UseHangfireDashboard(IApplicationBuilder app, String pathMatch, DashboardOptions options, JobStorage storage) in C:\projects\hangfire-525\src\Hangfire.AspNetCore\HangfireApplicationBuilderExtensions.cs:line 44
2026-05-11T14:31:30Z app[e2862692b79d68] sin [info]   at Program.<Main>$(String[] args) in /src/FootballBlog.API/Program.cs:line 277
2026-05-11T14:31:30Z app[4d895d5ec41638] sin [info] INFO Starting init (commit: 3040ac0)...
2026-05-11T14:31:31Z app[4d895d5ec41638] sin [info] INFO Preparing to run: `dotnet FootballBlog.API.dll` as root
2026-05-11T14:31:31Z app[4d895d5ec41638] sin [info] INFO [fly api proxy] listening at /.fly/api
2026-05-11T14:31:31Z app[e2862692b79d68] sin [info] INFO Main child exited normally with code: 0
2026-05-11T14:31:31Z app[e2862692b79d68] sin [info] INFO Starting clean up.
2026-05-11T14:31:31Z runner[4d895d5ec41638] sin [info]Machine started in 1.36s
2026-05-11T14:31:31Z proxy[4d895d5ec41638] sin [info]machine started in 1.494429522s
2026-05-11T14:31:31Z app[e2862692b79d68] sin [info][    3.110706] reboot: Restarting system
2026-05-11T14:31:31Z app[4d895d5ec41638] sin [info]2026/05/11 14:31:31 INFO SSH listening listen_address=[fdaa:73:80de:a7b:85a:8a8b:a066:2]:22
2026-05-11T14:31:31Z runner[e2862692b79d68] sin [info]machine exited with exit code 0, not restarting
2026-05-11T14:31:34Z proxy[e2862692b79d68] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.202884723s so far)
2026-05-11T14:31:34Z proxy[e2862692b79d68] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.405555601s so far)
2026-05-11T14:31:36Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.207105227s so far)
2026-05-11T14:31:36Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.478387562s so far)
2026-05-11T14:31:36Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.546542625s so far)
2026-05-11T14:31:36Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.555570715s so far)
2026-05-11T14:31:37Z proxy[e2862692b79d68] sin [error][PM05] failed to connect to machine: gave up after 15 attempts (in 8.205723113s)
2026-05-11T14:31:37Z proxy[e2862692b79d68] sin [error][PM05] failed to connect to machine: gave up after 15 attempts (in 8.409589865s)
2026-05-11T14:31:37Z proxy[4d895d5ec41638] sin [info]Starting machine
2026-05-11T14:31:38Z app[4d895d5ec41638] sin [info]2026-05-11T14:31:38.268860016 [01KRBQ3HW6AEBKMDQK3FP5XA4M:main] Running Firecracker v1.14.4
2026-05-11T14:31:38Z app[4d895d5ec41638] sin [info]2026-05-11T14:31:38.269063139 [01KRBQ3HW6AEBKMDQK3FP5XA4M:main] Listening on API socket ("/fc.sock").
2026-05-11T14:31:39Z app[4d895d5ec41638] sin [info] INFO Starting init (commit: 3040ac0)...
2026-05-11T14:31:39Z app[4d895d5ec41638] sin [info] INFO Preparing to run: `dotnet FootballBlog.API.dll` as root
2026-05-11T14:31:39Z app[4d895d5ec41638] sin [info] INFO [fly api proxy] listening at /.fly/api
2026-05-11T14:31:39Z runner[4d895d5ec41638] sin [info]Machine started in 1.553s
2026-05-11T14:31:39Z proxy[4d895d5ec41638] sin [info]machine started in 1.690581603s
2026-05-11T14:31:39Z proxy[4d895d5ec41638] sin [error][PM05] failed to connect to machine: gave up after 15 attempts (in 8.488664141s)
2026-05-11T14:31:39Z proxy[4d895d5ec41638] sin [error][PM05] failed to connect to machine: gave up after 15 attempts (in 8.562169303s)
2026-05-11T14:31:39Z app[4d895d5ec41638] sin [info]2026/05/11 14:31:39 INFO SSH listening listen_address=[fdaa:73:80de:a7b:85a:8a8b:a066:2]:22
2026-05-11T14:31:39Z proxy[e2862692b79d68] sin [info]Starting machine
2026-05-11T14:31:40Z app[e2862692b79d68] sin [info]2026-05-11T14:31:40.169201354 [01KRBQ3HNZGMPSCDXTQANX4NQM:main] Running Firecracker v1.14.4
2026-05-11T14:31:40Z app[e2862692b79d68] sin [info]2026-05-11T14:31:40.169417796 [01KRBQ3HNZGMPSCDXTQANX4NQM:main] Listening on API socket ("/fc.sock").
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info][2026-05-11 14:31:40 FTL]  | Application terminated unexpectedly
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]System.ArgumentException: Connection string is not valid (Parameter 'connectionString')
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info] ---> System.ArgumentException: Couldn't set postgresql://neondb_owner:npg_e0cpeja4rdow@ep-shy-voice-aqrdqaz6-pooler.c-8.us-east-1.aws.neon.tech/neondb?sslmode (Parameter 'postgresql://neondb_owner:npg_e0cpeja4rdow@ep-shy-voice-aqrdqaz6-pooler.c-8.us-east-1.aws.neon.tech/neondb?sslmode')
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info] ---> System.Collections.Generic.KeyNotFoundException: The given key was not present in the dictionary.
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder.GeneratedActions(GeneratedAction action, String keyword, Object& value)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder.set_Item(String keyword, Object value)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   --- End of inner exception stack trace ---
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder.set_Item(String keyword, Object value)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at System.Data.Common.DbConnectionStringBuilder.set_ConnectionString(String value)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder..ctor(String connectionString)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Hangfire.PostgreSql.Factories.NpgsqlInstanceConnectionFactoryBase.SetupConnectionStringBuilder(String connectionString)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   --- End of inner exception stack trace ---
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Hangfire.PostgreSql.Factories.NpgsqlInstanceConnectionFactoryBase.SetupConnectionStringBuilder(String connectionString)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Hangfire.PostgreSql.Factories.NpgsqlConnectionFactory..ctor(String connectionString, PostgreSqlStorageOptions options, Action`1 connectionSetup)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Hangfire.PostgreSql.PostgreSqlBootstrapperOptions.UseNpgsqlConnection(String connectionString, Action`1 connectionSetup)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Program.<>c__DisplayClass0_0.<<Main>$>b__33(PostgreSqlBootstrapperOptions o) in /src/FootballBlog.API/Program.cs:line 223
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Hangfire.PostgreSql.PostgreSqlBootstrapperConfigurationExtensions.UsePostgreSqlStorage(IGlobalConfiguration configuration, Action`1 configure, PostgreSqlStorageOptions options)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Program.<>c__DisplayClass0_0.<<Main>$>b__10(IGlobalConfiguration cfg) in /src/FootballBlog.API/Program.cs:line 218
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Hangfire.HangfireServiceCollectionExtensions.<>c__DisplayClass0_0.<AddHangfire>b__0(IServiceProvider provider, IGlobalConfiguration config) in C:\projects\hangfire-525\src\Hangfire.NetCore\HangfireServiceCollectionExtensions.cs:line 40
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Hangfire.HangfireServiceCollectionExtensions.<>c__DisplayClass1_0.<AddHangfire>b__14(IServiceProvider serviceProvider) in C:\projects\hangfire-525\src\Hangfire.NetCore\HangfireServiceCollectionExtensions.cs:line 103
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitRootCache(ServiceCallSite callSite, RuntimeResolverContext context)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSite(ServiceCallSite callSite, TArgument argument)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.Resolve(ServiceCallSite callSite, ServiceProviderEngineScope scope)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProvider.CreateServiceAccessor(ServiceIdentifier serviceIdentifier)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at System.Collections.Concurrent.ConcurrentDictionary`2.GetOrAdd(TKey key, Func`2 valueFactory)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProvider.GetService(ServiceIdentifier serviceIdentifier, ServiceProviderEngineScope serviceProviderEngineScope)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProvider.GetService(Type serviceType)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService[T](IServiceProvider provider)
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Hangfire.HangfireServiceCollectionExtensions.ThrowIfNotConfigured(IServiceProvider serviceProvider) in C:\projects\hangfire-525\src\Hangfire.NetCore\HangfireServiceCollectionExtensions.cs:line 307
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Hangfire.HangfireApplicationBuilderExtensions.UseHangfireDashboard(IApplicationBuilder app, String pathMatch, DashboardOptions options, JobStorage storage) in C:\projects\hangfire-525\src\Hangfire.AspNetCore\HangfireApplicationBuilderExtensions.cs:line 44
2026-05-11T14:31:40Z app[4d895d5ec41638] sin [info]   at Program.<Main>$(String[] args) in /src/FootballBlog.API/Program.cs:line 277
2026-05-11T14:31:41Z app[e2862692b79d68] sin [info] INFO Starting init (commit: 3040ac0)...
2026-05-11T14:31:41Z app[e2862692b79d68] sin [info] INFO Preparing to run: `dotnet FootballBlog.API.dll` as root
2026-05-11T14:31:41Z app[e2862692b79d68] sin [info] INFO [fly api proxy] listening at /.fly/api
2026-05-11T14:31:41Z proxy[4d895d5ec41638] sin [error][PM05] failed to connect to machine: gave up after 15 attempts (in 10.200427757s)
2026-05-11T14:31:41Z app[4d895d5ec41638] sin [info] INFO Main child exited normally with code: 0
2026-05-11T14:31:41Z app[4d895d5ec41638] sin [info] INFO Starting clean up.
2026-05-11T14:31:41Z runner[e2862692b79d68] sin [info]Machine started in 1.403s
2026-05-11T14:31:41Z proxy[e2862692b79d68] sin [info]machine started in 1.558026703s
2026-05-11T14:31:41Z proxy[e2862692b79d68] sin [error][PC01] instance refused connection. is your app listening on 0.0.0.0:8080? make sure it is not only listening on 127.0.0.1 (hint: look at your startup logs, servers often print the address they are listening on)
2026-05-11T14:31:41Z app[4d895d5ec41638] sin [info][    3.200226] reboot: Restarting system
2026-05-11T14:31:41Z app[e2862692b79d68] sin [info]2026/05/11 14:31:41 INFO SSH listening listen_address=[fdaa:73:80de:a7b:85c:3bf:c1b4:2]:22
2026-05-11T14:31:41Z runner[4d895d5ec41638] sin [info]machine exited with exit code 0, not restarting
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info][2026-05-11 14:31:42 FTL]  | Application terminated unexpectedly
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]System.ArgumentException: Connection string is not valid (Parameter 'connectionString')
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info] ---> System.ArgumentException: Couldn't set postgresql://neondb_owner:npg_e0cpeja4rdow@ep-shy-voice-aqrdqaz6-pooler.c-8.us-east-1.aws.neon.tech/neondb?sslmode (Parameter 'postgresql://neondb_owner:npg_e0cpeja4rdow@ep-shy-voice-aqrdqaz6-pooler.c-8.us-east-1.aws.neon.tech/neondb?sslmode')
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info] ---> System.Collections.Generic.KeyNotFoundException: The given key was not present in the dictionary.
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder.GeneratedActions(GeneratedAction action, String keyword, Object& value)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder.set_Item(String keyword, Object value)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   --- End of inner exception stack trace ---
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder.set_Item(String keyword, Object value)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at System.Data.Common.DbConnectionStringBuilder.set_ConnectionString(String value)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Npgsql.NpgsqlConnectionStringBuilder..ctor(String connectionString)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Hangfire.PostgreSql.Factories.NpgsqlInstanceConnectionFactoryBase.SetupConnectionStringBuilder(String connectionString)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   --- End of inner exception stack trace ---
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Hangfire.PostgreSql.Factories.NpgsqlInstanceConnectionFactoryBase.SetupConnectionStringBuilder(String connectionString)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Hangfire.PostgreSql.Factories.NpgsqlConnectionFactory..ctor(String connectionString, PostgreSqlStorageOptions options, Action`1 connectionSetup)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Hangfire.PostgreSql.PostgreSqlBootstrapperOptions.UseNpgsqlConnection(String connectionString, Action`1 connectionSetup)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Program.<>c__DisplayClass0_0.<<Main>$>b__33(PostgreSqlBootstrapperOptions o) in /src/FootballBlog.API/Program.cs:line 223
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Hangfire.PostgreSql.PostgreSqlBootstrapperConfigurationExtensions.UsePostgreSqlStorage(IGlobalConfiguration configuration, Action`1 configure, PostgreSqlStorageOptions options)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Program.<>c__DisplayClass0_0.<<Main>$>b__10(IGlobalConfiguration cfg) in /src/FootballBlog.API/Program.cs:line 218
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Hangfire.HangfireServiceCollectionExtensions.<>c__DisplayClass0_0.<AddHangfire>b__0(IServiceProvider provider, IGlobalConfiguration config) in C:\projects\hangfire-525\src\Hangfire.NetCore\HangfireServiceCollectionExtensions.cs:line 40
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Hangfire.HangfireServiceCollectionExtensions.<>c__DisplayClass1_0.<AddHangfire>b__14(IServiceProvider serviceProvider) in C:\projects\hangfire-525\src\Hangfire.NetCore\HangfireServiceCollectionExtensions.cs:line 103
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitRootCache(ServiceCallSite callSite, RuntimeResolverContext context)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSite(ServiceCallSite callSite, TArgument argument)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.Resolve(ServiceCallSite callSite, ServiceProviderEngineScope scope)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProvider.CreateServiceAccessor(ServiceIdentifier serviceIdentifier)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at System.Collections.Concurrent.ConcurrentDictionary`2.GetOrAdd(TKey key, Func`2 valueFactory)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProvider.GetService(ServiceIdentifier serviceIdentifier, ServiceProviderEngineScope serviceProviderEngineScope)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProvider.GetService(Type serviceType)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService[T](IServiceProvider provider)
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Hangfire.HangfireServiceCollectionExtensions.ThrowIfNotConfigured(IServiceProvider serviceProvider) in C:\projects\hangfire-525\src\Hangfire.NetCore\HangfireServiceCollectionExtensions.cs:line 307
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Hangfire.HangfireApplicationBuilderExtensions.UseHangfireDashboard(IApplicationBuilder app, String pathMatch, DashboardOptions options, JobStorage storage) in C:\projects\hangfire-525\src\Hangfire.AspNetCore\HangfireApplicationBuilderExtensions.cs:line 44
2026-05-11T14:31:42Z app[e2862692b79d68] sin [info]   at Program.<Main>$(String[] args) in /src/FootballBlog.API/Program.cs:line 277
2026-05-11T14:31:43Z app[e2862692b79d68] sin [info] INFO Main child exited normally with code: 0
2026-05-11T14:31:43Z app[e2862692b79d68] sin [info] INFO Starting clean up.
2026-05-11T14:31:43Z app[e2862692b79d68] sin [info][    3.147912] reboot: Restarting system
2026-05-11T14:31:43Z proxy[4d895d5ec41638] sin [error][PM05] failed to connect to machine: gave up after 15 attempts (in 12.480516145s)
2026-05-11T14:31:43Z runner[e2862692b79d68] sin [info]machine exited with exit code 0, not restarting
2026-05-11T14:31:44Z proxy[4d895d5ec41638] sin [info]Starting machine
2026-05-11T14:31:44Z app[4d895d5ec41638] sin [info]2026-05-11T14:31:44.661738283 [01KRBQ3HW6AEBKMDQK3FP5XA4M:main] Running Firecracker v1.14.4
2026-05-11T14:31:44Z app[4d895d5ec41638] sin [info]2026-05-11T14:31:44.661945734 [01KRBQ3HW6AEBKMDQK3FP5XA4M:main] Listening on API socket ("/fc.sock").
2026-05-11T14:31:45Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.472898707s so far)
2026-05-11T14:31:45Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.56956699s so far)
2026-05-11T14:31:45Z app[4d895d5ec41638] sin [info] INFO Starting init (commit: 3040ac0)...
2026-05-11T14:31:45Z app[4d895d5ec41638] sin [info] INFO Preparing to run: `dotnet FootballBlog.API.dll` as root
2026-05-11T14:31:45Z app[4d895d5ec41638] sin [info] INFO [fly api proxy] listening at /.fly/api
2026-05-11T14:31:45Z runner[4d895d5ec41638] sin [info]Machine started in 1.316s
2026-05-11T14:31:45Z proxy[4d895d5ec41638] sin [info]machine started in 1.471102075s
