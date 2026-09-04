using UnityEngine;

public class BRIOFailHole : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        BRIOBall ball = other.GetComponent<BRIOBall>();

        if (ball == null)
            return;

        ball.ResetBall();
    }
}