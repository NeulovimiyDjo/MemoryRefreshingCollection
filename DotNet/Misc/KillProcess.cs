private void KillProcesses(targetExePath)
{
	Process[] runningProcesses = Process.GetProcesses();
	List<Process> killTargets = new();
	foreach (Process process in runningProcesses)
	{
		if (process.ProcessName == Path.GetFileNameWithoutExtension(targetExePath) &&
			process.MainModule != null &&
			string.Compare(process.MainModule.FileName, targetExePath, StringComparison.InvariantCultureIgnoreCase) == 0)
		{
			process.Kill();
			killTargets.Add(process);
		}
	}

	foreach (Process process in killTargets)
		process.WaitForExit();
}