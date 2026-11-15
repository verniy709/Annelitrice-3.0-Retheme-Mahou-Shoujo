namespace Annelitrice.Retheme.MahouShoujo
{
    public sealed class CrystalFormationAudioTiming
    {
        public const int FacetATick=38,FacetBTick=59,FacetCTick=73;
        public const int SealTick=CrystalFormationGeometry.SealTick;
        private int played;
        public int Advance(int ageTicks)
        {
            int due=0;
            if(ageTicks>=FacetATick) due|=1;
            if(ageTicks>=FacetBTick) due|=2;
            if(ageTicks>=FacetCTick) due|=4;
            if(ageTicks>=SealTick) due|=8;
            int fresh=due&~played;played|=fresh;return fresh;
        }
    }
}
