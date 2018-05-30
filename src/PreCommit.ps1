param (
    [Parameter(Mandatory=$false)]
    [string]$basePath
 )

# UPPERCASE secrets
[String[]]$secrets = ";PASSWORD=", ".DATABASE.WINDOWS.NET", ".BLOB.CORE.WINDOWS.NET", ".BATCH.AZURE.COM", ";ACCOUNTKEY="

# UPPERCASE Azure key/value patterns (key="key" value="value", value="value==", <InstrumentationKey>, instrumentationKey:, "<GUID>")
[String[]]$patterns = "(?i)key`s*=`s*`".*`"`s*value(?-i)`s*=`s*`"[A-Z0-9]{32}`"", "(?i)value(?-i)`s*=`s*`"[A-Z0-9+`/]{86}==`"", "(?i)<`s*InstrumentationKey`s*>", "(?i)instrumentationKey:", "(?i)`"[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}`""

# UPPERCASE config file endings
[String[]]$configs = ".XML", ".CONFIG", ".CS", ".VB", ".JSON", ".JS", ".CSHTML", ".VBHTML"

# Variables
[System.Diagnostics.Stopwatch]$sw = [System.Diagnostics.Stopwatch]::StartNew()
[String[]]$filesArr
[String]$task = "Pre-Commit"

if ([string]::IsNullOrEmpty($basePath)) {
    # Git command to find files that are added or modified
    [String]$files = git diff --staged --name-only --diff-filter=AM
    if ([string]::IsNullOrEmpty($files)) {
        Write-Host "$($task)/Git has nothing to do"
        exit 0
    }

    [System.StringSplitOptions]$option = [System.StringSplitOptions]::RemoveEmptyEntries   
    $filesArr = $files.Split([Environment]::NewLine, $option)
} else {
    # Get files from the basePath recursively
    $filesArr = (Get-ChildItem -Recurse -File $basePath | Where-Object { $configs.Contains($_.Extension.ToUpper()) }).FullName
    $task = "Secret-Scanner"
    if ($filesArr.Length -eq 0) {
        Write-Host "$($task) has nothing to do"
        exit 0    
    }    
}

[Boolean]$found = 0
Write-Host "$($task) is scanning $($filesArr.Length) $($configs -Join ',') file(s)"

For ($i=0; $i -lt $filesArr.Length; $i++) {
    Write-Host $filesArr[$i]
    # Check that the file is of interest
    [Boolean]$config = 0
    [String]$currentFile = $filesArr[$i].ToUpper();
    For($j=0; $j -lt $configs.Length; $j++) {
        If ($currentFile.EndsWith($configs[$j])) {
            $config = 1
            Break
        }
    }
    if ($config -eq 0) {
        Continue
    }

    # Try to find a leak
    $lineNo = 1
    foreach($line in Get-Content $filesArr[$i]) {
        $line = $line.ToUpper();
        # Patterns
        For ($j=0; $j -lt $patterns.Length; $j++) {
            If ($line -cmatch $patterns[$j]) {
                Write-Host "$($filesArr[$i]) contains posible secret key at line ${lineNo}."
                $found = 1
            }
        }

        # Secrets
        For ($j=0; $j -lt $secrets.Length; $j++) {
            If ($line.IndexOf($secrets[$j]) -gt -1) {
                Write-Host "$($filesArr[$i]) contains secret leaker '$($secrets[$j])' at line ${lineNo}."
                $found = 1
            }
        }

        $lineNo = $lineNo + 1
    }
}

$sw.Stop();
if ($found) {
    Write-Host "$($task) found leaked secrets in $($sw.get_ElapsedMilliseconds())ms"
    exit 1
}

Write-Host "$($task) didn't find any leaked secrets in $($sw.get_ElapsedMilliseconds())ms"