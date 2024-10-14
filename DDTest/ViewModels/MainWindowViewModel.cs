using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace DDTest.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly SourceCache<ItemViewModel, int> _sourceCache = new(x => x.Id);
    private readonly ReadOnlyObservableCollection<ItemViewModel> _source;

    private bool _isLoading;
    private string? _searchQuery;
    private int _itemCount = 65000;

    public HierarchicalTreeDataGridSource<ItemViewModel> Source { get; set; }
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }

    public bool IsLoading 
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public string? SearchQuery
    {
        get => _searchQuery;
        set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
    }
    
    public MainWindowViewModel()
    {
        LoadCommand = ReactiveCommand.CreateFromObservable(Load);

        _sourceCache.Connect()
            .Filter(this.WhenAnyValue(a => a.SearchQuery)
                .Throttle(TimeSpan.FromSeconds(1))
                .DistinctUntilChanged()
                .Select(Filter))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(_ => IsLoading = true)
            .ObserveOn(RxApp.TaskpoolScheduler)
            .ObserveOn(RxApp.MainThreadScheduler)
            .TransformToTree(x => x.ParentId)
            .Transform(node => new ItemViewModel(node))
            .DisposeMany()
            .SortAndBind(out _source, SortExpressionComparer<ItemViewModel>.Ascending(a => a.Name))
            .Subscribe(_ => IsLoading = false);
        
        Source = new HierarchicalTreeDataGridSource<ItemViewModel>(_source)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<ItemViewModel>(
                    new TextColumn<ItemViewModel, string>(
                        "Name",
                        x => x.Name,
                        new GridLength(3, GridUnitType.Star)),
                    x => x.Children),
                new TextColumn<ItemViewModel, string>(
                    "SomeColumn1",
                    x => x.SomeColumn1,
                    new GridLength(1, GridUnitType.Star)),
                new TextColumn<ItemViewModel, string>(
                    "SomeColumn2",
                    x => x.SomeColumn2,
                    new GridLength(1, GridUnitType.Star)),
                new TextColumn<ItemViewModel, string>(
                    "SomeColumn3",
                    x => x.SomeColumn3,
                    new GridLength(1, GridUnitType.Star)),
                new TextColumn<ItemViewModel, string>(
                    "SomeColumn4",
                    x => x.SomeColumn4,
                    new GridLength(1, GridUnitType.Star)),
                new TextColumn<ItemViewModel, string>(
                    "SomeColumn5",
                    x => x.SomeColumn5,
                    new GridLength(1, GridUnitType.Star)),
                new TextColumn<ItemViewModel, string>(
                    "SomeColumn6",
                    x => x.SomeColumn6,
                    new GridLength(1, GridUnitType.Star)),
                new TextColumn<ItemViewModel, string>(
                    "SomeColumn7",
                    x => x.SomeColumn7,
                    new GridLength(1, GridUnitType.Star)),
                new TextColumn<ItemViewModel, string>(
                    "SomeColumn8",
                    x => x.SomeColumn8,
                    new GridLength(1, GridUnitType.Star)),
            }
        };

        LoadCommand
            .Execute()
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Subscribe();
        
        LoadCommand.ThrownExceptions.Subscribe(e =>
        {
            Console.WriteLine(e.Message);
            IsLoading = false;
        });
    }

    private static Func<ItemViewModel, bool> Filter(string? search) => record => 
        string.IsNullOrWhiteSpace(search) 
        || record.Id.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) 
        || record.ParentId.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) 
        || record.Name.Contains(search, StringComparison.OrdinalIgnoreCase);

    private IObservable<Unit> Load()
    {
        return Observable.Start(() =>
        {
            IsLoading = true;
            var rnd = new Random();

            var items = Enumerable.Range(1, _itemCount).Select(i =>
            {
                var parent = i % 1000 == 0 ? 0 : rnd.Next(0, i);
                return new ItemViewModel
                {
                    Id = i,
                    ParentId = parent,
                    Name = $"Item {i}",
                    SomeColumn1 = $"SomeValue #{i}",
                    SomeColumn2 = $"SomeValue #{i}",
                    SomeColumn3 = $"SomeValue #{i}",
                    SomeColumn4 = $"SomeValue #{i}",
                    SomeColumn5 = $"SomeValue #{i}",
                    SomeColumn6 = $"SomeValue #{i}",
                    SomeColumn7 = $"SomeValue #{i}",
                    SomeColumn8 = $"SomeValue #{i}",
                };
            });

            using var suspension = _sourceCache.SuspendNotifications();
            _sourceCache.Clear();
            _sourceCache.AddOrUpdate(items);
        });
    }
}