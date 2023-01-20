#!/bin/sh

# We rebased our repository on 2023-01-01.
# This commit will only exist on old histories. If it exists, it must not be pushed,
# otherwise all of the old history we took care to remove will be present again.

set -e

if git --no-replace-objects show -s d5eb0645d999623b72025d0bf19227a787af1fb1 2>/dev/null = 0; then
    if [ -n "$CI" ]; then
        echo -n "::error title=Old history pushed::!!!!! DO NOT MERGE THIS BRANCH INTO MASTER !!!!!%0A"
        echo -n "You have pushed the old repository history onto the remote repository.%0A"
        echo -n "Please rebase this branch to make sure only the new history (post 2023-01-01) is present.%0A"
        echo -n "If you are unsure what to do, please use the ~Entwicklung channel in Mattermost.%0A"
    else
        echo "Looks like you have a copy of the repository with a bad commit that should no longer be present."
        echo "Please save your work to a temporary location and delete this repository."
        echo "If you create a fresh clone, that should fix this problem."
    fi
    exit 1
fi
