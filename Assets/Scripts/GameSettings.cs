using UnityEngine;

public static class GameSettings
{
    public enum Difficulty { Facil, Medio, Dificil }

    public static Difficulty SelectedDifficulty = Difficulty.Medio;
    public static string GameSceneName = "GameScene";
    public static float GameDuration = 180f;
    public static int TargetCount = 6;
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
                TiempoVisible = 4f;
                TiempoEntreObjetos = 2f;
                VelocidadMovimiento = 4f;
                break;
            case Difficulty.Medio:
                GameDuration = 300f;   
                TargetCount = 6;
                TiempoVisible = 3f;      // aparecen/desaparecen más rápido
                TiempoEntreObjetos = 1.5f;
                VelocidadMovimiento = 6f;
                break;
            case Difficulty.Dificil:
                GameDuration = 300f;    
                TargetCount = 9;
                TiempoVisible = 2f;    // mucho más rápido
                TiempoEntreObjetos = 1f;
                VelocidadMovimiento = 9f;
                break;
        }
    }
}