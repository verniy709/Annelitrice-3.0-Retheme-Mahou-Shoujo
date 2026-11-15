namespace Annelitrice.Retheme.MahouShoujo
{
    public sealed class ShiningDebutAudioTiming
    {
        public const int BloomTick=ShiningDebutGeometry.DebutTick;
        public const int AnswerATick=68,AnswerBTick=83;
        public const int StarATick=112,StarBTick=118,StarCTick=124;
        private int delivered;
        public int Advance(int age)
        {
            int due=0;
            if(age>=BloomTick) due|=1;
            if(age>=AnswerATick) due|=2;
            if(age>=AnswerBTick) due|=4;
            if(age>=StarATick) due|=8;
            if(age>=StarBTick) due|=16;
            if(age>=StarCTick) due|=32;
            int fresh=due&~delivered;
            delivered|=due;
            return fresh;
        }
    }
}
