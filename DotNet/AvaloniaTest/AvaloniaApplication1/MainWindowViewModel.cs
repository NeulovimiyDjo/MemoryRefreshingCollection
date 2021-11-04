using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace AvaloniaApplication1
{
    internal class MainWindowViewModel : ReactiveObject
    {
        [Reactive] public string Text { get; set; }
        public ReactiveCommand<object, Unit> ChangeCommand { get; }

        public MainWindowViewModel()
        {
            Text = "start text";
            ChangeCommand = ReactiveCommand.CreateFromTask<object>(ChangeAsync);
        }

        private async Task ChangeAsync(object text)
        {
            Text = text as string + "1";
            await Task.Delay(5000);
            Text = text as string + "2";
        }
    }
}