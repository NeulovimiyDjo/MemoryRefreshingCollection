private class SemaphoreScope : IDisposable
{
	private SemaphoreSlim _semaphore;
	private bool _entered = false;
	public SemaphoreScope(SemaphoreSlim semaphore)
	{
		_semaphore = semaphore;
	}
	public async Task Enter()
	{
		await _semaphore.WaitAsync();
		_entered = true;
	}
	public void Dispose()
	{
		if (_entered)
			_semaphore.Release();
		_entered = false;
	}
}