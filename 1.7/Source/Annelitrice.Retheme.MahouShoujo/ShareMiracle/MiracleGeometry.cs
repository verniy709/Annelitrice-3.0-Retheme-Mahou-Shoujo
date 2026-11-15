using System;
using System.Collections.Generic;

namespace Annelitrice.Retheme.MahouShoujo
{
    // Game-independent geometry is shared verbatim with the offline preview exporter.
    public static class MiracleGeometry
    {
        public const int DurationTicks = 96;
        public const int ArrivalTick = 32;
        public struct Point
        {
            public float x, z;
            public Point(float x, float z) { this.x = x; this.z = z; }
        }
        public struct Mark
        {
            public float x, z, w, h, angle, r, g, b, a;
            public int shape, layer;
            public bool glow;
        }
        public struct Tint
        {
            public float r, g, b;
            public Tint(float r, float g, float b) { this.r=r; this.g=g; this.b=b; }
        }
        private static readonly Tint Pink = new Tint(1f,.25f,.57f);
        private static readonly Tint Blush = new Tint(1f,.64f,.81f);
        private static readonly Tint White = new Tint(1f,.97f,.91f);
        private static readonly Tint Gold = new Tint(1f,.79f,.32f);
        private static readonly Tint Blue = new Tint(.32f,.77f,1f);
        private static readonly Tint Lilac = new Tint(.74f,.52f,1f);
        private const float Pi = (float)Math.PI;
        private static float Sin(float x) { return (float)Math.Sin(x); }
        private static float Cos(float x) { return (float)Math.Cos(x); }
        private static float Clamp(float v) { return Math.Max(0f,Math.Min(1f,v)); }
        private static float Smooth(float v) { v=Clamp(v); return v*v*(3f-2f*v); }
        private static float Fade(float t,float a,float b,float c,float d)
        { return Smooth((t-a)/(b-a))*(1f-Smooth((t-c)/(d-c))); }
        private static float Hash(int n) { return (float)((uint)(n*747796405+2891336453u)%65521)/65521f; }
        private static Point Add(Point p,float x,float z) { return new Point(p.x+x,p.z+z); }
        private static Point Mix(Point a,Point b,float t) { return new Point(a.x+(b.x-a.x)*t,a.z+(b.z-a.z)*t); }

        // Shape 0 is the unit quad; shape 1 is a heart, shape 2 a four-point sparkle.
        // Heart and star are native meshes, so no generated sprite sheet is required.
        public static Point[] Outline(int shape)
        {
            if(shape==0) return new [] {new Point(-.5f,-.5f),new Point(.5f,-.5f),new Point(.5f,.5f),new Point(-.5f,.5f)};
            int count=shape==1?64:8;
            Point[] points=new Point[count];
            for(int i=0;i<count;i++)
            {
                float t=i*2f*Pi/count;
                if(shape==1) points[i]=new Point(.5f*Sin(t)*Sin(t)*Sin(t),
                    (13f*Cos(t)-5f*Cos(2*t)-2f*Cos(3*t)-Cos(4*t))/32f+.08f);
                else
                {
                    float radius=(i%2==0)?.5f:.105f;
                    points[i]=new Point(Sin(t)*radius,Cos(t)*radius);
                }
            }
            return points;
        }
        private static readonly Point[] HeartEdge=Outline(1);
        private static void MarkAt(List<Mark> q,Point p,float w,float h,float angle,Tint c,float alpha,int shape=0,int layer=20,bool glow=false)
        {
            if(w<=.0001f||h<=.0001f||alpha<=.003f) return;
            q.Add(new Mark{x=p.x,z=p.z,w=w,h=h,angle=angle,r=c.r,g=c.g,b=c.b,a=Clamp(alpha),shape=shape,layer=layer,glow=glow});
        }
        private static void Glow(List<Mark> q,Point p,float size,Tint c,float alpha,int layer=10)
        { MarkAt(q,p,size,size,0,c,alpha,0,layer,true); }
        private static void Line(List<Mark> q,Point a,Point b,float width,Tint c,float alpha,int layer=20)
        {
            float dx=b.x-a.x,dz=b.z-a.z;
            float length=(float)Math.Sqrt(dx*dx+dz*dz);
            if(length<.0001f) return;
            MarkAt(q,Mix(a,b,.5f),length+width*.35f,width,(float)Math.Atan2(dz,dx),c,alpha,0,layer);
        }
        private static void Ring(List<Mark> q,Point p,float radius,float flatten,float width,Tint c,float alpha,int layer=10)
        {
            for(int i=0;i<40;i++)
            {
                float a=i*2*Pi/40,b=(i+1)*2*Pi/40;
                Line(q,Add(p,Cos(a)*radius,Sin(a)*radius*flatten),Add(p,Cos(b)*radius,Sin(b)*radius*flatten),width,c,alpha,layer);
            }
        }
        private static void HeartLine(List<Mark> q,Point p,float size,float width,Tint c,float alpha,int layer=30)
        {
            for(int i=0;i<HeartEdge.Length;i++)
            {
                Point a=HeartEdge[i],b=HeartEdge[(i+1)%HeartEdge.Length];
                Line(q,Add(p,a.x*size,a.z*size),Add(p,b.x*size,b.z*size),width,c,alpha,layer);
            }
        }
        private static void Star(List<Mark> q,Point p,float size,float rotation,Tint c,float alpha,int layer=40)
        {
            Glow(q,p,size*3.8f,c,alpha*.25f,layer);
            MarkAt(q,p,size,size*1.22f,rotation,c,alpha,2,layer+1);
            MarkAt(q,p,size*.40f,size*.55f,rotation,White,alpha,2,layer+2);
        }
        private static void Heart(List<Mark> q,Point p,float size,float rotation,float alpha,int layer=30)
        {
            Glow(q,p,size*2.1f,Pink,alpha*.28f,layer-1);
            MarkAt(q,p,size,size,rotation,Pink,alpha,1,layer);
            MarkAt(q,Add(p,0,size*.015f),size*.78f,size*.78f,rotation,Blush,alpha,1,layer+1);
            MarkAt(q,Add(p,-size*.15f,size*.23f),size*.12f,size*.22f,-.55f,White,alpha,0,layer+2);
        }
        private static Point Path(Point from,Point to,float u)
        {
            Point p=Mix(from,to,u);
            float dx=to.x-from.x,dz=to.z-from.z;
            float len=(float)Math.Sqrt(dx*dx+dz*dz);
            float nx=len>.001f?-dz/len:0,nz=len>.001f?dx/len:1;
            if(nz<0) { nx=-nx;nz=-nz; }
            float arc=Sin(u*Pi)*(.24f+Math.Min(len,2f)*.11f);
            return Add(p,nx*arc,nz*arc);
        }
        private static void Wings(List<Mark> q,Point p,float size,float alpha)
        {
            for(int side=-1;side<=1;side+=2)
                for(int feather=0;feather<3;feather++)
                {
                    Point a=Add(p,side*size*.24f,-size*.02f);
                    Point b=Add(p,side*size*(.78f-feather*.13f),size*(.27f-feather*.19f));
                    Line(q,a,b,size*.17f,Gold,alpha,32);
                    Line(q,a,b,size*.10f,White,alpha,33);
                }
        }
        public static void Build(float tick,Point caster,Point recipient,List<Mark> q)
        {
            q.Clear();
            if(tick<0||tick>=DurationTicks) return;
            Point from=Add(caster,0,.19f),to=Add(recipient,0,.22f);
            float summon=Fade(tick,-1,5,13,25);
            if(summon>.003f)
            {
                Glow(q,from,.88f,Pink,summon*.38f);
                Wings(q,from,.32f,summon);
                Heart(q,from,.26f+Smooth(tick/10)*.06f,0,summon);
            }

            float travel=Smooth((tick-9)/23f);
            float bridge=Fade(tick,7,12,34,47);
            if(bridge>.003f)
            {
                // Two colored ribbons follow the heart with different phases and widths.
                float tail=Math.Max(0,travel-.72f);
                for(int ribbon=0;ribbon<2;ribbon++)
                {
                    Tint color=ribbon==0?Pink:Blue;
                    for(int i=0;i<30;i++)
                    {
                        float u=tail+(travel-tail)*i/30f,v=tail+(travel-tail)*(i+1)/30f;
                        Point a=Path(from,to,u),b=Path(from,to,v);
                        float offset=(ribbon==0?1:-1)*.08f;
                        a=Add(a,0,Sin(u*Pi*3+tick*.10f)*offset);
                        b=Add(b,0,Sin(v*Pi*3+tick*.10f)*offset);
                        float taper=Sin((i+.5f)/30f*Pi)*.65f+.35f;
                        Line(q,a,b,(ribbon==0?.044f:.032f)*taper,color,bridge*.9f,22+ribbon);
                        Line(q,a,b,.009f*taper,White,bridge*.62f,24+ribbon);
                    }
                }
                if(tick<33)
                {
                    Point main=Path(from,to,travel);
                    Wings(q,main,.30f,bridge);
                    Heart(q,main,.33f+travel*.07f,Sin(tick*.16f)*.08f,bridge,34);
                }
            }

            float seal=Fade(tick,29,35,51,72);
            if(seal>.003f)
            {
                float open=Smooth((tick-30)/14f);
                float size=.43f+open*.67f;
                Glow(q,to,1.7f,Pink,seal*.26f);
                HeartLine(q,to,size,.042f,Pink,seal,30);
                HeartLine(q,to,size,.016f,White,seal*.95f,31);
                HeartLine(q,to,size*1.19f,.012f,Gold,seal*.65f,29);
                // Bow-like crossing highlights below the hollow heart.
                Point knot=Add(to,0,-size*.43f);
                Line(q,Add(knot,-.18f,-.10f),Add(knot,.18f,.10f),.035f,Gold,seal*.8f,32);
                Line(q,Add(knot,-.18f,.10f),Add(knot,.18f,-.10f),.035f,Gold,seal*.8f,32);
                Star(q,knot,.095f,0,White,seal,33);
            }
            float pulse=Fade(tick,29,32,33,42);
            Glow(q,to,1.12f,White,pulse*.52f,35);
            Glow(q,to,1.85f,Gold,pulse*.25f,34);
            if(tick>=31&&tick<58)
            {
                float u=Clamp((tick-31)/27f);
                Ring(q,Add(recipient,0,-.30f),.20f+.65f*Smooth(u),.38f,.022f,Gold,(1-u)*.75f,12);
            }

            // Deterministic particle births keep the game and preview identical.
            for(int i=0;i<64;i++)
            {
                float h=Hash(i*17+3),j=Hash(i*31+11),k=Hash(i*43+19);
                float birth,life; Point p; float size;
                if(i<10)
                {
                    birth=h*9;life=18+j*12;
                    float u=(tick-birth)/life;
                    if(u<0||u>=1) continue;
                    float angle=h*2*Pi;
                    p=Add(from,Cos(angle)*(.13f+.26f*u),Sin(angle)*.15f+.34f*u);
                    size=.055f+j*.035f;
                }
                else if(i<30)
                {
                    birth=10+(i-10)*1.05f;life=16+j*12;
                    float u=(tick-birth)/life;
                    if(u<0||u>=1) continue;
                    p=Path(from,to,Smooth((birth-9)/23f));
                    p=Add(p,(h-.5f)*.17f+(k-.5f)*u*.20f,(j-.5f)*.18f+.12f*u);
                    size=.045f+j*.045f;
                }
                else if(i<54)
                {
                    birth=31+h*7;life=20+j*16;
                    float u=(tick-birth)/life;
                    if(u<0||u>=1) continue;
                    float angle=(i-30)*2*Pi/24f+h*.2f;
                    float radius=.18f+(.30f+k*.40f)*(1-(1-u)*(1-u));
                    p=Add(to,Cos(angle)*radius,Sin(angle)*radius*.72f+.12f*u);
                    size=.065f+j*.07f;
                }
                else
                {
                    birth=44+h*13;life=27+j*12;
                    float u=(tick-birth)/life;
                    if(u<0||u>=1) continue;
                    p=Add(to,(h-.5f)*.95f+Sin(u*4+j*5)*.06f,.06f+u*(.50f+k*.30f));
                    size=.065f+j*.045f;
                }
                float age=(tick-birth)/life;
                float envelope=Fade(age,0,.12f,.54f,1);
                float twinkle=.65f+.35f*Sin(age*Pi*3+h*2);
                Tint color=i%4==0?Blue:(i%4==1?Gold:(i%4==2?Blush:Lilac));
                if(i>=54) Heart(q,p,size*(.8f+.2f*Sin(age*Pi)),(h-.5f)*.4f,envelope*.85f,40);
                else Star(q,p,size*(.7f+.3f*twinkle),(h-.5f)*.8f+age*.25f,color,envelope*twinkle);
            }
            if(tick>=64)
            {
                for(int i=0;i<3;i++)
                {
                    float t=(tick-65-i*8)/15f;
                    if(t>=0&&t<1) Star(q,Add(to,-.35f+i*.32f,.42f+i*.10f),.12f*Sin(t*Pi),0,i==1?Gold:White,Sin(t*Pi));
                }
            }
        }
    }
}
