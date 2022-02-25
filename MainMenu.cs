using System;
using System.IO;
using System.Data.SQLite;

namespace SQLDemo
{
    public class MainMenu
    {
        public static void Main()
        {
            Console.Clear();


            // Opens connection with database in order to check status of DB tables
            string connectionString = "Data Source = ./Database.db";
            var menuConnection = new SQLiteConnection(connectionString);
            menuConnection.Open(); // Database is not closed until user closes or returns to main menu
            using var menuCommand = new SQLiteCommand(menuConnection);

            if (File.Exists("./Database.db") == false)
            {
                SQLiteConnection.CreateFile("./Database.db");
            }

            // Checks if Expenses table exists, if not it creates table
            menuCommand.CommandText = "create table if not exists Expenses (id INTEGER PRIMARY KEY, Name TEXT, Price MONEY, Date TEXT, Contributors INT, Flag INT)";
            menuCommand.ExecuteNonQuery();

            // Checks if Purchases table exists, if not it creates table
            menuCommand.CommandText = "create table if not exists Purchases (id INTEGER PRIMARY KEY, Name TEXT, Price MONEY, Date INT)";
            menuCommand.ExecuteNonQuery();

            // Determines whether UserInfo table exists, if not it creates it and passes some default values. Logic here is a bit longer as we are adding default values.
            menuCommand.CommandText = "SELECT count(*) FROM sqlite_master WHERE type = 'table'"; // Counts all the tables in the database
            int tableCount = Convert.ToInt32(menuCommand.ExecuteScalar()); // Executes the above command and stores output as an int value
            if (tableCount == 2) // If there are only two tables then create UserInfo. 
            {
                menuCommand.CommandText = "CREATE TABLE UserInfo(id INTEGER PRIMARY KEY, TakehomePay MONEY, PayDay TEXT)";
                menuCommand.ExecuteNonQuery();
                menuCommand.CommandText = "INSERT INTO UserInfo(TakehomePay, PayDay) VALUES(0, '2050-01-01')"; // placeholder values here, there's certain functions that rely on their being values hence adding them.
                menuCommand.ExecuteNonQuery();

                Console.WriteLine("Please Navigate to Menu Option 3 and Input a Pay Day and Takehome Salary.\n" +
                                  "");
            }

            // Pulls takehome pay and next pay day from UserInfo table
            (decimal takeHomePay, DateTime nextPayDay) = MenuMethods.GetUserInfo(menuConnection);

            // Compares dates, if today is after payday then reset data method is called
            int monthlyResetCheck = DateTime.Compare(DateTime.Now, nextPayDay);
            if (monthlyResetCheck >= 0) // If today is later than pay day
            {
                MenuMethods.ResetMonthlyData(menuConnection, menuCommand, nextPayDay);
            }
            // Determines which expenses have been paid and sets flags in DB accordingly
            MenuMethods.SetExpenseFlags(menuConnection, menuCommand);

            bool exitMenu = false; // initialise exit menu flag, not currently used.
            while (exitMenu == false)
            {
                Console.Write("Personal Finance & Budget Tracker.\n" +
                                  "\n" +
                                  $"Your Remaining Expendable Income This Month: £{takeHomePay - ExpensePageMethods.GetSumOfExpenses("USER CONTRIBUTION") - PurchasePageMethods.GetSumOfPurchases()}\n" +
                                  "\n" +
                                  "Please choose from a menu option below:\n" +
                                  "1. Monthly Purchases.\n" +
                                  "2. Expenses\n" +
                                  "3. Edit User Info.\n" +
                                  "Menu Choice: ");

                var mainMenuChoice = Console.ReadLine();

                if (mainMenuChoice == "1")
                {
                    PurchasesPage.PurchaseMenu();
                }
                else if (mainMenuChoice == "2")
                {
                    ExpensePage.ExpenseMenu();
                }
                else if (mainMenuChoice == "3")
                {
                    UserInfoPage.UserInfoMenu();
                }
                else if (mainMenuChoice == "q")
                {
                    exitMenu = true;
                }
                else
                {
                    Console.WriteLine("Invalid Input");
                }
            }
        }
    }
}
