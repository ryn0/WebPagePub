##########################
# Helper Functions
##########################

function Get-GithubPullRequestUrl {
    Param($GitConsoleOutput)

    Write-Host $GitConsoleOutput

    if ($GitConsoleOutput.Contains("Everything up-to-date"))
    {
        return
    }
 
    $matches = New-Object System.Collections.ArrayList
  
    $regex = '(http[s]?|[s]?ftp[s]?)(:\/\/)([^\s,]+)'

    $GitConsoleOutput | select-string -Pattern $regex -AllMatches | % { $_.Matches } | % { $matches.add($_.Value) | Out-Null }

    if ($matches.Count -ne 0) { 
        # note: line endings are difficult to parse, just remove the characters that get put at the end by mistake
        $countOfBadChactersAtEnd = 3
        $match = $matches[0]
        $pullRequestUrl = $match.Remove($match.Length - $countOfBadChactersAtEnd, $countOfBadChactersAtEnd)

        [System.Convert]::ToString($pullRequestUrl.TrimEnd(".")) # returns the first url
    }
    else 
    {
        # location is probably SSH, not URL
        $withoutBeginning = $GitConsoleOutput.Split(':')[1]
        $repoPath = $withoutBeginning.Substring(0, $withoutBeginning.IndexOf('.'))

        $repoUrl = "https://github.com/$repoPath"

        $repoUrl
    }
}

function Create-TableIfNotExists{    
    Param($TableName, 
          $StorageContext)

    Get-AzureStorageTable `
        -Name $TableName `
        -Context $StorageContext `
        -ErrorVariable ev `
        -ErrorAction SilentlyContinue | Out-Null
    
    if ($ev) {
        Write-Host "Creating table '$TableName'... " -NoNewline
        New-AzureStorageTable `
           –Name $TableName `
           –Context $StorageContext | Out-Null
        Write-Host "done." -NoNewline
        Write-Host ""
    }
    else {
        Write-Host "Table '$TableName' already exists."
    } 
}

function Push-ToGit {

    $currentBranch = git symbolic-ref --short -q HEAD
    
    if ($currentBranch -eq $MasterBranchName)
    {
        throw "You cannot push directly to the branch $MasterBranchName!."
    }
    
    $gitConsoleOutput = cmd /c git push `
             $RemoteOriginName `
             $currentBranch `
             2`>`&1
    
    $gitConsoleOutput
}

function Get-CurrentBranchName {

    $branchName = git symbolic-ref --short -q HEAD

    $branchName
}

function Stop-ProcessSafely {
    Param($Name)

    $runningProcess = Get-Process -Name $Name -ErrorAction Ignore

    if ($runningProcess)
    {
        Stop-Process -Name $Name
    }
}

function Run-TestsByFilter {
    Param($Filter)

        $xunitConsoleDirectory = "$NuGetPackagesDirectory\xunit.runner.console"
        $xunitConsolePath = (Get-ChildItem -Path $xunitConsoleDirectory -Filter xunit.console.exe -Recurse | 
                                            Select-Object -First 1).FullName

        $testDirectory = Resolve-Path -path "$CIRoot\..\Test"

        $testAssemblies = Get-ChildItem -Path $testDirectory -filter $Filter -Recurse | 
                                Select-Object -ExpandProperty FullName | 
                                where {$_ -like "*bin*"}      

        & $xunitConsolePath $testAssemblies

}