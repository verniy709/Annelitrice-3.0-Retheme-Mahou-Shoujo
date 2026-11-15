using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Annelitrice.Retheme.MahouShoujo
{
    public sealed class CompProperties_ShareMiracle : CompProperties_AbilityEffect
    {
        public CompProperties_ShareMiracle() { compClass=typeof(CompAbilityEffect_ShareMiracle); }
    }
    public sealed class CompAbilityEffect_ShareMiracle : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target,LocalTargetInfo dest)
        {
            base.Apply(target,dest);
            if(parent!=null) ShareMiracle.Start(parent.pawn,target.Pawn);
        }
    }
    public sealed class ShareMiracle : MapComponent
    {
        private sealed class Instance
        {
            internal Pawn caster,recipient;
            internal int startTick;
            internal readonly MiracleAudioTiming audio=new MiracleAudioTiming();
        }
        private readonly List<Instance> active=new List<Instance>();
        private readonly List<MiracleGeometry.Mark> marks=new List<MiracleGeometry.Mark>(512);
        private static Material solid,glow;
        private static Mesh heart,star;
        private MaterialPropertyBlock properties;
        public ShareMiracle(Map map) : base(map) { }
        internal static void Start(Pawn caster,Pawn recipient)
        {
            if(caster==null||recipient==null||!caster.Spawned||!recipient.Spawned||
                caster.Dead||recipient.Dead||caster.Map==null||caster.Map!=recipient.Map) return;
            ShareMiracle controller=caster.Map.GetComponent<ShareMiracle>();
            if(controller==null) return;
            // Cosmetic instances own no gameplay state and are bounded under repeated triggering.
            if(controller.active.Count>=24) controller.active.RemoveAt(0);
            controller.active.Add(new Instance{caster=caster,recipient=recipient,startTick=Find.TickManager.TicksGame});
            if(caster.Map==Find.CurrentMap)
            {
                int speed=MiracleAudioTiming.CastSpeed(Find.TickManager.TickRateMultiplier);
                PlaySound(recipient,"ShareMiracleCast"+(speed==1?"":"_Fast"+speed));
            }
        }
        private static void PlaySound(Pawn recipient,string suffix)
        {
            SoundDef sound=DefDatabase<SoundDef>.GetNamedSilentFail("Anneli_Retheme_MahouShoujo_"+suffix);
            if(sound!=null) sound.PlayOneShot(SoundInfo.InMap(new TargetInfo(recipient)));
        }
        private bool Valid(Instance i)
        {
            return i.caster!=null&&i.recipient!=null&&i.caster.Spawned&&i.recipient.Spawned&&
                !i.caster.Dead&&!i.recipient.Dead&&i.caster.Map==map&&i.recipient.Map==map;
        }
        public override void MapComponentTick()
        {
            for(int n=active.Count-1;n>=0;n--)
            {
                Instance i=active[n];
                if(!Valid(i)) { active.RemoveAt(n);continue; }
                int age=Find.TickManager.TicksGame-i.startTick;
                // Service cues on ticks so high game speed or a slow render cannot miss them.
                // Off-map cues are consumed silently and never replay when returning to the map.
                int cues=i.audio.Advance(age);
                if(map==Find.CurrentMap)
                {
                    if((cues&1)!=0) PlaySound(i.recipient,"ShareMiracleBloom");
                    if((cues&2)!=0)
                    {
                        int speed=MiracleAudioTiming.CastSpeed(Find.TickManager.TickRateMultiplier);
                        PlaySound(i.recipient,"ShareMiracleBubbles"+(speed==1?"":"_Fast"+speed));
                    }
                }
                if(age>=MiracleGeometry.DurationTicks) active.RemoveAt(n);
            }
        }
        private static Mesh MakeMesh(int shape)
        {
            MiracleGeometry.Point[] edge=MiracleGeometry.Outline(shape);
            Vector3[] vertices=new Vector3[edge.Length+1];
            Vector2[] uv=new Vector2[vertices.Length];
            Color[] colors=new Color[vertices.Length];
            int[] triangles=new int[edge.Length*3];
            uv[0]=new Vector2(.5f,.5f);
            colors[0]=Color.white;
            for(int i=0;i<edge.Length;i++)
            {
                vertices[i+1]=new Vector3(edge[i].x,0,edge[i].z);
                uv[i+1]=new Vector2(edge[i].x+.5f,edge[i].z+.5f);
                colors[i+1]=Color.white;
                triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=(i+1)%edge.Length+1;
            }
            Mesh mesh=new Mesh();
            mesh.name=shape==1?"MahouShoujo_MiracleHeart":"MahouShoujo_MiracleStar";
            mesh.vertices=vertices;mesh.uv=uv;mesh.colors=colors;mesh.triangles=triangles;
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            return mesh;
        }
        private static int CompareMarks(MiracleGeometry.Mark a,MiracleGeometry.Mark b)
        {
            int pass=a.glow.CompareTo(b.glow);
            return pass!=0?pass:a.layer.CompareTo(b.layer);
        }
        public override void MapComponentUpdate()
        {
            if(active.Count==0||map!=Find.CurrentMap||!WorldRendererUtility.DrawingMap) return;
            if(solid==null) solid=MaterialPool.MatFrom("Anneli_Retheme_MahouShoujo/Anneli_Effect/Solid",ShaderDatabase.Mote);
            if(glow==null) glow=MaterialPool.MatFrom("Anneli_Retheme_MahouShoujo/Anneli_Effect/SoftGlow",ShaderDatabase.MoteGlow);
            if(heart==null) heart=MakeMesh(1);
            if(star==null) star=MakeMesh(2);
            if(properties==null) properties=new MaterialPropertyBlock();
            float altitude=AltitudeLayer.MoteOverhead.AltitudeFor();
            foreach(Instance i in active)
            {
                if(!Valid(i)) continue;
                Vector3 a=i.caster.DrawPos,b=i.recipient.DrawPos;
                MiracleGeometry.Build(Find.TickManager.TicksGame-i.startTick,
                    new MiracleGeometry.Point(a.x,a.z),new MiracleGeometry.Point(b.x,b.z),marks);
                marks.Sort(CompareMarks);
                foreach(MiracleGeometry.Mark m in marks)
                {
                    properties.SetColor(ShaderPropertyIDs.Color,new Color(m.r,m.g,m.b,m.a));
                    Matrix4x4 matrix=Matrix4x4.TRS(new Vector3(m.x,altitude+m.layer*.0005f,m.z),
                        Quaternion.AngleAxis(-m.angle*Mathf.Rad2Deg,Vector3.up),new Vector3(m.w,1,m.h));
                    Graphics.DrawMesh(m.shape==1?heart:(m.shape==2?star:MeshPool.plane10),matrix,
                        m.glow?glow:solid,0,null,0,properties);
                }
            }
        }
        // Pausing freezes visuals; map ticks expire off-screen instances. Reload discards them.
    }
}
