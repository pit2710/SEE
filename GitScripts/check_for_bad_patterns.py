#!/usr/bin/env python3

# A script which takes in a unified diff expected to be generated by
# git diff -U0 | grep '^[+@]'
# and outputs any bad pattern matches. See documentation of
# BadPattern.to_comment() for details of the output format.
# Note that this script is only run on CI, not as part of the Git hooks,
# due to it being written in Python rather than as a shell script.

import sys
import re
import fileinput
from enum import Enum


class Level(str, Enum):
    """
    Severity level of a bad pattern, associated to a GitHub emoji.
    """

    INFO = ":information_source:"
    WARN = ":warning:"
    ERROR = ":x:"


# Extensions that a pattern will be applied to by default.
DEFAULT_EXTENSIONS = ["cs"]


class BadPattern:
    """
    A pattern within a file that must be avoided.
    """

    def __init__(
        self,
        regex,
        message,
        extensions=DEFAULT_EXTENSIONS,
        suggestion=None,
        level=Level.INFO,
    ):
        """
        Takes a compiled regular expression `regex` that is checked against
        every line within changed files having an extension contained in
        `extensions`, a `message` that shall be displayed to the user in case
        a match has been found, a regex substitution `suggestion` for a found
        bad pattern, and a severity `level`.
        """
        self.regex = regex
        self.message = message
        self.extensions = extensions
        self.suggestion = suggestion
        self.level = level

    def to_comment(self, filename: str, line_number: int, suggestion: str) -> str:
        """
        Turns this bad pattern match into a string containing the following
        components, separated by newlines:
        Filename of matched file, line number where match occurred,
        set level, set message, substituted suggestion (may be empty),
        set regular expression,
        """
        return (
            f"{filename}\n{line_number}\n{self.level.value}\n{self.message}\n"
            + f"{suggestion}\n{self.regex.pattern}"
        )


# *** MODIFY BELOW TO ADD NEW BAD PATTERNS ***

BAD_PATTERNS = [
    BadPattern(
        re.compile(r"^(.*(?<!= )new \w*NetAction\w*\([^()]*\))([^.].*)$"),
        "Don't forget to call `.Execute()` on newly created net actions!",
        suggestion=r"\1.Execute()\2",
        level=Level.ERROR,
    ),
    BadPattern(
        re.compile(r"(^\s*ActionManifestFileRelativeFilePath: StreamingAssets)\/SteamVR\/actions\.json(\s*)$"),
        """Slashes were unnecessarily changed to forward slashes.
This happens on Linux systems automatically, but Windows systems will change this back.
We should just leave it as a backslash.""",
        suggestion=r"\1\SteamVR\actions.json\2",
        extensions=["asset"],
        level=Level.WARN
    )
]

# *** MODIFY ABOVE TO ADD NEW BAD PATTERNS ***


def handle_chunk(open_diff, start_line, filename, lines) -> int:
    """
    Handles a single chunk of a unified diff and checks it against
    any bad patterns, printing comments for any matches it finds.
    """
    extension = filename.rsplit(".", 1)[1] if "." in filename else ""
    occurrences = 0
    for i in range(lines):
        chunk_line = open_diff.readline().rstrip()
        for pattern in BAD_PATTERNS:
            if extension in pattern.extensions and pattern.regex.match(
                # The first character in chunk_line is +. We skip it.
                chunk_line[1:]
            ):
                # We found a bad pattern.
                occurrences += 1
                # Try getting suggestion, if one exists.
                if pattern.suggestion:
                    suggestion = pattern.regex.sub(pattern.suggestion, chunk_line[1:])
                else:
                    suggestion = ""
                print(pattern.to_comment(filename, start_line + i, suggestion))
    return occurrences


def main():
    occurrences = 0
    with fileinput.input() as diff:
        current_file = None
        chunk_indicator = re.compile(r"^@@ -[0-9,]* \+(\d*)(?:,(\d*))? @@.*$")
        while line := diff.readline().rstrip():
            if line.startswith("+++"):
                # Start of a new file.
                current_file = line.split("/", 1)[1]
            elif line.startswith("@@"):
                # Start of a new chunk.
                start_line, line_count = chunk_indicator.match(line).group(1, 2)
                # We pass the diff object so that `handle_chunk` can advance lines.
                occurrences += handle_chunk(
                    diff,
                    int(start_line),
                    current_file,
                    1 if line_count is None else int(line_count),
                )
    sys.exit(min(occurrences, 255))


if __name__ == "__main__":
    main()
