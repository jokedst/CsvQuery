CSV Query - Notepad++ plugin
============================

A plugin to Notepad++ to parse different types of CSV files and display them in a table.
The data is stored in a in-memory SQLite database, so you can write SQL queries against the data.

It tries to auto-detect separators, headers and column types. If it can't it just fails - there is currently no way to tell it what separator to use.

License
-------
This package as a whole is licensed under the GPL v3. See gpl-3.0.txt

The CsharpSqlite code is licensed undet MIT license (as apparantly allowed by GPL v3 section 7). See CsharpSqlite\License-CsharpSqlite-MIT.txt


Planned features
----------------

* A settings window where you can specify separator if the auto-detect fails (or detects the wrong one)
* Support for quoted values where we ignore the separator (e.g. 12,"quote, here",700)
* Better type detection so numbers aren't treated as strings in the SQL queries :P
* Support for more types of CSV files (more test cases, basically)
* Optimizations

Auto-detection
--------------

CSV Query detects the separator by calculating the variance in occurance of characters on each line, then chose the one that seems best, prefering one of comma, semicolon, pipe or tab. It's certanly not perfect, but it handles all the files I regarly work with, which is why I wrote the plugin in the first place.

If the first line is all strings and the rest of the lines have at least one numeric column, it assumes the first line is a header and use it for the column names in the database.

The column types are detected by simply doing a *double.TryParse()* on the strings. It seems to be the fastest way.

Used Libraries
--------------

### NppPlugin.NET v0.6 by UFO-Pu55y

Base for making Notepad++ plugins in C#

As far as I know it doesn't have it's own page, but it's available in the downloads from the SourceCookifier Sourceforge project:
http://sourceforge.net/projects/sourcecookifier/
Licenced under GPL v3


### CSharpSQLite

SQLite database rewritten in C#.
I basically copied the whole codebase into CSV Query to get a single DLL (and to mess around with it a bit).

https://code.google.com/p/csharp-sqlite/
MIT Licence