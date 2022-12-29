#!/bin/sh
# This pre-commit hook verifies whether any drawn code cities have
# been left in the Assets/Scene folder. Only the diff for the attempted
# commit will be looked at, so existing code cities will be ignored.
# Only staged (to be committed) changes will be looked at.

output=""
while IFS= read -r scene
do
        if [ -n "$scene" ]; then
                output="$output
Warning: Rendered CodeCities detected in scene '$scene'!"
        fi
done << EOF
$(git diff --staged -S"m_TagString: Node" --name-only Assets/Scenes 2> /dev/null)
EOF

if [ -n "$output" ]; then
        printf '%s\nPlease delete all drawn CodeCities before committing.' "$output"
        exit 1
fi
exit 0
