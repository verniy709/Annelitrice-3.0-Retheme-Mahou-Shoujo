namespace Annelitrice.Retheme.MahouShoujo
{
    // Tick cues stay with the animation; emitted one-shots retain their natural real-time tails.
    public sealed class MiracleAudioTiming
    {
        public const int BloomTick=MiracleGeometry.ArrivalTick;
        public const int BubblesTick=44;
        private bool bloomPlayed,bubblesPlayed;
        public int Advance(int ageTicks)
        {
            int cues=0;
            if(!bloomPlayed&&ageTicks>=BloomTick) { bloomPlayed=true;cues|=1; }
            if(!bubblesPlayed&&ageTicks>=BubblesTick) { bubblesPlayed=true;cues|=2; }
            return cues;
        }
        public static int CastSpeed(float tickRate)
        {
            if(tickRate>=10) return 15;
            if(tickRate>=4.5f) return 6;
            if(tickRate>=2) return 3;
            return 1;
        }
    }
}
