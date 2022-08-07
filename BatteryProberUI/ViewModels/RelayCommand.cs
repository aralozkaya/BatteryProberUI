using System;
using System.Windows.Input;

namespace BatteryProberUI
{
    public class RelayCommand : ICommand
    {
        private readonly Action mAction;

        public RelayCommand(Action mAction)
        {
            this.mAction = mAction;
        }

        public event EventHandler? CanExecuteChanged = (sender, e) => { };

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            mAction();
        }
    }
}
