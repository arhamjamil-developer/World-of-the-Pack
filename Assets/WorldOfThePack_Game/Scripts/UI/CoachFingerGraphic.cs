using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.Game
{
    // Resolution-independent hand; the transform origin is the touching fingertip.
    public sealed class CoachFingerGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();
            Rounded(h,new Rect(-7,-73,28,30));
            Rounded(h,new Rect(-13,-57,47,30));
            Rounded(h,new Rect(-7,-39,14,45));
            Rounded(h,new Rect(5,-44,12,24));
            Rounded(h,new Rect(16,-46,12,23));
            Rounded(h,new Rect(26,-48,11,20));
            Rounded(h,new Rect(-22,-54,15,27));
        }
        private void Rounded(VertexHelper h,Rect r)
        {
            Capsule(h,new Rect(r.x-2,r.y-2,r.width+4,r.height+4),new Color(.16f,.09f,.035f,1));
            Capsule(h,r,new Color(1,.94f,.78f,1));
        }
        private void Capsule(VertexHelper h,Rect r,Color c)
        {
            int start=h.currentVertCount; var center=r.center;
            h.AddVert(center,c,Vector2.zero);
            const int count=32; float radius=Mathf.Min(r.width,r.height)*.5f;
            for(int i=0;i<=count;i++)
            {
                float angle=(float)i/count*Mathf.PI*2; var direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                var corner=center+new Vector2(Mathf.Sign(direction.x)*(r.width*.5f-radius),Mathf.Sign(direction.y)*(r.height*.5f-radius));
                h.AddVert(corner+direction*radius,c,Vector2.zero);
                if(i>0)h.AddTriangle(start,start+i,start+i+1);
            }
        }
    }
}
