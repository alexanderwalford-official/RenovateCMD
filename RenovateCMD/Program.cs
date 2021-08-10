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

            string username = "";
            string password = null;
            Console.Title = "RenovateCMD";
            checknetwork();
 
            Console.WriteLine("=================");
            Console.WriteLine("   RenovateCMD   ");
            Console.WriteLine("   Version 0.1   ");
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
                    { "username", username },
                    { "password", password }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://renovatesoftware.com/API/2fa_authenticate/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            string key = responseString;
            Console.WriteLine("");

            if (responseString.Contains("Login details do not match"))
            {
                // Incorrect details
                Console.Clear();
                Console.WriteLine("Login details do not match. Please try again.");
                Console.ReadLine();
                Environment.Exit(0);
            }

            // Here we could check the output of the server's response, for example:
            // We should expect a 2FA key if the login details were correct, otherwise they were likely wrong

            Console.WriteLine("Getting account information..");

            var values2 = new Dictionary<string, string>
            {
                { "key", key },
            };

            if (key == null)
            {
                Console.Clear();
                Console.WriteLine("You cannot use this application as you do not have 2FA enabled on your account.");
                Console.ReadLine();
                Environment.Exit(0);
            }

            var content2 = new FormUrlEncodedContent(values2);
            var response2 = await client.PostAsync("https://renovatesoftware.com/API/getuserdata/ ", content2);
            var responseString2 = response2.Content.ReadAsStringAsync().Result.ToString().Split(" | ");
            string first_name = responseString2[0];
            string last_name = responseString2[1];

            var content3 = new FormUrlEncodedContent(values2);
            var response3 = await client.PostAsync("https://renovatesoftware.com/API/getproductsdata/", content3);
            var responseString3 = response3.Content.ReadAsStringAsync().Result.ToString().Split(" | ");

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
                Console.WriteLine("5) Uninstall A Product");
                Console.WriteLine("6) Log Out");
                Console.WriteLine("");
                Console.Write("Selection: ");

                repsonse = Console.ReadLine();

                if (repsonse == "1")
                {
                    // Get user products
                    Console.Clear();
                    Console.WriteLine("");
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

                        if (webData == "In My Dreams")
                        {
                            // Correct
                            Console.WriteLine("Key is correct.");
                            Console.WriteLine(webData);

                            // Create a new WebClient instance.
                            WebClient myWebClient = new WebClient();

                            Console.WriteLine("Downloading the required game files from the server..");
                            // Download the Web resource and save it into the current filesystem folder.

                            myWebClient.DownloadFileAsync(new Uri("https://renovatesoftware.com:140/desktop-games/In%20My%20Dreams.zip"), @"C:\ProgramData\RenovateSoftware\imd-installer.zip");
                            myWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                            myWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted3);
                            Console.ReadLine();
                        }
                        else if (webData == "Still Light Version 4")
                        {
                            // Correct
                            Console.WriteLine("Key is correct.");
                            Console.WriteLine(webData);

                            // Create a new WebClient instance.
                            WebClient myWebClient = new WebClient();

                            Console.WriteLine("Downloading the required game files from the server..");
                            // Download the Web resource and save it into the current filesystem folder.

                            myWebClient.DownloadFileAsync(new Uri("https://renovatesoftware.com:140/desktop-games/Still%20Light%20v4.zip"), @"C:\ProgramData\RenovateSoftware\sl-installer.zip");
                            myWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                            myWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                            Console.ReadLine();
                        }
                        else if (webData == "The Land Inbetween")
                        {
                            // Correct
                            Console.WriteLine("Key is correct.");
                            Console.WriteLine(webData);

                            // Create a new WebClient instance.
                            WebClient myWebClient = new WebClient();

                            Console.WriteLine("Downloading the required game files from the server..");
                            // Download the Web resource and save it into the current filesystem folder.

                            myWebClient.DownloadFileAsync(new Uri("https://renovatesoftware.com:140/desktop-games/The%20Land%20Inbetween.zip"), @"C:\ProgramData\RenovateSoftware\tli-installer.zip");
                            myWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                            myWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted2);
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
                        Console.WriteLine("SL) Still Light V4");
                    }
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\In My Dreams\In My Dreams.exe"))
                    {
                        Console.WriteLine("IMD) In My Dreams");
                    }
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\The Land Inbetween\The Land Inbetween.exe"))
                    {
                        Console.WriteLine("TLI) The Land Inbetween");
                    }
                    Console.WriteLine("");
                    Console.Write("Selection: ");
                    string choice = Console.ReadLine();          

                    if (choice == "SL")
                    {
                        // Launch still light
                        System.Diagnostics.Process.Start(@"C:\ProgramData\RenovateSoftware\Still Light v4\Still Light.exe");
                        menu();
                    }

                    else if (choice == "IMD")
                    {
                        // Launch In My Dreams
                        System.Diagnostics.Process.Start(@"C:\ProgramData\RenovateSoftware\In My Dreams\In My Dreams.exe");
                        menu();
                    }

                    else if (choice == "TLI")
                    {
                        // Launch The Land Inbetween
                        System.Diagnostics.Process.Start(@"C:\ProgramData\RenovateSoftware\The Land Inbetween\The Land Inbetween.exe");
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
                    Console.Clear();
                    Console.WriteLine("Please select the product that you wish to uninstall.");
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\Still Light v4\Still Light.exe"))
                    {
                        Console.WriteLine("SL) Still Light V4");
                    }
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\In My Dreams\In My Dreams.exe"))
                    {
                        Console.WriteLine("IMD) In My Dreams");
                    }
                    if (File.Exists(@"C:\ProgramData\RenovateSoftware\The Land Inbetween\The Land Inbetween.exe"))
                    {
                        Console.WriteLine("TLI) The Land Inbetween");
                    }

                    Console.WriteLine("");
                    Console.Write("Choice: ");
                    string choice = Console.ReadLine();

                    if (choice == "SL")
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

                    else if (choice == "IMD")
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

                    else if (choice == "TLI")
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
                    else
                    {
                        Console.WriteLine("Invalid selection. Please try again.");
                        Console.ReadLine();
                        menu();
                    }

                }

                if (repsonse == "6")
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
                Console.WriteLine(e.ProgressPercentage + "% downloaded.");
            }

            void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
            {
                Console.WriteLine("\nFile downloaded. Attempting extraction..");
                try
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
                    menu();
                }
                catch
                {
                    // Do nothing
                }

            }

            void client_DownloadFileCompleted2(object sender, AsyncCompletedEventArgs e)
            {
                Console.WriteLine("\nFile downloaded. Attempting extraction..");
                try
                {
                    Directory.CreateDirectory(@"C:\ProgramData\RenovateSoftware\The Land Inbetween\");

                    string zipPath = @"C:\ProgramData\RenovateSoftware\tli-installer.zip";
                    string extractPath = @"C:\ProgramData\RenovateSoftware\";

                    // Extract the file
                    ZipFile.ExtractToDirectory(zipPath, extractPath);

                    Console.WriteLine("Extraction complete. Cleaning up..");
                    File.Delete(zipPath);

                    Console.WriteLine("Clean up finished. Installation complete.");

                    Console.ReadLine();

                    // Game is installed, try to launch it
                    System.Diagnostics.Process.Start(@"C:\ProgramData\RenovateSoftware\The Land Inbetween\The Land Inbetween.exe");
                    menu();
                }
                catch
                {
                    // Do nothing
                }

            }

            void client_DownloadFileCompleted3(object sender, AsyncCompletedEventArgs e)
            {
                Console.WriteLine("\nFile downloaded. Attempting extraction..");
                try
                {
                    Directory.CreateDirectory(@"C:\ProgramData\RenovateSoftware\In My Dreams\");

                    string zipPath = @"C:\ProgramData\RenovateSoftware\imd-installer.zip";
                    string extractPath = @"C:\ProgramData\RenovateSoftware\";

                    // Extract the file
                    ZipFile.ExtractToDirectory(zipPath, extractPath);

                    Console.WriteLine("Extraction complete. Cleaning up..");
                    File.Delete(zipPath);

                    Console.WriteLine("Clean up finished. Installation complete.");

                    Console.ReadLine();

                    // Game is installed, try to launch it
                    System.Diagnostics.Process.Start(@"C:\ProgramData\RenovateSoftware\In My Dreams\In My Dreams.exe");
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
