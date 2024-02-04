/*
 *  Requires some major refactoring, seriously.
 *  This is an absolute mess with threads hanging.
 *  Also needs arrays to be implemented and admin access.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;

namespace RenovateCMD
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            string CurrentProductInstallID = "";
            string username = "";
            string password = "";
            Console.Title = "RenovateCMD";
            checknetwork();
 
            Console.WriteLine("=================");
            Console.WriteLine("   RenovateCMD   ");
            Console.WriteLine("   Version 0.2   ");
            Console.WriteLine("=================");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Please enter your login details..");
            Console.WriteLine("");
            Console.Write("Username: ");
            username = Console.ReadLine();

            if (username == "")
            {
                Console.WriteLine("You didn't enter a username. Please try again.");
                Environment.Exit(0);
            }

            Console.WriteLine("");
            Console.Write("Password (invisible): ");

            while (true)
            {
                var passkey = System.Console.ReadKey(true);
                if (passkey.Key == ConsoleKey.Enter)
                    break;
                password += passkey.KeyChar;
            }

            if (password == "")
            {
                Console.WriteLine("You didn't enter a password. Please try again.");
                Environment.Exit(0);
            }

            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Please wait whilst we check your login details..");
            Console.WriteLine("");
            Console.WriteLine("Getting 2FA key.. ");

            var values = new Dictionary<string, string>
                {
                    { "apikey", "550039706949" },
                    { "username", username },
                    { "password", password }
                };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://renovatesoftware.com/API/getkey/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            string key = responseString;

            Console.WriteLine("");

            if (responseString.Contains("Wrong details provided"))
            {
                // Incorrect details
                Console.Clear();
                Console.WriteLine("Login details do not match. Please try again.");
                Console.ReadLine();
                await Main(args);
            }
            else if (responseString.Contains("username"))
            {
                // Incorrect details
                Console.Clear();
                Console.WriteLine("Login details do not match. Please try again.");
                Console.ReadLine();
                await Main(args);
            }

            // Here we could check the output of the server's response, for example:
            // We should expect a 2FA key if the login details were correct, otherwise they were likely wrong

            Console.WriteLine("Getting account information..");

            var values2 = new Dictionary<string, string>
            {
                { "apikey" , "550039706949" },
                { "key", key }
            };

            if (key == null)
            {
                Console.Clear();
                Console.WriteLine("You cannot use this application as you do not have 2FA enabled on your account.");
                Console.ReadLine();
                Environment.Exit(0);
            }

            var content2 = new FormUrlEncodedContent(values2);
            var response2 = await client.PostAsync("https://renovatesoftware.com/API/getuserdata/", content2);
            string[] responseString2 = response2.Content.ReadAsStringAsync().Result.ToString().Split("|");
            string first_name = responseString2[0];
            string last_name = responseString2[1];

            var content3 = new FormUrlEncodedContent(values2);
            var response3 = await client.PostAsync("https://renovatesoftware.com/API/getproductsdata/", content3); // has recently been removed
            var responseString3 = response3.Content.ReadAsStringAsync().Result.ToString();

            Console.WriteLine("");
            Console.Clear();
            menu();

            void menu()
            {
                string repsonse = "";
                Console.Beep();
                Console.Clear();
                Console.WriteLine("=- Menu -=");
                Console.WriteLine("Hello " + first_name + ". Please select an option from below:");
                Console.WriteLine("");
                Console.WriteLine("1) List My Products");
                Console.WriteLine("2) List account details.");
                Console.WriteLine("3) Download A Product");
                Console.WriteLine("4) Launch A Product");
                Console.WriteLine("5) Purchase A Product");
                Console.WriteLine("6) Uninstall A Product");
                Console.WriteLine("7) Log Out");
                Console.WriteLine("");
                Console.Write("Selection: ");

                repsonse = Console.ReadLine();

                if (repsonse == "1")
                {
                    // Get user products
                    Console.Clear();
                    Console.WriteLine("-= Products Attached To Your Account =-");
                    var products = String.Concat(responseString3).Replace("|", "\n");
                    Console.WriteLine(products);
                    Console.WriteLine("");
                    Console.Write("OK");
                    Console.ReadLine();
                    menu();
                }
                if (repsonse == "2")
                {
                    // Get user details
                    Console.Clear();
                    Console.WriteLine("");
                    Console.WriteLine("Name: " + first_name + " " + last_name);
                    Console.WriteLine("Username: " + username);
                    Console.WriteLine("");
                    Console.Write("OK");
                    Console.ReadLine();
                    menu();
                }
                if (repsonse == "3")
                {
                    // Download a product
                    Console.Clear();
                    Console.WriteLine("");
                    Console.WriteLine("Please enter the PK (Product Key) of the product to download and install:");
                    Console.WriteLine("");
                    Console.Write("PK: ");
                    string PK = Console.ReadLine();
                    Console.Clear();

                    if (PK == "")
                    {
                        Console.WriteLine("You did not enter a product key. Please try again.");
                        menu();
                    }

                    string key = PK;

                    try
                    {
                        System.Net.WebClient wc = new System.Net.WebClient();
                        string webData = wc.DownloadString("https://renovatesoftware.com/webapp/verify/" + key);
                        Console.Clear();
                        if (webData == "inmydreams")
                        {
                            // Correct
                            Console.WriteLine("Key is correct.");
                            Console.WriteLine(webData);

                            // Create a new WebClient instance.
                            WebClient myWebClient = new WebClient();

                            Console.WriteLine("Downloading the required game files from the server..");
                            // Download the Web resource and save it into the current filesystem folder.
                            CurrentProductInstallID = "inmydreams";
                            myWebClient.DownloadFileAsync(new Uri("https://renovatesoftware.com/media/desktop-games/In%20My%20Dreams.zip"), @"C:\ProgramData\RenovateSoftware\imd-installer.zip");
                            myWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                            myWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                            Console.ReadLine();
                        }
                        else if (webData == "stilllight")
                        {
                            // Correct
                            Console.WriteLine("Key is correct.");
                            Console.WriteLine(webData);

                            // Create a new WebClient instance.
                            WebClient myWebClient = new WebClient();

                            Console.WriteLine("Downloading the required game files from the server..");
                            // Download the Web resource and save it into the current filesystem folder.
                            CurrentProductInstallID = "stilllight";
                            myWebClient.DownloadFileAsync(new Uri("https://renovatesoftware.com/media/desktop-games/Still%20Light.zip"), @"C:\ProgramData\RenovateSoftware\sl-installer.zip");
                            myWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                            myWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                            Console.ReadLine();
                        }
                        else if (webData == "survivalcraft")
                        {
                            Console.WriteLine("Key is correct.");
                            Console.WriteLine(webData);
                            // Create a new WebClient instance.
                            WebClient myWebClient = new WebClient();

                            Console.WriteLine("Downloading the required game files from the server..");
                            // Download the Web resource and save it into the current filesystem folder.
                            CurrentProductInstallID = "survivalcraft";
                            myWebClient.DownloadFileAsync(new Uri("https://renovatesoftware.com/media/desktop-games/SurvivalCraft.zip"), @"C:\ProgramData\RenovateSoftware\svc-installer.zip");
                            myWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                            myWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                            Console.ReadLine();
                        }
                        else if (webData == "shriekingdarkness")
                        {
                            Console.WriteLine("Key is correct.");
                            Console.WriteLine(webData);

                            // Create a new WebClient instance.
                            WebClient myWebClient = new WebClient();

                            Console.WriteLine("Downloading the required game files from the server..");
                            // Download the Web resource and save it into the current filesystem folder.
                            CurrentProductInstallID = "shriekingdarkness";
                            myWebClient.DownloadFileAsync(new Uri("https://renovatesoftware.com/media/desktop-games/ShriekingDarkness.zip"), @"C:\ProgramData\RenovateSoftware\sd-installer.zip");
                            myWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                            myWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                            Console.ReadLine();

                        }
                        else if (webData == "hardwareapp")
                        {
                            Console.WriteLine("Application not supported on this device!");
                            Console.WriteLine(webData);
                            Console.ReadLine();
                        }
                        else if (webData == "renIDE")
                        {
                            Console.WriteLine("Application not supported on this device!");
                            Console.WriteLine(webData);
                            Console.ReadLine();
                        }
                        else if (webData == "tli")
                        {
                            // Correct
                            Console.WriteLine("Key is correct.");
                            Console.WriteLine(webData);

                            // Create a new WebClient instance.
                            WebClient myWebClient = new WebClient();

                            Console.WriteLine("Downloading the required game files from the server..");
                            // Download the Web resource and save it into the current filesystem folder.
                            CurrentProductInstallID = "tli";
                            myWebClient.DownloadFileAsync(new Uri("https://renovatesoftware.com/media/desktop-games/The%20Land%20Inbetween.zip"), @"C:\ProgramData\RenovateSoftware\tli-installer.zip");
                            myWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                            myWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                            Console.ReadLine();
                        }
                        else if (webData == "false")
                        {
                            // Invalid
                            Console.WriteLine("Invalid PK. Please try again. " + key);
                            Console.ReadLine();
                            menu();
                        }
                        else
                        {
                            // Invalid but likely for a different product.
                            Console.WriteLine("Invalid PK. Please try again. " + webData);
                            Console.ReadLine();
                            menu();
                        }
                    }
                    catch (Exception e)
                    {
                        // Invalid
                        Console.WriteLine("Invalid PK. Please try again. " + e.ToString());
                        Console.ReadLine();
                        menu();
                    }

                }

                if (repsonse == "4")
                {
                    Console.Clear();
                    // Launch a product
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\Still Light v4\Still Light.exe"))
                    {
                        Console.WriteLine("stilllight");
                    }
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\In My Dreams\In My Dreams.exe"))
                    {
                        Console.WriteLine("inmydreams");
                    }
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\The Land Inbetween\The Land Inbetween.exe"))
                    {
                        Console.WriteLine("tli");
                    }
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\Survival Craft\SC Console.bat"))
                    {
                        Console.WriteLine("survivalcraft");
                    }
                    Console.WriteLine("");
                    Console.Write("Selection: ");
                    string choice = Console.ReadLine();          

                    if (choice == "stilllight")
                    {
                        // Launch still light
                        System.Diagnostics.Process.Start(@"C:\ProgramData\RenovateSoftware\Still Light v4\Still Light.exe");
                        menu();
                    }

                    else if (choice == "inmydreams")
                    {
                        // Launch In My Dreams
                        System.Diagnostics.Process.Start(@"C:\ProgramData\RenovateSoftware\In My Dreams\In My Dreams.exe");
                        menu();
                    }

                    else if (choice == "tli")
                    {
                        // Launch The Land Inbetween
                        System.Diagnostics.Process.Start(@"C:\ProgramData\RenovateSoftware\The Land Inbetween\The Land Inbetween.exe");
                        menu();
                    }
                    else if (choice == "survivalcraft")
                    {
                        // Launch Survival Craft
                        System.Diagnostics.Process.Start(@"C:\ProgramData\RenovateSoftware\Survival Craft\SC Console.bat");
                        menu();
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection. Please try again.");
                        Console.ReadLine();
                        menu();
                    }

                }

                if (repsonse == "5")
                {
                    try
                    {
                        System.Diagnostics.Process.Start("cmd", "/c start https://renovatesoftware.com/webapp/store/");   
                    }
                    catch { }
                    menu();
                }

                if (repsonse == "6")
                {
                    Console.Clear();
                    Console.WriteLine("Please select the product that you wish to uninstall.");
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\Still Light v4\Still Light.exe"))
                    {
                        Console.WriteLine("stilllight");
                    }
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\In My Dreams\In My Dreams.exe"))
                    {
                        Console.WriteLine("inmydreams");
                    }
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\The Land Inbetween\The Land Inbetween.exe"))
                    {
                        Console.WriteLine("tli");
                    }
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\survivalcraft"))
                    {
                        Console.WriteLine("survivalcraft");
                    }

                    Console.WriteLine("");
                    Console.Write("Choice: ");
                    string choice = Console.ReadLine();

                    if (choice == "stilllight")
                    {
                        // Remove still light      

                        System.IO.DirectoryInfo di = new DirectoryInfo(@"C:\ProgramData\RenovateSoftware\Still Light v4\");

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }

                        Directory.Delete(@"C:\ProgramData\RenovateSoftware\Still Light v4\");

                        menu();
                    }

                    else if (choice == "inmydreams")
                    {
                        // Remove In My Dreams
                        System.IO.DirectoryInfo di = new DirectoryInfo(@"C:\ProgramData\RenovateSoftware\In My Dreams\");

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                        Directory.Delete(@"C:\ProgramData\RenovateSoftware\In My Dreams\");
                        menu();
                    }

                    else if (choice == "tli")
                    {
                        // Remove The Land Inbetween
                        System.IO.DirectoryInfo di = new DirectoryInfo(@"C:\ProgramData\RenovateSoftware\The Land Inbetween\");

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                        Directory.Delete(@"C:\ProgramData\RenovateSoftware\The Land Inbetween\");
                        menu();
                    }
                    else if (choice == "survivalcraft")
                    {
                        // Remove SC
                        System.IO.DirectoryInfo di = new DirectoryInfo(@"C:\ProgramData\RenovateSoftware\Survival Craft\");

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                        Directory.Delete(@"C:\ProgramData\RenovateSoftware\Survival Craft\");
                        menu();
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection. Please try again.");
                        Console.ReadLine();
                        menu();
                    }

                }

                if (repsonse == "7")
                {
                    // Log out
                    Console.Clear();
                    Console.WriteLine("Logged out. Goodbye " + first_name + "!");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }

            void client_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
            {
                Console.WriteLine(CurrentProductInstallID + " is " + e.ProgressPercentage + "% downloaded.");
            }

            void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
            {
                Console.WriteLine("\nFile downloaded. Attempting extraction.. " + CurrentProductInstallID);
                try
                {
                    if (CurrentProductInstallID == "stilllight")
                    {
                        Directory.CreateDirectory(@"C:\ProgramData\RenovateSoftware\Still Light v4\");

                        string zipPath = @"C:\ProgramData\RenovateSoftware\sl-installer.zip";
                        string extractPath = @"C:\ProgramData\RenovateSoftware\";

                        // Extract the file
                        ZipFile.ExtractToDirectory(zipPath, extractPath);

                        Console.WriteLine("Extraction complete. Cleaning up..");
                        File.Delete(zipPath);

                        Console.WriteLine("Clean up finished. Installation complete.");

                        Console.ReadLine();

                        // Game is installed, try to launch it
                        System.Diagnostics.Process.Start(@"C:\ProgramData\RenovateSoftware\Still Light v4\Still Light.exe");
                    }
                    else if (CurrentProductInstallID == "survivalcraft")
                    {
                        Directory.CreateDirectory(@"C:\ProgramData\RenovateSoftware\Survival Craft\");

                        string zipPath = @"C:\ProgramData\RenovateSoftware\svc-installer.zip";
                        string extractPath = @"C:\ProgramData\RenovateSoftware\";

                        // Extract the file
                        ZipFile.ExtractToDirectory(zipPath, extractPath);

                        Console.WriteLine("Extraction complete. Cleaning up..");
                        File.Delete(zipPath);

                        Console.WriteLine("Clean up finished. Installation complete.");

                        Console.ReadLine();

                        // Game is installed, try to launch it
                        System.Diagnostics.Process.Start(@"C:\ProgramData\RenovateSoftware\Survival Craft\SC Console.bat");
                    }
                    else
                    {
                        Console.WriteLine("[ X ] An error occoured when trying to extract the files.");
                    }

                    CurrentProductInstallID = "";
                    menu();
                }
                catch
                {
                    // Do nothing
                }

            }

            async void checknetwork()
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://renovatesoftware.com/webapp/home/");
                request.Timeout = 15000;
                request.Method = "HEAD";
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        // Server is reachable
                    }
                }
                catch (WebException)
                {
                    // Error, cannot connect
                    Console.Clear();
                    Console.WriteLine("ERROR: Could not connect to the server. Please try again later.");
                    Console.ReadLine();
                }
                await Task.Delay(8000);
                checknetwork();
            }
        }
    }
}
