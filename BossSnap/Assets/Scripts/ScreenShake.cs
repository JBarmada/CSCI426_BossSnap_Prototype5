using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    private Vector3 currentShakeOffset;
    private Coroutine shakeRoutine;

    public Vector3 CurrentOffset => currentShakeOffset;

    public void Shake(float intensity, float duration)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            currentShakeOffset = Random.insideUnitSphere * intensity;

            elapsed += Time.deltaTime;
            yield return null;
        }

        currentShakeOffset = Vector3.zero;
        shakeRoutine = null;
    }
}