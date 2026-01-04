# Fixing GitHub Push Protection - Secret in Commit History

## Problem
GitHub detected an OpenAI API key in commits:
- `05cab3df6cb57b0437c0b2d58502cfb5b96890c2`
- `d5449f1382667e917bd39b422b143c89dbe4e74f`

## Solution Options

### Option 1: Use BFG Repo-Cleaner (Recommended - Fastest)

1. **Download BFG Repo-Cleaner**
   ```powershell
   # Install with Chocolatey
   choco install bfg-repo-cleaner
   
   # Or download manually from: https://rtyley.github.io/bfg-repo-cleaner/
   ```

2. **Create a backup**
   ```powershell
   cd C:\development\dotnet_apps\
   cp -r blazor-ai-agent-todo blazor-ai-agent-todo-backup
   ```

3. **Clone a fresh bare copy**
   ```powershell
   cd C:\development\dotnet_apps\
   git clone --mirror https://github.com/dareljohnson/blazor-todo-ai-agent-app.git
   ```

4. **Run BFG to remove the secret**
   ```powershell
   # Replace YOUR-API-KEY-HERE with the actual key that was committed
   bfg --replace-text <(echo "YOUR-API-KEY-HERE==>REDACTED") blazor-todo-ai-agent-app.git
   
   cd blazor-todo-ai-agent-app.git
   git reflog expire --expire=now --all
   git gc --prune=now --aggressive
   ```

5. **Force push the cleaned history**
   ```powershell
   git push --force
   ```

6. **Re-clone your working directory**
   ```powershell
   cd C:\development\dotnet_apps\
   rm -rf blazor-ai-agent-todo
   git clone https://github.com/dareljohnson/blazor-todo-ai-agent-app.git blazor-ai-agent-todo
   ```

### Option 2: Use git filter-repo (Recommended by GitHub)

1. **Install git-filter-repo**
   ```powershell
   pip install git-filter-repo
   ```

2. **Create a backup**
   ```powershell
   cd C:\development\dotnet_apps\
   cp -r blazor-ai-agent-todo blazor-ai-agent-todo-backup
   ```

3. **Remove the file from history**
   ```powershell
   cd C:\development\dotnet_apps\blazor-ai-agent-todo
   
   # Remove appsettings.json from all commits
   git filter-repo --path appsettings.json --invert-paths --force
   ```

4. **Add back the safe version**
   ```powershell
   # Your current appsettings.json is safe, so add it back
   git add appsettings.json
   git commit -m "Add safe appsettings.json without secrets"
   ```

5. **Force push**
   ```powershell
   git remote add origin https://github.com/dareljohnson/blazor-todo-ai-agent-app.git
   git push origin main --force
   ```

### Option 3: Interactive Rebase (If few commits)

If you have only a few commits since adding the secret:

1. **Start interactive rebase**
   ```powershell
   # Find the commit before the secret was added
   git log --oneline
   
   # Start rebase (replace <commit-before-secret> with actual hash)
   git rebase -i <commit-before-secret>
   ```

2. **Mark commits to edit**
   - Change `pick` to `edit` for commits containing the secret
   - Save and close

3. **For each commit, remove the secret**
   ```powershell
   # Edit appsettings.json to remove the secret
   # Then:
   git add appsettings.json
   git commit --amend --no-edit
   git rebase --continue
   ```

4. **Force push**
   ```powershell
   git push origin main --force
   ```

### Option 4: Start Fresh (Nuclear Option)

If the repository is new and doesn't have important history:

1. **Delete the GitHub repository** at https://github.com/dareljohnson/blazor-todo-ai-agent-app

2. **Create a new repository** with the same name

3. **Push fresh commits**
   ```powershell
   cd C:\development\dotnet_apps\blazor-ai-agent-todo
   
   # Remove git history
   rm -rf .git
   
   # Initialize fresh repo
   git init
   git add .
   git commit -m "Initial commit - Blazor AI Agent Todo"
   
   # Push to new repository
   git remote add origin https://github.com/dareljohnson/blazor-todo-ai-agent-app.git
   git branch -M main
   git push -u origin main
   ```

## After Cleaning History

### 1. **IMPORTANT: Revoke the exposed API key**
   - Go to OpenAI Dashboard: https://platform.openai.com/api-keys
   - Delete the exposed key immediately
   - Generate a new one

### 2. **Configure the app securely**
   Use one of these methods (in order of preference):

   **A. Environment Variables**
   ```powershell
   $env:OpenAI__ApiKey = "your-new-api-key"
   $env:OpenAI__Model = "gpt-4o"
   ```

   **B. User Secrets**
   ```powershell
   dotnet user-secrets set "OpenAI:ApiKey" "your-new-api-key"
   dotnet user-secrets set "OpenAI:Model" "gpt-4o"
   ```

   **C. appsettings.Development.json** (already in .gitignore)
   ```json
   {
     "OpenAI": {
       "ApiKey": "your-new-api-key",
       "Model": "gpt-4o"
     }
   }
   ```

### 3. **Verify .gitignore**
   Ensure these are in your `.gitignore`:
   ```
   appsettings.Development.json
   appsettings.*.json
   !appsettings.json
   *.env
   .env
   secrets.json
   ```

### 4. **Test the push**
   ```powershell
   git push origin main
   ```

## Prevention Tips

1. ? Always use `.env` files or user secrets for API keys
2. ? Add sensitive files to `.gitignore` BEFORE committing
3. ? Use placeholder values in `appsettings.json` (like "no-secret")
4. ? Review commits before pushing: `git diff --cached`
5. ? Enable pre-commit hooks to scan for secrets
6. ? Use GitHub secret scanning alerts

## Need Help?

- GitHub Secret Scanning: https://docs.github.com/code-security/secret-scanning
- BFG Repo-Cleaner: https://rtyley.github.io/bfg-repo-cleaner/
- git-filter-repo: https://github.com/newren/git-filter-repo
