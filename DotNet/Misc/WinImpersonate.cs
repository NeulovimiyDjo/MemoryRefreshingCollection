public class Impersonator : IDisposable
{
	[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
	int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

	const int LOGON32_PROVIDER_DEFAULT = 0;
	//This parameter causes LogonUser to create a primary token.   
	const int LOGON32_LOGON_INTERACTIVE = 2;
	SafeAccessTokenHandle safeAccessTokenHandle;

	public Impersonator(string domain, string userName, string password)
	{
		bool returnValue = LogonUser(userName, domain, password,
			LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
			out safeAccessTokenHandle);

		if (returnValue == false)
		{
			int ret = Marshal.GetLastWin32Error();
			throw new System.ComponentModel.Win32Exception(ret);
		}
	}

	public void Run(Action act)
	{
		WindowsIdentity.RunImpersonated(safeAccessTokenHandle, act);
	}

	public async Task RunAsync(Func<Task> act)
	{
		await WindowsIdentity.RunImpersonated(safeAccessTokenHandle, act);
	}

	public void Dispose()
	{
		safeAccessTokenHandle?.Dispose();
	}
}



protected async Task<HttpWebResponse> PostRequestAsync(
	string url,
	CookieContainer cookies,
	X509Certificate2Collection certificates,
	string body,
	Dictionary<string, string> headers)
{
	HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
	request.Method = "POST";

	UTF8Encoding encoding = new UTF8Encoding();
	byte[] bytes = encoding.GetBytes(body);

	var stream = request.GetRequestStream();
	stream.Write(bytes, 0, bytes.Length);


	request.CookieContainer = cookies;
	request.ClientCertificates = certificates;
	//request.ContentType = "application/json";
	request.ContentLength = bytes.Length;
	foreach (var pair in headers)
	{
		request.Headers.Add(pair.Key, pair.Value);
	}
	HttpWebResponse response;
	try
	{
		response = (HttpWebResponse)await Task<WebResponse>.Factory
			.FromAsync(request.BeginGetResponse, request.EndGetResponse, null);
	}
	catch (WebException ex)
	{
		response = (HttpWebResponse) ex.Response;
	}

	return response;
}


public async Task<HttpWebResponse> PostRequestImpersonatedAsync(string url, string body, Dictionary<string, string> headers)
{
	HttpWebResponse response;
	await this.impersonator.RunAsync(
		async () => response = await PostRequestAsync(url, cookies, this.certificates, body, headers)
	);
	return response;
}