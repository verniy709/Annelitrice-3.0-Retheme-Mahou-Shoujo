using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld.Planet;

namespace Annelitrice.Retheme.MahouShoujo
{
    public sealed class SubEffecter_CrystalFormation : SubEffecter
    {
        public SubEffecter_CrystalFormation(SubEffecterDef def,Effecter parent):base(def,parent) { }
        public override void SubTrigger(TargetInfo A,TargetInfo B,int overrideSpawnTick,bool force)
        {
            Map map=A.Map??B.Map;
            if(map==null) return;
            IntVec3 cell=A.Map==map?A.Cell:B.Cell;
            if(cell.InBounds(map)) CrystalFormation.Start(map,cell);
        }
    }
    public sealed class CrystalFormation : MapComponent
    {
        private sealed class Instance
        {
            internal IntVec3 cell;
            internal Vector3 origin;
            internal Thing crystal;
            internal int startTick;
            internal readonly HashSet<int> previousCrystals=new HashSet<int>();
            internal readonly CrystalFormationAudioTiming audio=new CrystalFormationAudioTiming();
        }
        private readonly List<Instance> active=new List<Instance>();
        private readonly List<MiracleGeometry.Mark> marks=new List<MiracleGeometry.Mark>(640);
        private static Material solid,glow;
        private static Mesh heart,star;
        private static readonly Mesh[] facets=new Mesh[4];
        private MaterialPropertyBlock properties;
        public CrystalFormation(Map map):base(map) { }
        private bool Claimed(Thing thing)
        { foreach(Instance i in active) if(i.crystal==thing) return true;return false; }
        private void VisitNearby(Instance instance,bool snapshot)
        {
            Thing best=null;int bestDistance=int.MaxValue;
            for(int x=-1;x<=1;x++) for(int z=-1;z<=1;z++)
            {
                IntVec3 cell=instance.cell+new IntVec3(x,0,z);
                if(!cell.InBounds(map)) continue;
                List<Thing> things=map.thingGrid.ThingsListAtFast(cell);
                for(int n=0;n<things.Count;n++)
                {
                    Thing thing=things[n];
                    if(thing.def.defName!="Anneli_Pupa") continue;
                    if(snapshot) { instance.previousCrystals.Add(thing.thingIDNumber);continue; }
                    if(thing.TickSpawned!=instance.startTick||instance.previousCrystals.Contains(thing.thingIDNumber)||Claimed(thing)) continue;
                    int distance=cell.DistanceToSquared(instance.cell);
                    if(distance<bestDistance) { best=thing;bestDistance=distance; }
                }
            }
            if(!snapshot) instance.crystal=best;
        }
        internal static void Start(Map map,IntVec3 cell)
        {
            CrystalFormation controller=map.GetComponent<CrystalFormation>();
            if(controller==null) return;
            if(controller.active.Count>=24) controller.active.RemoveAt(0);
            Instance instance=new Instance{cell=cell,origin=cell.ToVector3Shifted(),startTick=Find.TickManager.TicksGame};
            // The base game creates the pupa after this trigger returns. Snapshot old items now.
            controller.VisitNearby(instance,true);
            controller.active.Add(instance);
            if(map==Find.CurrentMap)
            {
                int speed=MiracleAudioTiming.CastSpeed(Find.TickManager.TickRateMultiplier);
                controller.PlaySound(instance,"CrystalFormationGather"+(speed==1?"":"_Fast"+speed));
            }
        }
        private void PlaySound(Instance i,string suffix)
        {
            SoundDef sound=DefDatabase<SoundDef>.GetNamedSilentFail("Anneli_Retheme_MahouShoujo_"+suffix);
            if(sound==null) return;
            TargetInfo location=i.crystal==null?new TargetInfo(i.cell,map):new TargetInfo(i.crystal);
            sound.PlayOneShot(SoundInfo.InMap(location));
        }
        private bool Valid(Instance i)
        {
            if(Find.TickManager.TicksGame-i.startTick>=CrystalFormationGeometry.DurationTicks) return false;
            return i.crystal==null||(i.crystal.Spawned&&!i.crystal.Destroyed&&i.crystal.Map==map);
        }
        private void BindNewCrystal(Instance i)
        {
            if(i.crystal==null&&Find.TickManager.TicksGame-i.startTick<=8) VisitNearby(i,false);
        }
        public override void MapComponentTick()
        {
            for(int n=active.Count-1;n>=0;n--)
            {
                Instance i=active[n];
                if(!Valid(i)) { active.RemoveAt(n);continue; }
                BindNewCrystal(i);
                // Tick cues survive skipped render frames. Off-map cues are consumed silently.
                int cues=i.audio.Advance(Find.TickManager.TicksGame-i.startTick);
                if(map==Find.CurrentMap)
                {
                    if((cues&1)!=0) PlaySound(i,"CrystalFormationFacetA");
                    if((cues&2)!=0) PlaySound(i,"CrystalFormationFacetB");
                    if((cues&4)!=0) PlaySound(i,"CrystalFormationFacetC");
                    if((cues&8)!=0) PlaySound(i,"CrystalFormationSeal");
                }
            }
        }
        private static Mesh MakeMesh(int shape)
        {
            MiracleGeometry.Point[] edge=CrystalFormationGeometry.Outline(shape);
            bool triangle=shape>=3;
            Vector3[] vertices=new Vector3[edge.Length+(triangle?0:1)];
            Vector2[] uv=new Vector2[vertices.Length];Color[] colors=new Color[vertices.Length];
            int[] triangles=new int[triangle?3:edge.Length*3];
            if(!triangle) { uv[0]=new Vector2(.5f,.5f);colors[0]=Color.white; }
            for(int i=0;i<edge.Length;i++)
            {
                int at=i+(triangle?0:1);
                vertices[at]=new Vector3(edge[i].x,0,edge[i].z);uv[at]=new Vector2(edge[i].x+.5f,edge[i].z+.5f);colors[at]=Color.white;
                if(triangle) triangles[i]=i;
                else { triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=(i+1)%edge.Length+1; }
            }
            Mesh mesh=new Mesh();mesh.name="MahouShoujo_CrystalShape"+shape;
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
            if(heart==null) heart=MakeMesh(1);if(star==null) star=MakeMesh(2);
            for(int n=0;n<4;n++) if(facets[n]==null) facets[n]=MakeMesh(n+3);
            if(properties==null) properties=new MaterialPropertyBlock();
            float low=AltitudeLayer.MoteLow.AltitudeFor(),high=AltitudeLayer.MoteOverhead.AltitudeFor();
            foreach(Instance i in active)
            {
                if(!Valid(i)) continue;
                BindNewCrystal(i);
                Vector3 p=i.crystal==null?i.origin:i.crystal.DrawPos;
                CrystalFormationGeometry.Build(Find.TickManager.TicksGame-i.startTick,
                    new MiracleGeometry.Point(i.origin.x,i.origin.z),new MiracleGeometry.Point(p.x,p.z),marks);
                marks.Sort(Compare);
                foreach(MiracleGeometry.Mark m in marks)
                {
                    properties.SetColor(ShaderPropertyIDs.Color,new Color(m.r,m.g,m.b,m.a));
                    Matrix4x4 matrix=Matrix4x4.TRS(new Vector3(m.x,(m.layer<20?low:high)+m.layer*.0005f,m.z),
                        Quaternion.AngleAxis(-m.angle*Mathf.Rad2Deg,Vector3.up),new Vector3(m.w,1,m.h));
                    Mesh mesh=m.shape==1?heart:(m.shape==2?star:(m.shape>=3?facets[m.shape-3]:MeshPool.plane10));
                    Graphics.DrawMesh(mesh,matrix,m.glow?glow:solid,0,null,0,properties);
                }
            }
        }
        // No gameplay state is owned here; off-map instances expire and reload drops cosmetics.
    }
}
