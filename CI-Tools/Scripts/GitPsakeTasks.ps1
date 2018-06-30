# Git Tasks
task GitPull `
    -alias pull `
    -description "Pulls source code from origin master branch name." {

    exec {

        Write-host "Displaying any modified files:"
        git status --short

        Write-Host "Pulling from source repository... "
        git pull $RemoteOriginName $MasterBranchName
    }
}

task GitPush `
    -alias push `
    -description "Pushes to current branch, preventing pushes to master branch name, creates new branch if not created remotely." `
    -action {

    exec {
        $gitConsoleOutput = Push-ToGit

        Write-Host $gitConsoleOutput
    }
}

task GitPullRequest `
    -alias pr `
    -description "When there are local commits which have not been pushed remotely, pushes them and opens browser to create pull request." {

    exec {

        $gitConsoleOutput = Push-ToGit

        [string]$pullRequestUrl = Get-GithubPullRequestUrl -GitConsoleOutput $gitConsoleOutput

        Write-host $pullRequestUrl
        
        if ([string]::IsNullOrWhiteSpace($pullRequestUrl)) { return }
                    
        Write-Host "Opening Pull Request '$pullRequestUrl'" -ForegroundColor Magenta
        Start-Sleep -Seconds 2 # wait for files to be read by server first
        Start-Process $DefaultBrowser $pullRequestUrl
    }
}

task GitCreateBranch `
     -alias branch `
     -description "Creates branch from current branch using prompt." {

    exec  {
        $branchName = Read-Host "Enter branch name"

        $branchName = $branchName.Trim()

        $existingBranch = git branch --list $branchName

        if ($existingBranch)
        {
            git checkout $branchName
        }
        else 
        {
            git checkout -b $branchName
        }
    }
}

task GitListBranches `
     -alias branches `
     -description "Lists all the current local branches." {

    exec  {
        # TODO: implement https://github.com/thinkbeforecoding/PSCompletion for autocompeltion of branch name selection
        git branch
    }
}

task GitDeleteBranch `
    -alias deletebranch `
    -description "Deletes the current branch, locally and remotely then checkouts to main branch." {

    exec {
        
        $branchName = Get-CurrentBranchName

        $confirmation = Read-Host "Are you sure you want to delete the branch '$branchName' locally and remotely? (Y/N)"
        
        if ($confirmation.ToUpper() -ne 'Y') {
            "Branch '$branchName', unchanged."
            return
        }

        Write-Warning "Deleting '$branchName'..."
        git checkout $MasterBranchName
        git push origin --delete $branchName
        git branch -D $branchName
        Write-Warning "Deleted '$branchName'."
    }
}


task GitCommitAll `
    -alias com `
    -description "Adds all files to git stage, prompts for commits message and creates commit." {

    exec {
        
        git status --short
        $commitMessage = Read-Host "Enter commit message"
        git add -A
        git commit -m $commitMessage
    }
}