using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media; 
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace TicTacToe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public Game Game { get; set; }

        public MainWindow()
        {
            Game = new Game();

            InitializeComponent();
        }
   

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Game.Finished)
            {
                var b = ((Button)sender).Name;
                int n = Int32.Parse(b[6].ToString());

                if (Game.GameGrid[n] == null)
                {
                    if (Game.MovesCounter % 2 == 0)
                    {
                        Game.GameGrid[n] = "X";
                    }
                    else
                    {
                        Game.GameGrid[n] = "O";
                    }
                }

                Game.MovesCounter++;

                RaisePropertyChanged("Game");

                Game.Finished = Game.Check();
                if (Game.Finished)
                {
                    this.TextBlock.Text = "Game finished";
                }
            }
        }

        private void Button_Click_Again(object sender, RoutedEventArgs e)
        {
            Game = new Game();
            RaisePropertyChanged("Game");
            this.TextBlock.Text = "";
        }
    }
}
