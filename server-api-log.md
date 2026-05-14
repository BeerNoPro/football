2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    SqlState: 42703
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    MessageText: column m.EtAwayScore does not exist
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    Hint: Perhaps you meant to reference the column "m.AwayScore".
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    Position: 556
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    File: parse_relation.c
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    Line: 3723
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    Routine: errorMissingColumn
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info][2026-05-11 14:53:05 ERR] Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware | An unhandled exception has occurred while executing the request.
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]Npgsql.PostgresException (0x80004005): 42703: column m.EtAwayScore does not exist
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]POSITION: 556
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Npgsql.NpgsqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at FootballBlog.API.Controllers.FixturesController.GetAll(Nullable`1 leagueId, Nullable`1 date, Nullable`1 fromDate, Nullable`1 toDate, String status, String season, String search, Boolean sortAsc, Int32 page, Int32 pageSize) in /src/FootballBlog.API/Controllers/FixturesController.cs:line 100
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at lambda_method175(Closure, Object)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.AwaitableObjectResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIMiddleware.Invoke(HttpContext httpContext)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Swashbuckle.AspNetCore.Swagger.SwaggerMiddleware.Invoke(HttpContext httpContext, ISwaggerProvider swaggerProvider)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]  Exception data:
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    Severity: ERROR
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    SqlState: 42703
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    MessageText: column m.EtAwayScore does not exist
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    Hint: Perhaps you meant to reference the column "m.AwayScore".
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    Position: 556
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    File: parse_relation.c
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    Line: 3723
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info]    Routine: errorMissingColumn
2026-05-11T14:53:05Z app[4d895d5ec41638] sin [info][2026-05-11 14:53:05 ERR] Serilog.AspNetCore.RequestLoggingMiddleware | HTTP GET /api/fixtures responded 500 in 596.4946 ms
2026-05-11T14:53:11Z app[e2862692b79d68] sin [info][2026-05-11 14:53:11 ERR] Microsoft.AspNetCore.SignalR.StackExchangeRedis.RedisHubLifetimeManager | Error connecting to Redis.
2026-05-11T14:53:11Z app[e2862692b79d68] sin [info]StackExchange.Redis.RedisConnectionException: It was not possible to connect to the redis server(s). Error connecting right now. To allow this multiplexer to continue retrying until it's able to connect, use abortConnect=false in your connection string or AbortOnConnectFail=false; in your code.
2026-05-11T14:53:11Z app[e2862692b79d68] sin [info]   at StackExchange.Redis.ConnectionMultiplexer.ConnectImplAsync(ConfigurationOptions configuration, TextWriter writer, Nullable`1 serverType) in /_/src/StackExchange.Redis/ConnectionMultiplexer.cs:line 598
2026-05-11T14:53:11Z app[e2862692b79d68] sin [info]   at Microsoft.AspNetCore.SignalR.StackExchangeRedis.RedisOptions.ConnectAsync(TextWriter log)
2026-05-11T14:53:11Z app[e2862692b79d68] sin [info]   at Microsoft.AspNetCore.SignalR.StackExchangeRedis.RedisHubLifetimeManager`1.EnsureRedisServerConnection()
2026-05-11T14:56:10Z app[4d895d5ec41638] sin [info][2026-05-11 14:56:10 INF] Serilog.AspNetCore.RequestLoggingMiddleware | HTTP GET /api/leagues responded 200 in 234.5135 ms
2026-05-11T14:56:10Z app[4d895d5ec41638] sin [info][2026-05-11 14:56:10 INF] Serilog.AspNetCore.RequestLoggingMiddleware | HTTP GET /api/leagues responded 200 in 263.7145 ms
2026-05-11T14:56:10Z app[e2862692b79d68] sin [info][2026-05-11 14:56:10 WRN] Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware | Failed to determine the https port for redirect.
2026-05-11T14:56:10Z app[4d895d5ec41638] sin [info][2026-05-11 14:56:10 INF] Serilog.AspNetCore.RequestLoggingMiddleware | HTTP GET /api/fixtures responded 200 in 542.1254 ms
2026-05-11T14:56:11Z app[4d895d5ec41638] sin [info][2026-05-11 14:56:11 INF] Serilog.AspNetCore.RequestLoggingMiddleware | HTTP GET /api/posts responded 200 in 623.1288 ms
2026-05-11T14:56:11Z app[e2862692b79d68] sin [info][2026-05-11 14:56:11 INF] Serilog.AspNetCore.RequestLoggingMiddleware | HTTP GET /api/fixtures responded 200 in 1167.1094 ms
2026-05-11T14:56:11Z app[e2862692b79d68] sin [info][2026-05-11 14:56:11 INF] Serilog.AspNetCore.RequestLoggingMiddleware | HTTP GET /api/fixtures responded 200 in 1175.0342 ms
2026-05-11T14:56:14Z app[4d895d5ec41638] sin [info][2026-05-11 14:56:14 INF] Serilog.AspNetCore.RequestLoggingMiddleware | HTTP GET /api/leagues responded 200 in 248.6102 ms
2026-05-11T14:56:14Z app[4d895d5ec41638] sin [info][2026-05-11 14:56:14 INF] Serilog.AspNetCore.RequestLoggingMiddleware | HTTP GET /api/fixtures responded 200 in 476.5561 ms
2026-05-11T14:56:14Z app[4d895d5ec41638] sin [info][2026-05-11 14:56:14 INF] Serilog.AspNetCore.RequestLoggingMiddleware | HTTP GET /api/fixtures responded 200 in 480.5911 ms
2026-05-11T14:56:14Z app[4d895d5ec41638] sin [info][2026-05-11 14:56:14 INF] Serilog.AspNetCore.RequestLoggingMiddleware | HTTP GET /api/fixtures responded 200 in 484.4041 ms
2026-05-11T14:57:34Z runner[4d895d5ec41638] sin [warn]Trial machine stopping. To run for longer than 5m0s, add a credit card by visiting https://fly.io/trial.
2026-05-11T14:57:34Z app[4d895d5ec41638] sin [info] INFO Sending signal SIGINT to main child process w/ PID 636
2026-05-11T14:57:34Z app[4d895d5ec41638] sin [info][2026-05-11 14:57:34 INF] Hangfire.Server.BackgroundServerProcess | Server 4d895d5ec41638:636:521321eb caught stopping signal...
2026-05-11T14:57:34Z app[4d895d5ec41638] sin [info][2026-05-11 14:57:34 INF] Hangfire.Server.BackgroundServerProcess | Server 4d895d5ec41638:636:521321eb All dispatchers stopped
2026-05-11T14:57:35Z app[4d895d5ec41638] sin [info][2026-05-11 14:57:35 INF] Hangfire.Server.BackgroundServerProcess | Server 4d895d5ec41638:636:521321eb successfully reported itself as stopped in 242.311 ms
2026-05-11T14:57:35Z app[4d895d5ec41638] sin [info][2026-05-11 14:57:35 INF] Hangfire.Server.BackgroundServerProcess | Server 4d895d5ec41638:636:521321eb has been stopped in total 281.6955 ms
2026-05-11T14:57:36Z app[4d895d5ec41638] sin [info] INFO Main child exited normally with code: 0
2026-05-11T14:57:36Z app[4d895d5ec41638] sin [info] INFO Starting clean up.
2026-05-11T14:57:36Z app[4d895d5ec41638] sin [info][  302.420960] reboot: Restarting system
2026-05-11T14:57:44Z runner[e2862692b79d68] sin [warn]Trial machine stopping. To run for longer than 5m0s, add a credit card by visiting https://fly.io/trial.
2026-05-11T14:57:44Z app[e2862692b79d68] sin [info] INFO Sending signal SIGINT to main child process w/ PID 636
2026-05-11T14:57:45Z app[e2862692b79d68] sin [info][2026-05-11 14:57:45 INF] Hangfire.Server.BackgroundServerProcess | Server e2862692b79d68:636:8b7fd5c2 caught stopping signal...
2026-05-11T14:57:45Z app[e2862692b79d68] sin [info][2026-05-11 14:57:45 INF] Hangfire.Server.BackgroundServerProcess | Server e2862692b79d68:636:8b7fd5c2 All dispatchers stopped
2026-05-11T14:57:45Z app[e2862692b79d68] sin [info][2026-05-11 14:57:45 INF] Hangfire.Server.BackgroundServerProcess | Server e2862692b79d68:636:8b7fd5c2 successfully reported itself as stopped in 233.7626 ms
2026-05-11T14:57:45Z app[e2862692b79d68] sin [info][2026-05-11 14:57:45 INF] Hangfire.Server.BackgroundServerProcess | Server e2862692b79d68:636:8b7fd5c2 has been stopped in total 278.5585 ms
2026-05-11T14:57:46Z app[e2862692b79d68] sin [info] INFO Main child exited normally with code: 0
2026-05-11T14:57:46Z app[e2862692b79d68] sin [info] INFO Starting clean up.
2026-05-11T14:57:46Z app[e2862692b79d68] sin [info][  302.395485] reboot: Restarting system
2026-05-11T14:58:05Z runner[4d895d5ec41638] sin [info]Pulling container image registry.fly.io/footballblog-api@sha256:f12fdf44de842fb01daf0ebe3e246e6fed71a3aaf6c251bb3b62b2d0de484cce
2026-05-11T14:58:05Z runner[4d895d5ec41638] sin [info]Container image registry.fly.io/footballblog-api@sha256:f12fdf44de842fb01daf0ebe3e246e6fed71a3aaf6c251bb3b62b2d0de484cce already prepared
2026-05-11T14:58:06Z runner[4d895d5ec41638] sin [info]Configuring firecracker
2026-05-11T14:58:07Z runner[e2862692b79d68] sin [info]Pulling container image registry.fly.io/footballblog-api@sha256:f12fdf44de842fb01daf0ebe3e246e6fed71a3aaf6c251bb3b62b2d0de484cce
2026-05-11T14:58:07Z runner[e2862692b79d68] sin [info]Container image registry.fly.io/footballblog-api@sha256:f12fdf44de842fb01daf0ebe3e246e6fed71a3aaf6c251bb3b62b2d0de484cce already prepared
2026-05-11T14:58:08Z runner[e2862692b79d68] sin [info]Configuring firecracker
2026-05-11T14:58:42Z proxy[4d895d5ec41638] sin [info]Starting machine
2026-05-11T14:58:42Z app[4d895d5ec41638] sin [info]2026-05-11T14:58:42.710332740 [01KRBRQ6EJ4MG3QBAQ3RYFHKR3:main] Running Firecracker v1.14.4
2026-05-11T14:58:42Z app[4d895d5ec41638] sin [info]2026-05-11T14:58:42.710537571 [01KRBRQ6EJ4MG3QBAQ3RYFHKR3:main] Listening on API socket ("/fc.sock").
2026-05-11T14:58:43Z app[4d895d5ec41638] sin [info] INFO Starting init (commit: 3040ac0)...
2026-05-11T14:58:43Z app[4d895d5ec41638] sin [info] INFO Preparing to run: `dotnet FootballBlog.API.dll` as root
2026-05-11T14:58:43Z app[4d895d5ec41638] sin [info] INFO [fly api proxy] listening at /.fly/api
2026-05-11T14:58:44Z runner[4d895d5ec41638] sin [info]Machine started in 1.397s
2026-05-11T14:58:44Z proxy[4d895d5ec41638] sin [info]machine started in 1.546911874s
2026-05-11T14:58:44Z app[4d895d5ec41638] sin [info]2026/05/11 14:58:44 INFO SSH listening listen_address=[fdaa:73:80de:a7b:85a:8a8b:a066:2]:22
2026-05-11T14:58:48Z app[4d895d5ec41638] sin [info][2026-05-11 14:58:48 INF] Hangfire.PostgreSql.PostgreSqlStorage | Start installing Hangfire SQL objects...
2026-05-11T14:58:49Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.133424476s so far)
2026-05-11T14:58:49Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.208746379s so far)
2026-05-11T14:58:49Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.245055843s so far)
2026-05-11T14:58:49Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.346016063s so far)
2026-05-11T14:58:49Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.377090868s so far)
2026-05-11T14:58:49Z proxy[4d895d5ec41638] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.494217313s so far)
error.message="failed to connect to machine: gave up after 15 attempts (in 8.143978001s)" 2026-05-11T14:58:52Z proxy[4d895d5ec41638] sin [error]
2026-05-11T14:58:52Z proxy[e2862692b79d68] sin [info]Starting machine
error.message="failed to connect to machine: gave up after 15 attempts (in 8.220466771s)" 2026-05-11T14:58:52Z proxy[4d895d5ec41638] sin [error]
error.message="failed to connect to machine: gave up after 15 attempts (in 8.255370431s)" 2026-05-11T14:58:52Z proxy[4d895d5ec41638] sin [error]
error.message="failed to connect to machine: gave up after 15 attempts (in 8.360198363s)" 2026-05-11T14:58:52Z proxy[4d895d5ec41638] sin [error]
error.message="failed to connect to machine: gave up after 15 attempts (in 8.369089927s)" 2026-05-11T14:58:52Z proxy[4d895d5ec41638] sin [error]
2026-05-11T14:58:52Z app[e2862692b79d68] sin [info]2026-05-11T14:58:52.420714670 [01KRBRQ8AWD8AQWG9F3C39Q0EW:main] Running Firecracker v1.14.4
2026-05-11T14:58:52Z app[e2862692b79d68] sin [info]2026-05-11T14:58:52.420942793 [01KRBRQ8AWD8AQWG9F3C39Q0EW:main] Listening on API socket ("/fc.sock").
error.message="failed to connect to machine: gave up after 15 attempts (in 8.505043654s)" 2026-05-11T14:58:52Z proxy[4d895d5ec41638] sin [error]
2026-05-11T14:58:57Z app[e2862692b79d68] sin [info][2026-05-11 14:58:57 INF] Hangfire.PostgreSql.PostgreSqlStorage | Start installing Hangfire SQL objects...
2026-05-11T14:58:58Z proxy[e2862692b79d68] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.220391483s so far)
2026-05-11T14:58:58Z proxy[e2862692b79d68] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.272393298s so far)
2026-05-11T14:58:59Z proxy[e2862692b79d68] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.400123974s so far)
2026-05-11T14:58:59Z proxy[e2862692b79d68] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.400235611s so far)
2026-05-11T14:58:59Z proxy[e2862692b79d68] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.462543586s so far)
2026-05-11T14:58:59Z proxy[e2862692b79d68] sin [info]waiting for machine to be reachable on 0.0.0.0:8080 (waited 5.595141009s so far)
2026-05-11T14:59:01Z proxy[e2862692b79d68] sin [error][PM05] failed to connect to machine: gave up after 15 attempts (in 8.102201247s)
2026-05-11T14:59:01Z proxy[e2862692b79d68] sin [error][PM05] failed to connect to machine: gave up after 15 attempts (in 8.231087357s)
2026-05-11T14:59:01Z proxy[e2862692b79d68] sin [error][PM05] failed to connect to machine: gave up after 15 attempts (in 8.276456198s)
2026-05-11T14:59:02Z proxy[e2862692b79d68] sin [error][PM05] failed to connect to machine: gave up after 15 attempts (in 8.398191653s)
2026-05-11T14:59:02Z proxy[e2862692b79d68] sin [error][PM05] failed to connect to machine: gave up after 15 attempts (in 8.409270591s)
2026-05-11T14:59:02Z proxy[e2862692b79d68] sin [error][PC01] instance refused connection. is your app listening on 0.0.0.0:8080? make sure it is not only listening on 127.0.0.1 (hint: look at your startup logs, servers often print the address they are listening on)
2026-05-11T14:59:02Z proxy[e2862692b79d68] sin [error][PM05] failed to connect to machine: gave up after 15 attempts (in 8.603605132s)
2026-05-11T14:59:02Z app[e2862692b79d68] sin [info][2026-05-11 14:59:02 INF] Hangfire.PostgreSql.PostgreSqlStorage | Hangfire SQL objects installed.
2026-05-11T14:59:02Z proxy[4d895d5ec41638] sin [error][PC01] instance refused connection. is your app listening on 0.0.0.0:8080? make sure it is not only listening on 127.0.0.1 (hint: look at your startup logs, servers often print the address they are listening on)
2026-05-11T14:59:05Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:05 WRN] Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository | Storing keys in a directory '/root/.aspnet/DataProtection-Keys' that may not be persisted outside of the container. Protected data will be unavailable when container is destroyed. For more information go to https://aka.ms/aspnet/dataprotectionwarning
2026-05-11T14:59:07Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:07 INF]  | FootballBlog API starting up
2026-05-11T14:59:07Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:07 WRN] Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager | No XML encryptor configured. Key {860a26b2-ccf8-45fc-8b96-b60f2a3cf6c2} may be persisted to storage in unencrypted form.
2026-05-11T14:59:07Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:07 WRN] Microsoft.AspNetCore.Hosting.Diagnostics | Overriding HTTP_PORTS '8080' and HTTPS_PORTS ''. Binding to values defined by URLS instead 'http://+:8080'.
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:08 INF] Hangfire.BackgroundJobServer | Starting Hangfire Server using job storage: 'PostgreSQL Server: Host: ep-shy-voice-aqrdqaz6-pooler.c-8.us-east-1.aws.neon.tech, DB: neondb, Schema: hangfire'
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:08 INF] Hangfire.BackgroundJobServer | Using the following options for PostgreSQL job storage:
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:08 INF] Hangfire.BackgroundJobServer |     Queue poll interval: 00:00:15.
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:08 INF] Hangfire.BackgroundJobServer |     Invisibility timeout: 00:30:00.
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:08 INF] Hangfire.BackgroundJobServer |     Use sliding invisibility timeout: False.
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:08 INF] Hangfire.BackgroundJobServer | Using the following options for Hangfire Server:
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info]    Worker count: 2
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info]    Listening queues: 'default'
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info]    Shutdown timeout: 00:00:15
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info]    Schedule polling interval: 00:00:15
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:08 INF] Hangfire.Server.BackgroundServerProcess | Server 4d895d5ec41638:637:c4383bb2 successfully announced in 349.3508 ms
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:08 INF] Hangfire.Server.BackgroundServerProcess | Server 4d895d5ec41638:637:c4383bb2 is starting the registered dispatchers: ServerWatchdog, ServerJobCancellationWatcher, ExpirationManager, CountersAggregator, Worker, DelayedJobScheduler, RecurringJobScheduler...
2026-05-11T14:59:08Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:08 INF] Hangfire.Server.BackgroundServerProcess | Server 4d895d5ec41638:637:c4383bb2 all the dispatchers started
2026-05-11T14:59:11Z app[e2862692b79d68] sin [info][2026-05-11 14:59:11 WRN] Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository | Storing keys in a directory '/root/.aspnet/DataProtection-Keys' that may not be persisted outside of the container. Protected data will be unavailable when container is destroyed. For more information go to https://aka.ms/aspnet/dataprotectionwarning
2026-05-11T14:59:12Z proxy[e2862692b79d68] sin [error][PC01] instance refused connection. is your app listening on 0.0.0.0:8080? make sure it is not only listening on 127.0.0.1 (hint: look at your startup logs, servers often print the address they are listening on)
2026-05-11T14:59:13Z app[4d895d5ec41638] sin [info][2026-05-11 14:59:13 WRN] Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware | Failed to determine the https port for redirect.
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info][2026-05-11 14:59:13 INF]  | FootballBlog API starting up
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info][2026-05-11 14:59:13 WRN] Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager | No XML encryptor configured. Key {036f8347-84f1-44bc-adbd-e8e4f0a442a6} may be persisted to storage in unencrypted form.
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info][2026-05-11 14:59:13 WRN] Microsoft.AspNetCore.Hosting.Diagnostics | Overriding HTTP_PORTS '8080' and HTTPS_PORTS ''. Binding to values defined by URLS instead 'http://+:8080'.
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info][2026-05-11 14:59:13 INF] Hangfire.BackgroundJobServer | Starting Hangfire Server using job storage: 'PostgreSQL Server: Host: ep-shy-voice-aqrdqaz6-pooler.c-8.us-east-1.aws.neon.tech, DB: neondb, Schema: hangfire'
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info][2026-05-11 14:59:13 INF] Hangfire.BackgroundJobServer | Using the following options for PostgreSQL job storage:
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info][2026-05-11 14:59:13 INF] Hangfire.BackgroundJobServer |     Queue poll interval: 00:00:15.
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info][2026-05-11 14:59:13 INF] Hangfire.BackgroundJobServer |     Invisibility timeout: 00:30:00.
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info][2026-05-11 14:59:13 INF] Hangfire.BackgroundJobServer |     Use sliding invisibility timeout: False.
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info][2026-05-11 14:59:13 INF] Hangfire.BackgroundJobServer | Using the following options for Hangfire Server:
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info]    Worker count: 2
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info]    Listening queues: 'default'
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info]    Shutdown timeout: 00:00:15
2026-05-11T14:59:13Z app[e2862692b79d68] sin [info]    Schedule polling interval: 00:00:15
