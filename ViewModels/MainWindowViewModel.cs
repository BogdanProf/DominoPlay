using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DominoPlay.Models;

namespace DominoPlay.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Game _game;
        private DominoTile? _selectedTile;

        public Game Game => _game;
        public ObservableCollection<DominoTile> PlayerHand => _game.Player.Hand;
        public ObservableCollection<DominoTile> ComputerHand => _game.Computer.Hand;
        public ObservableCollection<DominoTile> TableTiles => _game.Table;

        public DominoTile? SelectedTile
        {
            get => _selectedTile;
            set
            {
                _selectedTile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanPlaceSelectedTile));
            }
        }

        public bool CanPlaceSelectedTile =>
            _selectedTile != null &&
            !_game.IsGameOver &&
            _game.CurrentPlayer == _game.Player &&
            _game.CanPlay(_selectedTile);

        public string StatusMessage
        {
            get => _game.StatusMessage;
            set
            {
                _game.StatusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsGameOver => _game.IsGameOver;
        public string CurrentPlayerName => _game.CurrentPlayer?.Name ?? "";

        public ICommand NewGameCommand { get; }
        public ICommand SelectTileCommand { get; }
        public ICommand AutoPlaceCommand { get; }
        public ICommand TakeFromStockCommand { get; }
        public ICommand PassCommand { get; }

        public MainWindowViewModel()
        {
            _game = new Game();

            NewGameCommand = new RelayCommand(_ => NewGame());
            SelectTileCommand = new RelayCommand(parameter => SelectTile(parameter));
            AutoPlaceCommand = new RelayCommand(_ => AutoPlace(), _ => !IsGameOver && _game.CurrentPlayer == _game.Player);
            TakeFromStockCommand = new RelayCommand(_ => TakeFromStock(), _ => !IsGameOver && _game.CurrentPlayer == _game.Player);
            PassCommand = new RelayCommand(_ => Pass(), _ => !IsGameOver && _game.CurrentPlayer == _game.Player);

            _game.Player.PropertyChanged += (_, _) => OnPropertyChanged(nameof(PlayerHand));
            _game.Computer.PropertyChanged += (_, _) => OnPropertyChanged(nameof(ComputerHand));
            _game.Table.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TableTiles));
        }

        private void NewGame()
        {
            _game.StartNewGame();
            SelectedTile = null;
            OnAllPropertiesChanged();
        }

        private void SelectTile(object? parameter)
        {
            if (parameter is DominoTile tile)
            {
                SelectedTile = SelectedTile == tile ? null : tile;
            }
        }

        private void AutoPlace()
        {
            if (IsGameOver || _game.CurrentPlayer != _game.Player) return;

            var tile = _game.Player.Hand.FirstOrDefault(t => _game.CanPlay(t));
            if (tile == null)
            {
                StatusMessage = "Нет подходящих костей!";
                return;
            }

            bool leftSide = false;
            if (_game.Table.Count > 0)
            {
                int leftValue = _game.Table[0].Left;
                int rightValue = _game.Table[^1].Right;

                if (tile.Left == leftValue || tile.Right == leftValue)
                    leftSide = true;
                else if (tile.Left == rightValue || tile.Right == rightValue)
                    leftSide = false;
                else
                    return;
            }

            if (_game.MakeMove(tile, _game.Player, leftSide))
            {
                OnAllPropertiesChanged();
            }
        }

        private void TakeFromStock()
        {
            _game.PlayerTakeFromStock();
            OnAllPropertiesChanged();
        }

        private void Pass()
        {
            _game.PlayerPass();
            OnAllPropertiesChanged();
        }

        public void OnAllPropertiesChanged()
        {
            OnPropertyChanged(nameof(PlayerHand));
            OnPropertyChanged(nameof(ComputerHand));
            OnPropertyChanged(nameof(TableTiles));
            OnPropertyChanged(nameof(StatusMessage));
            OnPropertyChanged(nameof(IsGameOver));
            OnPropertyChanged(nameof(CurrentPlayerName));
            OnPropertyChanged(nameof(CanPlaceSelectedTile));
            OnPropertyChanged(nameof(SelectedTile));
        }
    }
}