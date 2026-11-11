namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// One item per run: the first one that is in exactly two containers and fits into one of them.
        /// The exchange can need several rounds (moving the item over makes room for the next foreign
        /// items), so Swap is repeated until it is finished, at most MaxRounds times.
        /// </summary>
        public class StackingSwapJob : StackingStrategyJob
        {
            const int MaxRounds = 10;
            bool _finished;

            protected override int StepCount => MaxRounds;

            public override bool InitStacking()
            {
                _finished = false;
                ItemStacks.CurrentStackingType = ItemStacks.StackingType.Stack;
                ItemStacksList.Sort();
                while (ItemStacksList.Count > 0)
                {
                    var s = ItemStacksList[0];
                    if (s.PrepareSwap()) break;
                    ItemStacksList.Remove(s);
                }
                return ItemStacksList.Count > 0;
            }

            protected override void ProcessingIndex(int index)
            {
                // index is the round: always the same item, the following rounds do nothing once it is finished
                if (!_finished) _finished = ItemStacksList[0].Swap();
            }
        }
    }
}
