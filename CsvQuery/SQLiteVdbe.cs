//  $Header$
using System;

namespace Community.CsharpSqlite
{

    using Vdbe = Sqlite3.Vdbe;

    /// <summary>
    /// C#-SQLite wrapper with functions for opening, closing and executing queries.
    /// </summary>
    public class SQLiteVdbe
    {
        private Vdbe vm = null;
        private string LastError = "";
        private int LastResult = 0;

        /// <summary>
        /// Creates new instance of SQLiteVdbe class by compiling a statement
        /// </summary>
        /// <param name="query"></param>
        /// <returns>Vdbe</returns>
        public SQLiteVdbe(SQLiteDatabase db, String query)
        {
            vm = null;

            // prepare and compile 
#if NET_35
      Sqlite3.PrepareV2NoTail
#else
            Sqlite3.sqlite3_prepare_v2
#endif
(db.Connection(), query, query.Length, ref vm, 0);
        }

        /// <summary>
        /// Return Virtual Machine Pointer
        /// </summary>
        /// <param name="query"></param>
        /// <returns>Vdbe</returns>
        public Vdbe VirtualMachine()
        {
            return vm;
        }

        /// <summary>
        /// <summary>
        /// BindInteger
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bInteger"></param>
        /// <returns>LastResult</returns>
        public int BindInteger(int index, int bInteger)
        {
            if ((LastResult =
#if NET_35
 Sqlite3.BindInt
#else
 Sqlite3.sqlite3_bind_int
#endif
(vm, index, bInteger)) == Sqlite3.SQLITE_OK)
            { LastError = ""; }
            else
            {
                LastError = "Error " + LastError + "binding Integer [" + bInteger + "]";
            }
            return LastResult;
        }

        /// <summary>
        /// <summary>
        /// BindLong
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bLong"></param>
        /// <returns>LastResult</returns>
        public int BindLong(int index, long bLong)
        {
            if ((LastResult =
#if NET_35
 Sqlite3.BindInt64
#else
 Sqlite3.sqlite3_bind_int64
#endif
(vm, index, bLong)) == Sqlite3.SQLITE_OK)
            { LastError = ""; }
            else
            {
                LastError = "Error " + LastError + "binding Long [" + bLong + "]";
            }
            return LastResult;
        }

        /// <summary>
        /// BindText
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bLong"></param>
        /// <returns>LastResult</returns>
        public int BindText(int index, string bText)
        {
            if ((LastResult =
#if NET_35
 Sqlite3.BindText
#else
 Sqlite3.sqlite3_bind_text
#endif
(vm, index, bText, -1, null)) == Sqlite3.SQLITE_OK)
            { LastError = ""; }
            else
            {
                LastError = "Error " + LastError + "binding Text [" + bText + "]";
            }
            return LastResult;
        }

        /// <summary>
        /// Execute statement
        /// </summary>
        /// </param>
        /// <returns>LastResult</returns>
        public int ExecuteStep()
        {
            // Execute the statement
            int LastResult =
#if NET_35
 Sqlite3.Step
#else
 Sqlite3.sqlite3_step
#endif
(vm);
            return LastResult;
        }

        /// <summary>
        /// Returns Result column as Long
        /// </summary>
        /// </param>
        /// <returns>Result column</returns>
        public long Result_Long(int index)
        {
            return
#if NET_35
 Sqlite3.ColumnInt64
#else
 Sqlite3.sqlite3_column_int64
#endif
(vm, index);
        }

        /// <summary>
        /// Returns Result column as Text
        /// </summary>
        /// </param>
        /// <returns>Result column</returns>
        public string Result_Text(int index)
        {
            return
#if NET_35
 Sqlite3.ColumnText
#else
 Sqlite3.sqlite3_column_text
#endif
(vm, index);
        }


        /// <summary>
        /// Returns Count of Result Rows
        /// </summary>
        /// </param>
        /// <returns>Count of Results</returns>
        public int ResultColumnCount()
        {
            return vm.pResultSet == null ? 0 : vm.pResultSet.Length;
        }

        /// <summary>
        /// Reset statement
        /// </summary>
        /// </param>
        /// </returns>
        public void Reset()
        {
            // Reset the statment so it's ready to use again
#if NET_35
      Sqlite3.Reset
#else
            Sqlite3.sqlite3_reset
#endif
(vm);
        }

        /// <summary>
        /// Closes statement
        /// </summary>
        /// <returns>LastResult</returns>
        public void Close()
        {
#if NET_35
      Sqlite3.Finalize
#else
            Sqlite3.sqlite3_finalize
#endif
(vm);
        }

    }
}
