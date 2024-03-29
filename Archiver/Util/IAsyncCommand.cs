﻿using System.Threading.Tasks;
using System.Windows.Input;

namespace Archiver.Util
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();

        bool CanExecute();
    }
}
