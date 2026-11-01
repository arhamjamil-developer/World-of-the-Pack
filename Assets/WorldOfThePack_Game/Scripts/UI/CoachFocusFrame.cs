using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.Game
{
    public sealed class CoachFocusFrame : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear(); var r=rectTransform.rect; const float stroke=4;
            Quad(helper,new Rect(r.xMin,r.yMin,r.width,stroke));
            Quad(helper,new Rect(r.xMin,r.yMax-stroke,r.width,stroke));
            Quad(helper,new Rect(r.xMin,r.yMin,stroke,r.height));
            Quad(helper,new Rect(r.xMax-stroke,r.yMin,stroke,r.height));
        }
        private void Quad(VertexHelper h,Rect r)
        {
            int n=h.currentVertCount;
            h.AddVert(new Vector3(r.xMin,r.yMin),color,Vector2.zero); h.AddVert(new Vector3(r.xMax,r.yMin),color,Vector2.zero);
            h.AddVert(new Vector3(r.xMax,r.yMax),color,Vector2.zero); h.AddVert(new Vector3(r.xMin,r.yMax),color,Vector2.zero);
            h.AddTriangle(n,n+1,n+2);h.AddTriangle(n,n+2,n+3);
        }
    }
}
