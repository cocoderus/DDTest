using System;
using System.Collections.ObjectModel;
using DynamicData;
using ReactiveUI;

namespace DDTest.ViewModels;

public class ItemViewModel : ViewModelBase
{
    private string _name;
    private readonly ReadOnlyObservableCollection<ItemViewModel> _children;
    public ReadOnlyObservableCollection<ItemViewModel> Children => _children;
    
    public int Id { get; set; }
    public int ParentId { get; set; }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    public string SomeColumn1 { get; set; }
    public string SomeColumn2 { get; set; }
    public string SomeColumn3 { get; set; }
    public string SomeColumn4 { get; set; }
    public string SomeColumn5 { get; set; }
    public string SomeColumn6 { get; set; }
    public string SomeColumn7 { get; set; }
    public string SomeColumn8 { get; set; }
    
    public ItemViewModel()
    {
    }

    public ItemViewModel(Node<ItemViewModel, int> node)
    {
        Id = node.Item.Id;
        ParentId = node.Item.ParentId;
        Name = node.Item.Name;
        SomeColumn1 = node.Item.SomeColumn1;
        SomeColumn2 = node.Item.SomeColumn2;
        SomeColumn3 = node.Item.SomeColumn3;
        SomeColumn4 = node.Item.SomeColumn4;
        SomeColumn5 = node.Item.SomeColumn5;
        SomeColumn6 = node.Item.SomeColumn6;
        SomeColumn7 = node.Item.SomeColumn7;
        SomeColumn8 = node.Item.SomeColumn8;

        node.Children.Connect()
            .Transform(e => new ItemViewModel(e))
            .Bind(out _children)
            .DisposeMany()
            .Subscribe();
    }
    
    public void ChangeName()
    {
        Name = "Changed Name";
    }
}