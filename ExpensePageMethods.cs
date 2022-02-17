using System;
using System.Data.SQLite;
namespace SQLDemo
{
    /// <summary>
    /// Holds all the methods used in the Expense Page class 
    /// </summary>
    public class ExpensePageMethods
    {

        /// <summary>
        ///  Displays all expenses in the expense table with column headers. showIdColumn Arg is true if ID column is displayed, false if not. filter is ALL for all expenses in the table, UPCOMING is future expenses.
        /// </summary>
        public static void ShowExpenses(bool showIdColumn, string filter)
        {
            // Opens Database
            string expenseMethodConString = "Data Source = ./Database.db"; // Connection string for this menu 
            using var expenseMethodCon = new SQLiteConnection(expenseMethodConString);
            expenseMethodCon.Open();

            if (filter == "ALL")
            {
                // Selects all from Expenses and intialises the reader 
                string expenseMethodStatement = "SELECT * FROM Expenses";
                using var expenseMethodCommand = new SQLiteCommand(expenseMethodStatement, expenseMethodCon);
                using SQLiteDataReader expenseMethodReader = expenseMethodCommand.ExecuteReader();

                // true argument displays ID column, false does not. ID column only required on edit and delete menu options.
                if (showIdColumn == true)
                {
                    // writes headers and then iterates through expenses table writing each entry
                    Console.WriteLine($"{expenseMethodReader.GetName(0),-3} {expenseMethodReader.GetName(1),-15} {expenseMethodReader.GetName(2),-8} {expenseMethodReader.GetName(3),-8} {expenseMethodReader.GetName(4),-3}");
                    while (expenseMethodReader.Read())
                    {
                        Console.WriteLine($@"{expenseMethodReader.GetInt32(0),-3} {expenseMethodReader.GetString(1),-15} £{expenseMethodReader.GetDecimal(2),-8} {expenseMethodReader.GetInt32(3),-8} {expenseMethodReader.GetInt32(4),-3}");
                    }
                }
                else
                {
                    // writes headers (minus id) and then iterates through expenses table writing each entry (again minus id)
                    Console.WriteLine($"{expenseMethodReader.GetName(1),-18} {expenseMethodReader.GetName(2),-8} {expenseMethodReader.GetName(3),-8} {expenseMethodReader.GetName(4),-3}");
                    while (expenseMethodReader.Read())
                    {
                        Console.WriteLine($@"{expenseMethodReader.GetString(1),-18} £{expenseMethodReader.GetDecimal(2),-8} {expenseMethodReader.GetInt32(3),-8} {expenseMethodReader.GetInt32(4),-3}");
                    }
                }
            }
            // Displays all the upcoming expenses, i.e. the ones on or after todays date
            else if (filter == "UPCOMING")
            {
                string expenseMethodStatement = "SELECT * FROM Expenses order by Date";
                using var expenseMethodCommand = new SQLiteCommand(expenseMethodStatement, expenseMethodCon);
                using SQLiteDataReader expenseMethodReader = expenseMethodCommand.ExecuteReader();

                // Displays column headers, no ID column
                Console.WriteLine($"{expenseMethodReader.GetName(1),-18} {expenseMethodReader.GetName(2),-8} {expenseMethodReader.GetName(3),-8} {expenseMethodReader.GetName(4),-3}");

                // Todays date for determining which payments are still to be made
                int dayOfMonth = DateTime.Today.Day;

                // Iterates through each record of expenses table
                while (expenseMethodReader.Read())
                {
                    // if payment is due after todays date, write the record
                    if (dayOfMonth <= expenseMethodReader.GetInt32(3))
                    {
                        Console.WriteLine($@"{expenseMethodReader.GetString(1),-18} £{expenseMethodReader.GetDecimal(2),-8} {expenseMethodReader.GetInt32(3),-8} {expenseMethodReader.GetInt32(4),-3}");
                    }
                }
            }
            
            // linebreak 
            Console.WriteLine("");

            expenseMethodCon.Close();
        }




        /// <summary>
        /// Gets the total of the price column from Expenses table as well as the total user contribution based on the contribution field.
        /// filter can either be TOTAL or USER CONTRIBUTION depending on which of the above you want to return.
        /// </summary>
        public static decimal GetSumOfExpenses(string filter)
        {
            string expenseMethodConString = "Data Source = ./Database.db"; // Connection string for this menu 
            using var expenseMethodCon = new SQLiteConnection(expenseMethodConString);
            expenseMethodCon.Open();

            if (filter == "TOTAL") {
                using var expenseMethodCommand = new SQLiteCommand(expenseMethodCon);
                expenseMethodCommand.CommandText = "SELECT SUM(Price) FROM Expenses";
                expenseMethodCommand.ExecuteNonQuery();

                // initiliase return value here
                decimal totalExpenses;

                // DBNull check here otherwise program will throw an error
                if (expenseMethodCommand.ExecuteScalar() is DBNull)
                {
                    totalExpenses = 0;
                }
                else
                {
                    totalExpenses = Convert.ToDecimal(expenseMethodCommand.ExecuteScalar());
                }

                // close database connection and return total expenses price
                expenseMethodCon.Close();
                return totalExpenses;
            }

            else if (filter == "USER CONTRIBUTION")
            {

                // Selects all from Expenses and intialises the reader 
                string expenseMethodStatement = "SELECT * FROM Expenses";
                using var expenseMethodCommand = new SQLiteCommand(expenseMethodStatement, expenseMethodCon);
                using SQLiteDataReader expenseMethodReader = expenseMethodCommand.ExecuteReader();

                decimal totalUserContribution = 0;
                while (expenseMethodReader.Read())
                {
                    // if contributor is greater than 1, divide by number of contributors before adding to total
                    if (expenseMethodReader.GetInt32(4) > 1)
                    {
                        totalUserContribution += (expenseMethodReader.GetDecimal(2) / expenseMethodReader.GetInt32(4));
                    }
                    else
                    {
                        totalUserContribution += expenseMethodReader.GetDecimal(2);
                    }
                }
                expenseMethodCon.Close();

                // rounds to 2 decimal places and returns the value
                totalUserContribution = Math.Round(totalUserContribution, 2);
                return totalUserContribution;
            }
            else
            {
                return 0;
            }
        }




        // Calculates the total price of upcoming expenses, as well as the users contribution to these upcoming expenses based on the contributors table field.
        // filter can either be TOTAL or USER CONTRIBUTION depending on which of the above you want to return.
        public static decimal GetSumUpcomingExpenses(string filter)
        {

            string expenseMethodConString = "Data Source = ./Database.db"; // Connection string for this menu 
            using var expenseMethodCon = new SQLiteConnection(expenseMethodConString);
            expenseMethodCon.Open();

            // Initialises command to select all from Expenses table. 
            using var expenseMethodCommand = new SQLiteCommand(expenseMethodCon);
            string viewUpcomingExpensesStatement = "SELECT * FROM Expenses";
            using var viewUpcomingExpensesCommand = new SQLiteCommand(viewUpcomingExpensesStatement, expenseMethodCon);
            using SQLiteDataReader viewExpensesReader = viewUpcomingExpensesCommand.ExecuteReader();

            // Todays date for determining which payments are still to be made
            int dayOfMonth = DateTime.Today.Day;

            if (filter == "TOTAL")
            {
                // currentTotal used below to hold the total of all upcoming expenses
                decimal upcomingTotal = 0;

                // loop iterates through each record in the expenses table
                while (viewExpensesReader.Read())
                {
                    // if the payment is scheduled for after today's date, add price to currentTotal
                    if (dayOfMonth <= viewExpensesReader.GetInt32(3))
                    {
                        upcomingTotal += viewExpensesReader.GetDecimal(2);
                    }
                }
                return upcomingTotal;
            }

            // Calculates the users contribution to the upcoming expenses
            if (filter == "USER CONTRIBUTION")
            {
                // currentTotal used below to hold the total of all upcoming expenses
                decimal upcomingContribution = 0;

                // loop iterates through each record in the expenses table
                while (viewExpensesReader.Read())
                {
                    // if the payment is scheduled for after today's date, divide payment by contributors and add to upcomingContribution
                    if (dayOfMonth <= viewExpensesReader.GetInt32(3))
                    {
                        upcomingContribution += viewExpensesReader.GetDecimal(2) / viewExpensesReader.GetInt32(4);
                    }
                }
                // rounds to 2 decimal places and returns value 
                upcomingContribution = Math.Round(upcomingContribution, 2);
                return upcomingContribution;     
            }

            else
            {
                return 0;
            }

        }    

        // Used in the add and edit expenses menu options, validPrice argument checks against the
        // price input parameters whilst validDate checks against the specified date parameters. 
        public static bool CheckInvalidInputs(string userInput, string checkType)
        {
            // valid price is a decimal
            if (checkType == "validNum")
            {
                if (decimal.TryParse(userInput, out _))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            // a valid date is an integer between 1 and 31
            else if (checkType == "validDate")
            {
                // tries to parse the user input as an int, temp is just used as a throwaway variable here.
                int temp;
                if (int.TryParse(userInput, out temp) && temp <= 31 && temp != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (checkType == "validID")
            {
                string expenseMethodConString = "Data Source = ./Database.db"; // Connection string for this menu 
                using var expenseMethodCon = new SQLiteConnection(expenseMethodConString);
                expenseMethodCon.Open();

                // Initialises command to select all from Expenses table in Date order (low to high) 
                using var expenseMethodCommand = new SQLiteCommand(expenseMethodCon);
                string viewUpcomingExpensesStatement = "SELECT * FROM Expenses";
                using var viewUpcomingExpensesCommand = new SQLiteCommand(viewUpcomingExpensesStatement, expenseMethodCon);
                using SQLiteDataReader viewExpensesReader = viewUpcomingExpensesCommand.ExecuteReader();

                bool validID = false;
                while (viewExpensesReader.Read())
                {
                    // if the payment is scheduled for after today's date, divide payment by contributors and add to upcomingContribution
                    if (int.Parse(userInput) == viewExpensesReader.GetInt32(0))
                    {
                        validID = true;
                        break;
                    }
                    else
                    {
                        validID = false;
                    }
                }
                return validID;
            }

            else
            {
                return false;
            }

        }



        // Method for editing the Price value of a record - created as code is used twice
       public static void EditExpensePrice(SQLiteCommand editExpenseCommand, string expenseID)
        {
            // loop for checking valid inputs, if valid adds value to table
            bool invalidInput = true;
            while (invalidInput == true)
            {
                Console.Write("New expense price: ");
                string tempPrice = Console.ReadLine();
                if (CheckInvalidInputs(tempPrice, "validPrice")) // function returns true if tempPrice is number
                {
                    decimal newExpensePrice = decimal.Parse(tempPrice);

                    editExpenseCommand.CommandText = "UPDATE Expenses " +
                                                                      "SET Price = @ExpensePrice WHERE id = @ExpenseID";

                    editExpenseCommand.Parameters.AddWithValue("@ExpensePrice", newExpensePrice);
                    editExpenseCommand.Parameters.AddWithValue("@ExpenseID", expenseID);
                    editExpenseCommand.Prepare();
                    editExpenseCommand.ExecuteNonQuery();

                    // changes flag and exits loop 
                    invalidInput = false;
                }
                else
                {
                    Console.WriteLine($"{tempPrice} is an invalid input.");
                }
            }
        }

        // Method for editing the Date value of a record, as with the function above this was created as code is used twice
        public static void EditExpenseDate(SQLiteCommand editExpenseCommand, string expenseID)
        {
            // loop for checking valid inputs, if valid adds value to table
            bool invalidInput = true;
            while (invalidInput == true)
            {
                Console.Write("New payment date: ");
                string tempDate = Console.ReadLine();
                if (CheckInvalidInputs(tempDate, "validDate")) // function checks that tempDate is a number between 1 - 31
                {
                    int newExpenseDate = int.Parse(tempDate);

                    editExpenseCommand.CommandText = "UPDATE Expenses " +
                                      "SET Date = @ExpenseDate WHERE id = @ExpenseID";

                    editExpenseCommand.Parameters.AddWithValue("@ExpenseDate", newExpenseDate);
                    editExpenseCommand.Parameters.AddWithValue("@ExpenseID", expenseID);
                    editExpenseCommand.Prepare();
                    editExpenseCommand.ExecuteNonQuery();

                    // changes flag and exits the loop
                    invalidInput = false;
                }
                else
                {
                    Console.WriteLine($"{tempDate} is an invalid input.");
                }
            }
        }

        // Method for editing the Contributors value of a record, as with the functions above this was created as code is used twice
        public static void EditExpenseContributors(SQLiteCommand editExpenseCommand, string expenseID)
        {
            // loop for checking valid inputs, if valid adds value to table
            bool invalidInput = true;
            while (invalidInput == true)
            {
                Console.Write("Expense Contributors: ");
                string tempContributors = Console.ReadLine();
                if (CheckInvalidInputs(tempContributors, "validPrice")) // function checks that temp is a number 
                {
                    int newExpenseContributors = int.Parse(tempContributors);

                    editExpenseCommand.CommandText = "UPDATE Expenses " +
                                      "SET Contributors = @ExpenseContributors WHERE id = @ExpenseID";

                    editExpenseCommand.Parameters.AddWithValue("@ExpenseContributors", newExpenseContributors);
                    editExpenseCommand.Parameters.AddWithValue("@ExpenseID", expenseID);
                    editExpenseCommand.Prepare();
                    editExpenseCommand.ExecuteNonQuery();

                    // changes flag and exits the loop
                    invalidInput = false;
                }
                else
                {
                    Console.WriteLine($"{tempContributors} is an invalid input.");
                }
            }
        }


    }
}
