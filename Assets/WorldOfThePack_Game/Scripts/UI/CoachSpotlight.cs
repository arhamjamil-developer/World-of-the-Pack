using UnityEngine;
using UnityEngine.UI;

namespace WorldOfThePack.Game
{
    // A real hole in the dimmer: only the demonstrated control receives pointer input.
    public sealed class CoachSpotlight : MaskableGraphic, ICanvasRaycastFilter
    {
        public Rect Hole { get; private set; }
        public void Focus(Rect hole) { Hole=hole; SetVerticesDirty(); }
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screenPoint,eventCamera,out var local);
            return !Hole.Contains(local);
        }
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear(); var r=rectTransform.rect;
            Add(helper,new Rect(r.xMin,r.yMin,Mathf.Max(0,Hole.xMin-r.xMin),r.height));
            Add(helper,new Rect(Hole.xMax,r.yMin,Mathf.Max(0,r.xMax-Hole.xMax),r.height));
            Add(helper,new Rect(Hole.xMin,r.yMin,Hole.width,Mathf.Max(0,Hole.yMin-r.yMin)));
            Add(helper,new Rect(Hole.xMin,Hole.yMax,Hole.width,Mathf.Max(0,r.yMax-Hole.yMax)));
        }
        private void Add(VertexHelper helper,Rect r)
        {
            if(r.width<=0 || r.height<=0)return;
            int start=helper.currentVertCount;
            helper.AddVert(new Vector3(r.xMin,r.yMin),color,Vector2.zero);
            helper.AddVert(new Vector3(r.xMax,r.yMin),color,Vector2.zero);
            helper.AddVert(new Vector3(r.xMax,r.yMax),color,Vector2.zero);
            helper.AddVert(new Vector3(r.xMin,r.yMax),color,Vector2.zero);
            helper.AddTriangle(start,start+1,start+2); helper.AddTriangle(start,start+2,start+3);
        }
    }
}
