using System;
using System.Collections.Generic;
using Point=Annelitrice.Retheme.MahouShoujo.MiracleGeometry.Point;
using Mark=Annelitrice.Retheme.MahouShoujo.MiracleGeometry.Mark;
using Tint=Annelitrice.Retheme.MahouShoujo.MiracleGeometry.Tint;

namespace Annelitrice.Retheme.MahouShoujo
{
    public static class CrystalFormationGeometry
    {
        public const int DurationTicks=120;
        public const int SealTick=81;
        private const float Pi=(float)Math.PI;
        private static readonly Tint Pink=new Tint(1,.36f,.68f),Blue=new Tint(.36f,.78f,1),
            White=new Tint(1,.97f,.91f),Gold=new Tint(1,.80f,.39f),Lilac=new Tint(.76f,.54f,1);
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
            if(shape<3) return MiracleGeometry.Outline(shape);
            // Four right-triangle facets join at the centre of the abstract diamond.
            int index=shape-3;float x=(index==0||index==3)?1:-1,z=index<2?1:-1;
            if(x*z>0) return new[]{new Point(0,0),new Point(0,z),new Point(x,0)};
            return new[]{new Point(0,0),new Point(x,0),new Point(0,z)};
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
            float dx=b.x-a.x,dz=b.z-a.z;
            float length=(float)Math.Sqrt(dx*dx+dz*dz);
            if(length>.0001f) Put(q,Mix(a,b,.5f),length+width*.35f,width,(float)Math.Atan2(dz,dx),c,alpha,0,layer);
        }
        private static void Ring(List<Mark> q,Point p,float radius,float flatten,float width,Tint c,float alpha,int layer)
        {
            for(int i=0;i<36;i++)
            {
                float a=i*2*Pi/36,b=(i+1)*2*Pi/36;
                Line(q,Add(p,Cos(a)*radius,Sin(a)*radius*flatten),Add(p,Cos(b)*radius,Sin(b)*radius*flatten),width,c,alpha,layer);
            }
        }
        private static void Star(List<Mark> q,Point p,float size,float angle,Tint c,float alpha,int layer=36)
        {
            Glow(q,p,size*3.5f,c,alpha*.24f,layer);
            Put(q,p,size,size*1.18f,angle,c,alpha,2,layer+1);
            Put(q,p,size*.38f,size*.50f,angle,White,alpha,2,layer+2);
        }
        private static void Diamond(List<Mark> q,Point p,float w,float h,float progress,float width,Tint c,float alpha,int layer)
        {
            Point[] edges={Add(p,0,-h),Add(p,w,0),Add(p,0,h),Add(p,-w,0),Add(p,0,-h)};
            float total=Clamp(progress)*4;
            for(int i=0;i<4;i++)
            {
                float amount=Clamp(total-i);
                if(amount>0) Line(q,edges[i],Mix(edges[i],edges[i+1],amount),width,c,alpha,layer);
            }
        }
        private static Point Coil(Point p,float t,float tick,float radius,float phase)
        {
            float angle=phase+tick*.083f+t*Pi*2.35f;
            return Add(p,Cos(angle)*radius,-.54f+t*1.12f+Sin(angle)*radius*.28f);
        }
        public static void Build(float tick,Point origin,Point crystal,List<Mark> q)
        {
            q.Clear();
            if(tick<0||tick>=DurationTicks) return;
            Point body=Add(crystal,0,.025f),floor=Add(origin,0,-.43f);
            float flash=1-Smooth(tick/8f);
            Glow(q,body,1.55f,White,flash*.43f,24);
            Glow(q,body,1.95f,Pink,flash*.20f,23);
            for(int i=0;i<4;i++) Put(q,body,.45f,.68f,0,i%2==0?White:Pink,flash*.68f,3+i,24);

            float gather=Fade(tick,0,8,59,82);
            float closing=Smooth((tick-8)/63f);
            if(gather>.003f)
            {
                float radius=.98f-.53f*closing;
                Ring(q,floor,radius,.44f,.018f,Gold,gather*.69f,4);
                Ring(q,floor,radius*.79f,.44f,.013f,Blue,gather*.56f,5);
                Glow(q,floor,1.9f-closing*.62f,Blue,gather*.13f,3);
                // The two ribbons wind inward, with rear segments below the item plane.
                float r=.78f-.39f*closing;
                for(int strand=0;strand<2;strand++)
                {
                    Tint color=strand==0?Pink:Blue;float phase=strand*Pi;
                    for(int i=0;i<36;i++)
                    {
                        float t=i/36f,u=(i+1)/36f;
                        Point a=Coil(body,t,tick,r,phase),b=Coil(body,u,tick,r,phase);
                        float alpha=gather*Sin((t+u)*.5f*Pi)*.76f;
                        int layer=Sin(phase+tick*.083f+(t+u)*.5f*Pi*2.35f)<0?28:12;
                        Line(q,a,b,.036f,color,alpha,layer);
                        Line(q,a,b,.009f,White,alpha*.65f,layer+1);
                    }
                }
            }

            float facets=Fade(tick,19,38,86,103);
            for(int i=0;i<4;i++)
            {
                bool upper=i<2;float u=Smooth((tick-(upper?38:24))/30f);
                float x=(i==0||i==3)?1:-1,z=upper?1:-1;
                Point p=Add(body,x*.22f*(1-u),z*.15f*(1-u));
                Put(q,p,.46f,.69f,0,i%2==0?Pink:Blue,facets*u*.15f,3+i,22);
                Point a=Add(p,x*.46f,0),b=Add(p,0,z*.69f);
                Line(q,a,b,.020f,i%2==0?Pink:Blue,facets*u*.78f,23);
                Line(q,p,b,.010f,White,facets*u*.33f,24);
            }
            if(facets>.003f)
            {
                float trace=Smooth((tick-23)/43f);
                float outer=.16f*(1-trace);
                Diamond(q,body,.46f+outer,.69f+outer,trace,.023f,Gold,facets*.83f,25);
                Diamond(q,body,.46f,.69f,trace,.010f,White,facets*.78f,26);
                Glow(q,Add(body,0,.66f),.48f,Gold,facets*.13f,27);
            }

            float seal=Fade(tick,74,81,83,96);
            if(seal>.003f)
            {
                Point crest=Add(body,0,.80f);
                Glow(q,crest,.66f,Pink,seal*.26f,32);
                Put(q,crest,.17f,.17f,0,Pink,seal,1,33);
                Put(q,Add(crest,-.008f,.008f),.12f,.12f,0,White,seal*.76f,1,34);
                Star(q,Add(body,0,.70f),.21f,0,White,seal,35);
                Glow(q,body,1.55f,White,seal*.18f,30);
            }
            // Clip a diagonal gloss streak to the abstract diamond, leaving the inner girl legible.
            if(tick>=80&&tick<96)
            {
                float u=(tick-80)/16f,s=-.79f+u*1.58f;
                float alpha=Sin(u*Pi)*.72f;
                for(int i=0;i<32;i++)
                {
                    float z=-.69f+i*1.38f/32,z2=-.69f+(i+1)*1.38f/32;
                    float x=s-.58f*z,x2=s-.58f*z2;
                    if(Math.Abs(x)/.46f+Math.Abs(z)/.69f>1||Math.Abs(x2)/.46f+Math.Abs(z2)/.69f>1) continue;
                    Point a=Add(body,x,z),b=Add(body,x2,z2);
                    Line(q,a,b,.055f,Gold,alpha*.21f,36);
                    Line(q,a,b,.019f,White,alpha,37);
                }
            }

            for(int i=0;i<96;i++)
            {
                float h=Hash(i*17+9),j=Hash(i*29+7),k=Hash(i*43+15);
                float birth,life,age,size;Point p;int layer;
                if(i<64)
                {
                    birth=2+h*45;life=24+j*22;age=(tick-birth)/life;
                    if(age<0||age>=1) continue;
                    float angle=i*2.39996f+age*1.4f;
                    float radius=.07f+(1.00f+k*.27f)*(float)Math.Pow(1-age,1.5);
                    p=Add(body,Cos(angle)*radius,Sin(angle)*radius*.67f+(j-.5f)*.16f*(1-age));
                    size=.052f+j*.067f;layer=38;
                    if(i<32&&age<.8f)
                    {
                        Point tail=Add(p,Cos(angle)*.10f*(1-age),Sin(angle)*.10f*(1-age)*.67f);
                        Line(q,p,tail,.010f,i%2==0?Pink:Blue,Fade(age,0,.1f,.50f,1)*.38f,35);
                    }
                }
                else if(i<80)
                {
                    birth=8+h*30;life=23+j*22;age=(tick-birth)/life;
                    if(age<0||age>=1) continue;
                    float a=h*2*Pi+age*1.8f,r=.10f+.90f*(1-age)*(1-age);
                    p=Add(floor,Cos(a)*r,Sin(a)*r*.44f);size=.052f+j*.045f;layer=6;
                }
                else
                {
                    birth=78+h*15;life=14+j*15;age=(tick-birth)/life;
                    if(age<0||age>=1) continue;
                    float a=(i-80)*2*Pi/16;
                    p=Add(body,Cos(a)*(.42f+.10f*age),Sin(a)*(.67f+.08f*age));
                    size=.060f+j*.060f;layer=40;
                }
                float alpha=Fade(age,0,.12f,.5f,1),twinkle=.72f+.28f*Sin(age*Pi*3+h*4);
                Tint color=i%4==0?Pink:(i%4==1?Blue:(i%4==2?Gold:Lilac));
                if(i>=48&&i<64)
                {
                    Put(q,p,size*.63f,size*1.1f,.65f+age,color,alpha*.85f,0,layer);
                    Glow(q,p,size*2.8f,color,alpha*.18f,layer);
                }
                else Star(q,p,size*(.75f+.25f*twinkle),(h-.5f)*.6f,color,alpha*twinkle,layer);
            }
            if(tick>=96)
            {
                float alpha=Fade(tick,95,101,106,118);
                Star(q,Add(body,.29f,.38f),.12f,0,White,alpha,44);
            }
        }
    }
}
