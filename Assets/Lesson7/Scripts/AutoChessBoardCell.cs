using UnityEngine;

namespace AlgoLesson7
{
    public sealed class AutoChessBoardCell : MonoBehaviour
    {
        [SerializeField] private int index;
        [SerializeField] private int team;
        [SerializeField] private int row;
        [SerializeField] private int column;

        public int Index => index;
        public int Team => team;
        public int Row => row;
        public int Column => column;
        public AutoChessBattleUnit Occupant { get; set; }

        public void Initialize(int index, int team, int boardRow, int boardColumn)
        {
            this.index = index;
            this.team = team;
            row = boardRow;
            column = boardColumn;
            name = $"Hex_R{row}_C{column}_Team{team}";
        }

        public void ConfigureCoordinates(int boardRow, int boardColumn)
        {
            row = boardRow;
            column = boardColumn;
        }
    }
}
