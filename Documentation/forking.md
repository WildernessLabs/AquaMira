# Forking AquaMira to a private repository

The goals are to 
- Fork AquaMira
- Have the new for in a private repository
- Maintain the ability to pull in updates from the main repository.

## Step 1: Create the Private Repository on GitHub

- Go to GitHub and create a new repository.  For this document we'll call it `MyAquaMira`
- Make the repo private
- Do not initialize with README, .gitignore, or license (keep it empty)

## Step 2: Navigate to your local workspace folder

The repo folder will get created as a chile to this repository
```
> mkdir MyAquaMira
> cd MyRepoCollectionFolder
```

## Step 3: Clone the *public* repo into your folder

Use the name of your private repo as the folder name

```
> git clone https://github.com/WildernessLabs/AquaMira.git MyAquaMira
> cd MyAquaMira
```

## Step 4: Add your private repo as a remote
```
> git remote add private https://github.com/MyOrg/MyAquaMira.git
```

## Step 5: Rename the current origin remote to upstream and push the base code to your private repo
```
> git remote rename origin upstream
> git push private --all
```

## Step 6: Change the private repo remote to origin
```
> git remote rename private origin
```

## Step 7: Changet the upstream branch to origin (the private repo)
```
> git branch --set-upstream-to=origin/main main
```

> NOTE: the fork will have project references to Meadow libraries.  If you want NuGet packages, you must modify the project files, which may make merges interesting in the future.

