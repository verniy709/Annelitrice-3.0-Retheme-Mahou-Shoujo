using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld.Planet;

namespace Annelitrice.Retheme.MahouShoujo
{
    public sealed class SubEffecter_ShiningDebut : SubEffecter
    {
        public SubEffecter_ShiningDebut(SubEffecterDef def,Effecter parent):base(def,parent) { }
        public override void SubTrigger(TargetInfo A,TargetInfo B,int overrideSpawnTick,bool force)
        {
            Map map=A.Map??B.Map;
            if(map==null) return;
            IntVec3 cell=A.Map==map?A.Cell:B.Cell;
            if(cell.InBounds(map)) ShiningDebut.Start(map,cell);
        }
    }
    public sealed class ShiningDebut : MapComponent
    {
        private sealed class Instance
        {
            internal IntVec3 cell;
            internal Vector3 origin;
            internal Pawn adult;
            internal int startTick;
            internal readonly ShiningDebutAudioTiming audio=new ShiningDebutAudioTiming();
        }
        private readonly List<Instance> active=new List<Instance>();
        private readonly List<MiracleGeometry.Mark> marks=new List<MiracleGeometry.Mark>(800);
        private static readonly Mesh[] shapes=new Mesh[9];
        private static Material solid,glow;
        private MaterialPropertyBlock properties;
        public ShiningDebut(Map map):base(map) { }
        private bool Claimed(Pawn pawn)
        { foreach(Instance i in active) if(i.adult==pawn) return true;return false; }
        internal static void Start(Map map,IntVec3 cell)
        {
            ShiningDebut controller=map.GetComponent<ShiningDebut>();
            if(controller==null) return;
            int now=Find.TickManager.TicksGame;
            Pawn adult=null;float best=64f;
            // TryDrop(...Near) has already released the pawn before this event.
            // Match this tick's Annelitrice release; older bystanders are excluded.
            IReadOnlyList<Pawn> pawns=map.mapPawns.AllPawnsSpawned;
            for(int n=0;n<pawns.Count;n++)
            {
                Pawn pawn=pawns[n];
                if(pawn.def.defName!="Annelitrice"||pawn.TickSpawned!=now||controller.Claimed(pawn)) continue;
                float distance=(pawn.Position.ToVector3Shifted()-cell.ToVector3Shifted()).sqrMagnitude;
                if(distance<best) { best=distance;adult=pawn; }
            }
            if(controller.active.Count>=24) controller.active.RemoveAt(0);
            Instance instance=new Instance{cell=cell,origin=cell.ToVector3Shifted(),adult=adult,startTick=now};
            controller.active.Add(instance);
            if(map==Find.CurrentMap)
            {
                int speed=MiracleAudioTiming.CastSpeed(Find.TickManager.TickRateMultiplier);
                controller.PlaySound(instance,"ShiningDebutOpen"+(speed==1?"":"_Fast"+speed));
            }
        }
        private void PlaySound(Instance i,string suffix)
        {
            SoundDef sound=DefDatabase<SoundDef>.GetNamedSilentFail("Anneli_Retheme_MahouShoujo_"+suffix);
            if(sound==null) return;
            TargetInfo location=i.adult==null?new TargetInfo(i.cell,map):new TargetInfo(i.adult);
            sound.PlayOneShot(SoundInfo.InMap(location));
        }
        private bool Valid(Instance i)
        {
            int age=Find.TickManager.TicksGame-i.startTick;
            if(age<0||age>=ShiningDebutGeometry.DurationTicks) return false;
            return i.adult==null||(i.adult.Spawned&&!i.adult.Destroyed&&!i.adult.Dead&&i.adult.Map==map);
        }
        public override void MapComponentTick()
        {
            for(int n=active.Count-1;n>=0;n--)
            {
                Instance i=active[n];
                if(!Valid(i)) { active.RemoveAt(n);continue; }
                // Consume cues once per instance, including when this map is not visible.
                int cues=i.audio.Advance(Find.TickManager.TicksGame-i.startTick);
                if(map!=Find.CurrentMap) continue;
                if((cues&1)!=0) PlaySound(i,"ShiningDebutBloom");
                if((cues&2)!=0) PlaySound(i,"ShiningDebutAnswerA");
                if((cues&4)!=0) PlaySound(i,"ShiningDebutAnswerB");
                if((cues&8)!=0) PlaySound(i,"ShiningDebutStarA");
                if((cues&16)!=0) PlaySound(i,"ShiningDebutStarB");
                if((cues&32)!=0) PlaySound(i,"ShiningDebutStarC");
            }
        }
        private static Mesh MakeMesh(int shape)
        {
            MiracleGeometry.Point[] edge=ShiningDebutGeometry.Outline(shape);
            bool triangle=shape>=3&&shape<=6;
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
            Mesh mesh=new Mesh();mesh.name="MahouShoujo_DebutShape"+shape;
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
            for(int n=1;n<shapes.Length;n++) if(shapes[n]==null) shapes[n]=MakeMesh(n);
            if(properties==null) properties=new MaterialPropertyBlock();
            float low=AltitudeLayer.MoteLow.AltitudeFor(),high=AltitudeLayer.MoteOverhead.AltitudeFor();
            foreach(Instance i in active)
            {
                if(!Valid(i)) continue;
                Vector3 p=i.adult==null?i.origin:i.adult.DrawPos;
                ShiningDebutGeometry.Build(Find.TickManager.TicksGame-i.startTick,
                    new MiracleGeometry.Point(i.origin.x,i.origin.z),new MiracleGeometry.Point(p.x,p.z),marks);
                marks.Sort(Compare);
                foreach(MiracleGeometry.Mark m in marks)
                {
                    properties.SetColor(ShaderPropertyIDs.Color,new Color(m.r,m.g,m.b,m.a));
                    Matrix4x4 matrix=Matrix4x4.TRS(new Vector3(m.x,(m.layer<20?low:high)+m.layer*.0005f,m.z),
                        Quaternion.AngleAxis(-m.angle*Mathf.Rad2Deg,Vector3.up),new Vector3(m.w,1,m.h));
                    Graphics.DrawMesh(m.shape==0?MeshPool.plane10:shapes[m.shape],matrix,m.glow?glow:solid,0,null,0,properties);
                }
            }
        }
        // Only transient cosmetics are owned; movement, clothes and saved pawn state are untouched.
    }
}
