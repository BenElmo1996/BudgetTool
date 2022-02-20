﻿using System;
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
            }

            // initiliases takeHomePay, nextPayDate and the SQLite reader to pull these values from the UserInfo table.
            string menuStatement = "SELECT * FROM UserInfo";
            using var menuReaderCommand = new SQLiteCommand(menuStatement, menuConnection);
            using SQLiteDataReader menuReader = menuReaderCommand.ExecuteReader();
            decimal takeHomePay = 0;
            DateTime nextPayDate = DateTime.Now; // Initialised as DateTime.Now but then assigned to next pay day below (throws error otherwise)
            while (menuReader.Read())
            {
                takeHomePay = menuReader.GetDecimal(1); // returns either the placeholder value of 0, or the users takehome pay if they have entered one
                nextPayDate = menuReader.GetDateTime(2); // Returns pay day as a datetime value
            }

            // Assigns a value based on comparison of next pay date and today's date. If today is earlier the below if statement is true.
            int monthlyResetCheck = DateTime.Compare(DateTime.Now, nextPayDate);
            if (monthlyResetCheck >= 0) // If today is later than pay day
            {
                
                nextPayDate = nextPayDate.AddMonths(1); // Increments next pay day by a month

                // Inserts the new pay day into UserInfo table 
                menuCommand.CommandText = "UPDATE UserInfo " +
                                              "SET NextPayDate = @PayDay WHERE id = 1";
                menuCommand.Parameters.AddWithValue("@NextPayDate", nextPayDate);
                menuCommand.Prepare();
                menuCommand.ExecuteNonQuery();

                // RESET PURCHASES TABLE IN HERE
                menuCommand.CommandText = "DROP TABLE Purchases";
                menuCommand.ExecuteNonQuery();

                // creates table again or program will throw error when attempting to reopen
                menuCommand.CommandText = "create table if not exists Purchases (id INTEGER PRIMARY KEY, Name TEXT, Price MONEY, Date INT)";
                menuCommand.ExecuteNonQuery();

                Console.WriteLine("Purchase Table has been reset following a new pay month.\n" +
                                  "");

                menuCommand.CommandText = "Update Expenses SET Flag = 0";
                menuCommand.ExecuteNonQuery();

            }

            menuStatement = "SELECT * FROM Expenses";
            using var menuReaderCommand2 = new SQLiteCommand(menuStatement, menuConnection);
            using SQLiteDataReader menuReader2 = menuReaderCommand2.ExecuteReader();
            int paidFlag;
            while (menuReader2.Read())
            {
                // if the payment is scheduled for before today's date, divide payment by contributors and add to upcomingContribution
                int upcomingExpenseCheck = DateTime.Compare(DateTime.Now, menuReader2.GetDateTime(3));

                int expenseID = menuReader2.GetInt32(0);
                menuCommand.CommandText = "UPDATE Expenses " +
                                          "SET Flag = @ExpenseFlag WHERE id = @ExpenseID";

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
