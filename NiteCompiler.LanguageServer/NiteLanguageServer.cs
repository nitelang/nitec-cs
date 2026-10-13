namespace NiteCompiler.LanguageServer;

internal sealed class NiteLanguageServer : INiteLanguageServer
{
	private readonly TaskCompletionSource _exited = new(TaskCreationOptions.RunContinuationsAsynchronously);

	public bool ShutdownRequested { get; private set; }

	public Task Exited => _exited.Task;

	public Task<InitializeResult> InitializeAsync(InitializeParams? parameters)
	{
		// TODO: track initialization state and use the client's advertised capabilities.
		return Task.FromResult(new InitializeResult
		{
			ServerInfo = new ServerInfo { Version = typeof(NiteLanguageServer).Assembly.InformationalVersion },
		});
	}

	public Task InitializedAsync(InitializedParams? parameters)
	{
		return Task.CompletedTask;
	}

	public Task ShutdownAsync()
	{
		ShutdownRequested = true;
		return Task.CompletedTask;
	}

	public Task ExitAsync()
	{
		_exited.TrySetResult();
		return Task.CompletedTask;
	}
}