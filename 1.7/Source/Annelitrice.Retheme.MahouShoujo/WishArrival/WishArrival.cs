using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld.Planet;

namespace Annelitrice.Retheme.MahouShoujo
{
    public sealed class SubEffecter_WishArrival : SubEffecter
    {
        public SubEffecter_WishArrival(SubEffecterDef def,Effecter parent):base(def,parent) { }
        public override void SubTrigger(TargetInfo A,TargetInfo B,int overrideSpawnTick,bool force)
        {
            Map map=A.Map??B.Map;
            if(map==null) return;
            IntVec3 cell=A.Map==map?A.Cell:B.Cell;
            if(cell.InBounds(map)) WishArrival.Start(map,cell);
        }
    }
    public sealed class WishArrival : MapComponent
    {
        private sealed class Instance
        {
            internal Vector3 origin;
            internal Pawn fairy;
            internal int startTick;
            internal IntVec3 cell;
            internal readonly WishArrivalAudioTiming audio=new WishArrivalAudioTiming();
        }
        private readonly List<Instance> active=new List<Instance>();
        private readonly List<MiracleGeometry.Mark> marks=new List<MiracleGeometry.Mark>(640);
        private static Material solid,glow;
        private static Mesh heart,star,petal;
        private MaterialPropertyBlock properties;
        public WishArrival(Map map):base(map) { }
        private bool Claimed(Pawn pawn)
        {
            foreach(Instance i in active) if(i.fairy==pawn) return true;
            return false;
        }
        private Pawn FindFairy(IntVec3 cell,int tick)
        {
            // All original hatch paths spawn the fairy before triggering Anneli_Hatch.
            // Restrict the binding to the newborn pawn so an older fairy is never decorated.
            IReadOnlyList<Pawn> pawns=map.mapPawns.AllPawnsSpawned;
            Pawn best=null;int distance=int.MaxValue;
            foreach(Pawn pawn in pawns)
            {
                if(pawn.def.defName!="AnnelitriceLarvaAsAnimal"||pawn.Dead||pawn.TickSpawned!=tick||Claimed(pawn)) continue;
                int d=pawn.Position.DistanceToSquared(cell);
                if(d<=9&&d<distance) { best=pawn;distance=d; }
            }
            return best;
        }
        internal static void Start(Map map,IntVec3 cell)
        {
            WishArrival controller=map.GetComponent<WishArrival>();
            if(controller==null) return;
            if(controller.active.Count>=24) controller.active.RemoveAt(0);
            int tick=Find.TickManager.TicksGame;
            Instance instance=new Instance{origin=cell.ToVector3Shifted(),fairy=controller.FindFairy(cell,tick),startTick=tick,cell=cell};
            controller.active.Add(instance);
            if(map==Find.CurrentMap)
            {
                int speed=MiracleAudioTiming.CastSpeed(Find.TickManager.TickRateMultiplier);
                controller.PlaySound(instance,"WishArrivalFall"+(speed==1?"":"_Fast"+speed));
            }
        }
        private void PlaySound(Instance instance,string suffix)
        {
            SoundDef sound=DefDatabase<SoundDef>.GetNamedSilentFail("Anneli_Retheme_MahouShoujo_"+suffix);
            if(sound==null) return;
            TargetInfo location=instance.fairy==null?new TargetInfo(instance.cell,map):new TargetInfo(instance.fairy);
            sound.PlayOneShot(SoundInfo.InMap(location));
        }
        private bool Valid(Instance i)
        {
            if(Find.TickManager.TicksGame-i.startTick>=WishArrivalGeometry.DurationTicks) return false;
            return i.fairy==null||(i.fairy.Spawned&&!i.fairy.Dead&&i.fairy.Map==map);
        }
        public override void MapComponentTick()
        {
            for(int n=active.Count-1;n>=0;n--)
            {
                Instance instance=active[n];
                if(!Valid(instance)) { active.RemoveAt(n);continue; }
                int cues=instance.audio.Advance(Find.TickManager.TicksGame-instance.startTick);
                // Tick-driven cues cannot be missed by fast game speed or a slow render.
                // Consume off-map cues silently instead of replaying them when returning.
                if(map==Find.CurrentMap)
                {
                    if((cues&1)!=0) PlaySound(instance,"WishArrivalBloom");
                    if((cues&2)!=0) PlaySound(instance,"WishArrivalSparkle");
                }
            }
        }
        private static Mesh MakeMesh(int shape)
        {
            MiracleGeometry.Point[] edge=shape==3?WishArrivalGeometry.PetalOutline():MiracleGeometry.Outline(shape);
            Vector3[] vertices=new Vector3[edge.Length+1];Vector2[] uv=new Vector2[vertices.Length];
            Color[] colors=new Color[vertices.Length];int[] triangles=new int[edge.Length*3];
            uv[0]=new Vector2(.5f,.5f);colors[0]=Color.white;
            for(int i=0;i<edge.Length;i++)
            {
                vertices[i+1]=new Vector3(edge[i].x,0,edge[i].z);uv[i+1]=new Vector2(edge[i].x+.5f,edge[i].z+.5f);colors[i+1]=Color.white;
                triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=(i+1)%edge.Length+1;
            }
            Mesh mesh=new Mesh();mesh.name="MahouShoujo_WishShape"+shape;
            mesh.vertices=vertices;mesh.uv=uv;mesh.colors=colors;mesh.triangles=triangles;
            mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        private static int Compare(MiracleGeometry.Mark a,MiracleGeometry.Mark b)
        { int pass=a.glow.CompareTo(b.glow);return pass!=0?pass:a.layer.CompareTo(b.layer); }
        public override void MapComponentUpdate()
        {
            if(active.Count==0||map!=Find.CurrentMap||!WorldRendererUtility.DrawingMap) return;
            if(solid==null) solid=MaterialPool.MatFrom("Anneli_Retheme_MahouShoujo/Anneli_Effect/Solid",ShaderDatabase.Mote);
            if(glow==null) glow=MaterialPool.MatFrom("Anneli_Retheme_MahouShoujo/Anneli_Effect/SoftGlow",ShaderDatabase.MoteGlow);
            if(heart==null) heart=MakeMesh(1);if(star==null) star=MakeMesh(2);if(petal==null) petal=MakeMesh(3);
            if(properties==null) properties=new MaterialPropertyBlock();
            float low=AltitudeLayer.MoteLow.AltitudeFor(),high=AltitudeLayer.MoteOverhead.AltitudeFor();
            foreach(Instance i in active)
            {
                if(!Valid(i)) continue;
                Vector3 fairy=i.fairy==null?i.origin:i.fairy.DrawPos;
                WishArrivalGeometry.Build(Find.TickManager.TicksGame-i.startTick,
                    new MiracleGeometry.Point(i.origin.x,i.origin.z),new MiracleGeometry.Point(fairy.x,fairy.z),marks);
                marks.Sort(Compare);
                foreach(MiracleGeometry.Mark m in marks)
                {
                    properties.SetColor(ShaderPropertyIDs.Color,new Color(m.r,m.g,m.b,m.a));
                    Matrix4x4 matrix=Matrix4x4.TRS(new Vector3(m.x,(m.layer<20?low:high)+m.layer*.0005f,m.z),
                        Quaternion.AngleAxis(-m.angle*Mathf.Rad2Deg,Vector3.up),new Vector3(m.w,1,m.h));
                    Mesh mesh=m.shape==1?heart:(m.shape==2?star:(m.shape==3?petal:MeshPool.plane10));
                    Graphics.DrawMesh(mesh,matrix,m.glow?glow:solid,0,null,0,properties);
                }
            }
        }
        // Transient cosmetics expire off-map and pause with game ticks; they own no save state.
    }
}
