using System;
using System.Data.SQLite;

namespace SQLDemo
{
    public class PurchasesPage
    {

        private class Purchase
        {
            public string purchaseName { get; set; }
            public decimal purchasePrice { get; set; }
            public int purchaseDate { get; set; }
        }

        // MAIN PURCHASE PAGE 
        public static void PurchaseMenu()
        {

            // Initialises an SQLiteConnection for use throughout this page
            string connectionString = "Data Source = ./Database.db";
            var purchasesConnection = new SQLiteConnection(connectionString);
            purchasesConnection.Open();

            // Main Purchase menu loop
            while (true)
            {
                Console.Write("Monthly Purchase Menu.\n" +
                                   "1. View purchases.\n" +
                                   "2. Add a purchase.\n" +
                                   "3. Edit a purchase.\n" +
                                   "4. Delete a purchase.\n" +
                                   "5. Delete monthly purchases table.\n" +
                                   "6. Return to Main Menu.\n" +
                                   "Menu Choice: ");

                string purchaseMenuChoice = Console.ReadLine();

                //line break
                Console.WriteLine("");


                // VIEW PURCHASES BLOCK
                if (purchaseMenuChoice == "1")
                {
                    // false argument because we don't want to display ID column.
                    PurchasePageMethods.ShowPurchases(false);

                    Console.WriteLine($"Total purchases this month equate to £{PurchasePageMethods.GetSumOfPurchases()}");

                    //linebreak
                    Console.WriteLine("");
                }

                // ADD NEW PURCHASE BLOCK
                else if (purchaseMenuChoice == "2")
                {
                    bool exitAddPurchase = false; // this flag changed to true and loop is exited when user inputs 'q'

                    // Main Loop for adding expenses
                    while (exitAddPurchase == false)
                    {
                        Console.WriteLine("\n" +
                                          "Please enter a name and price of your purchase.\n" + 
                                          "Press enter to continue with adding a purchase or alternatively input 'q' to return to menu.");

                        var userInput = Console.ReadLine();
                        if (userInput == "")
                        {
                            // creates a new Expense object from user input
                            Purchase purchase = new Purchase();
                    
                            Console.Write("Name: ");
                            purchase.purchaseName = Console.ReadLine();

                            // takes user input for price and checks validity of input 
                            bool invalidInput = true;
                            while (invalidInput == true)
                            {
                                Console.Write("Price: ");
                                string tempPrice = Console.ReadLine();
                                if (ExpensePageMethods.CheckInvalidInputs(tempPrice, "validNum"))
                                {
                                    purchase.purchasePrice = decimal.Parse(tempPrice);
                                    invalidInput = false;
                                }
                                else
                                {
                                    Console.WriteLine($"{tempPrice} is an invalid input");
                                }
                            }

                            // Initialises purchase date as date of entry
                            purchase.purchaseDate = DateTime.Now.Day;


                            // Initiliases sql command and passes placeholder values for new purchase record
                            using var addPurchaseCommand = new SQLiteCommand(purchasesConnection);
                            addPurchaseCommand.CommandText = "INSERT INTO Purchases(Name, Price, Date) VALUES(@PurchaseName, @PurchasePrice, @PurchaseDate)";

                            addPurchaseCommand.Parameters.AddWithValue("@PurchaseName", purchase.purchaseName);
                            addPurchaseCommand.Parameters.AddWithValue("@PurchasePrice", purchase.purchasePrice);
                            addPurchaseCommand.Parameters.AddWithValue("@PurchaseDate", purchase.purchaseDate);
                            addPurchaseCommand.Prepare();

                            addPurchaseCommand.ExecuteNonQuery();

                            Console.WriteLine("Purchase Added");
                        }
                        // changes flag and quits this if block
                        else if (userInput == "q")
                        {
                            exitAddPurchase = true;
                        }
                        else
                        {
                            Console.WriteLine($"{userInput} is Invalid Input.");
                        }
                    }

                }

                // EDIT Purchases BLOCK
                else if (purchaseMenuChoice == "3")
                {
                    // shows all purchases from purchase table
                    PurchasePageMethods.ShowPurchases(true);

                    // exit flag for this loop, change to true when user inputs 'q'.
                    bool exitEditPurchases = false;
                    while (exitEditPurchases == false)

                    {
                        // Initiliases sqlite command 
                        using var editPurchaseCommand = new SQLiteCommand(purchasesConnection);

                        // This will be the purchase edited by user
                        Console.Write("Please select which purchase to ammend from the ID's above (or input 'q' to quit): ");
                        var purchaseID = Console.ReadLine();

                        //linebreak
                        Console.WriteLine("");

                        // exit loop here
                        if (purchaseID == "q")
                        {
                            exitEditPurchases = true;
                        }
                        // Checks if expenseID is a number or present in the ID column
                        else if (ExpensePageMethods.CheckInvalidInputs(purchaseID, "validNum") == false || ExpensePageMethods.CheckInvalidInputs(purchaseID, "validID") == false)
                        {
                            Console.WriteLine($"{purchaseID} is not a valid ID\n" +
                                              $"");
                        }
                        else
                        {
                            Console.Write("Select which element of the expense you would like to update (or input 'q' to quit):\n" +
                                           "1. Name.\n" +
                                           "2. Price.\n" +
                                           "3. Purchase Date.\n" +
                                           "4. All of the above.\n" +
                                           "Menu Choice: ");
                            var editMenuChoice = Console.ReadLine();

                            // NAMES EDITED IN HERE
                            if (editMenuChoice == "1")
                            {
                                Console.Write("Please enter the new expense name: ");
                                string newPurchaseName = Console.ReadLine();

                                editPurchaseCommand.CommandText = "UPDATE Purchases " +
                                                          "SET Name = @PurchaseName WHERE id = @PurchaseID";

                                editPurchaseCommand.Parameters.AddWithValue("@PurchaseName", newPurchaseName);
                                editPurchaseCommand.Parameters.AddWithValue("@PurchaseID", purchaseID);
                                editPurchaseCommand.Prepare();

                                editPurchaseCommand.ExecuteNonQuery();

                                Console.WriteLine("Purchase Name Edited");
                            }

                            // PRICE EDITED IN HERE
                            else if (editMenuChoice == "2")
                            {
                                PurchasePageMethods.EditPurchasePrice(editPurchaseCommand, purchaseID);
                                Console.WriteLine("Purchase Price Edited");
                            }
                            // DATES EDITED IN HERE
                            else if (editMenuChoice == "3")
                            {
                                PurchasePageMethods.EditPurchaseDate(editPurchaseCommand, purchaseID);
                                Console.WriteLine("Purchase Date Edited");
                            }

                            // NAME / PRICE / DATE EDITED IN HERE 
                            else if (editMenuChoice == "4")
                            {
                                Console.Write("Please enter the new purchase name: ");
                                string newPurchaseName = Console.ReadLine();
                                editPurchaseCommand.CommandText = "UPDATE Purchases " +
                                                         "SET Name = @PurchaseName WHERE id = @PurchaseID";
                                editPurchaseCommand.Parameters.AddWithValue("@PurchaseName", newPurchaseName);
                                editPurchaseCommand.Parameters.AddWithValue("@PurchaseID", purchaseID);
                                editPurchaseCommand.Prepare();
                                editPurchaseCommand.ExecuteNonQuery();

                                PurchasePageMethods.EditPurchasePrice(editPurchaseCommand, purchaseID);

                                PurchasePageMethods.EditPurchaseDate(editPurchaseCommand, purchaseID);

                                Console.WriteLine("Purchase Edited");

                            }
                            else if (editMenuChoice == "q")
                            {
                                exitEditPurchases = true;
                            }
                            else
                            {
                                Console.WriteLine($"{editMenuChoice} is invalid input, please re-enter ID and appropriate menu choice.");
                            }
                        }

                    }
                }

                // DELETE PURCHASE BLOCK
                else if (purchaseMenuChoice == "4")
                {
                    PurchasePageMethods.ShowPurchases(true);

                    bool exitMenu = false;

                    while (exitMenu == false)
                    {
                        Console.Write("Please select which purchase to delete from the ID's above (or input 'q' to quit): ");
                        var purchaseID = Console.ReadLine();
                        if (purchaseID == "q")
                        {
                            exitMenu = true;
                        }
                        else if (ExpensePageMethods.CheckInvalidInputs(purchaseID, "validNum") == false || ExpensePageMethods.CheckInvalidInputs(purchaseID, "validID") == false)
                        {
                            Console.WriteLine($"{purchaseID} is not a valid ID\n" +
                                              $"");
                        }
                        else
                        {
                            // Initiliases command and passes placeholder values for new expense record
                            using var deletePurchaseCommand = new SQLiteCommand(purchasesConnection);
                            deletePurchaseCommand.CommandText = "DELETE FROM Purchases WHERE id = @PurchaseID";

                            deletePurchaseCommand.Parameters.AddWithValue("@PurchaseID", purchaseID);
                            deletePurchaseCommand.Prepare();

                            deletePurchaseCommand.ExecuteNonQuery();

                            Console.WriteLine("Purchase Deleted\n" +
                                              "");
                        }
                    }
                }

                // DELETE ALL PURCHASE BLOCK
                else if (purchaseMenuChoice == "5")
                {
                    using var deleteTableCommand = new SQLiteCommand(purchasesConnection);
                    deleteTableCommand.CommandText = "DROP TABLE Purchases";
                    deleteTableCommand.ExecuteNonQuery();

                    // creates table again or program will throw error when attempting to reopen
                    deleteTableCommand.CommandText = "create table if not exists Purchase (id INTEGER PRIMARY KEY, Name TEXT, Price MONEY, Date INT)";
                    deleteTableCommand.ExecuteNonQuery();

                    Console.WriteLine("Purchase Table has been deleted.\n" +
                                      "");

                }

                else if (purchaseMenuChoice == "6")
                {
                    purchasesConnection.Close();
                    MainMenu.Main();
                }

            }


        }
    }

}
