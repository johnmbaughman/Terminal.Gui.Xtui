using System.ComponentModel;
using Xunit;

namespace Terminal.Gui.Xaml.Binding;

public class DataBindingEngineTests
{
    private class TestSource : INotifyPropertyChanged
    {
        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private class TestTarget
    {
        public int Value { get; set; }
    }

    [Fact]
    public void Bind_UpdatesTargetOnSourceChange()
    {
        var source = new TestSource { Value = 1 };
        var target = new TestTarget();
        var engine = new DataBindingEngine();
        engine.Bind(source, target, nameof(TestSource.Value), nameof(TestTarget.Value));
        source.Value = 42;
        Assert.Equal(42, target.Value);
    }

    [Fact]
    public void Unbind_DoesNotUpdateTarget()
    {
        var source = new TestSource { Value = 1 };
        var target = new TestTarget();
        var engine = new DataBindingEngine();
        engine.Bind(source, target, nameof(TestSource.Value), nameof(TestTarget.Value));
        engine.Unbind(source, nameof(TestSource.Value));
        source.Value = 99;
        Assert.NotEqual(99, target.Value);
    }
}
