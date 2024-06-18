## pwgen
Cross-platform program written in C# that generates passwords from the terminal.

[Repository on GitHub](https://github.com/pmpwsk/pwgen)

# Main features
- Generating passwords
- Simple output so the password can be piped into other programs
- Various character sets, including all senseful ASCII characters and all possible characters in C#
- URL encoding of the result
- Copying the result to the clipboard on all platforms

# Installation
This program is portable, simply download a binary for your system from the releases on GitHub (SC means self-contained, FD means framework-dependent so it needs dotnet installed) or build the code yourself using the dotnet command or an IDE of your choosing, then execute the file <code>pwgen</code> on Linux and macOS or <code>pwgen.exe</code> on Windows.

# Usage
Run <code>pwgen -h</code> for detailed instructions on how to use pwgen.

pwgen exits when a password was generated, so it should be executed from a terminal if you want to see the password. Simply copying the password without seeing it is possible.