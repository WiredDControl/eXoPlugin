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
            int gamefoldersCount = 0;
            string gamePath = "";

            string[] gamefolders = Path.GetDirectoryName(selectedGames[0].ApplicationPath).Split(Path.DirectorySeparatorChar);
            if (Path.GetDirectoryName(selectedGames[0].ApplicationPath).Contains("eXo\\Magazines\\"))
            {
                gamefoldersCount = gamefolders.Length;
                gamePath = Path.Combine(basePath, "eXo", "Magazines", gamefolders[gamefoldersCount - 1], Path.GetFileNameWithoutExtension(selectedGames[0].ApplicationPath));
            }
            else
            {
                gamefoldersCount = gamefolders.Length;
                gamePath = Path.Combine(basePath, "eXo", gamefolders[gamefoldersCount - 3], gamefolders[gamefoldersCount - 2], gamefolders[gamefoldersCount - 1]);

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

            if (!Directory.Exists(Path.Combine(gamePath, "Extras")))
            {
                return Array.Empty<IGameMenuItem>();
            }

            var itemList = new List<IGameMenuItem>();

            // Add the alternate launcher (Pixel Perfect) menu entry
            foreach (var filename in new DirectoryInfo(Path.Combine(gamePath, "Extras")).GetFiles("*.*").Where(x => x.Name != "Alternate Launcher.bat"))
            {
                itemList.Add(new GameMenuItem(selectedGames[0], filename.ToString()));
            }


            if (itemList.Count > 0)
            {
                if (Path.GetDirectoryName(selectedGames[0].ApplicationPath).Contains("eXo\\Magazines\\"))
                {
                    if (File.Exists(Path.Combine(gamePath, "Extras", "Alternate Launcher.bat")))
                    {
                        return new[]
                        {
                        new GameMenuItem(selectedGames[0], Path.Combine(gamePath, "Extras", "Alternate Launcher.bat"), "Pixel Perfect & Shader Options"),
                        new GameMenuItem("magazine articles", itemList, "english-flag.png")
                    };
                    }
                    else
                    {
                        return new[]
                        {
                        new GameMenuItem("magazine articles", itemList, "english-flag.png")
                    };
                    }
                }
                else
                {
                    if (File.Exists(Path.Combine(gamePath, "Extras", "Alternate Launcher.bat")))
                    {
                        return new[]
                        {
                        new GameMenuItem(selectedGames[0], Path.Combine(gamePath, "Extras", "Alternate Launcher.bat"), "Pixel Perfect & Shader Options"),
                        new GameMenuItem("English Extras", itemList, "english-flag.png")
                    };
                    }
                    else
                    {
                        return new[]
                        {
                        new GameMenuItem("English Extras", itemList, "english-flag.png")
                    };
                    }
                }


                    
            }
            else
            {
                if (File.Exists(Path.Combine(gamePath, "Extras", "Alternate Launcher.bat")))
                {
                    return new[]
                    {
                        new GameMenuItem(selectedGames[0], Path.Combine(gamePath, "Extras", "Alternate Launcher.bat"), "Pixel Perfect & Shader Options")
                    };
                }
                else
                {
                    return Array.Empty<IGameMenuItem>();
                }
            }
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
