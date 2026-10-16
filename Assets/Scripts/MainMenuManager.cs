using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject optionsCanvas;
    [SerializeField] private DifficultySelector difficultySelector;

    void Awake()
    {
        if (mainMenuCanvas == null) Debug.LogWarning("MainMenuManager: asigna `mainMenuCanvas` en el Inspector.");
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
        // asegurar que el selector de dificultad empieza oculto si existe
        if (difficultySelector != null && difficultySelector.gameObject != null)
            difficultySelector.gameObject.SetActive(false);
    }

    // Métodos públicos pensados para asignar a Button -> OnClick() desde el Inspector
    public void Play()
    {
        // Abrir selector de dificultad en lugar de cargar la escena del juego
        if (difficultySelector != null)
        {
            if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
            difficultySelector.Show();
        }
        else
        {
            Debug.LogWarning("MainMenuManager: `difficultySelector` no asignado. Asigna el componente `DifficultySelector` en el Inspector.");
        }
    }

    public void OpenOptions()
    {
        if (optionsCanvas != null) optionsCanvas.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}