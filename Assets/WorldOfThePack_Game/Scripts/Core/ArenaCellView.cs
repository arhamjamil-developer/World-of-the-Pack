using UnityEngine;

namespace WorldOfThePack.Game
{
    public sealed class ArenaCellView : MonoBehaviour
    {
        public int Lane, Row;
        public bool Enemy, EnemyOccupied;
        public SpriteRenderer Fill, Cross, CrossVertical;
        public LineRenderer Border;
        public void Refresh(bool occupied,bool focus,float visibility)
        {
            float pulse=.5f+.5f*Mathf.Sin(Time.unscaledTime*5);
            Color tint=Enemy?new Color(1,.29f,.24f):occupied?new Color(.35f,.9f,.62f):new Color(.25f,.8f,.95f);
            float intensity=focus?.5f+pulse*.22f:occupied?.26f:.1f;
            Fill.color=new Color(tint.r,tint.g,tint.b,intensity*visibility);
            var border=new Color(tint.r,tint.g,tint.b,(focus?1:occupied?.85f:.38f)*visibility);
            Border.startColor=Border.endColor=border;Border.startWidth=Border.endWidth=focus?.04f:.018f;
            Cross.color=new Color(tint.r,tint.g,tint.b,(focus?.8f:occupied?0:.3f)*visibility);
            if (CrossVertical != null) CrossVertical.color=Cross.color;
        }
    }
}
