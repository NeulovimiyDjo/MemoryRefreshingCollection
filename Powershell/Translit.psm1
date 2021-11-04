$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

$translitChart = @{
    [char]'а' = "a"
    [char]'А' = "A"
    [char]'б' = "b"
    [char]'Б' = "B"
    [char]'в' = "v"
    [char]'В' = "V"
    [char]'г' = "g"
    [char]'Г' = "G"
    [char]'д' = "d"
    [char]'Д' = "D"
    [char]'е' = "e"
    [char]'Е' = "E"
    [char]'ё' = "yo"
    [char]'Ё' = "Yo"
    [char]'ж' = "zh"
    [char]'Ж' = "Zh"
    [char]'з' = "z"
    [char]'З' = "Z"
    [char]'и' = "i"
    [char]'И' = "I"
    [char]'й' = "j"
    [char]'Й' = "J"
    [char]'к' = "k"
    [char]'К' = "K"
    [char]'л' = "l"
    [char]'Л' = "L"
    [char]'м' = "m"
    [char]'М' = "M"
    [char]'н' = "n"
    [char]'Н' = "N"
    [char]'о' = "o"
    [char]'О' = "O"
    [char]'п' = "p"
    [char]'П' = "P"
    [char]'р' = "r"
    [char]'Р' = "R"
    [char]'с' = "s"
    [char]'С' = "S"
    [char]'т' = "t"
    [char]'Т' = "T"
    [char]'у' = "u"
    [char]'У' = "U"
    [char]'ф' = "f"
    [char]'Ф' = "F"
    [char]'х' = "h"
    [char]'Х' = "H"
    [char]'ц' = "c"
    [char]'Ц' = "C"
    [char]'ч' = "ch"
    [char]'Ч' = "Ch"
    [char]'ш' = "sh"
    [char]'Ш' = "Sh"
    [char]'щ' = "sch"
    [char]'Щ' = "Sch"
    [char]'ъ' = ""
    [char]'Ъ' = ""
    [char]'ы' = "y"
    [char]'Ы' = "Y"
    [char]'ь' = ""
    [char]'Ь' = ""
    [char]'э' = "e"
    [char]'Э' = "E"
    [char]'ю' = "yu"
    [char]'Ю' = "Yu"
    [char]'я' = "ya"
    [char]'Я' = "Ya"
    [char]' ' = "_"
}

function CheckNoCyrillicFileNames([string]$dir) {
    $files = Get-ChildItem -Path $dir -Recurse -File
    foreach ($f in $files) {
        foreach ($chr in $f.Name.ToCharArray()) {
            if ($null -cne $translitChart[$chr]) {
                throw "File name contains cyrillic symbols: '$($f.FullName)'"
            }
        }
    }
}
Export-ModuleMember -Function CheckNoCyrillicFileNames

function TransliterateCyrillicFileNames([string]$dir) {
    Get-ChildItem -Path $dir -Recurse -File |
        Where-Object { ContainsCyrillicChar $_.Name } |
        Rename-Item -Force -NewName { Transliterate $_.Name }
}
Export-ModuleMember -Function TransliterateCyrillicFileNames

function ContainsCyrillicChar([string]$inString) {
    foreach ($chr in $inString.ToCharArray()) {
        if ($null -cne $translitChart[$chr]) {
            return $true
        }
    }
    return $false
}

function Transliterate([string]$inString) {
    $translitedText = ""
    foreach ($chr in $inString.ToCharArray()) {
        if ($null -cne $translitChart[$chr]) {
            $translitedText += $translitChart[$chr]
        } else {
            $translitedText += $chr
        }
    }
    return $translitedText
}
