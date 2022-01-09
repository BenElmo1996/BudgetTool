using System;
using System.Data.SQLite;

namespace SQLDemo
{
    
    public class ExpensePage
    {
        /// <summary>
        /// Contains all classes and methods used by the expenses menu / sub menu's / options
        /// </summary>

        public class Expense
        {
            /// <summary>
            /// blueprint for all user created expenses
            /// </summary>
            public string expenseName;
            public double expenseAmount;
            public int expenseDueDate;
        }

            static void ShowAllExpenses()
        {
            /// <summary>
            /// Function that displays all expense records with labelled columns. Iterates through the Datbase.db expenses table
            /// and prints line by line. Used in menu options 1 (view expenses) and 3 (edit expense).
            /// </summary>

            string viewExpensesCS = "Data Source = ./Database.db"; // Connection string for this menu 

            // Opens connection with the DB
            using var viewExpensesCon = new SQLiteConnection(viewExpensesCS);
            viewExpensesCon.Open();

            // SQL statement to select all from the 'Expenses' table and passes statement to the DB connection
            string viewExpensesSTM = "SELECT * FROM Expenses";
            using var viewExpensesCMD = new SQLiteCommand(viewExpensesSTM, viewExpensesCon);

            using SQLiteDataReader viewExpensesRDR = viewExpensesCMD.ExecuteReader();// Creates reader from the command created above, iterates over each record and passes them to print
            Console.WriteLine($"{viewExpensesRDR.GetName(0),-3} {viewExpensesRDR.GetName(1),-15} {viewExpensesRDR.GetName(2),-8} {viewExpensesRDR.GetName(3),-3}"); // This line shows column headers

            // iterates through expenses table and prints each record 
            while (viewExpensesRDR.Read())
            {
                Console.WriteLine($"{viewExpensesRDR.GetInt32(0),-3} {viewExpensesRDR.GetString(1),-15} {viewExpensesRDR.GetFloat(2),-8} {viewExpensesRDR.GetInt32(3),-3}");
            }

            // prints a linebreak between list of expenses and next line
            Console.WriteLine("");

            viewExpensesCon.Close();
        }



        public static void ExpenseMenu()
        {
            // Main Expense menu loop
            while (true)
            {
                Console.Write("Expense Management\n" +
                                   "1. View expenses.\n" +
                                   "2. Add an expense.\n" +
                                   "3. Edit an expense.\n" +
                                   "4. Delete an expenses.\n" +
                                   "" +
                                   "Menu Choice: ");

                string expenseMenuChoice = Console.ReadLine();


                // View Expenses block
                if (expenseMenuChoice == "1")
                {
                    ShowAllExpenses();
                }

                // Add Expenses block
                else if (expenseMenuChoice == "2")
                {

                    bool exitAddExpense = false; // this flag changed to true and loop is exited when user inputs 'q'

                    // Main Loop for adding expenses
                    while (exitAddExpense == false)
                    {
                        Console.WriteLine("\n" +
                                          "Please enter a name, amount and payment date for your expense.\n" +
                                          "Date is the day of the month the payment is due.\n" +
                                          "\n" +
                                          "Press enter to add an expense or input 'q' to quit.");

                        var userInput = Console.ReadLine();

                        // if user hits enter
                        if (userInput == "")
                        {
                            // Opens connection with the DB
                            string addExpensesCS = "Data Source = ./Database.db"; // Connection string for this menu 
                            var addExpensesCon = new SQLiteConnection(addExpensesCS);
                            addExpensesCon.Open();

                            // creates a new Expense object from user input
                            Expense expense = new Expense();
                            Console.Write("Name: ");
                            expense.expenseName = Console.ReadLine();
                            Console.Write("Amount: ");
                            expense.expenseAmount = double.Parse(Console.ReadLine());
                            Console.Write("Payment Date: ");
                            expense.expenseDueDate = int.Parse(Console.ReadLine());

                            // Initiliases command and passes placeholder values for new expense record
                            using var addExpensesCMD = new SQLiteCommand(addExpensesCon);
                            addExpensesCMD.CommandText = "INSERT INTO Expenses(Name, Amount, Date) VALUES(@ExpenseName, @ExpenseAmount, @ExpenseDate)";

                            // Placeholders - dont understand these yet
                            addExpensesCMD.Parameters.AddWithValue("@ExpenseName", expense.expenseName);
                            addExpensesCMD.Parameters.AddWithValue("@ExpenseAmount", expense.expenseAmount);
                            addExpensesCMD.Parameters.AddWithValue("@ExpenseDate", expense.expenseDueDate);
                            addExpensesCMD.Prepare();

                            addExpensesCMD.ExecuteNonQuery();

                            Console.WriteLine("Expense Added");

                            addExpensesCon.Close();
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
                    // exit flag for this loop, change to true when user inputs 'q'.
                    bool exitEditExpense = false;

                    ShowAllExpenses();

                    while (exitEditExpense == false)
                    {



                        // Opens connection with the DB
                        string editExpensesCS = "Data Source = ./Database.db"; // Connection string for this menu 
                        var editExpensesCon = new SQLiteConnection(editExpensesCS);
                        editExpensesCon.Open();


                        // Initiliases command and passes placeholder values for new expense record
                        using var editExpensesCMD = new SQLiteCommand(editExpensesCon);

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

                        // If block for just editing the name
                        if (editMenuChoice == "1")
                        {
                            Console.Write("Please enter the new expense name: ");
                            string newExpenseName = Console.ReadLine();

                            editExpensesCMD.CommandText = "UPDATE Expenses " +
                                                      "SET Name = @ExpenseName WHERE id = @ExpenseID";

                            editExpensesCMD.Parameters.AddWithValue("@ExpenseName", newExpenseName);
                            editExpensesCMD.Parameters.AddWithValue("@ExpenseID", expenseID);
                            editExpensesCMD.Prepare();

                            editExpensesCMD.ExecuteNonQuery();

                            Console.WriteLine("Expense Edited");

                            editExpensesCon.Close();
                        }

                        // if block for editing the price
                        else if (editMenuChoice == "2")
                        {
                            Console.Write("Please enter the new expense price : ");
                            float newExpensePrice = float.Parse(Console.ReadLine());

                            editExpensesCMD.CommandText = "UPDATE Expenses " +
                                                      "SET Amount = @ExpensePrice WHERE id = @ExpenseID";

                            editExpensesCMD.Parameters.AddWithValue("@ExpensePrice", newExpensePrice);
                            editExpensesCMD.Parameters.AddWithValue("@ExpenseID", expenseID);
                            editExpensesCMD.Prepare();

                            editExpensesCMD.ExecuteNonQuery();

                            Console.WriteLine("Expense Edited");

                            editExpensesCon.Close();
                        }

                        // if block for editing the date
                        else if (editMenuChoice == "3")
                        {
                            Console.Write("Please enter the new expense payment date : ");
                            int newExpenseDate = int.Parse(Console.ReadLine());

                            editExpensesCMD.CommandText = "UPDATE Expenses " +
                                                      "SET Date = @ExpenseDate WHERE id = @ExpenseID";

                            editExpensesCMD.Parameters.AddWithValue("@ExpenseDate", newExpenseDate);
                            editExpensesCMD.Parameters.AddWithValue("@ExpenseID", expenseID);
                            editExpensesCMD.Prepare();

                            editExpensesCMD.ExecuteNonQuery();

                            Console.WriteLine("Expense Edited");

                            editExpensesCon.Close();
                        }

                        // if block for editing the entire expense (name / price / date) 
                        else if (editMenuChoice == "4")
                        {
                            Console.Write("Please enter the new expense name: ");
                            string newExpenseName = Console.ReadLine();
                            Console.Write("Please enter the new expense price : ");
                            float newExpensePrice = float.Parse(Console.ReadLine());
                            Console.Write("Please enter the new expense payment date : ");
                            int newExpenseDate = int.Parse(Console.ReadLine());

                            editExpensesCMD.CommandText = "UPDATE Expenses " +
                                                      "SET Name = @ExpenseName, Amount = @ExpensePrice, Date = @ExpenseDate WHERE id = @ExpenseID";


                            editExpensesCMD.Parameters.AddWithValue("@ExpenseName", newExpenseName);
                            editExpensesCMD.Parameters.AddWithValue("@ExpensePrice", newExpensePrice);
                            editExpensesCMD.Parameters.AddWithValue("@ExpenseDate", newExpenseDate);
                            editExpensesCMD.Parameters.AddWithValue("@ExpenseID", expenseID);
                            editExpensesCMD.Prepare();

                            editExpensesCMD.ExecuteNonQuery();

                            Console.WriteLine("Expense Edited");

                            editExpensesCon.Close();
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

                else if (expenseMenuChoice == "4")
                {

                    ShowAllExpenses();

                    // Opens connection with the DB
                    string deleteExpensesCS = "Data Source = ./Database.db"; // Connection string for this menu 
                    var deleteExpensesCon = new SQLiteConnection(deleteExpensesCS);
                    deleteExpensesCon.Open();


                    Console.Write("Please select which expense to delete from the ID's above: ");
                    var expenseID = Console.ReadLine();

                    // Initiliases command and passes placeholder values for new expense record
                    using var addExpensesCMD = new SQLiteCommand(deleteExpensesCon);
                    addExpensesCMD.CommandText = "DELETE FROM Expenses WHERE id = @ExpenseID";

                    // Placeholders - dont understand these yet
                    addExpensesCMD.Parameters.AddWithValue("@ExpenseID", expenseID);
                    addExpensesCMD.Prepare();

                    addExpensesCMD.ExecuteNonQuery();

                    Console.WriteLine("Expense Deleted");

                    deleteExpensesCon.Close();

                }

            }


        }

    }

}