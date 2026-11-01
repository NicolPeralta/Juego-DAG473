using UnityEngine;

public static class GameSettings
{
    public enum Difficulty { Facil, Medio, Dificil }

    public static Difficulty SelectedDifficulty = Difficulty.Medio;
    public static string GameSceneName = "GameScene";
    public static float GameDuration = 180f;
    public static int TargetCount = 6;
    public static int ConcurrentTargets = 2;
    public static float TiempoVisible = 3f;
    public static float TiempoEntreObjetos = 2f;
    public static float VelocidadMovimiento = 5f;

    public static void ApplyDifficulty(Difficulty d)
    {
        SelectedDifficulty = d;
        switch (d)
        {
            case Difficulty.Facil:
                GameDuration = 300f;      
                TargetCount = 3;
                ConcurrentTargets = 1;
                TiempoVisible = 4f;
                TiempoEntreObjetos = 2f;
                VelocidadMovimiento = 4f;
                break;
            case Difficulty.Medio:
                GameDuration = 300f;   
                TargetCount = 6;
                ConcurrentTargets = 2;
                TiempoVisible = 3.5f;
                TiempoEntreObjetos = 1.25f;
                VelocidadMovimiento = 6f;
                break;
            case Difficulty.Dificil:
                GameDuration = 300f;    
                TargetCount = 6;
                ConcurrentTargets = 3;
                TiempoVisible = 3f;
                TiempoEntreObjetos = 1f;
                VelocidadMovimiento = 9f;
                break;
        }
    }
}
