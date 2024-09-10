# Cosmatic Drift contribution guidelines

Most of the information on upstream's [documentation website](https://docs.spacestation14.com/index.html) is relevant to
CD. It is worth a look if you have never contributed to a SS14 server before.

For setting up a development environment, we recommend following upstream's
[Setting up a Development Environment](https://docs.spacestation14.com/en/general-development/setup/setting-up-a-development-environment.html) guide.

You should follow [upstream's pull request guidelines](https://docs.spacestation14.com/en/general-development/codebase-info/pull-request-guidelines.html). The major points we want to emphasize and a few deviations are listed below.

**All changes must be tested in-game unless you have been told otherwise by a maintainer**

### Small PRs are easier to review then larger ones.

Reviewing large PRs is significantly harder then small PRs. Please try your best to keep the size down by following
upstream's advice. We would prefer you make multiple PRs rather then one large PR.

### Changelogs

We do not have the bot upstream uses to automatically create changelogs. Simply write a summery of your changes to be
listed in #progress-reports. If you would like to be credited as something other then you github username please include the
name that you would like to be credited as.

### Code Reviews

Maintainers typically review PRs leading up to playtest sessions. If multiple playtests have passed without a review
please ask for one on discord. We do not typically review draft PRs unless requested.

### Namespaceing

All contributions must be placed under the `_CD` namespace where appropriate. For content under `Resources/` this is
typically a subdirectory called `_CD`. For C# changes, there is a C# namespace under almost every `Content.*` folder. If
one does not exist, feel free to make one.

Changes to wizden code should be clearly marked with a comment. For one line changes this typically takes the form of `#
CD: why you changed it`. Larger changes are typically located between the comments.

Some old code does not follow these guidelines. If you are modifying it, please try to bring it up to our modern
standards.

### Pre-Approval

Before you work on a medium to large PR please ask the maintainers on discord if the concept is alright before starting
it. This is to prevent you from wasting your time working on something that would not get merged. For large PRs we would
prefer if you would create a design document outlining your changes. You can do this as a discord thread or a github
issue. Github issues are preferred.
