# How to Contribute

Contributions take many forms from submitting issues, writing docs, to making
code changes - we welcome it all!

## Getting Started

If you don't have a GitHub account, you can [sign up](https://github.com/signup/free)
as it will help you to participate with the project.

If you are looking to contribute to the codebase and don't already have a preferred IDE, 
we recommend you have Visual Studio 2015 installed - you can download the Community edition from
[here](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx)

If you are running GitHub Desktop, you can clone this repository locally from
GitHub using the "Clone in Desktop" button from the chummer5a project page,
or run this command in your Git-enabled shell:

`git clone https://github.com/chummer5a/chummer5a.git Chummer5a`

If you want to make contributions to the project,
[forking the project](https://help.github.com/articles/fork-a-repo) is the
easiest way to do this. You can then clone down your fork instead:

`git clone https://github.com/MY-USERNAME-HERE//chummer5a.git Chummer5a`

When building the project, ensure that your IDE is set to transform T4 templates. 

## How can I get involved?

If you don't have a specific issue that you'd like to resolve, you can search through 
our [issue log](https://github.com/chummer5a/chummer5a/issues)

If you've found something you'd like to contribute to, leave a comment in the issue
so everyone is aware. If an issue is already allocated to a contributor but hasn't 
been updated recently, feel free to comment and ask if there's any assistance required.

## Making Changes

When you're ready to make a change, create a branch off the `master` branch:

```
git checkout master
git pull origin master
git checkout -b SOME-BRANCH-NAME
```

We use `master` as the default branch for the repository, and it holds the most
recent contributions. By working in a branch away from `master` you can handle
potential conflicts that may occur in the future.

If you make focused commits (instead of one monolithic commit) and have descriptive
commit messages, this will help speed up the review process.

### Submitting Changes

You can publish your branch from GitHub for Windows, or run this command from
the Git Shell:

`git push origin MY-BRANCH-NAME`

Once your changes are ready to be reviewed, publish the branch to GitHub and
[open a pull request](https://help.github.com/articles/using-pull-requests)
against it.

A few suggestions when opening a pull request:

 - if you are addressing a particular issue, reference it like this:

>   Fixes #1145

 - prefix the title with `[WIP]` to indicate this is a work-in-progress. It's
   always good to get feedback early, so don't be afraid to open the PR before
   it's "done".
 - use [checklists](https://github.com/blog/1375-task-lists-in-gfm-issues-pulls-comments)
   to indicate the tasks which need to be done, so everyone knows how close you
   are to done.
 - add comments to the PR about things that are unclear or you would like
   suggestions on

Some things that will increase the chance that your pull request is accepted:

* Follow existing code conventions. Most of what we do follows [standard .NET
  conventions](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md) except in a few places. 
  If in doubt, use your best judgement and mirror the existing coding style. 
* Update the documentation, the surrounding one, examples elsewhere, guides,
  whatever is affected by your contribution

# Additional Resources

* [General GitHub documentation](http://help.github.com/)
0
