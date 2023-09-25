##########################
# Git Helper Functions
##########################

function Get-CurrentBranchName {

    $branchName = git symbolic-ref --short -q HEAD

    $branchName
}

function Get-GithubPullRequestUrl {
    Param($GitConsoleOutput)

    Write-Host $GitConsoleOutput

    if ($GitConsoleOutput.Contains("Everything up-to-date") -or `
        $GitConsoleOutput.Contains("Permission denied (publickey)"))
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

function Retry-Command {
    [CmdletBinding()]
    Param(
        [Parameter(Position=0, Mandatory=$true)]
        [scriptblock]$ScriptBlock,

        [Parameter(Position=1, Mandatory=$false)]
        [int]$Maximum = 5,

        [Parameter(Position=2, Mandatory=$false)]
        [int]$Delay = 500
    )

    Begin {
        $cnt = 0
    }

    Process {
        do {
            $cnt++
            try {
                # If you want messages from the ScriptBlock
                # Invoke-Command -Command $ScriptBlock
                # Otherwise use this command which won't display underlying script messages
                $ScriptBlock.Invoke()
                return
            } catch {
                Write-Host "Failed, retrying $cnt of $Maximum..."
                Start-Sleep -Milliseconds $Delay
            }
        } while ($cnt -lt $Maximum)

        # Throw an error after $Maximum unsuccessful invocations. Doesn't need
        # a condition, since the function returns upon successful invocation.
        throw 'Execution failed.'
    }
}