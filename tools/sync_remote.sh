#!/usr/bin/env bash
# @summary: Connects the local repo to GitHub when this session can reach it; merges remote state into dev. Prints REMOTE=github|none.
set -u
REMOTE_URL="https://github.com/piesiopiesio/FactoryTalk-Optix---Factory-demo.git"
cd "$(dirname "$0")/.."

git show-ref --verify --quiet refs/heads/dev || git branch dev
git checkout -q dev

if ! git ls-remote "$REMOTE_URL" >/dev/null 2>&1; then
  echo "REMOTE=none (no GitHub access in this session; work continues on the bundle copy)"
  exit 0
fi

git remote get-url origin >/dev/null 2>&1 || git remote add origin "$REMOTE_URL"
git fetch -q origin

if git show-ref --verify --quiet refs/remotes/origin/dev; then
  # Remote dev is the shared truth; local copy (from the bundle) may be behind or ahead.
  git merge -q --no-edit origin/dev || { echo "CONFLICT merging origin/dev - resolve before work"; exit 1; }
elif git show-ref --verify --quiet refs/remotes/origin/main; then
  # First contact: remote has only the user's initial commit(s). Join histories, our files win.
  if ! git merge-base --is-ancestor origin/main dev 2>/dev/null; then
    git merge -q --no-edit --allow-unrelated-histories -X ours origin/main -m "merge: GitHub main into dev (first sync)"
  fi
fi
echo "REMOTE=github"
