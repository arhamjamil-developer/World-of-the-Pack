using UnityEngine;

namespace WorldOfThePack.CharacterArt
{
    // Art review only: does not control movement, combat, spawning, or damage.
    public sealed class PackCharacterAnimationPreview : MonoBehaviour
    {
        public Animator[] actors;
        public float secondsPerAction = 1.5f;
        public bool cycle = true;
        [Range(0, 5)] public int action;
        private int previous = -1;

        private void Update()
        {
            int selected = cycle ? Mathf.FloorToInt(Time.unscaledTime / secondsPerAction) % 6 : action;
            if (selected == previous) return;
            previous = selected;
            foreach (var actor in actors)
            {
                if (actor == null) continue;
                int available = actor.name.EndsWith("_Human") && selected > 3 ? 0 : selected;
                actor.SetInteger("Action", available);
            }
        }
    }
}
