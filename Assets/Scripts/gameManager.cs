using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject[] objetos;

    public float alturaSubida = 4f;
    public float velocidadMovimiento = 5f;
    public float tiempoVisible = 3f;
    public float tiempoEntreObjetos = 2f;

    private Vector3[] posicionesIniciales;
    private bool[] objetosGolpeados;
    private int ultimoObjeto = -1;
    private bool juegoActivo = true;

    //esta parte del codigo es para controlar el movimiento de los objetos que suben y bajan, y para controlar si han sido golpeados o no
    void Start()
    {
        if (objetos == null || objetos.Length == 0)
        {
            Debug.LogWarning("GameManager: no hay objetos asignados.");
            return;
        }

        posicionesIniciales = new Vector3[objetos.Length];
        objetosGolpeados = new bool[objetos.Length];

        for (int i = 0; i < objetos.Length; i++)
        {
            posicionesIniciales[i] = objetos[i].transform.position;
            objetosGolpeados[i] = false;
        }

        StartCoroutine(ControlarObjetos());
    }

    IEnumerator ControlarObjetos()
    {
        while (juegoActivo)
        {
            int indice;
            do
            {
                indice = Random.Range(0, objetos.Length);
            }
            while (objetos.Length > 1 && indice == ultimoObjeto);

            ultimoObjeto = indice;
            yield return StartCoroutine(AdministrarObjeto(indice));
            yield return new WaitForSeconds(tiempoEntreObjetos);
        }
    }
    //esta corrutina es para administrar el movimiento de un objeto en particular, subiendo y bajando, y controlando si ha sido golpeado o no
    IEnumerator AdministrarObjeto(int indice)
    {
        GameObject objeto = objetos[indice];

        Vector3 posicionAbajo = posicionesIniciales[indice];
        Vector3 posicionArriba = posicionAbajo + Vector3.up * alturaSubida;

        objetosGolpeados[indice] = false;

        // Subir
        yield return StartCoroutine(
            MoverObjeto(objeto, posicionAbajo, posicionArriba)
        );
        float tiempo = 0f;

        while (tiempo < tiempoVisible && !objetosGolpeados[indice])
        {
            tiempo += Time.deltaTime;
            yield return null;
        }
        yield return StartCoroutine(
            MoverObjeto(objeto, posicionArriba, posicionAbajo)
        );
        objetosGolpeados[indice] = false;
    }
    // todo esto son corrutinas para mover los objetos
    IEnumerator MoverObjeto(GameObject objeto, Vector3 inicio, Vector3 destino)
    {
        float tiempo = 0f;

        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime * velocidadMovimiento;

            objeto.transform.position = Vector3.Lerp(
                inicio,
                destino,
                Mathf.Clamp01(tiempo)
            );

            yield return null;
        }

        objeto.transform.position = destino;
    }
    // esta es la funcion que se llama cuando un objeto es golpeado, para marcarlo como golpeado y que deje de subir y baje
    public void BajarObjeto(GameObject objetoGolpeado)
    {
        if (objetoGolpeado == null)
            return;
        for (int i = 0; i < objetos.Length; i++)
        {
            if (objetos[i] == objetoGolpeado)
            {
                objetosGolpeados[i] = true;
                return;
            }
        }
        for (int i = 0; i < objetos.Length; i++)
        {
            if (objetoGolpeado.transform.IsChildOf(objetos[i].transform))
            {
                objetosGolpeados[i] = true;
                return;
            }
        }
        for (int i = 0; i < objetos.Length; i++)
        {
            if (objetoGolpeado.transform.root.gameObject == objetos[i])
            {
                objetosGolpeados[i] = true;
                return;
            }
        }
    }
}