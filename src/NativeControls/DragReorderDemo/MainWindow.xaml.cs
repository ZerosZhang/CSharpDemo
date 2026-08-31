using System.Windows;
using System.Collections.ObjectModel;

namespace DragReorderDemo;

public partial class MainWindow : Window
{
    public ObservableCollection<string> Items { get; } = ["甲", "乙", "丙", "丁"];
    public ObservableCollection<string> ListA { get; } = ["子", "丑", "寅"];
    public ObservableCollection<string> ListB { get; } = ["卯", "辰", "巳"];

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }
}
