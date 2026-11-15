using System;
using System.Collections.Generic;
using Point=Annelitrice.Retheme.MahouShoujo.MiracleGeometry.Point;
using Mark=Annelitrice.Retheme.MahouShoujo.MiracleGeometry.Mark;
using Tint=Annelitrice.Retheme.MahouShoujo.MiracleGeometry.Tint;

namespace Annelitrice.Retheme.MahouShoujo
{
    public static class ShiningDebutGeometry
    {
        public const int DurationTicks=144;
        public const int DebutTick=33;
        private const float Pi=(float)Math.PI;
        private static readonly Tint Pink=new Tint(1,.30f,.64f),Blue=new Tint(.35f,.79f,1),
            White=new Tint(1,.97f,.91f),Gold=new Tint(1,.80f,.38f),Lilac=new Tint(.76f,.55f,1);
        private static readonly Point[] Heart=MiracleGeometry.Outline(1);
        private static float Sin(float t) { return (float)Math.Sin(t); }
        private static float Cos(float t) { return (float)Math.Cos(t); }
        private static float Clamp(float t) { return Math.Max(0,Math.Min(1,t)); }
        private static float Smooth(float t) { t=Clamp(t);return t*t*(3-2*t); }
        private static float Fade(float t,float a,float b,float c,float d)
        { return Smooth((t-a)/(b-a))*(1-Smooth((t-c)/(d-c))); }
        private static float Hash(int n) { return (uint)(n*747796405+2891336453u)%65521/65521f; }
        private static Point Add(Point p,float x,float z) { return new Point(p.x+x,p.z+z); }
        private static Point Mix(Point a,Point b,float u) { return new Point(a.x+(b.x-a.x)*u,a.z+(b.z-a.z)*u); }
        public static Point[] Outline(int shape)
        {
            if(shape<=6) return CrystalFormationGeometry.Outline(shape);
            if(shape==8) return new[]{new Point(0,.5f),new Point(.5f,0),new Point(0,-.5f),new Point(-.5f,0)};
            Point[] edge=new Point[32];
            for(int i=0;i<edge.Length;i++)
            {
                float t=i*2*Pi/edge.Length;
                edge[i]=new Point(.5f*Sin(t)*Math.Abs(Sin(t)),.5f*Cos(t));
            }
            return edge;
        }
        private static void Put(List<Mark> q,Point p,float w,float h,float angle,Tint c,float alpha,int shape=0,int layer=25,bool glow=false)
        {
            if(w<=.0001f||h<=.0001f||alpha<=.003f) return;
            q.Add(new Mark{x=p.x,z=p.z,w=w,h=h,angle=angle,r=c.r,g=c.g,b=c.b,a=Clamp(alpha),shape=shape,layer=layer,glow=glow});
        }
        private static void Glow(List<Mark> q,Point p,float size,Tint c,float alpha,int layer=25)
        { Put(q,p,size,size,0,c,alpha,0,layer,true); }
        private static void Line(List<Mark> q,Point a,Point b,float width,Tint c,float alpha,int layer)
        {
            float dx=b.x-a.x,dz=b.z-a.z,length=(float)Math.Sqrt(dx*dx+dz*dz);
            if(length>.0001f) Put(q,Mix(a,b,.5f),length+width*.35f,width,(float)Math.Atan2(dz,dx),c,alpha,0,layer);
        }
        private static void Ring(List<Mark> q,Point p,float r,float flatten,float width,Tint c,float alpha,int layer)
        {
            for(int i=0;i<48;i++)
            {
                float a=i*2*Pi/48,b=(i+1)*2*Pi/48;
                Line(q,Add(p,Cos(a)*r,Sin(a)*r*flatten),Add(p,Cos(b)*r,Sin(b)*r*flatten),width,c,alpha,layer);
            }
        }
        private static void Star(List<Mark> q,Point p,float size,float angle,Tint c,float alpha,int layer=30)
        {
            Glow(q,p,size*3.6f,c,alpha*.25f,layer);
            Put(q,p,size,size*1.22f,angle,c,alpha,2,layer+1);
            Put(q,p,size*.36f,size*.48f,angle,White,alpha,2,layer+2);
        }
        private static void SmallHeart(List<Mark> q,Point p,float size,float angle,Tint c,float alpha,int layer=28)
        {
            Glow(q,p,size*2.6f,c,alpha*.19f,layer);
            Put(q,p,size,size,angle,c,alpha,1,layer+1);
            Put(q,Add(p,-size*.11f,size*.17f),size*.18f,size*.22f,-.5f,White,alpha*.85f,7,layer+2);
        }
        private static void HeartLine(List<Mark> q,Point p,float w,float h,float width,Tint c,float alpha,int layer)
        {
            for(int i=0;i<Heart.Length;i++)
            {
                Point a=Heart[i],b=Heart[(i+1)%Heart.Length];
                Line(q,Add(p,a.x*w,a.z*h),Add(p,b.x*w,b.z*h),width,c,alpha,layer);
            }
        }
        private static Point Ribbon(Point p,float t,float open,int side,float tick)
        {
            return Add(p,side*(.12f+t*(.28f+.92f*open)+Sin(t*Pi*2.1f)*.16f*open),
                -.16f+Sin(t*Pi*1.65f)*(.22f+.32f*open)+t*.48f+Sin(t*Pi*3+tick*.045f)*.035f);
        }
        public static void Build(float tick,Point origin,Point adult,List<Mark> q)
        {
            q.Clear();
            if(tick<0||tick>=DurationTicks) return;
            Point body=Add(adult,0,.04f),floor=Add(origin,0,-.43f);
            float pulse=1-Smooth(tick/9f);
            Glow(q,body,1.75f,White,pulse*.46f,23);
            Glow(q,body,2.10f,Pink,pulse*.20f,22);
            // Six crystalline light petals unfold outward and dissolve before the reveal.
            float release=Smooth((tick-7)/27f),petal=Fade(tick,6,11,25,40);
            for(int i=0;i<6;i++)
            {
                float a=Pi*.18f+i*2*Pi/6,r=.22f+release*.82f;
                Point p=Add(body,Cos(a)*r,Sin(a)*r*.79f);
                Tint c=i%2==0?Pink:Blue;
                Put(q,p,.21f*(1-release*.4f),.52f*(1-release*.18f),a-Pi*.5f,c,petal*.32f,8,24);
                Put(q,p,.15f,.38f,a-Pi*.5f,White,petal*.24f,7,25);
                Glow(q,p,.52f,c,petal*.19f,24);
                Star(q,p,.12f,release*.4f,Gold,petal*.75f,26);
            }

            float stage=Fade(tick,9,33,73,106),spread=Smooth((tick-9)/30f);
            if(stage>.003f)
            {
                float r=.56f+spread*.62f;
                Ring(q,floor,r,.39f,.024f,Gold,stage*.82f,4);
                Ring(q,floor,r*.85f,.39f,.012f,Pink,stage*.70f,5);
                Glow(q,floor,2.35f,Blue,stage*.14f,3);
                for(int i=0;i<8;i++)
                {
                    float a=i*2*Pi/8+Pi/8;
                    Point p=Add(floor,Cos(a)*r*.70f,Sin(a)*r*.28f);
                    Put(q,p,.24f,.47f,a-Pi*.5f,i%2==0?Pink:Blue,stage*.18f,7,6);
                    Star(q,Add(floor,Cos(a)*r,Sin(a)*r*.39f),.078f,0,Gold,stage*.85f,7);
                }
            }

            float ribbons=Fade(tick,8,22,59,93),opening=Smooth((tick-8)/27f);
            if(ribbons>.003f)
            {
                for(int side=-1;side<=1;side+=2)
                {
                    Tint c=side<0?Pink:Blue;
                    for(int n=0;n<38;n++)
                    {
                        float a=n/38f,b=(n+1)/38f;
                        Point p=Ribbon(body,a,opening,side,tick),p2=Ribbon(body,b,opening,side,tick);
                        float alpha=ribbons*(.32f+.68f*Sin((a+b)*Pi*.5f));
                        Line(q,p,p2,.048f,c,alpha*.80f,14);
                        Line(q,p,p2,.012f,White,alpha*.87f,15);
                    }
                    Point tip=Ribbon(body,1,opening,side,tick);
                    SmallHeart(q,tip,.17f,-side*.28f,c,ribbons*.85f,17);
                }
            }

            float crown=Fade(tick,25,33,63,94),grow=Smooth((tick-22)/13f);
            if(crown>.003f)
            {
                Point center=Add(body,0,.27f);
                // Hollow heart and a seven-point fan sit behind the actual pawn.
                HeartLine(q,center,1.66f*grow,1.59f*grow,.043f,Pink,crown*.65f,12);
                HeartLine(q,center,1.73f*grow,1.67f*grow,.013f,White,crown*.86f,13);
                for(int i=0;i<7;i++)
                {
                    float a=.13f*Pi+i*.74f*Pi/6;
                    Point p=Add(body,Cos(a)*1.10f*grow,.10f+Sin(a)*1.10f*grow);
                    Point inner=Add(body,Cos(a)*.76f*grow,.10f+Sin(a)*.76f*grow);
                    Line(q,inner,p,.017f,Gold,crown*.70f,16);
                    if(i==3) SmallHeart(q,p,.22f,0,Pink,crown,17);
                    else Star(q,p,i%2==0?.15f:.12f,0,i%2==0?Gold:Blue,crown,17);
                }
                Glow(q,Add(body,0,.95f),1.50f,Gold,crown*.10f,11);
            }
            float reveal=Fade(tick,28,33,35,45);
            Glow(q,body,1.80f,White,reveal*.16f,23);
            Glow(q,Add(body,0,.65f),1.90f,Gold,reveal*.13f,12);
            if(tick>=29&&tick<58)
            {
                float u=(tick-29)/29f;
                Ring(q,Add(body,0,.12f),.42f+u*1.05f,.78f,.021f,White,Sin(u*Pi)*.47f,16);
            }

            // 128 staggered particles: crystal release, debut star shower, and late afterglow.
            for(int i=0;i<128;i++)
            {
                float h=Hash(i*17+9),j=Hash(i*29+7),k=Hash(i*43+15);
                float birth,life,age;Point p;int layer=30;
                if(i<24)
                {
                    birth=8+h*13;life=18+j*17;age=(tick-birth)/life;
                    if(age<0||age>=1) continue;
                    float a=i*2.39996f,r=.18f+age*(.7f+k*.42f);
                    p=Add(body,Cos(a)*r,Sin(a)*r*.72f+age*.13f);
                }
                else if(i<96)
                {
                    birth=29+h*22;life=29+j*28;age=(tick-birth)/life;
                    if(age<0||age>=1) continue;
                    int side=i%2==0?-1:1;
                    float r=.28f+(.62f+k*.60f)*(1-(1-age)*(1-age));
                    p=Add(body,side*r,(-.17f+j*.9f)+Sin(age*Pi*.80f)*(.22f+k*.27f)+age*.19f);
                    if(i%4==0)
                    {
                        Point tail=Add(p,-side*.11f*(1-age),-.08f*(1-age));
                        Line(q,p,tail,.012f,Gold,Fade(age,0,.08f,.45f,1)*.49f,29);
                    }
                }
                else
                {
                    birth=65+h*43;life=21+j*24;age=(tick-birth)/life;
                    if(age<0||age>=1) continue;
                    p=Add(body,(k-.5f)*2.30f+Sin(age*Pi*1.5f+h*6)*.07f,-.14f+j*1.03f+age*.34f);
                }
                float alpha=Fade(age,0,.10f,.43f,1),size=.053f+j*.085f;
                Tint c=i%4==0?Pink:(i%4==1?Blue:(i%4==2?Gold:Lilac));
                if(i>=96&&i%3==0) SmallHeart(q,p,size*1.08f,(h-.5f)*.45f,c,alpha*.85f,layer);
                else Star(q,p,size,(h-.5f)*.65f,c,alpha*(.76f+.24f*Sin(age*Pi*3)),layer);
            }
            // Two deliberate answers after the main reveal, then three clean final glints.
            float beat1=Fade(tick,62,68,70,81),beat2=Fade(tick,77,83,85,96);
            Star(q,Add(body,-.77f,.73f),.25f,0,White,beat1,37);
            Star(q,Add(body,.72f,.43f),.22f,0,Gold,beat2,37);
            for(int i=0;i<3;i++)
            {
                float a=Fade(tick,106+i*6,112+i*6,114+i*6,130+i*6);
                Point p=Add(body,i==0?-.47f:(i==1?.56f:.11f),i==0?.42f:(i==1?.70f:1.01f));
                Star(q,p,i==2?.14f:.11f,0,i==1?Pink:White,a,38);
            }
        }
    }
}
