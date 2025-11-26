namespace Xtui.Mvvm;

public partial class MyWindow
{
    private readonly MainViewModel _viewModel;

    public MyWindow(MainViewModel viewModel)        
    {
        _viewModel = viewModel;
        InitializeComponent();
    }
}