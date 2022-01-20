using System;
using System.Data.SQLite;
using System.ComponentModel.DataAnnotations;

namespace SQLDemo
{

    public class ExpensePage
    {

        private class Expense
        {   

            public string expenseName { get; set; }
            public decimal expenseAmount { get; set; } 
            public int expenseDueDate { get; set; }


        }


    

    static void ShowAllExpenses(bool showIdColumn)
        {
            /// <summary>
            /// Function that displays all expense records with labelled columns. Iterates through the Datbase.db Expenses table
            /// and prints line by line. Used in menu options 1 (view expenses) and 3 (edit expense).
            ///
            /// Takes one argument, if true displays all expenses including record ID. If false displays name / price / payment date and no ID column.
            /// </summary>

            // Opens Database
            string viewExpensesConString = "Data Source = ./Database.db"; // Connection string for this menu 
            using var viewExpensesCon = new SQLiteConnection(viewExpensesConString);
            viewExpensesCon.Open();

            // Selects all from Expenses and intialises the reader 
            string viewExpensesStatement = "SELECT * FROM Expenses";
            using var viewExpensesCommand = new SQLiteCommand(viewExpensesStatement, viewExpensesCon);
            using SQLiteDataReader viewExpensesReader = viewExpensesCommand.ExecuteReader();

            // Edit and delete expense use true argument when calling this function, view expenses uses false argument. Keeps display tidy as user doesn't interact with ID's on view expense.
            if (showIdColumn == true)
            {
                Console.WriteLine($"{viewExpensesReader.GetName(0),-3} {viewExpensesReader.GetName(1),-15} {viewExpensesReader.GetName(2),-8} {viewExpensesReader.GetName(3),-3}"); 
                while (viewExpensesReader.Read())
                {
                    Console.WriteLine($"{viewExpensesReader.GetInt32(0),-3} {viewExpensesReader.GetString(1),-15} {viewExpensesReader.GetDecimal(2),-8} {viewExpensesReader.GetInt32(3),-3}");
                }
            }
            else
            {
                Console.WriteLine($"{viewExpensesReader.GetName(1),-15} {viewExpensesReader.GetName(2),-8} {viewExpensesReader.GetName(3),-3}"); 
                while (viewExpensesReader.Read())
                {
                    Console.WriteLine($"{viewExpensesReader.GetString(1),-15} {viewExpensesReader.GetDecimal(2),-8} {viewExpensesReader.GetInt32(3),-3}");
                }
            }
            // linebreak 
            Console.WriteLine("");
            
            viewExpensesCon.Close();
        }

        static void GetSumOfExpenses()
        {
            string getSumOfExpensesConString = "Data Source = ./Database.db"; // Connection string for this menu 
            using var getSumOfExpensesCon = new SQLiteConnection(getSumOfExpensesConString);
            getSumOfExpensesCon.Open();

            using var getTotalExpenses = new SQLiteCommand(getSumOfExpensesCon);
            getTotalExpenses.CommandText = "SELECT SUM(Amount) FROM Expenses";
            getTotalExpenses.ExecuteNonQuery();

            // DBNull check here otherwise program will throw an error
            if (getTotalExpenses.ExecuteScalar() is DBNull)
            {
                Console.WriteLine("No expenses recorded yet, when you have added some they will appear here.\n" +
                                  "");

            }
            else
            {
                decimal totalExpenses = Convert.ToDecimal(getTotalExpenses.ExecuteScalar());

                Console.WriteLine($"Your total monthly expenses equate to £{totalExpenses}\n" +
                                  $"");
            }
        }


        public static void ExpenseMenu()
        {

            // Opens connection with database, this connection string and connection are used multiple times throughout menu.
            string connectionString = "Data Source = ./Database.db"; 
            var expensesConnection = new SQLiteConnection(connectionString);
            expensesConnection.Open(); // Database is not closed until user closes or returns to main menu

            // Checks if Expenses table exists, if not it creates table
            using var checkTable = new SQLiteCommand(expensesConnection);
            checkTable.CommandText = "create table if not exists Expenses (id INTEGER PRIMARY KEY, Name TEXT, Amount MONEY, Date INT)";
            checkTable.ExecuteNonQuery();


            // Main Expense menu loop
            while (true)
            {
                Console.Write("Expense Management\n" +
                                   "1. View expenses.\n" +
                                   "2. Add an expense.\n" +
                                   "3. View upcoming expenses.\n" +
                                   "4. Edit an expense.\n" +
                                   "5. Delete an expenses.\n" +
                                   "6. Delete expenses table.\n" +
                                   "" +
                                   "Menu Choice: ");

                string expenseMenuChoice = Console.ReadLine();


                // View Expenses block
                if (expenseMenuChoice == "1")
                {
                    // false argument because we don't want to display ID column
                    ShowAllExpenses(false);

                    using var getTotalExpenses = new SQLiteCommand(expensesConnection);
                    getTotalExpenses.CommandText = "SELECT SUM(Amount) FROM Expenses";
                    getTotalExpenses.ExecuteNonQuery();

                    // DBNull check here otherwise program will throw an error
                    if (getTotalExpenses.ExecuteScalar() is DBNull)
                    {
                        Console.WriteLine("No expenses recorded yet, when you have added some they will appear here.\n" +
                                          "");

                    }
                    else
                    {
                        decimal totalExpenses = Convert.ToDecimal(getTotalExpenses.ExecuteScalar());

                        Console.WriteLine($"Your total monthly expenses equate to £{totalExpenses}\n" +
                                          $"");
                    }
                }

                // Add an Expense block
                else if (expenseMenuChoice == "2")
                {

                    bool exitAddExpense = false; // this flag changed to true and loop is exited when user inputs 'q'

                    // Main Loop for adding expenses
                    while (exitAddExpense == false)
                    {
                        Console.WriteLine("\n" +
                                          "Please enter a Name, Price and Payment Date for your expense.\n" +
                                          "Your payment date should be entered as a number, the suffix (i.e. th / rd / nd) is not required.\n" +
                                          "\n" +
                                          "Press enter to continiue with adding an expense or alternatively input 'q' to return to menu.");

                        var userInput = Console.ReadLine();

                        // if user hits enter
                        if (userInput == "")
                        {
                            // creates a new Expense object from user input
                            Expense expense = new Expense();
                            Console.Write("Name: ");
                            expense.expenseName = Console.ReadLine();
                            Console.Write("Amount: ");
                            expense.expenseAmount = decimal.Parse(Console.ReadLine());
                            Console.Write("Payment Date: ");
                            expense.expenseDueDate = int.Parse(Console.ReadLine());

                            // Initiliases command and passes placeholder values for new expense record
                            using var addExpenseCommand = new SQLiteCommand(expensesConnection);
                            addExpenseCommand.CommandText = "INSERT INTO Expenses(Name, Amount, Date) VALUES(@ExpenseName, @ExpenseAmount, @ExpenseDate)";

                            addExpenseCommand.Parameters.AddWithValue("@ExpenseName", expense.expenseName);
                            addExpenseCommand.Parameters.AddWithValue("@ExpenseAmount", expense.expenseAmount);
                            addExpenseCommand.Parameters.AddWithValue("@ExpenseDate", expense.expenseDueDate);
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
                else if (expenseMenuChoice == "3")
                {
                    // Initialises command to select all from Expenses table in Date order (low to high) 
                    using var viewUpcomingExpenses = new SQLiteCommand(expensesConnection);
                    string viewUpcomingExpensesStatement = "SELECT * FROM Expenses order by Date";
                    using var viewUpcomingExpensesCommand = new SQLiteCommand(viewUpcomingExpensesStatement, expensesConnection);
                    using SQLiteDataReader viewExpensesReader = viewUpcomingExpensesCommand.ExecuteReader();

                    // Displays column headers, no ID column
                    Console.WriteLine($"{viewExpensesReader.GetName(1),-15} {viewExpensesReader.GetName(2),-8} {viewExpensesReader.GetName(3),-3}"); 

                    // Todays date for determining which payments are still to be made
                    int dayOfMonth = DateTime.Today.Day;

                    // Writes all upcoming payments due on or after todays date this month
                    while (viewExpensesReader.Read())
                    {
                        if (dayOfMonth <= viewExpensesReader.GetInt32(3))
                        {
                            Console.WriteLine($"{viewExpensesReader.GetInt32(0),-3} {viewExpensesReader.GetString(1),-15} {viewExpensesReader.GetDecimal(2),-8} {viewExpensesReader.GetInt32(3),-3}");
                        }
                    }

                    GetSumOfExpenses();

                    // linebreak
                    Console.WriteLine("");
                }

                // Edit current expenses records in this block
                else if (expenseMenuChoice == "4")
                {
                    // exit flag for this loop, change to true when user inputs 'q'.
                    bool exitEditExpense = false;

                    

                    while (exitEditExpense == false)
                       
                    {
                        ShowAllExpenses(true);

                        // Initiliases command and passes placeholder values for new expense record
                        using var editExpenseCommand = new SQLiteCommand(expensesConnection);

                        // This will be the expense edited by user
                        Console.Write("Please select which expense to ammend from the ID's above: ");
                        var expenseID = Console.ReadLine();

                        Console.Write("Select which element of the expense you would like to update (or input 'q' to quit):\n" +
                                       "1. Name.\n" +
                                       "2. Price.\n" +
                                       "3. Payment Date.\n" +
                                       "4. All of the above.\n" +
                                       "" +
                                       "Menu Choice: ");
                        var editMenuChoice = Console.ReadLine();

                        // If block for just editing the expense name
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

                            Console.WriteLine("Expense Edited");
                        }

                        // if block for editing the expense price
                        else if (editMenuChoice == "2")
                        {
                            Console.Write("Please enter the new expense price : ");
                            decimal newExpensePrice = decimal.Parse(Console.ReadLine());

                            editExpenseCommand.CommandText = "UPDATE Expenses " +
                                                      "SET Amount = @ExpensePrice WHERE id = @ExpenseID";

                            editExpenseCommand.Parameters.AddWithValue("@ExpensePrice", newExpensePrice);
                            editExpenseCommand.Parameters.AddWithValue("@ExpenseID", expenseID);
                            editExpenseCommand.Prepare();

                            editExpenseCommand.ExecuteNonQuery();

                            Console.WriteLine("Expense Edited");

                        }

                        // if block for editing the payment date
                        else if (editMenuChoice == "3")
                        {
                            Console.Write("Please enter the new expense payment date : ");
                            int newExpenseDate = int.Parse(Console.ReadLine());

                            editExpenseCommand.CommandText = "UPDATE Expenses " +
                                                      "SET Date = @ExpenseDate WHERE id = @ExpenseID";

                            editExpenseCommand.Parameters.AddWithValue("@ExpenseDate", newExpenseDate);
                            editExpenseCommand.Parameters.AddWithValue("@ExpenseID", expenseID);
                            editExpenseCommand.Prepare();

                            editExpenseCommand.ExecuteNonQuery();

                            Console.WriteLine("Expense Edited");

                        }

                        // if block for editing the entire expense (name / price / date) 
                        else if (editMenuChoice == "4")
                        {
                            Console.Write("Please enter the new expense name: ");
                            string newExpenseName = Console.ReadLine();
                            Console.Write("Please enter the new expense price : ");
                            decimal newExpensePrice = decimal.Parse(Console.ReadLine());
                            Console.Write("Please enter the new expense payment date : ");
                            int newExpenseDate = int.Parse(Console.ReadLine());

                            editExpenseCommand.CommandText = "UPDATE Expenses " +
                                                      "SET Name = @ExpenseName, Amount = @ExpensePrice, Date = @ExpenseDate WHERE id = @ExpenseID";


                            editExpenseCommand.Parameters.AddWithValue("@ExpenseName", newExpenseName);
                            editExpenseCommand.Parameters.AddWithValue("@ExpensePrice", newExpensePrice);
                            editExpenseCommand.Parameters.AddWithValue("@ExpenseDate", newExpenseDate);
                            editExpenseCommand.Parameters.AddWithValue("@ExpenseID", expenseID);
                            editExpenseCommand.Prepare();

                            editExpenseCommand.ExecuteNonQuery();

                            Console.WriteLine("Expense Edited");

                        }
                        else if (editMenuChoice == "q")
                        {
                            exitEditExpense = true;
                        }
                        else
                        {
                            Console.WriteLine($"{editMenuChoice} is invalid input, please enter a menu choice or 'q' to quit.");
                        }
                    }
                }

                // Block for deleting individual records
                else if (expenseMenuChoice == "5")
                {

                    ShowAllExpenses(true);

                    Console.Write("Please select which expense to delete from the ID's above: ");
                    var expenseID = Console.ReadLine();

                    // Initiliases command and passes placeholder values for new expense record
                    using var deleteExpenseCommand = new SQLiteCommand(expensesConnection);
                    deleteExpenseCommand.CommandText = "DELETE FROM Expenses WHERE id = @ExpenseID";

                    deleteExpenseCommand.Parameters.AddWithValue("@ExpenseID", expenseID);
                    deleteExpenseCommand.Prepare();

                    deleteExpenseCommand.ExecuteNonQuery();

                    Console.WriteLine("Expense Deleted");

                }

                // Deletes the expenses table 
                else if (expenseMenuChoice == "5")
                {
                    using var deleteTableCommand = new SQLiteCommand(expensesConnection);
                    deleteTableCommand.CommandText = "DROP TABLE Expenses";
                    deleteTableCommand.ExecuteNonQuery();

                    // creates table again or program will throw error when attempting to reopen
                    checkTable.CommandText = "create table if not exists Expenses (id INTEGER PRIMARY KEY, Name TEXT, Amount MONEY, Date INT)";
                    checkTable.ExecuteNonQuery();

                }

            }


        }

    }

}