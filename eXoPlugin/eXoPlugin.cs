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

                //check the base pack path and every language pack (maybe change this to a dynamically call when checking the *.LANG files)
                if (Directory.Exists(Path.Combine(launchBoxPath, "eXo", folders[folderCount - 3], folders[folderCount - 1])) 
                    || Directory.Exists(Path.Combine(launchBoxPath, "eXo", "!german", folders[folderCount - 3], folders[folderCount - 1]))
                    || Directory.Exists(Path.Combine(launchBoxPath, "eXo", "!chinese", folders[folderCount - 3], folders[folderCount - 1]))
                    || Directory.Exists(Path.Combine(launchBoxPath, "eXo", "!french", folders[folderCount - 3], folders[folderCount - 1]))
                    || Directory.Exists(Path.Combine(launchBoxPath, "eXo", "!italian", folders[folderCount - 3], folders[folderCount - 1]))
                    || Directory.Exists(Path.Combine(launchBoxPath, "eXo", "!korean", folders[folderCount - 3], folders[folderCount - 1]))
                    || Directory.Exists(Path.Combine(launchBoxPath, "eXo", "!polish", folders[folderCount - 3], folders[folderCount - 1]))
                    || Directory.Exists(Path.Combine(launchBoxPath, "eXo", "!russian", folders[folderCount - 3], folders[folderCount - 1]))
                    || Directory.Exists(Path.Combine(launchBoxPath, "eXo", "!spanish", folders[folderCount - 3], folders[folderCount - 1])))
                {
                    game.Installed = true; // if path exists, set Installed = true, otherwise set Installed = false
                }

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


            //*********** EXTRAS ENTRIES ************//


            // Add the base pack extras files as menu entries
            var BPitemList = new List<IGameMenuItem>();

            if (Directory.Exists(Path.Combine(gamePath, "Extras")))
            {
                foreach (var filename in new DirectoryInfo(Path.Combine(gamePath, "Extras")).GetFiles("*.*").Where(x => x.Name != "Alternate Launcher.bat"))
                {
                    BPitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                }

                if (BPitemList.Count > 0)
                {

                    returnList.Add(new GameMenuItem("English Extras", BPitemList, "english-flag.png"));

                }
            }

            // Add the language pack extra files as menue entries (maybe change this to a dynamically call when checking the *.LANG files)

            // GERMAN LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-German.lang")))
            {

                var GLPitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!german\\"), "Extras")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!german\\"), "Extras")).GetFiles("*.*").Where(x => x.Name != "Alternate Launcher.bat"))
                    {
                        GLPitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (GLPitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("German Extras", GLPitemList, "german-flag.png"));

                    }
                }

            }

            // POLISH LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-Polish.lang")))
            {
                var PLPitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!polish\\"), "Extras")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!polish\\"), "Extras")).GetFiles("*.*").Where(x => x.Name != "Alternate Launcher.bat"))
                    {
                        PLPitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (PLPitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("Polish Extras", PLPitemList, "polish-flag.png"));

                    }
                }
            }

            // FRENCH LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-French.lang")))
            {
                var FLPitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!french\\"), "Extras")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!french\\"), "Extras")).GetFiles("*.*").Where(x => x.Name != "Alternate Launcher.bat"))
                    {
                        FLPitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (FLPitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("French Extras", FLPitemList, "french-flag.png"));

                    }
                }
            }

            // ITALIAN LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-Italian.lang")))
            {
                var ILPitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!italian\\"), "Extras")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!italian\\"), "Extras")).GetFiles("*.*").Where(x => x.Name != "Alternate Launcher.bat"))
                    {
                        ILPitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (ILPitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("Italian Extras", ILPitemList, "italian-flag.png"));

                    }
                }
            }

            // SPANISH LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-Spanish.lang")))
            {
                var SLPitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!spanish\\"), "Extras")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!spanish\\"), "Extras")).GetFiles("*.*").Where(x => x.Name != "Alternate Launcher.bat"))
                    {
                        SLPitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (SLPitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("Spanish Extras", SLPitemList, "spanish-flag.png"));

                    }
                }
            }

            // RUSSIAN LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-Russian.lang")))
            {
                var RLPitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!russian\\"), "Extras")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!russian\\"), "Extras")).GetFiles("*.*").Where(x => x.Name != "Alternate Launcher.bat"))
                    {
                        RLPitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (RLPitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("Russian Extras", RLPitemList, "russian-flag.png"));

                    }
                }
            }

            // KOREAN LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-Korean.lang")))
            {
                var KLPitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!korean\\"), "Extras")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!korean\\"), "Extras")).GetFiles("*.*").Where(x => x.Name != "Alternate Launcher.bat"))
                    {
                        KLPitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (KLPitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("Korean Extras", KLPitemList, "korean-flag.png"));

                    }
                }
            }

            // CHINESE LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-Chinese.lang")))
            {
                var CLPitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!chinese\\"), "Extras")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!chinese\\"), "Extras")).GetFiles("*.*").Where(x => x.Name != "Alternate Launcher.bat"))
                    {
                        CLPitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (CLPitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("Chinese Extras", CLPitemList, "chinese-flag.png"));

                    }
                }
            }


            //*********** MAGAZINE LINK ENTRIES ************//

            // Add the base pack magazine links as menu entries
            var BPmagazineitemList = new List<IGameMenuItem>();

            if (Directory.Exists(Path.Combine(gamePath, "Magazines")))
            {
                foreach (var filename in new DirectoryInfo(Path.Combine(gamePath, "Magazines")).GetFiles("*.*"))
                {
                    BPmagazineitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                }

                if (BPmagazineitemList.Count > 0)
                {

                    returnList.Add(new GameMenuItem("English Magazines", BPmagazineitemList, "english-flag.png"));

                }
            }

            // Add the language pack magazine links as menue entries (maybe change this to a dynamically call when checking the *.LANG files)

            // GERMAN LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-German.lang")))
            {
                var GLPmagazineitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!german\\"), "Magazines")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!german\\"), "Magazines")).GetFiles("*.*"))
                    {
                        GLPmagazineitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (GLPmagazineitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("German Magazines", GLPmagazineitemList, "german-flag.png"));

                    }
                }
            }

            // POLISH LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-Polish.lang")))
            {
                var PLPmagazineitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!polish\\"), "Magazines")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!polish\\"), "Magazines")).GetFiles("*.*"))
                    {
                        PLPmagazineitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (PLPmagazineitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("Polish Magazines", PLPmagazineitemList, "polish-flag.png"));

                    }
                }
            }

            // FRENCH LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-French.lang")))
            {
                var FLPmagazineitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!french\\"), "Magazines")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!french\\"), "Magazines")).GetFiles("*.*"))
                    {
                        FLPmagazineitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (FLPmagazineitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("French Magazines", FLPmagazineitemList, "french-flag.png"));

                    }
                }
            }

            // ITALIAN LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-Italian.lang")))
            {
                var ILPmagazineitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!italian\\"), "Magazines")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!italian\\"), "Magazines")).GetFiles("*.*"))
                    {
                        ILPmagazineitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (ILPmagazineitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("Italian Magazines", ILPmagazineitemList, "italian-flag.png"));

                    }
                }
            }

            // SPANISH LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-Spanish.lang")))
            {
                var SLPmagazineitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!spanish\\"), "Magazines")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!spanish\\"), "Magazines")).GetFiles("*.*"))
                    {
                        SLPmagazineitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (SLPmagazineitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("Spanish Magazines", SLPmagazineitemList, "spanish-flag.png"));

                    }
                }
            }

            // RUSSIAN LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-Russian.lang")))
            {
                var RLPmagazineitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!russian\\"), "Magazines")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!russian\\"), "Magazines")).GetFiles("*.*"))
                    {
                        RLPmagazineitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (RLPmagazineitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("Russian Magazines", RLPmagazineitemList, "russian-flag.png"));

                    }
                }
            }

            // KOREAN LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-Korean.lang")))
            {
                var KLPmagazineitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!korean\\"), "Magazines")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!korean\\"), "Magazines")).GetFiles("*.*"))
                    {
                        KLPmagazineitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (KLPmagazineitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("Korean Magazines", KLPmagazineitemList, "korean-flag.png"));

                    }
                }
            }

            // CHINESE LANGUAGE PACK
            if (File.Exists(Path.Combine(basePath, "eXo", "util", "eXoDOS-Chinese.lang")))
            {
                var CLPmagazineitemList = new List<IGameMenuItem>();

                if (Directory.Exists(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!chinese\\"), "Magazines")))
                {
                    foreach (var filename in new DirectoryInfo(Path.Combine(gamePath.Replace("!dos\\", "!dos\\!chinese\\"), "Magazines")).GetFiles("*.*"))
                    {
                        CLPmagazineitemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
                    }

                    if (CLPmagazineitemList.Count > 0)
                    {

                        returnList.Add(new GameMenuItem("Chinese Magazines", CLPmagazineitemList, "chinese-flag.png"));

                    }
                }
            }

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

            if (addappfilename.Contains("\\Magazines\\")) // icon from file or magazine icon?
            { 
                var basePath = (new FileInfo(AppDomain.CurrentDomain.BaseDirectory)).Directory.Parent.FullName;
                var iconPath = Path.Combine(basePath, "eXo", "util", "magazine.png");
                this.Icon = Image.FromFile(iconPath);
            } 
            else
            {
                this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(addappfilename).ToBitmap();
            }

           
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
            if (File.Exists(iconPath))
            {
                this.Icon = Image.FromFile(iconPath);
            }
            else
            {
                this.Icon = Image.FromFile(Path.Combine(basePath, "eXo", "util", "no-flag.png"));
            }
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
