using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace eXoPlugin
{
    public class eXoPlugin : IGameMultiMenuItemPlugin
    {
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




            // Add the language pack extra files as menue entries
            String language = "";

            // do it for each installed language pack
            foreach (var languagefile in new DirectoryInfo(Path.Combine(basePath, "eXo", "util")).GetFiles("*.LANG"))
            {
                //get language from .LANG filename, e.g. GERMAN.LANG --> german
                language = Path.GetFileNameWithoutExtension(languagefile.ToString().ToLower());

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
