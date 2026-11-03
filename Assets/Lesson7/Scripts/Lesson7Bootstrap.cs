using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlgoLesson7
{
    public static class Lesson7Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureGameManager()
        {
            if (SceneManager.GetActiveScene().name != "Lesson07_AutoChessSynergyArena")
                return;

            if (Object.FindFirstObjectByType<AutoChessArenaGame>() != null)
                return;

            GameObject manager = new GameObject("07_GAME_MANAGER_AutoChess");
            manager.AddComponent<AutoChessArenaGame>();
        }
    }
}
