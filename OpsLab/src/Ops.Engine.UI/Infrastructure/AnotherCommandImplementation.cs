﻿using System;
using System.Windows.Input;

namespace Ops.Engine.UI.Infrastructure;

/// <summary>
/// <see cref="ICommand"/> 的简单实现。
/// </summary>
public class AnotherCommandImplementation : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool> _canExecute;

    public AnotherCommandImplementation(Action<object?> execute)
        : this(execute, null)
    { }

    public AnotherCommandImplementation(Action<object?> execute, Func<object?, bool>? canExecute)
    {
        if (execute is null)
            throw new ArgumentNullException(nameof(execute));

        _execute = execute;
        _canExecute = canExecute ?? (_ => true);
    }

    public bool CanExecute(object? parameter) => _canExecute(parameter);

    public void Execute(object? parameter) => _execute(parameter);

    public event EventHandler? CanExecuteChanged
    {
        add
        {
            CommandManager.RequerySuggested += value;
        }
        remove
        {
            CommandManager.RequerySuggested -= value;
        }
    }

    public void Refresh() => CommandManager.InvalidateRequerySuggested();
}
