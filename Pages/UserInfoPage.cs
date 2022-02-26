using System;
using System.Data.SQLite;

namespace SQLDemo
{
    public class UserInfoPage
    {
        // Updates the payday field of UserInfo table - Independant method as its used in MenuMethods for incrementing payday as part of ResetMonthlyData method.
        public static void UpdatePayDay(int userPayDay, SQLiteCommand userInfoCommand)
        {
            try
            {
                int year = DateTime.Now.Year;
                int month = DateTime.Now.Month;
                DateTime payDay = new DateTime(year, month, userPayDay);

                // if it has passed payday this month, payday set to that day the following month
                if (DateTime.Now.Day > userPayDay)
                {
                    payDay = payDay.AddMonths(1);
                }

                userInfoCommand.CommandText = "UPDATE UserInfo " +
                                      "SET PayDay = @PayDay WHERE id = 1";

                userInfoCommand.Parameters.AddWithValue("@PayDay", payDay);
                userInfoCommand.Prepare();

                userInfoCommand.ExecuteNonQuery();

                Console.WriteLine("Pay Day Updated.\n" +
                                      "");
            }
            // if input date does not exist in the month (e.g. 31st of Feb)
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Invalid date, select menu option 2 and try again.");
            }
            
        }





        public static  void UserInfoMenu()
        {
            Console.Clear();

            string connectionString = "Data Source = ./Database.db";
            var userInfoConnection = new SQLiteConnection(connectionString);
            using var userInfoCommand = new SQLiteCommand(userInfoConnection);
            userInfoConnection.Open();

            while (true)
            {
                // pulls current variables for header below
                (decimal takeHomePay, DateTime nextPayDay) = MenuMethods.GetUserInfo(userInfoConnection);
                if (takeHomePay != 0)
                {
                    Console.WriteLine($"Your current net pay is £{takeHomePay}, and your next pay date is on {nextPayDay.ToShortDateString()}.\n" +
                        $"");
                }
                

                Console.Write("Monthly Purchase Menu.\n" +
                                   "1. Update Monthly Takehome Pay.\n" +
                                   "2. Update Monthly Paydate.\n" +
                                   "3. Return to Main Menu.\n" +
                                   "Menu Choice: ");

                var menuChoice = Console.ReadLine();

                // Takehome pay is input in here
                if (menuChoice == "1")
                {
                    try
                    {
                        Console.WriteLine("Please enter your takehome (net) pay: ");
                        decimal takehomePay = decimal.Parse(Console.ReadLine());



                        userInfoCommand.CommandText = "UPDATE UserInfo " +
                                                  "SET TakeHomePay = @TakeHomePay WHERE id = 1";

                        userInfoCommand.Parameters.AddWithValue("@TakeHomePay", takehomePay);
                        userInfoCommand.Prepare();

                        userInfoCommand.ExecuteNonQuery();

                        Console.WriteLine("Takehome pay input.\n" +
                                          "");
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Invalid input, please select menu option 1 and try again.\n"+
                                          "");
                    }
                    

                }
                // Pay day is input in here 
                else if (menuChoice == "2")
                {
                    // this calculates the users next payday
                    Console.Write("Please enter the day of the month on which you are next paid: ");
                    
                    try
                    {
                        int userPayDay = int.Parse(Console.ReadLine());
                        UpdatePayDay(userPayDay, userInfoCommand);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Invalid input, please select menu option 2 and try again.\n"+
                                          "");
                    }

                    

                    
                }
                // Return to main menu here
                else if (menuChoice == "3")
                {
                    userInfoConnection.Close();
                    MainMenu.Main();
                }
            }
        }
    }
}
