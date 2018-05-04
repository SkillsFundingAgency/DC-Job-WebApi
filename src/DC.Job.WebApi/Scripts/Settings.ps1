param (
    [string]$ProjDirectory = ""
)

$privateFilesPath =  [io.path]::combine($(get-item $ProjDirectory).parent.parent.parent.FullName,"DC-Configs","DCWebSettings",$env:UserName)
Write-Host $privateFilesPath 
 
$destination = [io.path]::combine($(get-item $ProjDirectory ).FullName)
Write-Host $destination 


if (Test-Path $privateFilesPath)
{ 
    Write-Host "Found private repo files, copying..." 

    $repoFilesArray = Get-ChildItem $privateFilesPath -Recurse 
    
    Write-Host $repoFilesArray.Length

   foreach ($file in $repoFilesArray) 
   {
         $destFilePath = [io.path]::combine($destination,$file.Name)
        if (Test-Path $destFilePath)
        {   
            Write-Host "destination file exists - checking if its older than repo: $($destFilePath)"     
            $destFile = Get-Item $destFilePath

            if ($file.LastWriteTimeUtc -gt $destFile.LastWriteTimeUtc)
            {
                Write-Host "copying newer file: $($file.FullName)"
                Copy-Item $file.FullName $destFilePath -recurse -force
            }
            else 
            {
                Write-Host "skipping file: $($file.FullName)"
            }
        }
        else
        {
            Write-Host "copying file as destination doesnt exist: $($file.FullName)"
            Copy-Item $file.FullName $destFilePath -recurse -force
        }
        
    }

    
}
