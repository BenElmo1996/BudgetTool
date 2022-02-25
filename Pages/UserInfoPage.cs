using System;
using System.Data.SQLite;

namespace SQLDemo
{
    public class UserInfoPage
    {
        // Updates the payday field of UserInfo table - Independant method as its used in MenuMethods for incrementing payday as part of ResetMonthlyData method.
        public static void UpdatePayDay(int userPayDay, SQLiteCommand userInfoCommand)
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
                    Console.WriteLine($"Your current net pay is £{takeHomePay}, and your next pay date is on {nextPayDay.ToShortDateString()}.");
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
                    Console.Write("Please enter your takehome (net) pay: ");
                    string takehomePay = Console.ReadLine();

                   

                    userInfoCommand.CommandText = "UPDATE UserInfo " +
                                              "SET TakeHomePay = @TakeHomePay WHERE id = 1";

                    userInfoCommand.Parameters.AddWithValue("@TakeHomePay", takehomePay);
                    userInfoCommand.Prepare();

                    userInfoCommand.ExecuteNonQuery();

                    Console.WriteLine("Takehome pay input.\n"+
                                      "");

                }
                // Pay day is input in here 
                else if (menuChoice == "2")
                {
                    // this calculates the users next payday
                    Console.Write("Please enter the day of the month on which you are next paid: ");
                    int userPayDay = int.Parse(Console.ReadLine());

                    UpdatePayDay(userPayDay, userInfoCommand);

                    Console.WriteLine("Pay Day Input.\n"+
                                      "");
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
