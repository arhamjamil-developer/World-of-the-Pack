using UnityEditor;

namespace WorldOfThePack.EditorTools
{
    [InitializeOnLoad]
    public static class EditorKeepAlive
    {
        static EditorKeepAlive()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
