using System;
using System.Data.SQLite;

namespace SQLDemo
{
    public class MenuMethods
    {


        /// <summary>
        /// Pulls takehome pay and next pay date from the UserInfo DB table.
        /// Used throughout the main menu for determining expendable income and when to reset the data
        /// </summary>
        /// <param name="menuConnection"></param>
        /// <returns></returns>
        public static Tuple<decimal, DateTime> GetUserInfo(SQLiteConnection menuConnection)
        {
            string menuStatement = "SELECT * FROM UserInfo";
            using var menuReaderCommand = new SQLiteCommand(menuStatement, menuConnection);
            using SQLiteDataReader menuReader = menuReaderCommand.ExecuteReader();


            decimal takeHomePay = 0;
            DateTime nextPayDate = DateTime.Now; // Initialised as DateTime.Now but then assigned to next pay day below (throws error otherwise)
            while (menuReader.Read())
            {
                takeHomePay = menuReader.GetDecimal(1); 
                nextPayDate = menuReader.GetDateTime(2); 
            }

            return Tuple.Create(takeHomePay, nextPayDate);
        }



        /// <summary>
        /// Called in the main menu if the program is opened after the users pay day.
        /// When called this drops and recreates the purchases table, sets all expense paid flags to 0 or 3.
        /// 0 if the expenses payment date is in the future, 3 if its in the past.
        /// </summary>
        /// <param name="menuConnection"></param>
        /// <param name="menuCommand"></param>
        /// <param name="nextPayDate"></param>
        public static void ResetMonthlyData(SQLiteConnection menuConnection, SQLiteCommand menuCommand, DateTime nextPayDate)
        {
            // Assigns a value based on comparison of next pay date and today's date. If today is earlier the below if statement is true.
            int monthlyResetCheck = DateTime.Compare(DateTime.Now, nextPayDate);
            if (monthlyResetCheck >= 0) // If today is later than pay day
            {
                // Updates payday to next month's date
                UserInfoPage.UpdatePayDay(nextPayDate.Day, menuCommand);

                // Reset and recreate purchases table
                menuCommand.CommandText = "DROP TABLE Purchases";
                menuCommand.ExecuteNonQuery();
                menuCommand.CommandText = "create table if not exists Purchases (id INTEGER PRIMARY KEY, Name TEXT, Price MONEY, Date INT)";
                menuCommand.ExecuteNonQuery();

                Console.WriteLine("Purchase Table has been reset following a new pay month.\n" +
                                  "");

                
                string menuStatement = "SELECT * FROM Expenses";
                using var menuReaderCommand = new SQLiteCommand(menuStatement, menuConnection);
                using SQLiteDataReader menuReader = menuReaderCommand.ExecuteReader();

                // This Loop sets the expense flags, 0 is for upcoming 3 is for payments that have already passed.
                while (menuReader.Read())
                {
                    int paidFlag;
                    int expenseID = menuReader.GetInt32(0);
                    int year = DateTime.Now.Year;
                    int month = DateTime.Now.Month;
                    DateTime expensePaymentDay = new DateTime(year, month, menuReader.GetDateTime(3).Day);

                    // if it has passed payment day this month, payment day set to that day the following month
                    if (DateTime.Now.Day > menuReader.GetDateTime(3).Day)
                    {
                        expensePaymentDay = expensePaymentDay.AddMonths(1);
                        paidFlag = 3; // 3 is neccessary for the SetExpenseFlagMethods
                    }
                    else
                    {
                        paidFlag = 0; // 0 indicates payment is still to pay
                    }

                    menuCommand.CommandText = "UPDATE Expenses " +
                                          "SET Date = @ExpenseDate, Flag = @ExpenseFlag WHERE id = @ExpenseID";

                    menuCommand.Parameters.AddWithValue("@ExpenseDate", expensePaymentDay);
                    menuCommand.Parameters.AddWithValue("@ExpenseFlag", paidFlag);
                    menuCommand.Parameters.AddWithValue("@ExpenseID", expenseID);
                    menuCommand.Prepare();
                    menuCommand.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Runs everytime main menu is launched. Iterates through the expenses table and assigns a flag of 1 if the date has passed.
        /// A flag of 3 indicates that the program was launched post pay day and expenses were missed, these payments were marked with a
        /// 3 flag, incremented by a month and are skipped.
        /// </summary>
        /// <param name="menuConnection"></param>
        /// <param name="menuCommand"></param>
        public static void SetExpenseFlags(SQLiteConnection menuConnection, SQLiteCommand menuCommand)
        {
            string menuStatement2 = "SELECT * FROM Expenses";
            using var menuReaderCommand2 = new SQLiteCommand(menuStatement2, menuConnection);
            using SQLiteDataReader menuReader2 = menuReaderCommand2.ExecuteReader();
            
            while (menuReader2.Read())
            {
               
                // compares current day with expense date, if expense date is later then this will return a value less than 0.
                int upcomingExpenseCheck = DateTime.Compare(DateTime.Now, menuReader2.GetDateTime(3));
                
                int expenseID = menuReader2.GetInt32(0);
               
                int paidFlag;
                if (menuReader2.GetInt32(5) != 3)
                {
                    
                    if (upcomingExpenseCheck >= 0)
                    {
                        paidFlag = 1;
                    }
                    else
                    {
                        paidFlag = 0;
                    }
                    menuCommand.Parameters.AddWithValue("@ExpenseFlag", paidFlag);
                    menuCommand.Parameters.AddWithValue("@ExpenseID", expenseID);
                    menuCommand.Prepare();
                    menuCommand.ExecuteNonQuery();
                }
            }
        }

    }
}
