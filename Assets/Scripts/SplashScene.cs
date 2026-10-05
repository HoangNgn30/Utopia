using System.Collections;
using UnityEngine;

public class SplashScene : MonoBehaviour
{
    [SerializeField] private float delayTime = 3f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delayTime);
        UIManager.LoadScene("HomeScene");
    }
}
