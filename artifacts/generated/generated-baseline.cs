#nullable enable
using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;

namespace Terminal.Gui.UICatalogXtui
{
    public partial class UICatalogTop : Toplevel
    {
        private MenuBar? menuBar;
        private MenuBarItem? menubaritem0;
        private MenuItem? menuitem0;
        private StatusBar? statusBar;
        private Shortcut? shortcut0;
        private Shortcut? shortcut1;
        private Shortcut? shortcut2;
        private CheckBox? checkbox0;
        private Shortcut? shortcut3;
        private void InitializeComponent()
        {
            this.menuBar = new MenuBar()
            {
                Title = "menuBar",
                Id = "menuBar"
            };
            this.menubaritem0 = new MenuBarItem()
            {
                Title = "_File"
            };
            menubaritem0.PopoverMenu = new PopoverMenu(new Menu());
            this.menuitem0 = new MenuItem()
            {
                Title = "_Quit",
                HelpText = "Quit UI Catalog",
                Key = Application.QuitKey,
                Command = Terminal.Gui.Input.Command.Quit
            };
            menubaritem0.PopoverMenu.Root.Add(this.menuitem0);
            menuBar.Add(this.menubaritem0);
            this.Add(this.menuBar);
            this.statusBar = new StatusBar()
            {
                Visible = ShowStatusBar,
                Id = "statusBar",
                AlignmentModes = Terminal.Gui.ViewBase.AlignmentModes.IgnoreFirstOrLast,
                CanFocus = false,
                Height = Dim.Auto(style: Terminal.Gui.ViewBase.DimAutoStyle.Auto, minimumContentDim: Dim.Func(_ => ShowStatusBar ? 1 : 0), maximumContentDim: Dim.Func(_ => ShowStatusBar ? 1 : 0))
            };
            this.shortcut0 = new Shortcut()
            {
                CanFocus = false,
                Title = "Quit",
                Key = Application.QuitKey
            };
            this.shortcut1 = new Shortcut()
            {
                CanFocus = false,
                Title = "Show/Hide Status Bar",
                Key = Key.F10
            };
            this.shortcut2 = new Shortcut()
            {
                CanFocus = false,
                HelpText = "",
                BindKeyToApplication = true,
                Key = Key.F7
            };
            this.checkbox0 = new CheckBox()
            {
                CanFocus = false,
                Title = "16 color mode",
                CheckedState = Is16ColorMode
            };
            shortcut2.CommandView = this.checkbox0;
            this.shortcut3 = new Shortcut()
            {
                CanFocus = false,
                Title = "Version Info"
            };
            statusBar.Add(this.shortcut0, this.shortcut1, this.shortcut2, this.shortcut3);
            this.Add(this.statusBar);
        }
    }
}