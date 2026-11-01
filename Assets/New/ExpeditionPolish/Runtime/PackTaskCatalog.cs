using UnityEngine;

namespace WorldOfThePack.Expedition
{
    public enum PackTaskKind { Defeat, Deploy, Moonshift, Travel, Camp }
    [System.Serializable]
    public class PackTask
    {
        public string title;
        [TextArea] public string hint;
        public PackTaskKind kind;
        [Min(1)] public int target = 3;
        [Min(1)] public int reward = 3;
    }
    [CreateAssetMenu(menuName = "World of the Pack/Expedition/Task Catalog")]
    public sealed class PackTaskCatalog : ScriptableObject
    {
        public PackTask[] tasks;
    }
}
