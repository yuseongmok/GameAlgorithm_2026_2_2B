using System;

namespace AlgoCourse.Lesson5
{
    public enum RuneSearchAction { Choose, Reject, Backtrack, Solved, Failed }

    [Serializable]
    public readonly struct RuneSearchStep
    {
        public RuneSearchAction Action { get; }
        public int RuneId { get; }
        public int Depth { get; }
        public int CurrentSum { get; }
        public string Reason { get; }

        public RuneSearchStep(RuneSearchAction action, int runeId, int depth, int currentSum, string reason)
        {
            Action = action;
            RuneId = runeId;
            Depth = depth;
            CurrentSum = currentSum;
            Reason = reason;
        }
    }
}
