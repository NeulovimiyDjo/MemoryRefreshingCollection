function UploadFileToNexus {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$FilePath,
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Server,
        [parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Repository,
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$GroupID,
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$ArtifactID,
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Version,
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$VersionTag,
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Username,
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Password
    )

    Begin {
        $uri = "${Server}/service/rest/v1/components?repository=${Repository}"
        $boundary = [System.Guid]::NewGuid().ToString()
        $lf = "`r`n"
    }

    Process {
        $fileName = Split-Path $FilePath -Leaf

        try {
            $fullFilePath = (Get-Item $FilePath).FullName
            $fileBytes = [System.IO.File]::ReadAllBytes($fullFilePath);
            $fileEnc = [System.Text.Encoding]::GetEncoding('ISO-8859-1').GetString($fileBytes);
        } catch {
            throw "Unable to read file $FilePath. Aborted."
        }

        $fileExtention = [System.IO.Path]::GetExtension($FilePath).Trim('.')

        $authHeaders = @{
            Authorization = "Basic $([System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes("$Username`:$Password")))"
        }

        $bodyLines = (
            "--${boundary}",
            "Content-Disposition: form-data; name=`"groupId`"",
            "",
            $GroupID,
            "--${boundary}",
            "Content-Disposition: form-data; name=`"artifactId`"",
            "",
            $ArtifactID,
            "--${boundary}",
            "Content-Disposition: form-data; name=`"version`"",
            "",
            $Version,
            "--${boundary}",
            "Content-Disposition: form-data; name=`"asset1.extension`"",
            "",
            $fileExtention,
            "--${boundary}",
            "Content-Disposition: form-data; name=`"tag`"",
            "",
            $VersionTag,
            "--${boundary}",
            "Content-Disposition: form-data; name=`"asset1`"; filename=`"${fileName}`"",
            "Content-Type: application/octet-stream",
            "",
            $fileEnc,
            "--${boundary}--",
            ""
        ) -join $lf

        return Invoke-WebRequest -Uri $uri -Method "POST" -Headers $authHeaders -ContentType "multipart/form-data; boundary=`"$boundary`"" -Body $bodyLines
    }
}