param
(
    [string][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$repoUrl,
    [string][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$supplyFile,
    [string][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$module,
    [string][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$version,
    [string[]][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$issues,
    [string[]][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$targets,
    [string][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$release
)

function MainFunction(
    [string]$repoUrl,
    [string]$supplyFile,
    [string]$module,
    [string]$version,
    [string[]]$issues,
    [string[]]$targets,
    [string]$release)
{
    $metaFile = './metafile.json'
    $metafileUrl = "$repoUrl/metafile.json"
    $supplyPath = "$module/$module-$version.zip"
    $supplyUrl = "$repoUrl/$supplyPath"
    $credential = Get-Credential

    UploadFile $supplyFile $supplyUrl $credential
    DownloadFile $metaFile $metafileUrl $credential
    AddSupplyToMetafile $metaFile $supplyPath $module $version $issues $targets $release
    UploadFile $metaFile $metafileUrl $credential
    Remove-Item $metaFile
}

function AddSupplyToMetafile(
		[string]$metaFile,
		[string]$supplyFile,
		[string]$module,
		[string]$version,
		[string[]]$issues,
		[string[]]$targets,
		[string]$release)
{   
    $json = Get-Content -Raw $metaFile | Out-String | ConvertFrom-Json
    $nodes = $json.PSObject.Properties
    $lastNode = $nodes | Select-Object -Last 1
    $lastNodeNumber = [int]$lastNode.Name

    $newNodeNumber = [string]($lastNodeNumber + 1)
    $newNode = [PSCustomObject]@{
        app = "$module"
        version = "$version"
        issues = $issues
        targets = $targets
        release = "$release"
        supply_path = "$supplyPath"
    } | ConvertTo-Json

    $json | Add-Member -Type NoteProperty -Name "$newNodeNumber" -Value (ConvertFrom-Json $newNode)
    $newMetaFileContent = $json | ConvertTo-Json -Depth 2
    Set-Content -Path $metaFile -Value $newMetaFileContent -Force
}

function UploadFile(
		[string]$filePath,
		[string]$fileUrl,
		[System.Management.Automation.PSCredential]$credential)
{
	Write-Output "Uploading $filePath to $fileUrl ..."
    Add-Type -AssemblyName System.Net.Http
    $content = CreateStreamContent $filePath	
    $response = UploadContent $fileUrl $content $credential
	Write-Output "Upload finished with response: $response"
}

function DownloadFile(
		[string]$filePath,
		[string]$fileUrl,
		[System.Management.Automation.PSCredential]$credential)
{
	Write-Output "Downloading from $fileUrl to $filePath ..."
    Add-Type -AssemblyName System.Net.Http
     
	$httpClientHandler = GetHttpClientHandler $credential
	$httpClient = New-Object -TypeName System.Net.Http.Httpclient $httpClientHandler
	$httpClient.Timeout = [timespan]::FromSeconds(600)
	$responseTask = $httpClient.GetAsync($fileUrl)
	
	$outputFileStream = [System.IO.FileStream]::new($filePath, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write)
	$downloadTask = $responseTask.Result.Content.CopyToAsync($outputFileStream)
	$outputFileStream.Close()

	Write-Output "Download finished with response: $response"
}

function CreateStreamContent()
{
	param
	(
		[string][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$filePath
	)
		$filePathStream = New-Object -TypeName System.IO.FileStream -ArgumentList @($filePath, [System.IO.FileMode]::Open)
		
		$contentDispositionHeaderValue = New-Object -TypeName  System.Net.Http.Headers.ContentDispositionHeaderValue "form-data"
		$contentDispositionHeaderValue.Name = "file"
		$contentDispositionHeaderValue.FileName = Split-Path $filePath -leaf

		$streamContent = New-Object -TypeName System.Net.Http.StreamContent $filePathStream
		$streamContent.Headers.ContentDisposition = $contentDispositionHeaderValue
		$streamContent.Headers.ContentType = New-Object -TypeName System.Net.Http.Headers.MediaTypeHeaderValue "application/octet-stream"

		return $streamContent
}

function UploadContent()
{
	param
	(
		[string][parameter(Mandatory = $true)]$fileUrl,
		[System.Net.Http.HttpContent][parameter(Mandatory = $true)]$content,
		[System.Management.Automation.PSCredential][parameter(Mandatory = $true)]$credential
	)

    $httpClientHandler = GetHttpClientHandler $credential
	$httpClient = New-Object -TypeName System.Net.Http.Httpclient $httpClientHandler
	$httpClient.Timeout = [timespan]::FromSeconds(600)
	try
	{
		$responseTask = $httpClient.PutAsync($fileUrl, $content)

		if ($responseTask.Result -eq $null)
		{
			$errorMessage = "Connection failed with the following error: {0}." -f $responseTask.Exception
			throw [System.Net.Http.HttpRequestException] $errorMessage
		}

		$response = $responseTask.Result
		if (!$response.IsSuccessStatusCode)
		{
			$responseBody = $response.Content.ReadAsStringAsync().Result
			$statusCode = $response.StatusCode
			$reasonPhrase = $response.ReasonPhrase
			$errorMessage = "Status code {0}. Reason {1}. Server reported the following message: {2}." -f $statusCode, $reasonPhrase, $responseBody
			throw [System.Net.Http.HttpRequestException] $errorMessage
		}

		return $response.Content.ReadAsStringAsync().Result
	}
	catch [Exception]
	{
		$PSCmdlet.ThrowTerminatingError($_)
	}
	finally
	{
		if($null -ne $httpClient)
		{
			$httpClient.Dispose()
		}

		if($null -ne $response)
		{
			$response.Dispose()
		}
	}
}

function GetHttpClientHandler()
{
	param
	(
		[System.Management.Automation.PSCredential][parameter(Mandatory = $true)]$Credential
	)

	$networkCredential = New-Object -TypeName System.Net.NetworkCredential -ArgumentList @($Credential.UserName, $Credential.Password)
	$httpClientHandler = New-Object -TypeName System.Net.Http.HttpClientHandler
	$httpClientHandler.Credentials = $networkCredential

	Add-Type @"
		using System.Net;
		using System.Security.Cryptography.X509Certificates;
		public class TrustAllCertsPolicy : ICertificatePolicy {
			public bool CheckValidationResult(
				ServicePoint srvPoint, X509Certificate certificate,
				WebRequest request, int certificateProblem) {
				return true;
			}
		}
"@
	$AllProtocols = [System.Net.SecurityProtocolType]'Ssl3,Tls,Tls11,Tls12'
	[System.Net.ServicePointManager]::SecurityProtocol = $AllProtocols
	[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy

	return $httpClientHandler
}

MainFunction $repoUrl $supplyFile $module $version $issues $targets $release
