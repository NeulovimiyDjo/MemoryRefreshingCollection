$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

function RedactPlaceholdersInUI([hashtable]$placeholdersDict) {
    [void] [System.Reflection.Assembly]::LoadWithPartialName("System.Drawing")
    [void] [System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms")

    $form = New-Object System.Windows.Forms.Form
    $form.Size = New-Object System.Drawing.Size(1200, 600)

    $dataGridView = New-Object System.Windows.Forms.DataGridView -Property @{
        Size = New-Object System.Drawing.Size(1150, 500)
        ColumnHeadersVisible = $true
        AllowUserToAddRows = $true
    }

    $dataGridView.ColumnCount = 3
    $dataGridView.Columns[0].Name = "PlaceholderKey"
    $dataGridView.Columns[0].MinimumWidth = 400
    $dataGridView.Columns[1].Name = "PlaceholderValue"
    $dataGridView.Columns[1].MinimumWidth = 600
    $dataGridView.Columns[2].Name = "PlaceholderDescription"
    $dataGridView.Columns[2].MinimumWidth = 900

    foreach ($placeholder in $placeholdersDict.Values | Sort-Object -Property Placeholder) {
        $dataGridView.Rows.Add(
            $placeholder.Placeholder,
            $placeholder.Value,
            $placeholder.Description) | Out-Null
    }

    $exitOption = "None"
    $redactedPlaceholdersDict = $placeholdersDict

    $buttonSave_click = {
		([ref]$redactedPlaceholdersDict).Value = @{}
        for ($i = 0; $i -lt $dataGridView.RowCount; $i++) {
            $name = $dataGridView.Rows[$i].Cells[0].Value
            $val = $dataGridView.Rows[$i].Cells[1].Value
            $descr = $dataGridView.Rows[$i].Cells[2].Value
            if ($name) {
                $placeholder = [PSCustomObject]@{
                    Placeholder = $name
                    Value = $val
                    Description = $descr
                    ValueChanged = $true
                    OldEncryptedValue = $null
                }
                if ($placeholdersDict.ContainsKey($name)) {
                    $oldPlaceholder = $placeholdersDict[$name]
                    $placeholder.ValueChanged = $oldPlaceholder.Value -ne $placeholder.Value
                    $placeholder.OldEncryptedValue = $oldPlaceholder.OldEncryptedValue
                }
                $redactedPlaceholdersDict[$name] = $placeholder
            }
        }
		([ref]$exitOption).Value = "Save"
        $form.Dispose()
    }
    $buttonSave = New-Object System.Windows.Forms.Button
    $buttonSave.Location = New-Object System.Drawing.Size(800, 525)
    $buttonSave.Size = New-Object System.Drawing.Size(120, 23)
    $buttonSave.Text = "Save"
    $buttonSave.Add_Click($buttonSave_Click)

    $buttonChangePassword_click = {
		([ref]$exitOption).Value = "ChangePassword"
        $form.Dispose()
    }
    $buttonChangePassword = New-Object System.Windows.Forms.Button
    $buttonChangePassword.Location = New-Object System.Drawing.Size(200, 525)
    $buttonChangePassword.Size = New-Object System.Drawing.Size(120, 23)
    $buttonChangePassword.Text = "Change password"
    $buttonChangePassword.Add_Click($buttonChangePassword_click)

    $form.Controls.Add($buttonSave)
    $form.Controls.Add($buttonChangePassword)
    $form.Controls.Add($dataGridView)
    $form.ShowDialog() | Out-Null

    return $exitOption, $redactedPlaceholdersDict
}
Export-ModuleMember -Function RedactPlaceholdersInUI
