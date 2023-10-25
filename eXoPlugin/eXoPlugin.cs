using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Forms;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;
using Unbroken.LaunchBox.Plugins.RetroAchievements;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace eXoPlugin
{

    public class eXoPluginEvent : ISystemEventsPlugin
    {

        public void OnEventRaised(string eventType)
        {
            
            
            //if (eventType == SystemEventTypes.LaunchBoxStartupCompleted || eventType == SystemEventTypes.BigBoxStartupCompleted)
            //{
                // Load list
                

                // update each game
                //game = GetGameById("42e8839e-58cd-4d85-a628-827ff8a3ea91");
                //System.Windows.MessageBox.Show(game.Series.ToString());
                //game.Series = "TEST";
                //System.Windows.MessageBox.Show("Series wurde gesetzt!");
                //PluginHelper.LaunchBoxMainViewModel?.RefreshData();
            //}

            //if (eventType == SystemEventTypes.LaunchBoxShutdownBeginning)
            //{
            //    var eventbasePath = new FileInfo(AppDomain.CurrentDomain.BaseDirectory).Directory.Parent.FullName;
            //    if (File.Exists(Path.Combine(eventbasePath, "eXo", "Update", "userwantsupdate.yes")))
            //    {
            //        System.Windows.MessageBox.Show("eXoDOS Update starts");
            //        Process.Start(Path.Combine(eventbasePath, "eXo", "Update", "update.bat"));
            //    }
            //}

            // wait for Launchbox to be loaded completely
            if (eventType == SystemEventTypes.LaunchBoxStartupCompleted
                || eventType == SystemEventTypes.BigBoxStartupCompleted)
            {
                // Check for the FAQ-file				
                string checkFile = "";  // we need the root path + /eXo/util
                var claunchBoxPath = (new FileInfo(AppDomain.CurrentDomain.BaseDirectory)).Directory.Parent.FullName;
                checkFile = Path.Combine(claunchBoxPath, "eXo", "util", "FAQread.txt");

                if (File.Exists(checkFile) == false)
                {
                    //show the startup dialog box
                    string textFile = Path.Combine(claunchBoxPath, "eXo", "util", "eXoplugin.txt");
                    string message = "";
                    if (File.Exists(textFile) == false)
                    {
                        message = "Thank you for downloading one of the eXo projects. This message will be displayed every time on start-up until you have opened the FAQ web page (select >Yes<).\r\n\r\nPlease note:\r\nSome games do not have cover art, as they were not actually released in boxes. Registered Launchbox users may switch to using the title screen shots instead.\r\nAlso, all eXo projects can be merged. An instructional video along with step by step instructions can be found on the project's FAQ page.\r\n\r\nWould you like to view it now?";
                    }
                    else
                    {
                        message = System.IO.File.ReadAllText(@textFile);
                    }

                    //new SplashScreen().Show();

                    DialogResult d;
                    d = System.Windows.Forms.MessageBox.Show(
                        new Form { TopMost = true },
                        message.ToString(),
                        "FAQ",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1,
                        0,
                        "https://www.retro-exo.com/FAQ.html",
                        ""
                        );

                    // if the user answers the dialog box with "yes":
                    if (d == DialogResult.Yes)
                    {
                        // write the check file
                        try
                        {
                            using (StreamWriter sw = File.CreateText(checkFile))
                            {
                                sw.WriteLine("Congrats, you've read the FAQ! ");
                            }
                        }
                        catch (Exception Ex)
                        {
                            System.Console.WriteLine(Ex.ToString());
                        }

                        // open the FAQ-Website
                        var psi = new ProcessStartInfo
                        {
                            FileName = "https://www.retro-exo.com/FAQ.html",
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                    }
                }



            }
        }




    }


    public class eXoPlugin : IGameMultiMenuItemPlugin, IGameLaunchingPlugin, IGameConfiguringPlugin
    {

        public IGame GameLaunched { get; set; }     // property for the game infos

        private void UpdateInstallFlag(IGame game)
        {
            string checkPath = ""; // initialise the variable

            var launchBoxPath = (new FileInfo(AppDomain.CurrentDomain.BaseDirectory)).Directory.Parent.FullName;
            string completeGamePath = Path.GetDirectoryName(game.ApplicationPath).ToLower(new CultureInfo("en-US", false));

            if (completeGamePath.Contains("exo"))
            {
                string[] folders = completeGamePath.Split(Path.DirectorySeparatorChar);                             // split into folder names
                int folderCount = folders.Length;																	// get the number of folders
                checkPath = Path.Combine(launchBoxPath, "eXo", folders[folderCount - 3], folders[folderCount - 1]);	 	// add gamefolder to checkPath
                game.Installed = Directory.Exists(checkPath);           											// if path exists, set Installed = true, otherwise set Installed = false
                PluginHelper.LaunchBoxMainViewModel?.RefreshData();     											// refresh the LB view
            }

        }

        public void OnAfterGameConfigurationOpens(IGame game)
        {
            //throw new NotImplementedException();
        }

        public void OnAfterGameLaunched(IGame game, IAdditionalApplication app, IEmulator emulator)
        {
            GameLaunched = game;
            
            ////if user starts the eXoDOS entry in LB:
            //if (game.Title == "eXoDOS")
            //{
            //    if (!File.Exists(Path.Combine(new FileInfo(AppDomain.CurrentDomain.BaseDirectory).Directory.Parent.FullName, "eXo", "Update", "userwantsupdate.yes")))
            //    {
            //        // write the check file
            //        try
            //        {
            //            using (StreamWriter sw = File.CreateText(Path.Combine(new FileInfo(AppDomain.CurrentDomain.BaseDirectory).Directory.Parent.FullName, "eXo", "Update", "userwantsupdate.yes")))
            //            {
            //                sw.WriteLine("Update will be launched on next LaunchBox shutdown! ");
            //            }
            //        }
            //        catch (Exception Ex)
            //        {
            //            Console.WriteLine(Ex.ToString());
            //        }
            //    }
            //}
        }

        public void OnBeforeGameConfigurationOpens(IGame game)
        {
            //throw new NotImplementedException();
        }

        public void OnBeforeGameLaunching(IGame game, IAdditionalApplication app, IEmulator emulator)
        {
            //throw new NotImplementedException();
        }

        public void OnGameConfigurationExited(IGame game)
        {
            UpdateInstallFlag(game);
        }

        public void OnGameExited()
        {

            var game = GameLaunched;    // get the property
            UpdateInstallFlag(game);    // do the magic
            GameLaunched = null;        // reset property

            // reload the XML files if the eXoDOS Updater was called
            if (game.Title == "eXoDOS")
            {

                //PluginHelper.DataManager.ForceReload();
                //PluginHelper.DataManager.ReloadIfNeeded();
                // both are not working - they don't reload the "new" replaced XML file, but they save the current RAM state back to the XML (so they get overwritten)

            }


        }

        public IEnumerable<IGameMenuItem> GetMenuItems(params IGame[] selectedGames)
        {

            var basePath = new FileInfo(AppDomain.CurrentDomain.BaseDirectory).Directory.Parent.FullName;   // Launchbox path
            string gamePath = "";


            // make sure it's a game from an eXo-project
            if (!Path.GetDirectoryName(selectedGames[0].ApplicationPath).Contains("eXo\\eXo"))
            {
                return Array.Empty<IGameMenuItem>();
            }
            else
            // get the path to the games folder
            {
                string gamefolder = Path.GetDirectoryName(selectedGames[0].ApplicationPath);
                gamePath = Path.Combine(basePath, gamefolder);
            }

            if (PluginHelper.StateManager.IsBigBox)
            {
                return Array.Empty<IGameMenuItem>();
            }

            if (selectedGames == null)
            {
                return Array.Empty<IGameMenuItem>();
            }

            if (selectedGames.Length > 1)
            {
                return Array.Empty<IGameMenuItem>();
            }

            // If there's no .../Extras folder, skip all and return an empty array
            if (!Directory.Exists(Path.Combine(gamePath, "Extras")))
            {
                return Array.Empty<IGameMenuItem>();
            }




            var returnList = new List<GameMenuItem>();

            //if there is an alternate launcher for the game, add it
            if (File.Exists(Path.Combine(gamePath, "Extras", "Alternate Launcher.bat")))
            {
                returnList.Add(new GameMenuItem(selectedGames[0], Path.Combine(gamePath, "Extras", "Alternate Launcher.bat"), "Pixel Perfect & Shader Options"));
            }




            // Add the base pack extras files as menu entries
            var enitemList = new List<IGameMenuItem>();
            foreach (var filename in new DirectoryInfo(Path.Combine(gamePath, "Extras")).GetFiles("*.*").Where(x => x.Name != "Alternate Launcher.bat"))
            {
                enitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
            }

            if (enitemList.Count > 0)
            {

                returnList.Add(new GameMenuItem("English Extras", enitemList, "english-flag.png"));
                
            }

            /*
            // Add the language pack extra files as menue entries
            String language = "";
            var lpitemList = new List<IGameMenuItem>();

            // do it for each installed language pack
            foreach (var languagefile in new DirectoryInfo(Path.Combine(basePath, "eXo", "util")).GetFiles("*.LANG"))
            {
                //get language from .LANG filename, e.g. GERMAN.LANG --> german
                language = Path.GetFileNameWithoutExtension(languagefile.ToString().ToLower());

                // Add the language pack extras files as menu entries
                lpitemList.Clear();
                var lpgamePath = gamePath.Replace("!dos\\", "!dos\\!" + language + "\\");

                foreach (var filename in new DirectoryInfo(Path.Combine(lpgamePath, "Extras")).GetFiles("*.*").Where(x => x.Name != "Alternate Launcher.bat"))
                {
                    lpitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                }

                if (lpitemList.Count > 0)
                {
                    
                    returnList.Add(new GameMenuItem(language[0].ToString().ToUpper() + language.Substring(1) + " Extras", lpitemList, language.ToString() + "-flag.png"));

                }

            }*/

            return returnList;

        }
    }


    public class GameMenuItem : IGameMenuItem
    {
        private readonly IGame game;

        public string Caption { get; }

        public string flag { get; }

        public string addappfilename { get; }

        public IEnumerable<IGameMenuItem> Children { get; }

        public bool Enabled { get; }

        public Image Icon { get; }

        public GameMenuItem(IGame game, string addappfilename)
        {
            this.game = game;
            this.addappfilename = addappfilename;
            this.Caption = Path.GetFileNameWithoutExtension(addappfilename).ToString();
            this.Enabled = true;
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(addappfilename).ToBitmap();
        }

        public GameMenuItem(IGame game, string addappfilename, string Caption)
        {
            this.game = game;
            this.addappfilename = addappfilename;
            this.Caption = Caption;
            this.Enabled = true;
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(addappfilename).ToBitmap();
        }

        public GameMenuItem(string caption, IEnumerable<IGameMenuItem> children, string flag)
        {
            var basePath = (new FileInfo(AppDomain.CurrentDomain.BaseDirectory)).Directory.Parent.FullName;
            var iconPath = Path.Combine(basePath, "eXo", "util", flag);
            this.Caption = caption;
            this.Children = children;
            this.Enabled = true;
            this.flag = flag;
            this.Icon = Image.FromFile(iconPath);
        }

        public void OnSelect(params IGame[] games)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @""+this.addappfilename+"";
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Path.GetDirectoryName(addappfilename).ToString();
            Process.Start(startInfo);
        }
    }
}
