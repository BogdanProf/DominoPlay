using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DominoPlay.Models
{
    public class Game
    {
        private readonly Random _random = new();
        private List<DominoTile> _stock = new();
        private string _statusMessage = "Игра началась!";
        private bool _isProcessingComputerTurn = false;

        public ObservableCollection<DominoTile> Table { get; } = new();
        public Player Player { get; set; }
        public Player Computer { get; set; }
        public Player CurrentPlayer { get; set; }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
            }
        }

        public bool IsGameOver { get; private set; }

        public Game()
        {
            Player = new Player("Игрок");
            Computer = new Player("Компьютер");
            StartNewGame();
        }

        public void StartNewGame()
        {
            _isProcessingComputerTurn = false;
            Table.Clear();
            Player.Hand.Clear();
            Computer.Hand.Clear();
            IsGameOver = false;

            _stock.Clear();
            for (int i = 0; i <= 6; i++)
                for (int j = i; j <= 6; j++)
                    _stock.Add(new DominoTile(i, j));

            for (int i = 0; i < _stock.Count; i++)
            {
                int swap = _random.Next(_stock.Count);
                (_stock[i], _stock[swap]) = (_stock[swap], _stock[i]);
            }

            for (int i = 0; i < 7; i++)
            {
                Player.Hand.Add(_stock[0]);
                _stock.RemoveAt(0);
                Computer.Hand.Add(_stock[0]);
                _stock.RemoveAt(0);
            }

            var playerDouble = Player.Hand.FirstOrDefault(t => t.IsDouble);
            var computerDouble = Computer.Hand.FirstOrDefault(t => t.IsDouble);

            if (playerDouble != null || computerDouble != null)
            {
                int maxPlayer = playerDouble?.Left ?? -1;
                int maxComputer = computerDouble?.Left ?? -1;

                if (maxPlayer >= maxComputer && maxPlayer != -1)
                {
                    CurrentPlayer = Player;
                    StatusMessage = "Вы ходите первым!";
                }
                else if (maxComputer != -1)
                {
                    CurrentPlayer = Computer;
                    StatusMessage = "Компьютер ходит первым!";
                }
                else
                {
                    CurrentPlayer = Player;
                    StatusMessage = "Вы ходите первым!";
                }
            }
            else
            {
                CurrentPlayer = Player;
                StatusMessage = "Вы ходите первым!";
            }

            if (CurrentPlayer == Computer && !IsGameOver)
            {
                Task.Delay(500).ContinueWith(_ => ComputerTurn());
            }
        }

        public bool CanPlay(DominoTile tile)
        {
            if (Table.Count == 0) return true;

            int leftValue = Table[0].Left;
            int rightValue = Table[^1].Right;

            return tile.Left == leftValue || tile.Right == leftValue ||
                   tile.Left == rightValue || tile.Right == rightValue;
        }

        public bool MakeMove(DominoTile tile, Player player, bool leftSide = false)
        {
            if (IsGameOver) return false;
            if (player != CurrentPlayer) return false;
            if (!CanPlay(tile)) return false;

            if (!player.Hand.Remove(tile)) return false;

            if (Table.Count == 0)
            {
                Table.Add(tile);
            }
            else if (leftSide)
            {
                int leftValue = Table[0].Left;
                if (tile.Right == leftValue)
                {
                    Table.Insert(0, tile);
                }
                else if (tile.Left == leftValue)
                {
                    tile.Flip();
                    Table.Insert(0, tile);
                }
                else
                {
                    player.Hand.Add(tile);
                    return false;
                }
            }
            else
            {
                int rightValue = Table[^1].Right;
                if (tile.Left == rightValue)
                {
                    Table.Add(tile);
                }
                else if (tile.Right == rightValue)
                {
                    tile.Flip();
                    Table.Add(tile);
                }
                else
                {
                    player.Hand.Add(tile);
                    return false;
                }
            }

            if (player.Hand.Count == 0)
            {
                IsGameOver = true;
                StatusMessage = $"{player.Name} победил!";
                return true;
            }

            if (IsGameBlocked())
            {
                IsGameOver = true;
                int playerSum = Player.Hand.Sum(t => t.Left + t.Right);
                int computerSum = Computer.Hand.Sum(t => t.Left + t.Right);

                if (playerSum < computerSum)
                    StatusMessage = "Игра заблокирована! Вы победили!";
                else if (computerSum < playerSum)
                    StatusMessage = "Игра заблокирована! Компьютер победил!";
                else
                    StatusMessage = "Игра заблокирована! Ничья!";
                return true;
            }

            SwitchPlayer();
            return true;
        }

        private bool IsGameBlocked()
        {
            if (Table.Count == 0) return false;

            bool playerCanPlay = Player.Hand.Any(t => CanPlay(t));
            bool computerCanPlay = Computer.Hand.Any(t => CanPlay(t));

            return !playerCanPlay && !computerCanPlay;
        }

        public void SwitchPlayer()
        {
            if (IsGameOver) return;

            CurrentPlayer = CurrentPlayer == Player ? Computer : Player;
            StatusMessage = $"Ход: {CurrentPlayer.Name}";

            if (CurrentPlayer == Computer && !IsGameOver && !_isProcessingComputerTurn)
            {
                _isProcessingComputerTurn = true;
                Task.Delay(600).ContinueWith(_ =>
                {
                    ComputerTurn();
                    _isProcessingComputerTurn = false;
                });
            }
        }

        public void ComputerTurn()
        {
            if (IsGameOver || CurrentPlayer != Computer) return;

            var playableTiles = Computer.Hand.Where(t => CanPlay(t)).ToList();

            if (playableTiles.Any())
            {
                var tile = playableTiles.First();
                bool leftSide = false;

                if (Table.Count > 0)
                {
                    int leftValue = Table[0].Left;
                    int rightValue = Table[^1].Right;

                    if (tile.Left == leftValue || tile.Right == leftValue)
                        leftSide = true;
                    else if (tile.Left == rightValue || tile.Right == rightValue)
                        leftSide = false;
                }

                MakeMove(tile, Computer, leftSide);
                StatusMessage = $"Компьютер поставил {tile}";
            }
            else
            {
                if (_stock.Any())
                {
                    var newTile = _stock[0];
                    _stock.RemoveAt(0);
                    Computer.Hand.Add(newTile);
                    StatusMessage = "Компьютер взял кость из базара";
                    Task.Delay(300).ContinueWith(_ => ComputerTurn());
                }
                else
                {
                    StatusMessage = "Компьютер пропускает ход";
                    SwitchPlayer();
                }
            }
        }

        public void PlayerTakeFromStock()
        {
            if (IsGameOver || CurrentPlayer != Player) return;

            if (_stock.Any())
            {
                var tile = _stock[0];
                _stock.RemoveAt(0);
                Player.Hand.Add(tile);
                StatusMessage = "Вы взяли кость из базара";
            }
            else
            {
                StatusMessage = "Базар пуст! Пропускаете ход.";
                SwitchPlayer();
            }
        }

        public void PlayerPass()
        {
            if (IsGameOver || CurrentPlayer != Player) return;
            StatusMessage = "Вы пропускаете ход";
            SwitchPlayer();
        }
    }
}