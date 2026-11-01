using UnityEngine;

// Decorative motion only; the characters use the imported skeletal animation.
public class MenuCoverMotion : MonoBehaviour
{
    public Transform[] candies;
    Vector3[] positions;
    Quaternion[] rotations;
    void Awake()
    {
        positions = new Vector3[candies.Length]; rotations = new Quaternion[candies.Length];
        for (int i = 0; i < candies.Length; i++) { positions[i] = candies[i].localPosition; rotations[i] = candies[i].localRotation; }
    }
    void Update()
    {
        for (int i = 0; i < candies.Length; i++)
        {
            float phase = Time.unscaledTime * .8f + i * 1.7f;
            candies[i].localPosition = positions[i] + Vector3.up * (Mathf.Sin(phase) * .045f);
            candies[i].localRotation = rotations[i] * Quaternion.Euler(0, Mathf.Sin(phase * .7f) * 5f, Mathf.Sin(phase) * 3f);
        }
    }
}
