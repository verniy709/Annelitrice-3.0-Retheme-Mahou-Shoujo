namespace Annelitrice.Retheme.MahouShoujo
{
    public sealed class WishArrivalAudioTiming
    {
        public const int BloomTick=WishArrivalGeometry.BlossomTick;
        public const int SparkleTick=79;
        private bool bloomPlayed,sparklePlayed;
        public int Advance(int ageTicks)
        {
            int cues=0;
            if(!bloomPlayed&&ageTicks>=BloomTick) { bloomPlayed=true;cues|=1; }
            if(!sparklePlayed&&ageTicks>=SparkleTick) { sparklePlayed=true;cues|=2; }
            return cues;
        }
    }
}
