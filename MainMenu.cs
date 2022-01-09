using System;
namespace SQLDemo
{
    public class MainMenu
    {
        static void Main(string[] args)
        {
            bool exitMainMenu = false;

            while (exitMainMenu == false)
            {
                Console.Write("Welcome to the finance and budget tracking app.\n" +
                                  "\n" +
                                  "Please choose from a menu option below\n" +
                                  "1. Monthly Purchases.\n" +
                                  "2. Expenses\n" +
                                  "\n"+
                                  "Menu Choice: ");

                var mainMenuChoice = Console.ReadLine();

                if (mainMenuChoice == "1")
                {
                    Console.WriteLine("Monthly Purchases in here");
                }

                else if (mainMenuChoice == "2")
                {
                    ExpensePage.ExpenseMenu();
                }
                else if (mainMenuChoice == "q")
                {
                    exitMainMenu = true;
                }
                else
                {
                    Console.WriteLine("Invalid Input");
                }
            }
        }
    }
}
