[HttpPost("TestMethod1")]
public async System.Threading.Tasks.Task<IActionResult> TestMethod1()
{
	using (var fileStream = System.IO.File.Create("c:/tmp/asdfxx.txt"))
	{
		await Request.Body.CopyToAsync(fileStream);
	}

	return Ok("test");
}


// services.UseKesterl maxrequestlength





public bool CallController()
{
	using (var stream = System.IO.File.OpenRead("c:/very_big_file.zz"))
	{
		using (var client = new HttpClient())
		{
			var response = client.PostAsync("http://localhost/api/controllername/TestMethod1", new StreamContent(stream)).ConfigureAwait(false).GetAwaiter().GetResult();
			return response.IsSuccessStatusCode;
		}
	}
}