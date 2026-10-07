#!/usr/bin/env bash
# @summary: Connects the local repo to GitHub when this session can reach it; merges remote state into claude/dev. Prints REMOTE=github|none.
set -u
REMOTE_URL="https://github.com/piesiopiesio/FactoryTalk-Optix---Factory-demo.git"
BRANCH="claude/dev"   # Claude cloud sessions may always push claude/* branches; main stays yours.
cd "$(dirname "$0")/.."

git show-ref --verify --quiet "refs/heads/$BRANCH" || git branch "$BRANCH"
git checkout -q "$BRANCH"

if ! git ls-remote "$REMOTE_URL" >/dev/null 2>&1; then
  echo "REMOTE=none (no GitHub access in this session; work continues on the bundle copy)"
  exit 0
fi

# Remote is always named "github" (a copy restored from the bundle has "origin" = the bundle file).
if git remote get-url github >/dev/null 2>&1; then git remote set-url github "$REMOTE_URL"; else git remote add github "$REMOTE_URL"; fi
git fetch -q github

if git show-ref --verify --quiet "refs/remotes/github/$BRANCH"; then
  # Remote branch is the shared truth; a local copy restored from the bundle may be behind or ahead.
  git merge -q --no-edit "github/$BRANCH" || { echo "CONFLICT merging github/$BRANCH - resolve before work"; exit 1; }
elif git show-ref --verify --quiet refs/remotes/github/main; then
  # First contact: remote has only the user's initial commit(s). Join histories, our files win.
  if ! git merge-base --is-ancestor github/main "$BRANCH" 2>/dev/null; then
    git merge -q --no-edit --allow-unrelated-histories -X ours github/main -m "merge: GitHub main into $BRANCH (first sync)"
  fi
fi
echo "REMOTE=github (push with: git push -u github $BRANCH)"
