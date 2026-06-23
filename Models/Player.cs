using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DominoPlay.Models
{
    public class Player : INotifyPropertyChanged
    {
        private ObservableCollection<DominoTile> _hand = new();

        public string Name { get; set; }
        public ObservableCollection<DominoTile> Hand
        {
            get => _hand;
            set
            {
                _hand = value;
                OnPropertyChanged();
            }
        }

        public Player(string name)
        {
            Name = name;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}