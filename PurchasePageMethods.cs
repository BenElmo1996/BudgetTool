using System;
using System.Data.SQLite;

namespace SQLDemo
{
    public class PurchasePageMethods
    {
        // Displays all e in the expense table with column headers. showIdColumn Arg is true if ID column is displayed, false if not. filter is ALL for all expenses in the table, UPCOMING is future expenses.
        public static void ShowPurchases(bool showIdColumn)
        {
            // Opens Database
            string purchaseMethodConString = "Data Source = ./Database.db"; // Connection string for this menu 
            using var purchaseMethodCon = new SQLiteConnection(purchaseMethodConString);
            purchaseMethodCon.Open();

                // Selects all from Expenses and intialises the reader 
                string purchaseMethodStatement = "SELECT * FROM Purchases";
                using var purchaseMethodCommand = new SQLiteCommand(purchaseMethodStatement, purchaseMethodCon);
                using SQLiteDataReader purchaseMethodReader = purchaseMethodCommand.ExecuteReader();

                // true argument displays ID column, false does not. ID column only required on edit and delete menu options.
                if (showIdColumn == true)
                {
                    // writes headers and then iterates through purchases table writing each entry
                    Console.WriteLine($"{purchaseMethodReader.GetName(0),-3} {purchaseMethodReader.GetName(1),-15} {purchaseMethodReader.GetName(2),-8} {purchaseMethodReader.GetName(3),-8}");
                    while (purchaseMethodReader.Read())
                    {
                    Console.WriteLine($@"{purchaseMethodReader.GetInt32(0),-3} {purchaseMethodReader.GetString(1),-15} £{purchaseMethodReader.GetDecimal(2),-8} {purchaseMethodReader.GetInt32(3),-8}");
                    }
                }
                else
                {
                    // writes headers (minus id) and then iterates through expenses table writing each entry (again minus id)
                    Console.WriteLine($"{purchaseMethodReader.GetName(1),-18} {purchaseMethodReader.GetName(2),-8} {purchaseMethodReader.GetName(3),-8}");
                    while (purchaseMethodReader.Read())
                    {
                        Console.WriteLine($@"{purchaseMethodReader.GetString(1),-18} £{purchaseMethodReader.GetDecimal(2),-8} {purchaseMethodReader.GetInt32(3),-8}");
                    }
                }
            
            // linebreak 
            Console.WriteLine("");

            purchaseMethodCon.Close();
        }



        /// <summary>
        /// Gets the total of the price column from Purchases table.
        /// filter can either be TOTAL or USER CONTRIBUTION depending on which of the above you want to return.
        /// </summary>
        public static decimal GetSumOfPurchases()
        {
            string purchaseMethodConString = "Data Source = ./Database.db"; // Connection string for this menu 
            using var purchaseMethodCon = new SQLiteConnection(purchaseMethodConString);
            purchaseMethodCon.Open();

          
                using var purchaseMethodCommand = new SQLiteCommand(purchaseMethodCon);
                purchaseMethodCommand.CommandText = "SELECT SUM(Price) FROM Purchases";
                purchaseMethodCommand.ExecuteNonQuery();

                // initiliase return value here
                decimal totalExpenses;

                // DBNull check here otherwise program will throw an error
                if (purchaseMethodCommand.ExecuteScalar() is DBNull)
                {
                    totalExpenses = 0;
                }
                else
                {
                    totalExpenses = Convert.ToDecimal(purchaseMethodCommand.ExecuteScalar());
                }

                // close database connection and return total expenses price
                purchaseMethodCon.Close();
                return totalExpenses;
            
        }


        // Method for editing the Price value of a record - created as code is used twice
        public static void EditPurchasePrice(SQLiteCommand editPurchaseCommand, string purchaseID)
        {
            // loop for checking valid inputs, if valid adds value to table
            bool invalidInput = true;
            while (invalidInput == true)
            {
                Console.Write("New purchase price: ");
                string tempPrice = Console.ReadLine();
                if (ExpensePageMethods.CheckInvalidInputs(tempPrice, "validNum")) // function returns true if tempPrice is number
                {
                    decimal newPurchasePrice = decimal.Parse(tempPrice);

                    editPurchaseCommand.CommandText = "UPDATE Purchases " +
                                                                      "SET Price = @PurchasePrice WHERE id = @PurchaseID";

                    editPurchaseCommand.Parameters.AddWithValue("@PurchasePrice", newPurchasePrice);
                    editPurchaseCommand.Parameters.AddWithValue("@PurchaseID", purchaseID);
                    editPurchaseCommand.Prepare();
                    editPurchaseCommand.ExecuteNonQuery();

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
        public static void EditPurchaseDate(SQLiteCommand editPurchaseCommand, string purchaseID)
        {
            // loop for checking valid inputs, if valid adds value to table
            bool invalidInput = true;
            while (invalidInput == true)
            {
                Console.Write("New purchase date: ");
                string tempDate = Console.ReadLine();
                if (ExpensePageMethods.CheckInvalidInputs(tempDate, "validDate")) // function checks that tempDate is a number between 1 - 31
                {
                    int newPurchaseDate = int.Parse(tempDate);

                    editPurchaseCommand.CommandText = "UPDATE Purchases " +
                                      "SET Date = @PurchaseDate WHERE id = @PurchaseID";

                    editPurchaseCommand.Parameters.AddWithValue("@PurchaseDate", newPurchaseDate);
                    editPurchaseCommand.Parameters.AddWithValue("@PurchaseID", purchaseID);
                    editPurchaseCommand.Prepare();
                    editPurchaseCommand.ExecuteNonQuery();

                    // changes flag and exits the loop
                    invalidInput = false;
                }
                else
                {
                    Console.WriteLine($"{tempDate} is an invalid input.");
                }
            }
        }

    }
}
