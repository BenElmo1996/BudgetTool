using System;
using System.Data.SQLite;


namespace SQLDemo
{
    /// <summary>
    /// Expense page is where all user expenses are viewed, inputted, edited and deleted. An expense would be a recurring monthly payment i.e. "bill".
    /// The methods used below are all stored in the ExpensePageMethods class.
    /// </summary>
    public class ExpensePage
    {

        private class Expense
        {
            public string expenseName { get; set; }
            public decimal expensePrice { get; set; }
            public int expenseDueDate { get; set; }
            public int expenseContributors { get; set; }
        }

        public static decimal totalUserContributions;

        // MAIN PAGE 
        public static void ExpenseMenu()
        {

            // Initialises an SQLiteConnection for use throughout this page
            string connectionString = "Data Source = ./Database.db";
            var expensesConnection = new SQLiteConnection(connectionString);
            expensesConnection.Open();

            // Main Expense menu loop
            while (true)
            {
                Console.Write("Expense Management\n" +
                                   "1. View expenses.\n" +
                                   "2. View upcoming expenses.\n" +
                                   "3. Add an expense.\n" +
                                   "4. Edit an expense.\n" +
                                   "5. Delete an expense.\n" +
                                   "6. Delete expenses table.\n" +
                                   "7. Return to Main Menu.\n" +
                                   "Menu Choice: ");

                string expenseMenuChoice = Console.ReadLine();

                //line break
                Console.WriteLine("");


                // VIEW EXPENSES BLOCK
                if (expenseMenuChoice == "1")
                {
                    // false argument because we don't want to display ID column. ALL arg to show all records.
                    ExpensePageMethods.ShowExpenses(false, "ALL");

                    Console.WriteLine($"Total expenses equate to £{ExpensePageMethods.GetSumOfExpenses("TOTAL")}. Your share of this total is £{ExpensePageMethods.GetSumOfExpenses("USER CONTRIBUTION")}");

                    //linebreak
                    Console.WriteLine("");
                }

                // VIEW UPCOMING EXPENSES BLOCK 
                else if (expenseMenuChoice == "2")
                {
                    // false argument because we don't want to display ID column. UPCOMING arg to show only future payments (after todays date).
                    ExpensePageMethods.ShowExpenses(false, "UPCOMING");

                    Console.WriteLine($"Total upcoming expenses equate to £{ExpensePageMethods.GetSumUpcomingExpenses("TOTAL")}. Your share of this total is £{ExpensePageMethods.GetSumUpcomingExpenses("USER CONTRIBUTION")}");

                    //linebreak
                    Console.WriteLine("");
                }

                // ADD NEW EXPENSE BLOCK
                else if (expenseMenuChoice == "3")
                {
                    bool exitAddExpense = false; // this flag changed to true and loop is exited when user inputs 'q'

                    // Main Loop for adding expenses
                    while (exitAddExpense == false)
                    {
                        Console.WriteLine("\n" +
                                          "Please enter a Name, Price, Payment Date and how many people the expense will be shared between.\n" +
                                          "Your payment date should be entered as a number, the suffix (i.e. th / rd / nd) is not required.\n" +
                                          "\n" +
                                          "Press enter to continue with adding an expense or alternatively input 'q' to return to menu.");

                        var userInput = Console.ReadLine();
                        if (userInput == "")
                        {
                            // creates a new Expense object from user input
                            Expense expense = new Expense();

                            
                            Console.Write("Name: ");
                            expense.expenseName = Console.ReadLine();

                            // takes user input for price and checks validity of input 
                            bool invalidInput = true;
                            while (invalidInput == true)
                            {
                                Console.Write("Price: ");
                                string tempPrice = Console.ReadLine();
                                if (ExpensePageMethods.CheckInvalidInputs(tempPrice, "validNum"))
                                {
                                    expense.expensePrice = decimal.Parse(tempPrice);
                                    invalidInput = false;
                                }
                                else
                                {
                                    Console.WriteLine($"{tempPrice} is an invalid input");
                                }
                            }

                            // takes user input for data and validity checks it
                            invalidInput = true;
                            while (invalidInput == true)
                            {
                                Console.Write("Payment Date: ");
                                string tempDate = Console.ReadLine();
                                if (ExpensePageMethods.CheckInvalidInputs(tempDate, "validDate"))
                                {
                                    expense.expenseDueDate = int.Parse(tempDate);
                                    invalidInput = false;
                                }
                                else
                                {
                                    Console.WriteLine($"{tempDate} is an invalid input");
                                }
                            }

                            // takes user input for how many contributors
                            invalidInput = true;
                            while (invalidInput == true)
                            {
                                Console.Write("Number of Contributors: ");
                                string tempContributors = Console.ReadLine();
                                if (ExpensePageMethods.CheckInvalidInputs(tempContributors, "validNum"))
                                {
                                    expense.expenseContributors = int.Parse(tempContributors);
                                    invalidInput = false;
                                }
                                else
                                {
                                    Console.WriteLine($"{tempContributors} is an invalid input");
                                }
                            }

                            // Initiliases command and passes placeholder values for new expense record
                            using var addExpenseCommand = new SQLiteCommand(expensesConnection);
                            addExpenseCommand.CommandText = "INSERT INTO Expenses(Name, Price, Date, Contributors) VALUES(@ExpenseName, @ExpensePrice, @ExpenseDate, @ExpenseContributors)";

                            addExpenseCommand.Parameters.AddWithValue("@ExpenseName", expense.expenseName);
                            addExpenseCommand.Parameters.AddWithValue("@ExpensePrice", expense.expensePrice);
                            addExpenseCommand.Parameters.AddWithValue("@ExpenseDate", expense.expenseDueDate);
                            addExpenseCommand.Parameters.AddWithValue("@ExpenseContributors", expense.expenseContributors);
                            addExpenseCommand.Prepare();

                            addExpenseCommand.ExecuteNonQuery();

                            Console.WriteLine("Expense Added");
                        }
                        // changes flag and quits this if block
                        else if (userInput == "q")
                        {
                            exitAddExpense = true;
                        }
                        else
                        {
                            Console.WriteLine($"{userInput} is Invalid Input.");
                        }
                    }
                    
                }

                // EDIT EXPENSES BLOCK
                else if (expenseMenuChoice == "4")
                {
                    // shows all expenses from expense table
                    ExpensePageMethods.ShowExpenses(true, "ALL");

                    // exit flag for this loop, change to true when user inputs 'q'.
                    bool exitEditExpense = false;
                    while (exitEditExpense == false)

                    {
                        // Initiliases sqlite command 
                        using var editExpenseCommand = new SQLiteCommand(expensesConnection);

                        // This will be the expense edited by user
                        Console.Write("Please select which expense to ammend from the ID's above (or input 'q' to quit): ");
                        var expenseID = Console.ReadLine();

                        //linebreak
                        Console.WriteLine("");

                        // exit loop here
                        if (expenseID == "q")
                        {
                            exitEditExpense = true;
                        }
                        // Checks if expenseID is a number or present in the ID column
                        else if (ExpensePageMethods.CheckInvalidInputs(expenseID, "validNum") == false || ExpensePageMethods.CheckInvalidInputs(expenseID, "validID") == false)
                        {
                            Console.WriteLine($"{expenseID} is not a valid ID\n" +
                                              $"");
                        }
                        else
                        {
                            Console.Write("Select which element of the expense you would like to update (or input 'q' to quit):\n" +
                                           "1. Name.\n" +
                                           "2. Price.\n" +
                                           "3. Payment Date.\n" +
                                           "4. Contributors.\n" +
                                           "5. All of the above.\n" +
                                           "" +
                                           "Menu Choice: ");
                            var editMenuChoice = Console.ReadLine();

                            // NAMES EDITED IN HERE
                            if (editMenuChoice == "1")
                            {
                                Console.Write("Please enter the new expense name: ");
                                string newExpenseName = Console.ReadLine();

                                editExpenseCommand.CommandText = "UPDATE Expenses " +
                                                          "SET Name = @ExpenseName WHERE id = @ExpenseID";

                                editExpenseCommand.Parameters.AddWithValue("@ExpenseName", newExpenseName);
                                editExpenseCommand.Parameters.AddWithValue("@ExpenseID", expenseID);
                                editExpenseCommand.Prepare();

                                editExpenseCommand.ExecuteNonQuery();

                                Console.WriteLine("Expense Name Edited");
                            }

                            // PRICE EDITED IN HERE
                            else if (editMenuChoice == "2")
                            {
                                ExpensePageMethods.EditExpensePrice(editExpenseCommand, expenseID);
                                Console.WriteLine("Expense Price Edited");
                            }
                            // DATES EDITED IN HERE
                            else if (editMenuChoice == "3")
                            {
                                ExpensePageMethods.EditExpenseDate(editExpenseCommand, expenseID);
                                Console.WriteLine("Payment Date Edited");
                            }
                            // CONTRIBUTORS EDITED IN HERE
                            else if (editMenuChoice == "4")
                            {
                                ExpensePageMethods.EditExpenseContributors(editExpenseCommand, expenseID);
                                Console.WriteLine("No. of Contributors Edited");
                            }

                            // NAME / PRICE / DATE EDITED IN HERE 
                            else if (editMenuChoice == "5")
                            {
                                Console.Write("Please enter the new expense name: ");
                                string newExpenseName = Console.ReadLine();
                                editExpenseCommand.CommandText = "UPDATE Expenses " +
                                                         "SET Name = @ExpenseName WHERE id = @ExpenseID";
                                editExpenseCommand.Parameters.AddWithValue("@ExpenseName", newExpenseName);
                                editExpenseCommand.Parameters.AddWithValue("@ExpenseID", expenseID);
                                editExpenseCommand.Prepare();
                                editExpenseCommand.ExecuteNonQuery();

                                ExpensePageMethods.EditExpensePrice(editExpenseCommand, expenseID);

                                ExpensePageMethods.EditExpenseDate(editExpenseCommand, expenseID);

                                Console.WriteLine("Expense Edited");

                            }
                            else if (editMenuChoice == "q")
                            {
                                exitEditExpense = true;
                            }
                            else
                            {
                                Console.WriteLine($"{editMenuChoice} is invalid input, please re-enter ID and appropriate menu choice.");
                            }
                        }

                    }
                }

                // DELETE EXPENSE BLOCK
                else if (expenseMenuChoice == "5")
                {
                    ExpensePageMethods.ShowExpenses(true, "ALL");

                    bool exitMenu = false;

                    while (exitMenu == false)
                    {
                        Console.Write("Please select which expense to delete from the ID's above (or input 'q' to quit): ");
                        var expenseID = Console.ReadLine();
                        if (expenseID == "q")
                        {
                            exitMenu = true;
                        }
                        else if (ExpensePageMethods.CheckInvalidInputs(expenseID, "validNum") == false || ExpensePageMethods.CheckInvalidInputs(expenseID, "validID") == false)
                        {
                            Console.WriteLine($"{expenseID} is not a valid ID\n" +
                                              $"");
                        }
                        else
                        {
                            // Initiliases command and passes placeholder values for new expense record
                            using var deleteExpenseCommand = new SQLiteCommand(expensesConnection);
                            deleteExpenseCommand.CommandText = "DELETE FROM Expenses WHERE id = @ExpenseID";

                            deleteExpenseCommand.Parameters.AddWithValue("@ExpenseID", expenseID);
                            deleteExpenseCommand.Prepare();

                            deleteExpenseCommand.ExecuteNonQuery();

                            Console.WriteLine("Expense Deleted\n" +
                                              "");
                        }
                    }
                }

                // Deletes ALL EXPENSES BLOCK
                else if (expenseMenuChoice == "6")
                {
                    using var deleteTableCommand = new SQLiteCommand(expensesConnection);
                    deleteTableCommand.CommandText = "DROP TABLE Expenses";
                    deleteTableCommand.ExecuteNonQuery();

                    // creates table again or program will throw error when attempting to reopen
                    deleteTableCommand.CommandText = "create table if not exists Expenses (id INTEGER PRIMARY KEY, Name TEXT, Price MONEY, Date INT, Contributors INT)";
                    deleteTableCommand.ExecuteNonQuery();

                    Console.WriteLine("Expenses Table has been deleted.\n" +
                                      "");

                }

                // RETURN TO MENU
                else if (expenseMenuChoice == "7")
                {
                    expensesConnection.Close();
                    MainMenu.Main();
                }

            }


        }

    }

}