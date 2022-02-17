using System;
using System.Data.SQLite;

namespace SQLDemo
{
    public class MainMenu
    {
        public static void Main()
        {
            // Opens connection with database in order to check status of DB tables
            string connectionString = "Data Source = ./Database.db";
            var menuConnection = new SQLiteConnection(connectionString);
            menuConnection.Open(); // Database is not closed until user closes or returns to main menu
            using var menuCommand = new SQLiteCommand(menuConnection);

            // Checks if Expenses table exists, if not it creates table
            menuCommand.CommandText = "create table if not exists Expenses (id INTEGER PRIMARY KEY, Name TEXT, Price MONEY, Date INT, Contributors INT)";
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
            }

            // initiliases takeHomePay, nextPayDate and the SQLite reader to pull these values from the UserInfo table.
            string menuStatement = "SELECT * FROM UserInfo";
            using var menuReaderCommand = new SQLiteCommand(menuStatement, menuConnection);
            using SQLiteDataReader menuReader = menuReaderCommand.ExecuteReader();
            decimal takeHomePay = 0;
            DateTime nextPayDate = DateTime.Now; // this variable used for calculating whether payday has passed.
            while (menuReader.Read())
            {
                takeHomePay = menuReader.GetDecimal(1); // returns either the placeholder value of 0, or the users takehome pay if they have entered one
                nextPayDate = menuReader.GetDateTime(2); // Returns pay day as a datetime value
            }

            // Assigns a value based on comparison of next pay date and today's date. If today is earlier the below if statement is true.
            int monthlyResetCheck = DateTime.Compare(DateTime.Now, nextPayDate);
            if (monthlyResetCheck >= 0) // If today is later than next pay day
            {
                // RESET PURCHASES TABLE IN HERE
                menuCommand.CommandText = "DROP TABLE Purchases";
                menuCommand.ExecuteNonQuery();

                // creates table again or program will throw error when attempting to reopen
                menuCommand.CommandText = "create table if not exists Purchases (id INTEGER PRIMARY KEY, Name TEXT, Price MONEY, Date INT)";
                menuCommand.ExecuteNonQuery();

                Console.WriteLine("Purchase Table has been reset following a new pay month.\n" +
                                  "");
            }

            bool exitMenu = false; // initialise exit menu flag, not currently used.
            while (exitMenu == false)
            {
                Console.Write("Welcome to the finance and budget tracking app.\n" +
                                  "\n" +
                                  $"Your remaining expendable income is: £{takeHomePay - ExpensePageMethods.GetSumOfExpenses("USER CONTRIBUTION") - PurchasePageMethods.GetSumOfPurchases()}\n" + 
                                  "\n" +
                                  "Please choose from a menu option below\n" +
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
