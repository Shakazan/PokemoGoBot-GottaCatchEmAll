﻿#region

using System;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Logic.Logging;
using System.Windows.Forms;
using PokemonGo.RocketAPI.Window;

#endregion


namespace PokemonGo.RocketAPI.Console
{
    internal class Program
    {
        static StatusWindow statusForm;
        [STAThread]
        private static void Main()
        {
            
            var culture = CultureInfo.CreateSpecificCulture("en-US");

            CultureInfo.DefaultThreadCurrentCulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            AppDomain.CurrentDomain.UnhandledException
                += delegate (object sender, UnhandledExceptionEventArgs eargs)
                {
                    Exception exception = (Exception)eargs.ExceptionObject;
                    System.Console.WriteLine(@"Unhandled exception: " + exception);
                    Environment.Exit(1);
                };

            statusForm = new StatusWindow();
            Application.EnableVisualStyles();
           
            ServicePointManager.ServerCertificateValidationCallback = Validator;
            Logger.SetLogger(statusForm);
            Task.Run(() =>
            {
                try
                {
                    new Logic.Logic(new Settings(), statusForm).Execute().Wait();
                }
                catch (PtcOfflineException)
                {
                    Logger.Write("PTC Servers are probably down OR your credentials are wrong. Try google", LogLevel.Error);
                    Logger.Write("Trying again in 60 seconds...");
                    Thread.Sleep(60000);
                    new Logic.Logic(new Settings(), statusForm).Execute().Wait();
                }
                catch (AccountNotVerifiedException)
                {
                    Logger.Write("Account not verified. - Exiting");
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Logger.Write($"Unhandled exception: {ex}", LogLevel.Error);
                    new Logic.Logic(new Settings(), statusForm).Execute().Wait();
                }
            });

            //statusForm.Show();

            //Do some stuff...
            bool Exit = false;
            while (!Exit)
            {
                Application.DoEvents(); //Now if you call "form.Show()" your form won´t be frozen
                                        //Do your stuff
            }
            System.Console.ReadLine();
        }

        public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
    }
}