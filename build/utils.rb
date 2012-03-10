def git_hash_and_date
    commit = `git log -1 --pretty=format:%h`
    git_date = `git log -1 --date=iso --pretty=format:%ad`
    commit_date = DateTime.parse( git_date ).strftime("%Y-%m-%d")  
	[commit, commit_date]
end
