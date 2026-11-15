using System;
using System.Collections.Generic;
using Point=Annelitrice.Retheme.MahouShoujo.MiracleGeometry.Point;
using Mark=Annelitrice.Retheme.MahouShoujo.MiracleGeometry.Mark;
using Tint=Annelitrice.Retheme.MahouShoujo.MiracleGeometry.Tint;

namespace Annelitrice.Retheme.MahouShoujo
{
    public static class WishArrivalGeometry
    {
        public const int DurationTicks=108;
        public const int BlossomTick=24;
        private const float Pi=(float)Math.PI;
        private static readonly Tint Pink=new Tint(1,.40f,.69f),Blue=new Tint(.38f,.79f,1),
            White=new Tint(1,.97f,.90f),Gold=new Tint(1,.79f,.36f),Lilac=new Tint(.76f,.60f,1);
        private static float Sin(float t) { return (float)Math.Sin(t); }
        private static float Cos(float t) { return (float)Math.Cos(t); }
        private static float Clamp(float t) { return Math.Max(0,Math.Min(1,t)); }
        private static float Smooth(float t) { t=Clamp(t);return t*t*(3-2*t); }
        private static float Fade(float t,float a,float b,float c,float d)
        { return Smooth((t-a)/(b-a))*(1-Smooth((t-c)/(d-c))); }
        private static float Hash(int n) { return (uint)(n*747796405+2891336453u)%65521/65521f; }
        private static Point Add(Point p,float x,float z) { return new Point(p.x+x,p.z+z); }
        public static Point[] PetalOutline()
        {
            Point[] p=new Point[24];
            for(int i=0;i<p.Length;i++)
            {
                float t=i*2*Pi/p.Length;
                p[i]=new Point(.5f*Sin(t)*Math.Abs(Sin(t)),.5f*Cos(t));
            }
            return p;
        }
        private static readonly Point[] Petal=PetalOutline();
        private static void Put(List<Mark> q,Point p,float w,float h,float angle,Tint c,float a,int shape=0,int layer=25,bool glow=false)
        {
            if(a<=.003f||w<=.0001f||h<=.0001f) return;
            q.Add(new Mark{x=p.x,z=p.z,w=w,h=h,angle=angle,r=c.r,g=c.g,b=c.b,a=Clamp(a),shape=shape,layer=layer,glow=glow});
        }
        private static void Glow(List<Mark> q,Point p,float size,Tint c,float a,int layer=15)
        { Put(q,p,size,size,0,c,a,0,layer,true); }
        private static void Line(List<Mark> q,Point a,Point b,float width,Tint c,float alpha,int layer)
        {
            float dx=b.x-a.x,dz=b.z-a.z;
            float length=(float)Math.Sqrt(dx*dx+dz*dz);
            if(length>.0001f) Put(q,new Point((a.x+b.x)*.5f,(a.z+b.z)*.5f),length+width*.35f,width,(float)Math.Atan2(dz,dx),c,alpha,0,layer);
        }
        private static void Arc(List<Mark> q,Point p,float radius,float flatten,float start,float end,float width,Tint color,float alpha,int layer)
        {
            int n=32;
            for(int i=0;i<n;i++)
            {
                float a=start+(end-start)*i/n,b=start+(end-start)*(i+1)/n;
                Line(q,Add(p,Cos(a)*radius,Sin(a)*radius*flatten),Add(p,Cos(b)*radius,Sin(b)*radius*flatten),width,color,alpha,layer);
            }
        }
        private static void Star(List<Mark> q,Point p,float size,float angle,Tint color,float alpha,int layer=35)
        {
            Glow(q,p,size*3.4f,color,alpha*.24f,layer);
            Put(q,p,size,size*1.18f,angle,color,alpha,2,layer+1);
            Put(q,p,size*.38f,size*.50f,angle,White,alpha,2,layer+2);
        }
        private static void Heart(List<Mark> q,Point p,float size,float angle,float alpha)
        {
            Glow(q,p,size*2,Pink,alpha*.18f,40);
            Put(q,p,size,size,angle,Pink,alpha,1,41);
            Put(q,Add(p,-size*.04f,size*.035f),size*.66f,size*.66f,angle,White,alpha*.7f,1,42);
        }
        private static Point Fall(Point origin,float u)
        {
            float descent=Smooth(u);
            return Add(origin,-.18f*(1-descent)+.035f*Sin(u*Pi),1.30f*(1-descent)+.02f);
        }
        private static void FlowerPetal(List<Mark> q,Point center,float width,float height,float angle,Tint color,float alpha)
        {
            Put(q,center,width,height,angle,color,alpha*.20f,3,6);
            float co=Cos(angle),si=Sin(angle);
            for(int i=0;i<Petal.Length;i++)
            {
                Point a=Petal[i],b=Petal[(i+1)%Petal.Length];
                Point aa=Add(center,a.x*width*co-a.z*height*si,a.x*width*si+a.z*height*co);
                Point bb=Add(center,b.x*width*co-b.z*height*si,b.x*width*si+b.z*height*co);
                Line(q,aa,bb,.018f,color,alpha*.8f,7);
            }
            Glow(q,center,Math.Max(width,height)*1.35f,color,alpha*.11f,8);
        }
        public static void Build(float tick,Point origin,Point fairy,List<Mark> q)
        {
            q.Clear();
            if(tick<0||tick>=DurationTicks) return;
            Point floor=Add(origin,0,-.38f);
            Point body=Add(fairy,0,.01f);
            // A brief local glimmer joins the already-spawned fairy to the summon event.
            float flash=1-Smooth(tick/9f);
            Glow(q,body,1.35f,White,flash*.46f,23);
            Glow(q,body,1.9f,Blue,flash*.22f,14);

            if(tick<27)
            {
                float u=Clamp(tick/23f),alpha=Fade(tick,-1,3,22,27);
                Point star=Fall(origin,u);
                for(int i=1;i<=12;i++)
                {
                    float v=u-i*.023f;
                    if(v<0) continue;
                    Point p=Fall(origin,v);
                    Glow(q,p,.09f+.015f*i,Gold,alpha*(1-i/14f)*.24f,31);
                }
                Star(q,star,.25f+.07f*Sin(u*Pi),u*.22f,Gold,alpha,32);
                Star(q,Add(star,-.095f,.085f),.075f,0,White,alpha*.8f,34);
            }

            float flower=Fade(tick,9,23,58,88);
            float opening=Smooth((tick-12)/28f);
            if(flower>.003f)
            {
                Glow(q,floor,2.1f,Blue,flower*.16f,4);
                Arc(q,floor,.25f+.62f*opening,.46f,0,2*Pi,.018f,Gold,flower*.72f,5);
                Arc(q,floor,.18f+.48f*opening,.46f,0,2*Pi,.012f,White,flower*.5f,5);
                for(int i=0;i<5;i++)
                {
                    float grow=Smooth((tick-13-i*2.7f)/15f);
                    float a=Pi*.5f+i*2*Pi/5;
                    float radius=.10f+.43f*grow;
                    Point p=Add(floor,Cos(a)*radius,Sin(a)*radius*.66f);
                    FlowerPetal(q,p,(.12f+.25f*grow),(.18f+.46f*grow),a-Pi*.5f,i%2==0?Pink:Blue,flower*grow);
                    Point tip=Add(floor,Cos(a)*(.37f+.42f*grow),Sin(a)*(.37f+.42f*grow)*.66f);
                    Star(q,tip,.08f+.025f*grow,0,Gold,flower*grow*.85f,9);
                }
            }

            float bloom=Fade(tick,20,24,26,38);
            Glow(q,body,1.3f,White,bloom*.24f,24);
            Glow(q,floor,2.15f,Gold,bloom*.19f,12);
            // An open moon arc frames the fairy's upper silhouette without covering its face.
            float moon=Fade(tick,17,30,48,76);
            if(moon>.003f)
            {
                Point top=Add(body,0,.06f);
                float r=.47f+.16f*Smooth((tick-18)/18f);
                Arc(q,top,r,.82f,-.10f*Pi,1.10f*Pi,.021f,Gold,moon*.82f,25);
                Arc(q,top,r+.052f,.82f,.03f*Pi,.97f*Pi,.011f,White,moon*.65f,26);
                Star(q,Add(top,-r*.94f,-.02f),.12f,0,Gold,moon,28);
                Star(q,Add(top,r*.94f,-.02f),.10f,0,Blue,moon,28);
            }

            float ribbon=Fade(tick,22,33,48,71);
            if(ribbon>.003f)
            {
                for(int side=-1;side<=1;side+=2)
                {
                    Tint color=side<0?Pink:Blue;
                    Point previous=Add(body,side*.47f,-.32f);
                    for(int j=1;j<=18;j++)
                    {
                        float u=j/18f;
                        float sway=Sin(u*Pi*2-tick*.07f)*.075f;
                        Point p=Add(body,side*(.47f-.13f*u+sway),-.32f+u*.91f);
                        float alpha=ribbon*Sin(u*Pi)*.64f;
                        Line(q,previous,p,.032f,color,alpha,27);
                        Line(q,previous,p,.008f,White,alpha*.7f,28);
                        previous=p;
                    }
                }
            }

            // 80 particles, staggered into falling dust, blossom rays, little hearts and a low orbit.
            for(int i=0;i<80;i++)
            {
                float h=Hash(i*17+5),j=Hash(i*29+7),k=Hash(i*41+13);
                float birth,life,age,size;Point p;int layer;
                if(i<12)
                {
                    birth=i*1.5f;life=12+j*10;age=(tick-birth)/life;
                    if(age<0||age>=1) continue;
                    p=Fall(origin,Clamp(birth/23f));
                    p=Add(p,(h-.5f)*.15f+Sin(age*4+h)*.06f,-age*.16f);
                    size=.04f+j*.035f;layer=34;
                }
                else if(i<52)
                {
                    birth=22+h*12;life=25+j*28;age=(tick-birth)/life;
                    if(age<0||age>=1) continue;
                    float a=(i-12)*2*Pi/40+h*.2f;
                    float radius=.16f+(.42f+k*.38f)*(1-(1-age)*(1-age));
                    p=Add(body,Cos(a)*radius,Sin(a)*radius*.63f+age*.28f);
                    size=.055f+j*.065f;layer=35;
                }
                else if(i<68)
                {
                    birth=39+h*19;life=34+j*16;age=(tick-birth)/life;
                    if(age<0||age>=1) continue;
                    p=Add(body,(h-.5f)*1.05f+Sin(age*4+j*5)*.065f,-.05f+age*(.58f+k*.33f));
                    size=.07f+j*.035f;layer=41;
                }
                else
                {
                    birth=11+h*14;life=30+j*17;age=(tick-birth)/life;
                    if(age<0||age>=1) continue;
                    float a=h*2*Pi+age*.9f;
                    p=Add(floor,Cos(a)*(.42f+age*.35f),Sin(a)*(.42f+age*.35f)*.48f);
                    size=.055f+j*.04f;layer=9;
                }
                float alpha=Fade(age,0,.12f,.55f,1);
                float twinkle=.70f+.30f*Sin(age*Pi*3+h*3);
                if(i>=52&&i<68) Heart(q,p,size*(.8f+.2f*twinkle),(h-.5f)*.4f,alpha*.76f);
                else Star(q,p,size*(.75f+.25f*twinkle),(h-.5f)*.65f,i%4==0?Pink:(i%4==1?Blue:(i%4==2?Gold:Lilac)),alpha*twinkle,layer);
            }
            for(int i=0;i<3;i++)
            {
                float t=(tick-79-i*7)/15f;
                if(t>=0&&t<1) Star(q,Add(body,-.32f+i*.30f,.53f+i*.09f),.125f*Sin(t*Pi),0,i==1?Gold:White,Sin(t*Pi),45);
            }
        }
    }
}
