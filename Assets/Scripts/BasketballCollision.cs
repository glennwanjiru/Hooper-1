using UnityEngine;

public class BasketballCollision : MonoBehaviour
{
    private BasketballCannonController cannonController;
    private bool hasHitHoop = false;

    public void SetCannonController(BasketballCannonController controller)
    {
        cannonController = controller;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (cannonController == null) return;

        // Play bounce sound for non-scoring objects
        if (!collision.gameObject.CompareTag("Hoop") &&
            !collision.gameObject.CompareTag("ScoreNet"))
        {
            cannonController.PlaySound(cannonController.bounceSound);
        }

        // Track if ball has hit the hoop
        if (collision.gameObject.CompareTag("Hoop"))
        {
            hasHitHoop = true;
        }
        // Handle scoring when ball hits the net
        else if (collision.gameObject.CompareTag("ScoreNet"))
        {
            // Determine points based on whether hoop was hit first
            int points = hasHitHoop ? cannonController.hoopPoints : cannonController.scoreNetPoints;
            cannonController.OnBallCollision(gameObject, collision.gameObject.tag, points);
        }
    }
}