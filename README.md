CSV Query - Notepad++ plugin
============================

[![Build status](https://ci.appveyor.com/api/projects/status/j1r9m77jwiyfsn4u?svg=true)](https://ci.appveyor.com/project/jokedst/csvquery)

A plugin to Notepad++ to parse different types of CSV files and display them in a table.
The data is stored in a in-memory SQLite database (or MSSQL if configured), so you can write SQL queries against the data.

It tries to auto-detect separators, headers and column types. If it fails it asks you for it.

![screenshot](/Meta/Screenshot.png?raw=true "Small file with header row parsed")

License
-------
This package as a whole is licensed under the GPL v3. See gpl-3.0.txt

The CsharpSqlite code is licensed under MIT license (which is apparently GPL v3 compatible). See CsharpSqlite\License-CsharpSqlite-MIT.txt


Planned features
----------------

* Support for more types of CSV files (more test cases, basically)
* Optimizations


Auto-detection
--------------

CSV Query detects the separator by calculating the variance in occurrence of characters on each line, then chose the one that seems best, preferring one of comma, semicolon, pipe or tab. It's certainly not perfect, but it handles all the files I regularly work with, which is why I wrote the plugin in the first place.

If the first line is "significantly different" from the rest it assumes the first line is a header and use it for the column names in the database.


Used Libraries
--------------

### NppPlugin.NET v0.6-0.7 by UFO-Pu55y and later by kbilsted

Base for making Notepad++ plugins in C#

https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
Licensed under GPL v3


### CSharpSQLite

SQLite database rewritten in C#.
I basically copied the whole codebase into CSV Query to get a single DLL (and to mess around with it a bit).

https://code.google.com/p/csharp-sqlite/  
MIT Licence